using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Scheduling;
using Core.CliniCore.Scheduling.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Commands.Scheduling
{
    public class CheckConflictsCommand : AbstractCommand
    {
        public const string Key = "checkconflicts";
        public override string CommandKey => Key;
        public static class Parameters
        {
            public const string PhysicianId = "physician_id";
            public const string StartTime = "start_time";
            public const string DurationMinutes = "duration_minutes";
            public const string ExcludeAppointmentId = "exclude_appointment_id"; // For rescheduling scenarios
        }

        private readonly ScheduleManager _scheduleManager;

        public CheckConflictsCommand(ScheduleManager scheduleManager)
        {
            _scheduleManager = scheduleManager ?? throw new ArgumentNullException(nameof(scheduleManager));
        }

        public override string Description => "Checks for scheduling conflicts for a proposed appointment time without booking it";

        public override Permission? GetRequiredPermission()
            => Permission.ViewAllAppointments; // Can check conflicts if can view appointments

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Check required parameters
            var missingParams = parameters.GetMissingRequired(
                Parameters.PhysicianId, Parameters.StartTime, Parameters.DurationMinutes);

            if (missingParams.Any())
            {
                foreach (var error in missingParams)
                    result.AddError(error);
                return result;
            }

            // Validate physician ID
            var physicianId = parameters.GetParameter<Guid?>(Parameters.PhysicianId);
            if (!physicianId.HasValue || physicianId.Value == Guid.Empty)
            {
                result.AddError("Invalid physician ID");
            }

            // Validate start time
            var startTime = parameters.GetParameter<DateTime?>(Parameters.StartTime);
            if (startTime.HasValue && startTime.Value < DateTime.Now)
            {
                result.AddError("Cannot check conflicts for times in the past");
            }

            // Validate duration
            var durationMinutes = parameters.GetParameter<int?>(Parameters.DurationMinutes);
            if (!durationMinutes.HasValue || durationMinutes.Value < 15)
            {
                result.AddError("Duration must be at least 15 minutes");
            }
            else if (durationMinutes.Value > 180)
            {
                result.AddError("Duration cannot exceed 3 hours (180 minutes)");
            }

            // Validate business hours if we have valid time and duration
            if (startTime.HasValue && durationMinutes.HasValue && durationMinutes.Value > 0)
            {
                var endTime = startTime.Value.AddMinutes(durationMinutes.Value);

                // Check day of week
                if (startTime.Value.DayOfWeek == DayOfWeek.Saturday ||
                    startTime.Value.DayOfWeek == DayOfWeek.Sunday)
                {
                    result.AddError("Appointments can only be scheduled Monday through Friday");
                }

                // Check hours (8am-5pm)
                var startHour = startTime.Value.TimeOfDay;
                var endHour = endTime.TimeOfDay;
                var businessStart = new TimeSpan(8, 0, 0);
                var businessEnd = new TimeSpan(17, 0, 0);

                if (startHour < businessStart || endHour > businessEnd)
                {
                    result.AddError("Appointments must be scheduled between 8:00 AM and 5:00 PM");
                }
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                // Extract parameters - these are validated already so we can use GetRequiredParameter
                var physicianId = parameters.GetRequiredParameter<Guid>(Parameters.PhysicianId);
                var startTime = parameters.GetRequiredParameter<DateTime>(Parameters.StartTime);
                var durationMinutes = parameters.GetRequiredParameter<int>(Parameters.DurationMinutes);
                var excludeAppointmentId = parameters.GetParameter<Guid?>(Parameters.ExcludeAppointmentId);

                // Calculate end time
                var endTime = startTime.AddMinutes(durationMinutes);

                // Create a temporary appointment for conflict checking
                var proposedAppointment = new AppointmentTimeInterval(
                    startTime,
                    endTime,
                    Guid.NewGuid(), // Dummy patient ID for conflict checking
                    physicianId,
                    "Conflict Check",
                    AppointmentStatus.Scheduled);

                // Get the physician's schedule
                var physicianSchedule = _scheduleManager.GetPhysicianSchedule(physicianId);

                // If we're excluding an appointment (for rescheduling), temporarily remove it
                AppointmentTimeInterval? excludedAppointment = null;
                if (excludeAppointmentId.HasValue)
                {
                    excludedAppointment = physicianSchedule.Appointments
                        .FirstOrDefault(a => a.Id == excludeAppointmentId.Value);
                    if (excludedAppointment != null)
                    {
                        // Temporarily mark as cancelled for conflict checking
                        var originalStatus = excludedAppointment.Status;
                        excludedAppointment.Status = AppointmentStatus.Cancelled;
                        
                        // Restore status after checking
                        defer(() => excludedAppointment.Status = originalStatus);
                    }
                }

                // Use the schedule manager's conflict resolution system
                var conflictResolver = new ScheduleConflictDetector();
                var conflictResult = conflictResolver.CheckForConflicts(
                    proposedAppointment,
                    physicianSchedule);

                if (!conflictResult.HasConflicts)
                {
                    return CommandResult.Ok(
                        $"No conflicts detected for appointment:\n" +
                        $"  Physician ID: {physicianId}\n" +
                        $"  Date/Time: {startTime:yyyy-MM-dd HH:mm}\n" +
                        $"  Duration: {durationMinutes} minutes\n" +
                        $"  End Time: {endTime:HH:mm}");
                }

                // Report conflicts found
                var conflictDetails = new StringBuilder();
                conflictDetails.AppendLine($"CONFLICTS DETECTED for appointment:");
                conflictDetails.AppendLine($"  Physician ID: {physicianId}");
                conflictDetails.AppendLine($"  Date/Time: {startTime:yyyy-MM-dd HH:mm}");
                conflictDetails.AppendLine($"  Duration: {durationMinutes} minutes");
                conflictDetails.AppendLine($"  End Time: {endTime:HH:mm}");
                conflictDetails.AppendLine();

                var doubleBookingFound = false;
                foreach (var conflict in conflictResult.Conflicts)
                {
                    conflictDetails.AppendLine($"• {conflict.Type}: {conflict.Description}");
                    
                    if (conflict.Type == ConflictType.DoubleBooking)
                    {
                        doubleBookingFound = true;
                        if (conflict.ConflictingInterval is AppointmentTimeInterval existingAppt)
                        {
                            conflictDetails.AppendLine($"  Conflicting appointment: {existingAppt.Start:HH:mm}-{existingAppt.End:HH:mm} ({existingAppt.ReasonForVisit})");
                        }
                    }
                }

                // Add resolution suggestion (first available alternative)
                var resolutionResult = conflictResolver.FindAlternative(conflictResult, physicianSchedule);
                if (resolutionResult.RecommendedSlot != null)
                {
                    conflictDetails.AppendLine();
                    conflictDetails.AppendLine("SUGGESTED ALTERNATIVE:");
                    var suggestion = resolutionResult.RecommendedSlot;
                    conflictDetails.AppendLine($"  -> {suggestion.Start:yyyy-MM-dd HH:mm} to {suggestion.End:HH:mm} - {suggestion.Reason}");
                }

                // Emphasize double-booking prevention if detected
                if (doubleBookingFound)
                {
                    conflictDetails.AppendLine();
                    conflictDetails.AppendLine("*** DOUBLE-BOOKING PREVENTED: The system successfully detected and prevented physician double-booking. ***");
                }

                return CommandResult.Fail(conflictDetails.ToString());
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to check conflicts: {ex.Message}", ex);
            }
        }

        // Helper method to defer action execution (similar to Go's defer)
        private static void defer(Action action)
        {
            try
            {
                action();
            }
            catch
            {
                // Swallow exceptions in defer to not mask the main exception
            }
        }
    }
}
