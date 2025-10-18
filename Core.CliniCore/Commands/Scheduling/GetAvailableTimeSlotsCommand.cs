using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain;
using Core.CliniCore.Scheduling.Management;
using Core.CliniCore.Scheduling.BookingStrategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Commands.Scheduling
{
    public class GetAvailableTimeSlotsCommand : AbstractCommand
    {
        public const string Key = "getavailabletimeslots";
        public override string CommandKey => Key;
        public static class Parameters
        {
            public const string PhysicianId = "physician_id";
            public const string Date = "date";
            public const string DurationMinutes = "duration_minutes";
            public const string MaxSlots = "max_slots";
        }

        private readonly ProfileRegistry _registry = ProfileRegistry.Instance;
        private readonly ScheduleManager _scheduleManager;
        private readonly IBookingStrategy _bookingStrategy;

        public GetAvailableTimeSlotsCommand(ScheduleManager scheduleManager, IBookingStrategy? bookingStrategy = null)
        {
            _scheduleManager = scheduleManager ?? throw new ArgumentNullException(nameof(scheduleManager));
            _bookingStrategy = bookingStrategy ?? new FirstAvailableBookingStrategy();
        }

        public override string Description => "Gets available appointment time slots for a physician on a specific date";

        public override Permission? GetRequiredPermission()
            => Permission.ScheduleAnyAppointment;

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Check required parameters
            var missingParams = parameters.GetMissingRequired(
                Parameters.PhysicianId, Parameters.Date, Parameters.DurationMinutes);

            if (missingParams.Any())
            {
                foreach (var error in missingParams)
                    result.AddError(error);
                return result;
            }

            // Validate physician exists
            var physicianId = parameters.GetParameter<Guid?>(Parameters.PhysicianId);
            if (!physicianId.HasValue || physicianId.Value == Guid.Empty)
            {
                result.AddError("Invalid physician ID");
            }
            else
            {
                var physician = _registry.GetProfileById(physicianId.Value);
                if (physician == null || physician.Role != UserRole.Physician)
                {
                    result.AddError($"Physician with ID {physicianId.Value} not found");
                }
            }

            // Validate date
            var date = parameters.GetParameter<DateTime?>(Parameters.Date);
            if (!date.HasValue)
            {
                result.AddError("Invalid date");
            }
            else
            {
                // Check that the date is not in the past
                if (date.Value.Date < DateTime.Today)
                {
                    result.AddError("Cannot get available slots for past dates");
                }

                // Validate that the date is a weekday (Monday-Friday)
                if (date.Value.DayOfWeek == DayOfWeek.Saturday || date.Value.DayOfWeek == DayOfWeek.Sunday)
                {
                    result.AddError("Appointments can only be scheduled Monday through Friday");
                }
            }

            // Validate duration
            var durationMinutes = parameters.GetParameter<int?>(Parameters.DurationMinutes);
            if (!durationMinutes.HasValue || durationMinutes.Value < 15)
            {
                result.AddError("Appointment duration must be at least 15 minutes");
            }
            else if (durationMinutes.Value > 180)
            {
                result.AddError("Appointment duration cannot exceed 3 hours (180 minutes)");
            }

            // Validate max slots (optional parameter)
            var maxSlots = parameters.GetParameter<int?>(Parameters.MaxSlots);
            if (maxSlots.HasValue && (maxSlots.Value < 1 || maxSlots.Value > 20))
            {
                result.AddError("Max slots must be between 1 and 20");
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                // Extract parameters - these are validated already so we can use GetRequiredParameter
                var physicianId = parameters.GetRequiredParameter<Guid>(Parameters.PhysicianId);
                var date = parameters.GetRequiredParameter<DateTime>(Parameters.Date);
                var durationMinutes = parameters.GetRequiredParameter<int>(Parameters.DurationMinutes);
                var maxSlots = parameters.GetParameter<int?>(Parameters.MaxSlots) ?? 10; // Default to 10 slots

                var duration = TimeSpan.FromMinutes(durationMinutes);
                
                // Get physician schedule
                var physicianSchedule = _scheduleManager.GetPhysicianSchedule(physicianId);
                var physician = _registry.GetProfileById(physicianId) as PhysicianProfile;

                // Find available slots starting from the beginning of the business day (8 AM)
                var searchStartTime = date.Date.AddHours(8); // 8:00 AM
                
                // Ensure search time is not in the past
                if (searchStartTime < DateTime.Now)
                {
                    searchStartTime = DateTime.Now;
                }

                // Use booking strategy to find available slots
                // This automatically enforces business hours (8 AM - 5 PM, Monday-Friday)
                var availableSlots = _bookingStrategy.FindAvailableSlots(
                    physicianSchedule,
                    duration,
                    searchStartTime,
                    maxSlots);

                // Filter to only include slots on the requested date
                var slotsForDate = availableSlots
                    .Where(slot => slot.Start.Date == date.Date)
                    .ToList();

                if (!slotsForDate.Any())
                {
                    return CommandResult.Ok(
                        $"No available time slots found for Dr. {physician?.Name ?? "Unknown"} on {date:yyyy-MM-dd}.\n" +
                        "Appointments are only available Monday through Friday from 8:00 AM to 5:00 PM.");
                }

                // Format the results
                var slotsText = new StringBuilder();
                slotsText.AppendLine($"Available time slots for Dr. {physician?.Name ?? "Unknown"} on {date:yyyy-MM-dd}:");
                slotsText.AppendLine($"Duration: {durationMinutes} minutes\n");

                foreach (var slot in slotsForDate)
                {
                    var optimalIndicator = slot.IsOptimal ? " (Optimal)" : "";
                    slotsText.AppendLine($"  {slot.Start:HH:mm} - {slot.End:HH:mm}{optimalIndicator}");
                }

                slotsText.AppendLine();
                slotsText.AppendLine("Note: All appointments must be scheduled between 8:00 AM and 5:00 PM, Monday through Friday.");

                return CommandResult.Ok(slotsText.ToString(), slotsForDate);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to get available time slots: {ex.Message}", ex);
            }
        }
    }
}
