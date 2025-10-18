using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Scheduling.BookingStrategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Scheduling.Management
{
    /// <summary>
    /// High-level facade for managing scheduling operations across the system
    /// </summary>
    public class ScheduleManager
    {
        private static ScheduleManager? _instance;
        private static readonly object _instanceLock = new object();
        
        private readonly Dictionary<Guid, PhysicianSchedule> _physicianSchedules;
        private readonly List<UnavailableTimeInterval> _facilityUnavailableBlocks;
        private readonly ScheduleConflictResolver _conflictResolver;
        private readonly IBookingStrategy _defaultBookingStrategy;
        private readonly object _lock = new object();

        public static ScheduleManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        _instance ??= new ScheduleManager();
                    }
                }
                return _instance;
            }
        }

        private ScheduleManager()
        {
            _physicianSchedules = new Dictionary<Guid, PhysicianSchedule>();
            _facilityUnavailableBlocks = new List<UnavailableTimeInterval>();
            _conflictResolver = new ScheduleConflictResolver();
            _defaultBookingStrategy = new FirstAvailableBookingStrategy();
            InitializeFacilitySchedule();
        }

        /// <summary>
        /// Initializes facility-wide scheduling rules
        /// </summary>
        private void InitializeFacilitySchedule()
        {
            // Add standard non-business hours for the next 90 days
            var today = DateTime.Today;
            for (int i = 0; i < 90; i++)
            {
                var date = today.AddDays(i);

                // Add weekend blocks
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    _facilityUnavailableBlocks.Add(new UnavailableTimeInterval(
                        date,
                        date.AddDays(1),
                        UnavailableTimeInterval.UnavailabilityReason.NonBusinessHours,
                        $"Weekend - {date.DayOfWeek}"
                    ));
                }
                else
                {
                    // Add before and after hours blocks
                    _facilityUnavailableBlocks.AddRange(
                        UnavailableTimeInterval.CreateNonBusinessHours(date));
                }
            }
        }

        /// <summary>
        /// Gets or creates a physician's schedule
        /// </summary>
        public PhysicianSchedule GetPhysicianSchedule(Guid physicianId)
        {
            lock (_lock)
            {
                if (!_physicianSchedules.ContainsKey(physicianId))
                {
                    _physicianSchedules[physicianId] = new PhysicianSchedule(physicianId);
                }
                return _physicianSchedules[physicianId];
            }
        }

        /// <summary>
        /// Schedules an appointment after checking for conflicts
        /// </summary>
        public ScheduleResult ScheduleAppointment(AppointmentTimeInterval appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            lock (_lock)
            {
                var physicianSchedule = GetPhysicianSchedule(appointment.PhysicianId);

                // Check for conflicts
                var conflictResult = _conflictResolver.CheckForConflicts(
                    appointment,
                    physicianSchedule,
                    _facilityUnavailableBlocks);

                if (conflictResult.HasConflicts)
                {
                    // Try to resolve conflicts
                    var resolution = _conflictResolver.ResolveConflicts(conflictResult, physicianSchedule);

                    return new ScheduleResult
                    {
                        Success = false,
                        AppointmentId = appointment.Id,
                        Conflicts = conflictResult.Conflicts,
                        AlternativeSuggestions = resolution.AlternativeSlots,
                        Message = conflictResult.GetSummary()
                    };
                }

                // No conflicts, add the appointment
                if (physicianSchedule.TryAddAppointment(appointment))
                {
                    return new ScheduleResult
                    {
                        Success = true,
                        AppointmentId = appointment.Id,
                        Message = "Appointment scheduled successfully."
                    };
                }
                else
                {
                    return new ScheduleResult
                    {
                        Success = false,
                        AppointmentId = appointment.Id,
                        Message = "Failed to add appointment to schedule."
                    };
                }
            }
        }

        /// <summary>
        /// Cancels an appointment
        /// </summary>
        public bool CancelAppointment(Guid physicianId, Guid appointmentId, string reason = "")
        {
            lock (_lock)
            {
                var schedule = GetPhysicianSchedule(physicianId);
                var appointment = schedule.Appointments.FirstOrDefault(a => a.Id == appointmentId);

                if (appointment != null)
                {
                    appointment.Cancel(reason);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Deletes an appointment from the schedule
        /// </summary>
        public bool DeleteAppointment(Guid physicianId, Guid appointmentId)
        {
            lock (_lock)
            {
                var schedule = GetPhysicianSchedule(physicianId);
                return schedule.RemoveAppointment(appointmentId);
            }
        }

        /// <summary>
        /// Reschedules an appointment
        /// </summary>
        public ScheduleResult RescheduleAppointment(
            Guid physicianId,
            Guid appointmentId,
            DateTime newStart,
            DateTime newEnd)
        {
            lock (_lock)
            {
                var schedule = GetPhysicianSchedule(physicianId);
                var originalAppointment = schedule.Appointments.FirstOrDefault(a => a.Id == appointmentId);

                if (originalAppointment == null)
                {
                    return new ScheduleResult
                    {
                        Success = false,
                        Message = "Original appointment not found."
                    };
                }

                // Create the rescheduled appointment
                var newAppointment = originalAppointment.Reschedule(newStart, newEnd);

                // Try to schedule the new appointment
                var result = ScheduleAppointment(newAppointment);

                if (!result.Success)
                {
                    // Restore original if rescheduling fails
                    originalAppointment.Status = AppointmentStatus.Scheduled;
                    originalAppointment.CancellationReason = null;
                }

                return result;
            }
        }

        /// <summary>
        /// Finds the next available appointment slot using the specified strategy
        /// </summary>
        public AppointmentSlot? FindNextAvailableSlot(
            Guid physicianId,
            TimeSpan duration,
            DateTime? afterTime = null,
            IBookingStrategy? strategy = null)
        {
            lock (_lock)
            {
                var schedule = GetPhysicianSchedule(physicianId);
                strategy = strategy ?? _defaultBookingStrategy;

                return strategy.FindNextAvailableSlot(
                    schedule,
                    duration,
                    afterTime ?? DateTime.Now,
                    _facilityUnavailableBlocks);
            }
        }

        /// <summary>
        /// Gets all appointments for a physician on a specific date
        /// </summary>
        public IEnumerable<AppointmentTimeInterval> GetDailySchedule(Guid physicianId, DateTime date)
        {
            lock (_lock)
            {
                var schedule = GetPhysicianSchedule(physicianId);
                return schedule.GetAppointmentsForDate(date);
            }
        }

        /// <summary>
        /// Gets all appointments for a physician in a date range
        /// </summary>
        public IEnumerable<AppointmentTimeInterval> GetScheduleInRange(
            Guid physicianId,
            DateTime startDate,
            DateTime endDate)
        {
            lock (_lock)
            {
                var schedule = GetPhysicianSchedule(physicianId);
                return schedule.GetAppointmentsInRange(startDate, endDate);
            }
        }

        /// <summary>
        /// Gets all appointments for a patient
        /// </summary>
        public IEnumerable<AppointmentTimeInterval> GetPatientAppointments(Guid patientId)
        {
            lock (_lock)
            {
                var appointments = new List<AppointmentTimeInterval>();

                foreach (var schedule in _physicianSchedules.Values)
                {
                    appointments.AddRange(
                        schedule.Appointments.Where(a => a.PatientId == patientId));
                }

                return appointments.OrderBy(a => a.Start);
            }
        }

        /// <summary>
        /// Gets all appointments across all physician schedules
        /// </summary>
        public IEnumerable<AppointmentTimeInterval> GetAllAppointments()
        {
            lock (_lock)
            {
                var appointments = new List<AppointmentTimeInterval>();

                foreach (var schedule in _physicianSchedules.Values)
                {
                    appointments.AddRange(schedule.Appointments);
                }

                return appointments.OrderBy(a => a.Start);
            }
        }

        /// <summary>
        /// Finds a specific appointment by ID across all physician schedules
        /// </summary>
        public AppointmentTimeInterval? FindAppointmentById(Guid appointmentId)
        {
            lock (_lock)
            {
                foreach (var schedule in _physicianSchedules.Values)
                {
                    var appointment = schedule.Appointments.FirstOrDefault(a => a.Id == appointmentId);
                    if (appointment != null)
                        return appointment;
                }
                return null;
            }
        }

        /// <summary>
        /// Adds a facility-wide unavailable block (holidays, emergencies, etc.)
        /// </summary>
        public void AddFacilityUnavailableBlock(UnavailableTimeInterval block)
        {
            if (block == null) return;

            lock (_lock)
            {
                _facilityUnavailableBlocks.Add(block);
            }
        }

        /// <summary>
        /// Adds a physician-specific unavailable block
        /// </summary>
        public void AddPhysicianUnavailableBlock(Guid physicianId, UnavailableTimeInterval block)
        {
            if (block == null) return;

            lock (_lock)
            {
                var schedule = GetPhysicianSchedule(physicianId);
                schedule.AddUnavailableBlock(block);
            }
        }

        /// <summary>
        /// Sets standard weekly availability for a physician
        /// </summary>
        public void SetPhysicianAvailability(
            Guid physicianId,
            Dictionary<DayOfWeek, (TimeSpan Start, TimeSpan End)> availability)
        {
            lock (_lock)
            {
                var schedule = GetPhysicianSchedule(physicianId);
                schedule.StandardAvailability = availability;
            }
        }

        /// <summary>
        /// Gets utilization statistics for a physician
        /// </summary>
        public ScheduleStatistics GetPhysicianStatistics(Guid physicianId, DateTime startDate, DateTime endDate)
        {
            lock (_lock)
            {
                var schedule = GetPhysicianSchedule(physicianId);
                var appointments = schedule.GetAppointmentsInRange(startDate, endDate).ToList();

                return new ScheduleStatistics
                {
                    PhysicianId = physicianId,
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalAppointments = appointments.Count,
                    CompletedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed),
                    CancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                    NoShowAppointments = appointments.Count(a => a.Status == AppointmentStatus.NoShow),
                    TotalScheduledHours = appointments
                        .Where(a => a.Status == AppointmentStatus.Scheduled ||
                                   a.Status == AppointmentStatus.Completed)
                        .Sum(a => a.Duration.TotalHours),
                    AverageAppointmentDuration = appointments.Any()
                        ? TimeSpan.FromMinutes(appointments.Average(a => a.Duration.TotalMinutes))
                        : TimeSpan.Zero
                };
            }
        }

        /// <summary>
        /// Performs cleanup of old appointments
        /// </summary>
        public int CleanupOldAppointments(DateTime beforeDate)
        {
            lock (_lock)
            {
                int totalCleaned = 0;

                foreach (var schedule in _physicianSchedules.Values)
                {
                    totalCleaned += schedule.ClearOldAppointments(beforeDate);
                }

                // Also clean up old facility blocks
                _facilityUnavailableBlocks.RemoveAll(b => b.End < beforeDate);

                return totalCleaned;
            }
        }
    }

    /// <summary>
    /// Result of a scheduling operation
    /// </summary>
    public class ScheduleResult
    {
        public bool Success { get; set; }
        public Guid AppointmentId { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ScheduleConflict> Conflicts { get; set; } = new List<ScheduleConflict>();
        public List<TimeSlotSuggestion> AlternativeSuggestions { get; set; } = new List<TimeSlotSuggestion>();
    }

    /// <summary>
    /// Available appointment slot
    /// </summary>
    public class AppointmentSlot
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public Guid PhysicianId { get; set; }
        public bool IsOptimal { get; set; }
    }

    /// <summary>
    /// Schedule statistics
    /// </summary>
    public class ScheduleStatistics
    {
        public Guid PhysicianId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int NoShowAppointments { get; set; }
        public double TotalScheduledHours { get; set; }
        public TimeSpan AverageAppointmentDuration { get; set; }

        public double CompletionRate => TotalAppointments > 0
            ? (double)CompletedAppointments / TotalAppointments * 100
            : 0;

        public double CancellationRate => TotalAppointments > 0
            ? (double)CancelledAppointments / TotalAppointments * 100
            : 0;

        public double NoShowRate => TotalAppointments > 0
            ? (double)NoShowAppointments / TotalAppointments * 100
            : 0;
    }
}