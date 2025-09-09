using Core.CliniCore.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Scheduling.Management
{
    /// <summary>
    /// Resolves scheduling conflicts using Chain of Responsibility pattern
    /// </summary>
    public class ScheduleConflictResolver
    {
        private readonly List<IConflictResolutionStrategy> _strategies;

        public ScheduleConflictResolver()
        {
            _strategies = new List<IConflictResolutionStrategy>
            {
                new DoubleBookingResolver(),
                new UnavailableTimeResolver(),
                new BusinessHoursResolver(),
                new MinimumDurationResolver(),
                new OverlapResolver()
            };
        }

        /// <summary>
        /// Detects all conflicts for a proposed appointment
        /// </summary>
        public ConflictCheckResult CheckForConflicts(
            AppointmentTimeInterval proposedAppointment,
            PhysicianSchedule physicianSchedule,
            List<UnavailableTimeInterval>? facilityUnavailable = null)
        {
            var result = new ConflictCheckResult
            {
                ProposedAppointment = proposedAppointment,
                HasConflicts = false
            };

            // Run through each conflict detection strategy
            foreach (var strategy in _strategies)
            {
                var conflicts = strategy.DetectConflicts(proposedAppointment, physicianSchedule, facilityUnavailable);
                if (conflicts.Any())
                {
                    result.Conflicts.AddRange(conflicts);
                    result.HasConflicts = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Attempts to resolve conflicts by suggesting alternative times
        /// </summary>
        public ConflictResolutionResult ResolveConflicts(
            ConflictCheckResult conflictResult,
            PhysicianSchedule physicianSchedule)
        {
            var resolution = new ConflictResolutionResult
            {
                OriginalAppointment = conflictResult.ProposedAppointment,
                Conflicts = conflictResult.Conflicts
            };

            if (!conflictResult.HasConflicts)
            {
                resolution.Resolved = true;
                return resolution;
            }

            // Try each strategy to resolve conflicts
            foreach (var strategy in _strategies)
            {
                var suggestions = strategy.SuggestAlternatives(
                    conflictResult.ProposedAppointment,
                    conflictResult.Conflicts,
                    physicianSchedule);

                if (suggestions.Any())
                {
                    resolution.AlternativeSlots.AddRange(suggestions);
                }
            }

            // Find the best alternative (earliest available)
            if (resolution.AlternativeSlots.Any())
            {
                resolution.RecommendedSlot = resolution.AlternativeSlots
                    .OrderBy(s => s.Start)
                    .First();
                resolution.Resolved = true;
            }

            return resolution;
        }

        /// <summary>
        /// Adds a custom conflict resolution strategy
        /// </summary>
        public void AddStrategy(IConflictResolutionStrategy strategy)
        {
            _strategies.Add(strategy);
        }
    }

    /// <summary>
    /// Interface for conflict resolution strategies
    /// </summary>
    public interface IConflictResolutionStrategy
    {
        /// <summary>
        /// Detects conflicts based on this strategy's rules
        /// </summary>
        List<ScheduleConflict> DetectConflicts(
            AppointmentTimeInterval proposedAppointment,
            PhysicianSchedule physicianSchedule,
            List<UnavailableTimeInterval>? facilityUnavailable);

        /// <summary>
        /// Suggests alternative time slots to resolve conflicts
        /// </summary>
        List<TimeSlotSuggestion> SuggestAlternatives(
            AppointmentTimeInterval proposedAppointment,
            List<ScheduleConflict> conflicts,
            PhysicianSchedule physicianSchedule);
    }

    /// <summary>
    /// Result of conflict checking
    /// </summary>
    public class ConflictCheckResult
    {
        public AppointmentTimeInterval ProposedAppointment { get; set; } = null!;
        public bool HasConflicts { get; set; }
        public List<ScheduleConflict> Conflicts { get; set; } = new List<ScheduleConflict>();

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
    public class ConflictResolutionResult
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
        public double Score { get; set; } // Higher is better
    }

    #region Concrete Resolution Strategies

    /// <summary>
    /// Resolves double-booking conflicts
    /// </summary>
    public class DoubleBookingResolver : IConflictResolutionStrategy
    {
        public List<ScheduleConflict> DetectConflicts(
            AppointmentTimeInterval proposedAppointment,
            PhysicianSchedule physicianSchedule,
            List<UnavailableTimeInterval>? facilityUnavailable)
        {
            var conflicts = new List<ScheduleConflict>();

            foreach (var existing in physicianSchedule.Appointments)
            {
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

        public List<TimeSlotSuggestion> SuggestAlternatives(
            AppointmentTimeInterval proposedAppointment,
            List<ScheduleConflict> conflicts,
            PhysicianSchedule physicianSchedule)
        {
            var suggestions = new List<TimeSlotSuggestion>();
            var duration = proposedAppointment.Duration;

            // Find next available slot
            var nextSlot = physicianSchedule.FindNextAvailableSlot(duration, proposedAppointment.Start);
            if (nextSlot.HasValue)
            {
                suggestions.Add(new TimeSlotSuggestion
                {
                    Start = nextSlot.Value,
                    End = nextSlot.Value.Add(duration),
                    Reason = "Next available slot",
                    Score = 100
                });
            }

            // Try same time next day
            var nextDay = proposedAppointment.Start.AddDays(1);
            if (physicianSchedule.IsTimeSlotAvailable(nextDay, nextDay.Add(duration)))
            {
                suggestions.Add(new TimeSlotSuggestion
                {
                    Start = nextDay,
                    End = nextDay.Add(duration),
                    Reason = "Same time next day",
                    Score = 90
                });
            }

            return suggestions;
        }
    }

    /// <summary>
    /// Resolves unavailable time conflicts
    /// </summary>
    public class UnavailableTimeResolver : IConflictResolutionStrategy
    {
        public List<ScheduleConflict> DetectConflicts(
            AppointmentTimeInterval proposedAppointment,
            PhysicianSchedule physicianSchedule,
            List<UnavailableTimeInterval>? facilityUnavailable)
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

        public List<TimeSlotSuggestion> SuggestAlternatives(
            AppointmentTimeInterval proposedAppointment,
            List<ScheduleConflict> conflicts,
            PhysicianSchedule physicianSchedule)
        {
            // Suggest times around the unavailable period
            return new List<TimeSlotSuggestion>();
        }
    }

    /// <summary>
    /// Ensures appointments are within business hours
    /// </summary>
    public class BusinessHoursResolver : IConflictResolutionStrategy
    {
        public List<ScheduleConflict> DetectConflicts(
            AppointmentTimeInterval proposedAppointment,
            PhysicianSchedule physicianSchedule,
            List<UnavailableTimeInterval>? facilityUnavailable)
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

        public List<TimeSlotSuggestion> SuggestAlternatives(
            AppointmentTimeInterval proposedAppointment,
            List<ScheduleConflict> conflicts,
            PhysicianSchedule physicianSchedule)
        {
            var suggestions = new List<TimeSlotSuggestion>();

            // If on weekend, suggest Monday
            if (proposedAppointment.Start.DayOfWeek == DayOfWeek.Saturday ||
                proposedAppointment.Start.DayOfWeek == DayOfWeek.Sunday)
            {
                var monday = proposedAppointment.Start;
                while (monday.DayOfWeek != DayOfWeek.Monday)
                    monday = monday.AddDays(1);

                monday = monday.Date.AddHours(9); // 9 AM Monday
                suggestions.Add(new TimeSlotSuggestion
                {
                    Start = monday,
                    End = monday.Add(proposedAppointment.Duration),
                    Reason = "First available Monday morning",
                    Score = 80
                });
            }

            return suggestions;
        }
    }

    /// <summary>
    /// Validates appointment duration
    /// </summary>
    public class MinimumDurationResolver : IConflictResolutionStrategy
    {
        private static readonly TimeSpan MinDuration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan MaxDuration = TimeSpan.FromHours(3);

        public List<ScheduleConflict> DetectConflicts(
            AppointmentTimeInterval proposedAppointment,
            PhysicianSchedule physicianSchedule,
            List<UnavailableTimeInterval>? facilityUnavailable)
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

        public List<TimeSlotSuggestion> SuggestAlternatives(
            AppointmentTimeInterval proposedAppointment,
            List<ScheduleConflict> conflicts,
            PhysicianSchedule physicianSchedule)
        {
            // No alternatives for duration issues - must be fixed by user
            return new List<TimeSlotSuggestion>();
        }
    }

    /// <summary>
    /// General overlap detection
    /// </summary>
    public class OverlapResolver : IConflictResolutionStrategy
    {
        public List<ScheduleConflict> DetectConflicts(
            AppointmentTimeInterval proposedAppointment,
            PhysicianSchedule physicianSchedule,
            List<UnavailableTimeInterval>? facilityUnavailable)
        {
            // This is handled by other resolvers, so we return empty
            return new List<ScheduleConflict>();
        }

        public List<TimeSlotSuggestion> SuggestAlternatives(
            AppointmentTimeInterval proposedAppointment,
            List<ScheduleConflict> conflicts,
            PhysicianSchedule physicianSchedule)
        {
            return new List<TimeSlotSuggestion>();
        }
    }

    #endregion
}