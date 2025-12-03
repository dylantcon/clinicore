using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Scheduling;

namespace Core.CliniCore.Repositories.InMemory
{
    /// <summary>
    /// In-memory implementation of IAppointmentRepository.
    /// Provides appointment-specific query operations including conflict detection.
    /// </summary>
    public class InMemoryAppointmentRepository : InMemoryRepositoryBase<AppointmentTimeInterval>, IAppointmentRepository
    {
        // Business hours for slot calculation
        private static readonly TimeSpan BusinessStart = new TimeSpan(8, 0, 0);
        private static readonly TimeSpan BusinessEnd = new TimeSpan(17, 0, 0);

        /// <summary>
        /// Gets all appointments for a specific date
        /// </summary>
        public IEnumerable<AppointmentTimeInterval> GetByDate(DateTime date)
        {
            lock (_lock)
            {
                return _entities.Values
                    .Where(a => a.Start.Date == date.Date)
                    .OrderBy(a => a.Start)
                    .ToList();
            }
        }

        /// <summary>
        /// Gets all appointments for a specific physician
        /// </summary>
        public IEnumerable<AppointmentTimeInterval> GetByPhysician(Guid physicianId)
        {
            lock (_lock)
            {
                return _entities.Values
                    .Where(a => a.PhysicianId == physicianId)
                    .OrderBy(a => a.Start)
                    .ToList();
            }
        }

        /// <summary>
        /// Gets all appointments for a specific patient
        /// </summary>
        public IEnumerable<AppointmentTimeInterval> GetByPatient(Guid patientId)
        {
            lock (_lock)
            {
                return _entities.Values
                    .Where(a => a.PatientId == patientId)
                    .OrderBy(a => a.Start)
                    .ToList();
            }
        }

        /// <summary>
        /// Gets appointments filtered by status
        /// </summary>
        public IEnumerable<AppointmentTimeInterval> GetByStatus(AppointmentStatus status)
        {
            lock (_lock)
            {
                return _entities.Values
                    .Where(a => a.Status == status)
                    .OrderBy(a => a.Start)
                    .ToList();
            }
        }

        /// <summary>
        /// Checks if a proposed appointment would conflict with existing ones
        /// </summary>
        /// <param name="physicianId">The physician's ID</param>
        /// <param name="start">Proposed start time</param>
        /// <param name="duration">Proposed duration</param>
        /// <param name="excludeAppointmentId">Optional ID to exclude (for rescheduling)</param>
        /// <returns>True if there would be a conflict</returns>
        public bool HasConflict(Guid physicianId, DateTime start, TimeSpan duration, Guid? excludeAppointmentId = null)
        {
            var proposedEnd = start + duration;

            lock (_lock)
            {
                return _entities.Values
                    .Where(a => a.PhysicianId == physicianId)
                    .Where(a => a.Status == AppointmentStatus.Scheduled) // Only active appointments
                    .Where(a => excludeAppointmentId == null || a.Id != excludeAppointmentId)
                    .Any(a => start < a.End && proposedEnd > a.Start); // Overlap check
            }
        }

        /// <summary>
        /// Gets available time slots for a physician on a given date.
        /// Finds gaps between existing appointments within business hours.
        /// </summary>
        public IEnumerable<(DateTime Start, DateTime End)> GetAvailableSlots(
            Guid physicianId, DateTime date, TimeSpan duration)
        {
            // Only weekdays
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                return Enumerable.Empty<(DateTime, DateTime)>();

            var dayStart = date.Date + BusinessStart;
            var dayEnd = date.Date + BusinessEnd;

            // Get scheduled appointments for this physician on this date
            var appointments = GetByPhysician(physicianId)
                .Where(a => a.Start.Date == date.Date)
                .Where(a => a.Status == AppointmentStatus.Scheduled)
                .OrderBy(a => a.Start)
                .ToList();

            var slots = new List<(DateTime Start, DateTime End)>();
            var currentStart = dayStart;

            foreach (var appt in appointments)
            {
                // If there's a gap before this appointment
                if (appt.Start > currentStart)
                {
                    var gapEnd = appt.Start;
                    // Check if the gap is large enough for the requested duration
                    if (gapEnd - currentStart >= duration)
                    {
                        slots.Add((currentStart, gapEnd));
                    }
                }

                // Move current start past this appointment
                if (appt.End > currentStart)
                {
                    currentStart = appt.End;
                }
            }

            // Check for remaining time at end of day
            if (dayEnd > currentStart && dayEnd - currentStart >= duration)
            {
                slots.Add((currentStart, dayEnd));
            }

            return slots;
        }

        /// <summary>
        /// Searches appointments by description, reason for visit, or notes
        /// </summary>
        public override IEnumerable<AppointmentTimeInterval> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return GetAll();

            var lowerQuery = query.ToLowerInvariant();

            lock (_lock)
            {
                return _entities.Values
                    .Where(a =>
                        a.Description.ToLowerInvariant().Contains(lowerQuery) ||
                        (a.ReasonForVisit?.ToLowerInvariant().Contains(lowerQuery) ?? false) ||
                        (a.Notes?.ToLowerInvariant().Contains(lowerQuery) ?? false) ||
                        a.AppointmentType.ToLowerInvariant().Contains(lowerQuery))
                    .OrderBy(a => a.Start)
                    .ToList();
            }
        }

        /// <summary>
        /// Gets upcoming appointments (scheduled, starting after now)
        /// </summary>
        public IEnumerable<AppointmentTimeInterval> GetUpcoming(int days = 7)
        {
            var now = DateTime.Now;
            var cutoff = now.AddDays(days);

            lock (_lock)
            {
                return _entities.Values
                    .Where(a => a.Status == AppointmentStatus.Scheduled)
                    .Where(a => a.Start > now && a.Start <= cutoff)
                    .OrderBy(a => a.Start)
                    .ToList();
            }
        }

        /// <summary>
        /// Gets appointments within a date range
        /// </summary>
        public IEnumerable<AppointmentTimeInterval> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            lock (_lock)
            {
                return _entities.Values
                    .Where(a => a.Start.Date >= startDate.Date && a.Start.Date <= endDate.Date)
                    .OrderBy(a => a.Start)
                    .ToList();
            }
        }
    }
}
