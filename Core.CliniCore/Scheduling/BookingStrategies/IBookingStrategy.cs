using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.CliniCore.Services;

namespace Core.CliniCore.Scheduling.BookingStrategies
{
    /// <summary>
    /// Interface for appointment booking strategies using Strategy pattern
    /// </summary>
    public interface IBookingStrategy
    {
        /// <summary>
        /// Finds the next available appointment slot based on the strategy
        /// </summary>
        /// <param name="physicianSchedule">The physician's schedule</param>
        /// <param name="requestedDuration">Duration needed for the appointment</param>
        /// <param name="earliestTime">Earliest time to consider</param>
        /// <param name="facilityUnavailable">Facility-wide unavailable blocks</param>
        /// <returns>Next available slot, or null if none found</returns>
        AppointmentSlot? FindNextAvailableSlot(
            PhysicianSchedule physicianSchedule,
            TimeSpan requestedDuration,
            DateTime earliestTime,
            List<UnavailableTimeInterval>? facilityUnavailable = null);

        /// <summary>
        /// Finds multiple available slots based on the strategy
        /// </summary>
        /// <param name="physicianSchedule">The physician's schedule</param>
        /// <param name="requestedDuration">Duration needed for the appointment</param>
        /// <param name="earliestTime">Earliest time to consider</param>
        /// <param name="maxResults">Maximum number of results to return</param>
        /// <param name="facilityUnavailable">Facility-wide unavailable blocks</param>
        /// <returns>List of available slots</returns>
        List<AppointmentSlot> FindAvailableSlots(
            PhysicianSchedule physicianSchedule,
            TimeSpan requestedDuration,
            DateTime earliestTime,
            int maxResults = 5,
            List<UnavailableTimeInterval>? facilityUnavailable = null);

        /// <summary>
        /// Gets the name of this booking strategy
        /// </summary>
        string StrategyName { get; }

        /// <summary>
        /// Gets a description of how this strategy works
        /// </summary>
        string Description { get; }
    }
}
