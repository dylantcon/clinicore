using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Scheduling.BookingStrategies;

namespace Core.CliniCore.Scheduling.Management
{
    /// <summary>
    /// Resolves scheduling conflicts using Chain of Responsibility pattern.
    /// Uses IBookingStrategy to find alternative time slots when conflicts are detected.
    /// </summary>
    public class ScheduleConflictDetector
    {
        private readonly List<IConflictDetectionStrategy> _strategies;
        private readonly IBookingStrategy _bookingStrategy;

        public ScheduleConflictDetector() : this(new FirstAvailableBookingStrategy())
        {
        }

        public ScheduleConflictDetector(IBookingStrategy bookingStrategy)
        {
            _bookingStrategy = bookingStrategy ?? throw new ArgumentNullException(nameof(bookingStrategy));
            _strategies =
            [
                new DoubleBookingDetector(),
                new UnavailableTimeDetector(),
                new InvalidDurationDetector()
            ];
        }

        /// <summary>
        /// Detects all conflicts for a proposed appointment
        /// </summary>
        /// <param name="proposedAppointment"></param>
        /// <param name="physicianSchedule"></param>
        /// <param name="facilityUnavailable"></param>
        /// <param name="excludeAppointmentId">Optional appointment ID to exclude from conflict checking (for updates)</param>
        public ConflictCheckResult CheckForConflicts(
            AppointmentTimeInterval proposedAppointment,
            PhysicianSchedule physicianSchedule,
            List<UnavailableTimeInterval>? facilityUnavailable = null,
            Guid? excludeAppointmentId = null)
        {
            var result = new ConflictCheckResult
            {
                ProposedAppointment = proposedAppointment,
                HasConflicts = false
            };

            // Run through each conflict detection strategy
            foreach (var strategy in _strategies)
            {
                var conflicts = strategy.DetectConflicts(proposedAppointment, physicianSchedule, facilityUnavailable, excludeAppointmentId);
                if (conflicts.Any())
                {
                    result.Conflicts.AddRange(conflicts);
                    result.HasConflicts = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Attempts to resolve conflicts by suggesting alternative times.
        /// Uses the injected IBookingStrategy to find and evaluate alternatives.
        /// </summary>
        public ConflictDetectionResult FindAlternative(
            ConflictCheckResult conflictResult,
            PhysicianSchedule physicianSchedule,
            List<UnavailableTimeInterval>? facilityUnavailable = null)
        {
            var resolution = new ConflictDetectionResult
            {
                OriginalAppointment = conflictResult.ProposedAppointment,
                Conflicts = conflictResult.Conflicts
            };

            if (!conflictResult.HasConflicts)
            {
                resolution.Resolved = true;
                return resolution;
            }

            // Find the first available alternative slot
            var proposedAppointment = conflictResult.ProposedAppointment;
            var nextSlot = _bookingStrategy.FindNextAvailableSlot(
                physicianSchedule,
                proposedAppointment.Duration,
                proposedAppointment.Start,
                facilityUnavailable);

            if (nextSlot != null)
            {
                var suggestion = new TimeSlotSuggestion
                {
                    Start = nextSlot.Start,
                    End = nextSlot.End,
                    Reason = "First available"
                };
                resolution.AlternativeSlots.Add(suggestion);
                resolution.RecommendedSlot = suggestion;
                resolution.Resolved = true;
            }

            return resolution;
        }

        /// <summary>
        /// Adds a custom conflict resolution strategy
        /// </summary>
        public void AddStrategy(IConflictDetectionStrategy strategy)
        {
            _strategies.Add(strategy);
        }
    }

    /// <summary>
    /// Interface for conflict detection strategies.
    /// Alternative suggestions are handled centrally by ScheduleConflictDetector using IBookingStrategy.
    /// </summary>
    public interface IConflictDetectionStrategy
    {
        /// <summary>
        /// Detects conflicts based on this strategy's rules
        /// </summary>
        /// <param name="proposedAppointment"></param>
        /// <param name="physicianSchedule"></param>
        /// <param name="facilityUnavailable"></param>
        /// <param name="excludeAppointmentId">Optional appointment ID to exclude from conflict checking (for updates)</param>
        List<ScheduleConflict> DetectConflicts(
            AppointmentTimeInterval proposedAppointment,
            PhysicianSchedule physicianSchedule,
            List<UnavailableTimeInterval>? facilityUnavailable,
            Guid? excludeAppointmentId = null);
    }

    /// <summary>
    /// Result of conflict checking
    /// </summary>
    public class ConflictCheckResult
    {
        public AppointmentTimeInterval ProposedAppointment { get; set; } = null!;
        public bool HasConflicts { get; set; }
        public List<ScheduleConflict> Conflicts { get; set; } = new List<ScheduleConflict>();
        public List<TimeSlotSuggestion> AlternativeSuggestions { get; set; } = new List<TimeSlotSuggestion>();

        public string GetSummary()
        {
            if (!HasConflicts)
                return "No conflicts detected.";

            var summary = $"Found {Conflicts.Count} conflict(s):\n";
            foreach (var conflict in Conflicts)
            {
                summary += $"- {conflict.Type}: {conflict.Description}\n";
            }
            return summary;
        }

        /// <summary>
        /// Returns formatted validation error strings for use in command validation.
        /// Includes conflict descriptions and alternative slot suggestions.
        /// </summary>
        public IEnumerable<string> GetValidationErrors()
        {
            foreach (var conflict in Conflicts)
            {
                yield return conflict.Description;
            }

            // Add suggestion as a separate validation message (info-level)
            if (AlternativeSuggestions.Any())
            {
                var suggestion = AlternativeSuggestions.First();
                yield return $"Suggested alternative: {suggestion.Start:MMM dd, yyyy} at {suggestion.Start:h:mm tt} ({suggestion.Reason})";
            }
        }
    }

    /// <summary>
    /// Represents a scheduling conflict
    /// </summary>
    public class ScheduleConflict
    {
        public ConflictType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public ITimeInterval? ConflictingInterval { get; set; }
        public bool CanOverride { get; set; }
    }

    /// <summary>
    /// Types of scheduling conflicts
    /// </summary>
    public enum ConflictType
    {
        DoubleBooking,
        UnavailableTime,
        OutsideBusinessHours,
        TooShort,
        TooLong,
        Overlap,
        Holiday,
        Other
    }

    /// <summary>
    /// Result of conflict resolution attempt
    /// </summary>
    public class ConflictDetectionResult
    {
        public AppointmentTimeInterval OriginalAppointment { get; set; } = null!;
        public List<ScheduleConflict> Conflicts { get; set; } = new List<ScheduleConflict>();
        public bool Resolved { get; set; }
        public List<TimeSlotSuggestion> AlternativeSlots { get; set; } = new List<TimeSlotSuggestion>();
        public TimeSlotSuggestion? RecommendedSlot { get; set; }
    }

    /// <summary>
    /// Suggested alternative time slot
    /// </summary>
    public class TimeSlotSuggestion
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    #region Concrete Resolution Strategies

    /// <summary>
    /// Resolves double-booking conflicts
    /// </summary>
    public class DoubleBookingDetector : IConflictDetectionStrategy
    {
        public List<ScheduleConflict> DetectConflicts(
            AppointmentTimeInterval proposedAppointment,
            PhysicianSchedule physicianSchedule,
            List<UnavailableTimeInterval>? facilityUnavailable,
            Guid? excludeAppointmentId = null)
        {
            var conflicts = new List<ScheduleConflict>();

            foreach (var existing in physicianSchedule.Appointments)
            {
                // Skip the appointment being updated (whitelist it)
                if (excludeAppointmentId.HasValue && existing.Id == excludeAppointmentId.Value)
                    continue;

                if (existing.Status == AppointmentStatus.Scheduled &&
                    existing.Overlaps(proposedAppointment))
                {
                    conflicts.Add(new ScheduleConflict
                    {
                        Type = ConflictType.DoubleBooking,
                        Description = $"Conflicts with existing appointment from {existing.Start:HH:mm} to {existing.End:HH:mm}",
                        ConflictingInterval = existing,
                        CanOverride = false
                    });
                }
            }

            return conflicts;
        }
    }

    /// <summary>
    /// Detects unavailable time conflicts
    /// </summary>
    public class UnavailableTimeDetector : IConflictDetectionStrategy
    {
        public List<ScheduleConflict> DetectConflicts(
            AppointmentTimeInterval proposedAppointment,
            PhysicianSchedule physicianSchedule,
            List<UnavailableTimeInterval>? facilityUnavailable,
            Guid? excludeAppointmentId = null)
        {
            var conflicts = new List<ScheduleConflict>();

            // Check physician unavailable times
            foreach (var block in physicianSchedule.UnavailableBlocks)
            {
                if (block.Overlaps(proposedAppointment))
                {
                    conflicts.Add(new ScheduleConflict
                    {
                        Type = ConflictType.UnavailableTime,
                        Description = $"Physician unavailable: {block.Reason}",
                        ConflictingInterval = block,
                        CanOverride = false
                    });
                }
            }

            // Check facility-wide unavailable times
            if (facilityUnavailable != null)
            {
                foreach (var block in facilityUnavailable.Where(b => !b.PhysicianId.HasValue))
                {
                    if (block.Overlaps(proposedAppointment))
                    {
                        conflicts.Add(new ScheduleConflict
                        {
                            Type = ConflictType.UnavailableTime,
                            Description = $"Facility unavailable: {block.Reason}",
                            ConflictingInterval = block,
                            CanOverride = false
                        });
                    }
                }
            }

            return conflicts;
        }
    }

    /// <summary>
    /// Detects appointments outside business hours
    /// </summary>
    public class OutsideHoursDetector : IConflictDetectionStrategy
    {
        public List<ScheduleConflict> DetectConflicts(
            AppointmentTimeInterval proposedAppointment,
            PhysicianSchedule physicianSchedule,
            List<UnavailableTimeInterval>? facilityUnavailable,
            Guid? excludeAppointmentId = null)
        {
            var conflicts = new List<ScheduleConflict>();

            if (!proposedAppointment.IsWithinBusinessHours())
            {
                conflicts.Add(new ScheduleConflict
                {
                    Type = ConflictType.OutsideBusinessHours,
                    Description = "Appointment must be scheduled during business hours (M-F 8:00 AM - 5:00 PM)",
                    CanOverride = false
                });
            }

            return conflicts;
        }
    }

    /// <summary>
    /// Detects invalid appointment durations
    /// </summary>
    public class InvalidDurationDetector : IConflictDetectionStrategy
    {
        private static readonly TimeSpan MinDuration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan MaxDuration = TimeSpan.FromHours(3);

        public List<ScheduleConflict> DetectConflicts(
            AppointmentTimeInterval proposedAppointment,
            PhysicianSchedule physicianSchedule,
            List<UnavailableTimeInterval>? facilityUnavailable,
            Guid? excludeAppointmentId = null)
        {
            var conflicts = new List<ScheduleConflict>();

            if (proposedAppointment.Duration < MinDuration)
            {
                conflicts.Add(new ScheduleConflict
                {
                    Type = ConflictType.TooShort,
                    Description = $"Appointment must be at least {MinDuration.TotalMinutes} minutes",
                    CanOverride = false
                });
            }

            if (proposedAppointment.Duration > MaxDuration)
            {
                conflicts.Add(new ScheduleConflict
                {
                    Type = ConflictType.TooLong,
                    Description = $"Appointment cannot exceed {MaxDuration.TotalHours} hours",
                    CanOverride = true
                });
            }

            return conflicts;
        }
    }

    #endregion
}