using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Scheduling.BookingStrategies
{
    /// <summary>
    /// Booking strategy that finds the first available time slot
    /// </summary>
    public class FirstAvailableBookingStrategy : IBookingStrategy
    {
        private static readonly TimeSpan SlotIncrement = TimeSpan.FromMinutes(15);
        private static readonly int MaxDaysToSearch = 30;

        public string StrategyName => "First Available";

        public string Description => "Finds the earliest available appointment slot that meets the duration requirements";

        /// <summary>
        /// Finds the next available appointment slot
        /// </summary>
        public AppointmentSlot? FindNextAvailableSlot(
            PhysicianSchedule physicianSchedule,
            TimeSpan requestedDuration,
            DateTime earliestTime,
            List<UnavailableTimeInterval>? facilityUnavailable = null)
        {
            if (physicianSchedule == null)
                throw new ArgumentNullException(nameof(physicianSchedule));

            // Round up to next 15-minute increment
            var searchTime = RoundToNextSlot(earliestTime);
            var latestTime = earliestTime.AddDays(MaxDaysToSearch);

            while (searchTime < latestTime)
            {
                // Skip weekends
                if (IsWeekend(searchTime))
                {
                    searchTime = GetNextWeekday(searchTime);
                    continue;
                }

                // Check if physician works this day
                if (!physicianSchedule.StandardAvailability.ContainsKey(searchTime.DayOfWeek))
                {
                    searchTime = searchTime.Date.AddDays(1).AddHours(8);
                    continue;
                }

                var dayAvailability = physicianSchedule.StandardAvailability[searchTime.DayOfWeek];

                // Ensure we're within working hours
                if (searchTime.TimeOfDay < dayAvailability.Start)
                {
                    searchTime = searchTime.Date.Add(dayAvailability.Start);
                }

                var slotEnd = searchTime.Add(requestedDuration);

                // Check if slot would extend past working hours
                if (slotEnd.TimeOfDay > dayAvailability.End || slotEnd.Date != searchTime.Date)
                {
                    searchTime = searchTime.Date.AddDays(1).AddHours(8);
                    continue;
                }

                // Check if slot is available
                if (IsSlotAvailable(searchTime, slotEnd, physicianSchedule, facilityUnavailable))
                {
                    return new AppointmentSlot
                    {
                        Start = searchTime,
                        End = slotEnd,
                        PhysicianId = physicianSchedule.PhysicianId,
                        IsOptimal = IsOptimalTime(searchTime)
                    };
                }

                // Try next slot
                searchTime = searchTime.Add(SlotIncrement);
            }

            return null; // No available slot found
        }

        /// <summary>
        /// Finds multiple available slots
        /// </summary>
        public List<AppointmentSlot> FindAvailableSlots(
            PhysicianSchedule physicianSchedule,
            TimeSpan requestedDuration,
            DateTime earliestTime,
            int maxResults = 5,
            List<UnavailableTimeInterval>? facilityUnavailable = null)
        {
            var slots = new List<AppointmentSlot>();
            var searchTime = RoundToNextSlot(earliestTime);
            var latestTime = earliestTime.AddDays(MaxDaysToSearch);

            while (searchTime < latestTime && slots.Count < maxResults)
            {
                // Skip weekends
                if (IsWeekend(searchTime))
                {
                    searchTime = GetNextWeekday(searchTime);
                    continue;
                }

                // Check if physician works this day
                if (!physicianSchedule.StandardAvailability.ContainsKey(searchTime.DayOfWeek))
                {
                    searchTime = searchTime.Date.AddDays(1).AddHours(8);
                    continue;
                }

                var dayAvailability = physicianSchedule.StandardAvailability[searchTime.DayOfWeek];

                // Ensure we're within working hours
                if (searchTime.TimeOfDay < dayAvailability.Start)
                {
                    searchTime = searchTime.Date.Add(dayAvailability.Start);
                }

                var slotEnd = searchTime.Add(requestedDuration);

                // Check if slot would extend past working hours
                if (slotEnd.TimeOfDay > dayAvailability.End || slotEnd.Date != searchTime.Date)
                {
                    searchTime = searchTime.Date.AddDays(1).AddHours(8);
                    continue;
                }

                // Check if slot is available
                if (IsSlotAvailable(searchTime, slotEnd, physicianSchedule, facilityUnavailable))
                {
                    slots.Add(new AppointmentSlot
                    {
                        Start = searchTime,
                        End = slotEnd,
                        PhysicianId = physicianSchedule.PhysicianId,
                        IsOptimal = IsOptimalTime(searchTime)
                    });

                    // Jump ahead to avoid overlapping suggestions
                    searchTime = slotEnd;
                }
                else
                {
                    searchTime = searchTime.Add(SlotIncrement);
                }
            }

            return slots;
        }

        #region Helper Methods

        /// <summary>
        /// Checks if a slot is available
        /// </summary>
        private bool IsSlotAvailable(
            DateTime start,
            DateTime end,
            PhysicianSchedule physicianSchedule,
            List<UnavailableTimeInterval>? facilityUnavailable)
        {
            // Create temporary appointment to check
            var tempAppointment = new AppointmentTimeInterval(
                start,
                end,
                Guid.Empty,
                physicianSchedule.PhysicianId,
                "Availability Check");

            // Check against physician's appointments
            foreach (var appointment in physicianSchedule.Appointments)
            {
                if (appointment.Status == AppointmentStatus.Scheduled &&
                    appointment.Overlaps(tempAppointment))
                {
                    return false;
                }
            }

            // Check against physician's unavailable blocks
            foreach (var block in physicianSchedule.UnavailableBlocks)
            {
                if (block.Overlaps(tempAppointment))
                {
                    return false;
                }
            }

            // Check against facility-wide unavailable blocks
            if (facilityUnavailable != null)
            {
                foreach (var block in facilityUnavailable)
                {
                    if (!block.PhysicianId.HasValue || block.PhysicianId == physicianSchedule.PhysicianId)
                    {
                        if (block.Overlaps(tempAppointment))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Rounds a time up to the next 15-minute slot
        /// </summary>
        private DateTime RoundToNextSlot(DateTime time)
        {
            var minutes = time.Minute;
            var roundedMinutes = ((minutes / 15) + (minutes % 15 == 0 ? 0 : 1)) * 15;

            if (roundedMinutes == 60)
            {
                return time.Date.AddHours(time.Hour + 1);
            }

            return time.Date.AddHours(time.Hour).AddMinutes(roundedMinutes);
        }

        /// <summary>
        /// Checks if a date is a weekend
        /// </summary>
        private bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        /// <summary>
        /// Gets the next weekday (Monday) after a weekend
        /// </summary>
        private DateTime GetNextWeekday(DateTime date)
        {
            while (IsWeekend(date))
            {
                date = date.AddDays(1);
            }
            return date.Date.AddHours(8); // Start at 8 AM
        }

        /// <summary>
        /// Determines if a time is considered optimal (morning hours)
        /// </summary>
        private bool IsOptimalTime(DateTime time)
        {
            return time.Hour >= 9 && time.Hour < 12; // 9 AM - 12 PM
        }

        #endregion
    }
}