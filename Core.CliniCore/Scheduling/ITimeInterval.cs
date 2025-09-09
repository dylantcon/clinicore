using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Scheduling
{
    /// <summary>
    /// Interface defining contract for objects representing distinct intervals in time
    /// </summary>
    public interface ITimeInterval
    {
        /// <summary>
        /// Start time of the interval
        /// </summary>
        DateTime Start { get; }

        /// <summary>
        /// End time of the interval
        /// </summary>
        DateTime End { get; }

        /// <summary>
        /// Duration of the interval
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        /// Unique identifier for this time interval
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Description or title of this time interval
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Checks if this interval overlaps with another
        /// </summary>
        /// <param name="other">The other interval to check</param>
        /// <returns>True if intervals overlap, false otherwise</returns>
        bool Overlaps(ITimeInterval other);

        /// <summary>
        /// Checks if this interval contains a specific point in time
        /// </summary>
        /// <param name="moment">The point in time to check</param>
        /// <returns>True if the moment is within this interval</returns>
        bool Contains(DateTime moment);

        /// <summary>
        /// Checks if this interval completely contains another interval
        /// </summary>
        /// <param name="other">The other interval to check</param>
        /// <returns>True if this interval completely contains the other</returns>
        bool Contains(ITimeInterval other);

        /// <summary>
        /// Checks if this interval is adjacent to another (touching but not overlapping)
        /// </summary>
        /// <param name="other">The other interval to check</param>
        /// <returns>True if intervals are adjacent</returns>
        bool IsAdjacentTo(ITimeInterval other);

        /// <summary>
        /// Validates that the interval is valid (end after start, reasonable duration, etc.)
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        bool IsValid();

        /// <summary>
        /// Gets validation errors if the interval is not valid
        /// </summary>
        /// <returns>List of validation error messages</returns>
        List<string> GetValidationErrors();

        /// <summary>
        /// Checks if this interval occurs within business hours
        /// </summary>
        /// <returns>True if within business hours (M-F 8am-5pm)</returns>
        bool IsWithinBusinessHours();

        /// <summary>
        /// Attempts to merge with another interval if they overlap or are adjacent
        /// </summary>
        /// <param name="other">The other interval to merge with</param>
        /// <returns>A new merged interval if possible, null otherwise</returns>
        ITimeInterval? MergeWith(ITimeInterval other);
    }
}
