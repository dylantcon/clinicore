using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Scheduling
{
    /// <summary>
    /// Represents a time interval when no appointments can be scheduled
    /// </summary>
    public class UnavailableTimeInterval : AbstractTimeInterval
    {
        public enum UnavailabilityReason
        {
            /// <summary>
            /// Outside of business hours
            /// </summary>
            NonBusinessHours,

            /// <summary>
            /// Lunch break
            /// </summary>
            Lunch,

            /// <summary>
            /// Physician is in a meeting
            /// </summary>
            Meeting,

            /// <summary>
            /// Physician is on vacation
            /// </summary>
            Vacation,

            /// <summary>
            /// Sick leave
            /// </summary>
            SickLeave,

            /// <summary>
            /// Holiday - facility closed
            /// </summary>
            Holiday,

            /// <summary>
            /// Reserved for administrative tasks
            /// </summary>
            Administrative,

            /// <summary>
            /// Emergency or unplanned absence
            /// </summary>
            Emergency,

            /// <summary>
            /// Other unspecified reason
            /// </summary>
            Other
        }

        public UnavailableTimeInterval(
            DateTime start,
            DateTime end,
            UnavailabilityReason reason,
            string description = "",
            Guid? physicianId = null)
            : base(start, end, description)
        {
            Reason = reason;
            PhysicianId = physicianId;
        }

        /// <summary>
        /// The reason for unavailability
        /// </summary>
        public UnavailabilityReason Reason { get; set; }

        /// <summary>
        /// Optional: specific physician this applies to (null = facility-wide)
        /// </summary>
        public Guid? PhysicianId { get; set; }

        /// <summary>
        /// Whether this is a recurring unavailability (e.g., lunch every day)
        /// </summary>
        public bool IsRecurring { get; set; }

        /// <summary>
        /// If recurring, the pattern (daily, weekly, etc.)
        /// </summary>
        public string? RecurrencePattern { get; set; }

        /// <summary>
        /// Override to provide meaningful description
        /// </summary>
        public override string Description
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(base.Description))
                    return base.Description;

                var scope = PhysicianId.HasValue ? $"Physician {PhysicianId:N}" : "Facility-wide";
                return $"{Reason} - {scope}";
            }
            protected set => base.Description = value;
        }

        /// <summary>
        /// Checks if this unavailability blocks a specific time slot
        /// </summary>
        public bool BlocksTimeSlot(DateTime proposedStart, DateTime proposedEnd)
        {
            // Check if the proposed slot overlaps with this unavailable period
            return proposedStart < End && proposedEnd > Start;
        }

        /// <summary>
        /// Creates standard lunch break intervals
        /// </summary>
        public static UnavailableTimeInterval CreateLunchBreak(DateTime date, Guid? physicianId = null)
        {
            var lunchStart = date.Date.AddHours(12); // 12:00 PM
            var lunchEnd = date.Date.AddHours(13);   // 1:00 PM

            return new UnavailableTimeInterval(
                lunchStart,
                lunchEnd,
                UnavailabilityReason.Lunch,
                "Lunch Break",
                physicianId)
            {
                IsRecurring = true,
                RecurrencePattern = "Daily"
            };
        }

        /// <summary>
        /// Creates non-business hours blocks for a specific date
        /// </summary>
        public static List<UnavailableTimeInterval> CreateNonBusinessHours(DateTime date)
        {
            var blocks = new List<UnavailableTimeInterval>();

            // Before business hours (midnight to 8 AM)
            blocks.Add(new UnavailableTimeInterval(
                date.Date,
                date.Date.AddHours(8),
                UnavailabilityReason.NonBusinessHours,
                "Before Business Hours"
            ));

            // After business hours (5 PM to midnight)
            blocks.Add(new UnavailableTimeInterval(
                date.Date.AddHours(17),
                date.Date.AddDays(1),
                UnavailabilityReason.NonBusinessHours,
                "After Business Hours"
            ));

            return blocks;
        }

        /// <summary>
        /// Creates weekend blocks
        /// </summary>
        public static List<UnavailableTimeInterval> CreateWeekendBlocks(DateTime weekStart)
        {
            var blocks = new List<UnavailableTimeInterval>();

            // Find Saturday and Sunday
            var currentDate = weekStart.Date;
            while (currentDate < weekStart.AddDays(7))
            {
                if (currentDate.DayOfWeek == DayOfWeek.Saturday ||
                    currentDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    blocks.Add(new UnavailableTimeInterval(
                        currentDate,
                        currentDate.AddDays(1),
                        UnavailabilityReason.NonBusinessHours,
                        $"Weekend - {currentDate.DayOfWeek}"
                    ));
                }
                currentDate = currentDate.AddDays(1);
            }

            return blocks;
        }

        /// <summary>
        /// Override validation to allow unavailable blocks outside business hours
        /// </summary>
        protected override List<string> GetSpecificValidationErrors()
        {
            var errors = new List<string>();

            // Unavailable intervals can be outside business hours (that's often the point)
            // So we override to not check business hours

            // But we still validate basic constraints
            if (Duration > TimeSpan.FromDays(365))
            {
                errors.Add("Unavailability period cannot exceed one year");
            }

            // Facility-wide blocks should have a valid reason
            if (!PhysicianId.HasValue && Reason == UnavailabilityReason.Other &&
                string.IsNullOrWhiteSpace(Description))
            {
                errors.Add("Facility-wide unavailability requires a description when reason is 'Other'");
            }

            return errors;
        }

        /// <summary>
        /// Creates a merged unavailable interval
        /// </summary>
        protected override ITimeInterval? CreateMergedInterval(
            DateTime start, DateTime end, string description, ITimeInterval other)
        {
            if (other is UnavailableTimeInterval otherUnavail &&
                otherUnavail.Reason == Reason &&
                otherUnavail.PhysicianId == PhysicianId)
            {
                return new UnavailableTimeInterval(start, end, Reason, description, PhysicianId);
            }
            return null;
        }

        public override string ToString()
        {
            var scope = PhysicianId.HasValue ? $"Physician {PhysicianId:N}" : "Facility";
            return $"Unavailable [{Reason}]: {Start:yyyy-MM-dd HH:mm} - {End:HH:mm} ({scope})";
        }
    }
}
