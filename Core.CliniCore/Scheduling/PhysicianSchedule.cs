using Core.CliniCore.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Scheduling
{
    /// <summary>
    /// Manages the schedule for an individual physician
    /// </summary>
    public class PhysicianSchedule
    {
        private readonly List<AppointmentTimeInterval> _appointments;
        private readonly List<UnavailableTimeInterval> _unavailableBlocks;
        private readonly object _lock = new object();

        public PhysicianSchedule(Guid physicianId)
        {
            PhysicianId = physicianId;
            _appointments = new List<AppointmentTimeInterval>();
            _unavailableBlocks = new List<UnavailableTimeInterval>();
            InitializeStandardUnavailability();
        }

        /// <summary>
        /// The physician this schedule belongs to
        /// </summary>
        public Guid PhysicianId { get; }

        /// <summary>
        /// All appointments for this physician
        /// </summary>
        public IReadOnlyList<AppointmentTimeInterval> Appointments => _appointments.AsReadOnly();

        /// <summary>
        /// All unavailable blocks for this physician
        /// </summary>
        public IReadOnlyList<UnavailableTimeInterval> UnavailableBlocks => _unavailableBlocks.AsReadOnly();

        /// <summary>
        /// Standard weekly availability pattern (e.g., which days physician works)
        /// </summary>
        public Dictionary<DayOfWeek, (TimeSpan Start, TimeSpan End)> StandardAvailability { get; set; }
            = new Dictionary<DayOfWeek, (TimeSpan, TimeSpan)>();

        /// <summary>
        /// Initializes standard unavailability (lunch breaks, non-business hours)
        /// </summary>
        private void InitializeStandardUnavailability()
        {
            // Set default availability Monday-Friday, 8 AM - 5 PM
            StandardAvailability[DayOfWeek.Monday] = (new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0));
            StandardAvailability[DayOfWeek.Tuesday] = (new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0));
            StandardAvailability[DayOfWeek.Wednesday] = (new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0));
            StandardAvailability[DayOfWeek.Thursday] = (new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0));
            StandardAvailability[DayOfWeek.Friday] = (new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0));
            // Weekends not available by default
        }

        /// <summary>
        /// Adds an appointment to the schedule if no conflicts exist
        /// </summary>
        public bool TryAddAppointment(AppointmentTimeInterval appointment)
        {
            if (appointment == null || appointment.PhysicianId != PhysicianId)
                return false;

            lock (_lock)
            {
                // Check for conflicts
                if (HasConflict(appointment))
                    return false;

                _appointments.Add(appointment);
                return true;
            }
        }

        /// <summary>
        /// Loads an appointment directly without conflict checking.
        /// Used when loading persisted data from repository.
        /// </summary>
        internal void LoadAppointment(AppointmentTimeInterval appointment)
        {
            if (appointment == null || appointment.PhysicianId != PhysicianId)
                return;

            lock (_lock)
            {
                _appointments.Add(appointment);
            }
        }

        /// <summary>
        /// Removes an appointment from the schedule
        /// </summary>
        public bool RemoveAppointment(Guid appointmentId)
        {
            lock (_lock)
            {
                return _appointments.RemoveAll(a => a.Id == appointmentId) > 0;
            }
        }

        /// <summary>
        /// Adds an unavailable block to the schedule
        /// </summary>
        public void AddUnavailableBlock(UnavailableTimeInterval block)
        {
            if (block == null) return;

            lock (_lock)
            {
                // Set physician ID if not already set
                if (!block.PhysicianId.HasValue)
                    block.PhysicianId = PhysicianId;

                _unavailableBlocks.Add(block);
            }
        }

        /// <summary>
        /// Checks if there's a conflict for a proposed appointment
        /// </summary>
        public bool HasConflict(AppointmentTimeInterval proposedAppointment)
        {
            if (proposedAppointment == null) return false;

            lock (_lock)
            {
                // Check against existing appointments
                foreach (var existing in _appointments)
                {
                    if (existing.Status == AppointmentStatus.Scheduled &&
                        existing.Id != proposedAppointment.Id &&
                        existing.Overlaps(proposedAppointment))
                    {
                        return true;
                    }
                }

                // Check against unavailable blocks
                foreach (var block in _unavailableBlocks)
                {
                    if (block.Overlaps(proposedAppointment))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Checks if a specific time slot is available
        /// </summary>
        public bool IsTimeSlotAvailable(DateTime start, DateTime end)
        {
            // Create a temporary appointment to check
            var tempAppointment = new AppointmentTimeInterval(
                start, end, Guid.Empty, PhysicianId, "Availability Check");

            return !HasConflict(tempAppointment) && IsWithinStandardAvailability(start, end);
        }

        /// <summary>
        /// Checks if a time slot falls within standard availability
        /// </summary>
        private bool IsWithinStandardAvailability(DateTime start, DateTime end)
        {
            // Check if the day is in standard availability
            if (!StandardAvailability.ContainsKey(start.DayOfWeek))
                return false;

            var dayAvailability = StandardAvailability[start.DayOfWeek];

            // Check if times fall within the day's availability
            return start.TimeOfDay >= dayAvailability.Start &&
                   end.TimeOfDay <= dayAvailability.End &&
                   start.Date == end.Date; // Don't span days
        }

        /// <summary>
        /// Gets all appointments for a specific date
        /// </summary>
        public IEnumerable<AppointmentTimeInterval> GetAppointmentsForDate(DateTime date)
        {
            lock (_lock)
            {
                return _appointments
                    .Where(a => a.Start.Date == date.Date)
                    .OrderBy(a => a.Start)
                    .ToList();
            }
        }

        /// <summary>
        /// Gets all appointments in a date range
        /// </summary>
        public IEnumerable<AppointmentTimeInterval> GetAppointmentsInRange(DateTime startDate, DateTime endDate)
        {
            lock (_lock)
            {
                return _appointments
                    .Where(a => a.Start >= startDate && a.End <= endDate)
                    .OrderBy(a => a.Start)
                    .ToList();
            }
        }

        /// <summary>
        /// Finds the next available time slot of a given duration
        /// </summary>
        public DateTime? FindNextAvailableSlot(TimeSpan duration, DateTime? afterTime = null)
        {
            var searchStart = afterTime ?? DateTime.Now;

            // Round up to next 15-minute increment
            var minutes = searchStart.Minute;
            var roundedMinutes = ((minutes / 15) + 1) * 15;
            if (roundedMinutes == 60)
            {
                searchStart = searchStart.Date.AddHours(searchStart.Hour + 1);
            }
            else
            {
                searchStart = searchStart.Date.AddHours(searchStart.Hour).AddMinutes(roundedMinutes);
            }

            // Search for up to 30 days
            var searchEnd = searchStart.AddDays(30);

            while (searchStart < searchEnd)
            {
                // Skip weekends
                if (searchStart.DayOfWeek == DayOfWeek.Saturday ||
                    searchStart.DayOfWeek == DayOfWeek.Sunday)
                {
                    searchStart = searchStart.Date.AddDays(1).AddHours(8); // Next Monday at 8 AM
                    continue;
                }

                // Check if this day has standard availability
                if (!StandardAvailability.ContainsKey(searchStart.DayOfWeek))
                {
                    searchStart = searchStart.Date.AddDays(1).AddHours(8);
                    continue;
                }

                var dayAvailability = StandardAvailability[searchStart.DayOfWeek];

                // Ensure we're within the day's hours
                if (searchStart.TimeOfDay < dayAvailability.Start)
                {
                    searchStart = searchStart.Date.Add(dayAvailability.Start);
                }

                var proposedEnd = searchStart.Add(duration);

                // If the slot would go past end of day, try next day
                if (proposedEnd.TimeOfDay > dayAvailability.End)
                {
                    searchStart = searchStart.Date.AddDays(1).AddHours(8);
                    continue;
                }

                // Check if this slot is available
                if (IsTimeSlotAvailable(searchStart, proposedEnd))
                {
                    return searchStart;
                }

                // Try next 15-minute slot
                searchStart = searchStart.AddMinutes(15);
            }

            return null; // No available slot found in next 30 days
        }

        /// <summary>
        /// Gets availability summary for a date
        /// </summary>
        public ScheduleAvailabilitySummary GetAvailabilitySummary(DateTime date)
        {
            var appointments = GetAppointmentsForDate(date).ToList();
            var totalScheduled = appointments.Where(a => a.Status == AppointmentStatus.Scheduled).Count();
            var totalHours = appointments.Sum(a => a.Duration.TotalHours);

            // Explicitly type the tuple to preserve names
            (TimeSpan Start, TimeSpan End) dayAvailability = StandardAvailability.ContainsKey(date.DayOfWeek)
                ? StandardAvailability[date.DayOfWeek]
                : (Start: new TimeSpan(0, 0, 0), End: new TimeSpan(0, 0, 0));

            var totalAvailableHours = (dayAvailability.End - dayAvailability.Start).TotalHours;

            return new ScheduleAvailabilitySummary
            {
                Date = date,
                TotalAppointments = totalScheduled,
                TotalBookedHours = totalHours,
                TotalAvailableHours = totalAvailableHours,
                UtilizationPercentage = totalAvailableHours > 0
                    ? (totalHours / totalAvailableHours) * 100
                    : 0
            };
        }

        /// <summary>
        /// Clears old appointments from the schedule
        /// </summary>
        public int ClearOldAppointments(DateTime beforeDate)
        {
            lock (_lock)
            {
                return _appointments.RemoveAll(a => a.End < beforeDate);
            }
        }
    }

    /// <summary>
    /// Summary of schedule availability for a date
    /// </summary>
    public class ScheduleAvailabilitySummary
    {
        public DateTime Date { get; set; }
        public int TotalAppointments { get; set; }
        public double TotalBookedHours { get; set; }
        public double TotalAvailableHours { get; set; }
        public double UtilizationPercentage { get; set; }

        public override string ToString()
        {
            return $"{Date:yyyy-MM-dd}: {TotalAppointments} appointments, " +
                   $"{TotalBookedHours:F1}/{TotalAvailableHours:F1} hours ({UtilizationPercentage:F1}% utilized)";
        }
    }
}