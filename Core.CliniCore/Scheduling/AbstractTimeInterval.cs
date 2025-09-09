using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Scheduling
{
    /// <summary>
    /// Base implementation for all time interval types
    /// </summary>
    public abstract class AbstractTimeInterval : ITimeInterval
    {
        // Business hours constraints for Assignment 1
        // TODO: rework later to characterize open and close at facility level
        private static readonly TimeSpan BusinessDayStart = new TimeSpan(8, 0, 0);  // 8:00 AM
        private static readonly TimeSpan BusinessDayEnd = new TimeSpan(17, 0, 0);   // 5:00 PM
        private static readonly TimeSpan MinimumDuration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan MaximumDuration = TimeSpan.FromHours(8);

        protected AbstractTimeInterval(DateTime start, DateTime end, string description = "")
        {
            if (end <= start)
            {
                throw new ArgumentException("End time must be after start time");
            }

            Id = Guid.NewGuid();
            Start = start;
            End = end;
            Description = description ?? string.Empty;
        }

        /// <summary>
        /// Unique identifier for this time interval
        /// </summary>
        public Guid Id { get; protected set; }

        /// <summary>
        /// Start time of the interval
        /// </summary>
        public DateTime Start { get; protected set; }

        /// <summary>
        /// End time of the interval
        /// </summary>
        public DateTime End { get; protected set; }

        /// <summary>
        /// Duration of the interval
        /// </summary>
        public TimeSpan Duration => End - Start;

        /// <summary>
        /// Description or title of this time interval
        /// </summary>
        public virtual string Description { get; protected set; }

        /// <summary>
        /// Checks if this interval overlaps with another
        /// </summary>
        public virtual bool Overlaps(ITimeInterval other)
        {
            if (other == null) return false;

            // Intervals overlap if:
            // - This starts before other ends AND
            // - Other starts before this ends
            return Start < other.End && other.Start < End;
        }

        /// <summary>
        /// Checks if this interval contains a specific point in time
        /// </summary>
        public virtual bool Contains(DateTime moment)
        {
            return moment >= Start && moment <= End;
        }

        /// <summary>
        /// Checks if this interval completely contains another interval
        /// </summary>
        public virtual bool Contains(ITimeInterval other)
        {
            if (other == null) return false;
            return Start <= other.Start && End >= other.End;
        }

        /// <summary>
        /// Checks if this interval is adjacent to another (touching but not overlapping)
        /// </summary>
        public virtual bool IsAdjacentTo(ITimeInterval other)
        {
            if (other == null) return false;

            // Adjacent means one ends exactly when the other starts
            return End == other.Start || other.End == Start;
        }

        /// <summary>
        /// Validates that the interval is valid
        /// </summary>
        public virtual bool IsValid()
        {
            return GetValidationErrors().Count == 0;
        }

        /// <summary>
        /// Gets validation errors if the interval is not valid
        /// </summary>
        public virtual List<string> GetValidationErrors()
        {
            var errors = new List<string>();

            // Basic validation
            if (End <= Start)
            {
                errors.Add("End time must be after start time");
            }

            // Duration validation
            if (Duration < MinimumDuration)
            {
                errors.Add($"Duration must be at least {MinimumDuration.TotalMinutes} minutes");
            }

            if (Duration > MaximumDuration)
            {
                errors.Add($"Duration cannot exceed {MaximumDuration.TotalHours} hours");
            }

            // Don't allow intervals spanning multiple days for appointments
            if (Start.Date != End.Date)
            {
                errors.Add("Time interval cannot span multiple days");
            }

            // Add any type-specific validation
            var specificErrors = GetSpecificValidationErrors();
            errors.AddRange(specificErrors);

            return errors;
        }

        /// <summary>
        /// Override in derived classes to add specific validation rules
        /// </summary>
        protected virtual List<string> GetSpecificValidationErrors()
        {
            return new List<string>();
        }

        /// <summary>
        /// Checks if this interval occurs within business hours (M-F 8am-5pm)
        /// </summary>
        public virtual bool IsWithinBusinessHours()
        {
            // Check if it's a weekday
            if (Start.DayOfWeek == DayOfWeek.Saturday || Start.DayOfWeek == DayOfWeek.Sunday)
                return false;

            if (End.DayOfWeek == DayOfWeek.Saturday || End.DayOfWeek == DayOfWeek.Sunday)
                return false;

            // Check if times are within business hours
            var startTime = Start.TimeOfDay;
            var endTime = End.TimeOfDay;

            return startTime >= BusinessDayStart &&
                   endTime <= BusinessDayEnd &&
                   Start.Date == End.Date; // Must be same day
        }

        /// <summary>
        /// Attempts to merge with another interval if they overlap or are adjacent
        /// </summary>
        public virtual ITimeInterval? MergeWith(ITimeInterval other)
        {
            if (other == null) return null;

            // Can only merge if intervals overlap or are adjacent
            if (!Overlaps(other) && !IsAdjacentTo(other))
                return null;

            // Can only merge intervals of the same type
            if (GetType() != other.GetType())
                return null;

            var newStart = Start < other.Start ? Start : other.Start;
            var newEnd = End > other.End ? End : other.End;
            var mergedDescription = $"{Description} + {other.Description}";

            return CreateMergedInterval(newStart, newEnd, mergedDescription, other);
        }

        /// <summary>
        /// Override in derived classes to create a properly typed merged interval
        /// </summary>
        protected abstract ITimeInterval? CreateMergedInterval(
            DateTime start, DateTime end, string description, ITimeInterval other);

        /// <summary>
        /// Gets the day of week for this interval
        /// </summary>
        public DayOfWeek DayOfWeek => Start.DayOfWeek;

        /// <summary>
        /// Static helper to check if a given time slot is available
        /// </summary>
        public static bool IsBusinessHoursSlot(DateTime start, DateTime end)
        {
            // Check weekday
            if (start.DayOfWeek == DayOfWeek.Saturday || start.DayOfWeek == DayOfWeek.Sunday)
                return false;

            // Check time range
            return start.TimeOfDay >= BusinessDayStart &&
                   end.TimeOfDay <= BusinessDayEnd &&
                   start.Date == end.Date;
        }

        /// <summary>
        /// Gets a display string for this interval
        /// </summary>
        public override string ToString()
        {
            return $"{GetType().Name} [{Id:N}]: {Start:yyyy-MM-dd HH:mm} - {End:HH:mm} ({Duration.TotalMinutes} min)";
        }

        /// <summary>
        /// Equality comparison
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is ITimeInterval other)
            {
                return Id == other.Id;
            }
            return false;
        }

        /// <summary>
        /// Hash code for dictionary storage
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
