// Core.CliniCore/Commands/Scheduling/UpdateAppointmentCommand.cs
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Scheduling;
using Core.CliniCore.Services;

namespace Core.CliniCore.Commands.Scheduling
{
    public class UpdateAppointmentCommand : AbstractCommand
    {
        public const string Key = "updateappointment";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string AppointmentId = "appointment_id";
            public const string ReasonForVisit = "reason";
            public const string Notes = "notes";
            public const string DurationMinutes = "duration_minutes";
            public const string NewStartTime = "new_start_time";
        }

        private readonly SchedulerService _scheduleManager;

        public UpdateAppointmentCommand(SchedulerService scheduleManager)
        {
            _scheduleManager = scheduleManager ?? throw new ArgumentNullException(nameof(scheduleManager));
        }

        public override string Description => "Updates appointment details (time, duration, reason, and notes)";

        public override bool CanUndo => false;

        public override Permission? GetRequiredPermission()
            => Permission.ScheduleAnyAppointment;

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            var appointmentId = parameters.GetParameter<Guid?>(Parameters.AppointmentId);
            if (!appointmentId.HasValue || appointmentId.Value == Guid.Empty)
            {
                result.AddError("Appointment ID is required");
                return result;
            }

            // Validate appointment exists
            var appointment = _scheduleManager.FindAppointmentById(appointmentId.Value);
            if (appointment == null)
            {
                result.AddError($"Appointment with ID {appointmentId.Value} not found");
                return result;
            }

            // Validate duration if provided
            var durationMinutes = parameters.GetParameter<int?>(Parameters.DurationMinutes);
            if (durationMinutes.HasValue)
            {
                if (durationMinutes.Value < 15)
                {
                    result.AddError("Appointment duration must be at least 15 minutes");
                }
                else if (durationMinutes.Value > 180)
                {
                    result.AddError("Appointment duration cannot exceed 3 hours (180 minutes)");
                }
            }

            // Get newStartTime early so we can check if only duration is changing
            var newStartTime = parameters.GetParameter<DateTime?>(Parameters.NewStartTime);

            // Check conflicts for duration-only change (no time change)
            if (durationMinutes.HasValue && !newStartTime.HasValue)
            {
                var currentDuration = (int)(appointment.End - appointment.Start).TotalMinutes;
                if (durationMinutes.Value != currentDuration)
                {
                    var proposedEnd = appointment.Start.AddMinutes(durationMinutes.Value);
                    var proposedAppointment = new AppointmentTimeInterval(
                        appointment.Start,
                        proposedEnd,
                        appointment.PatientId,
                        appointment.PhysicianId,
                        appointment.ReasonForVisit ?? "Consultation",
                        appointment.Status);

                    var conflictResult = _scheduleManager.CheckForConflicts(
                        proposedAppointment,
                        excludeAppointmentId: appointmentId.Value,
                        includeSuggestions: true);

                    if (conflictResult.HasConflicts)
                    {
                        foreach (var error in conflictResult.GetValidationErrors())
                        {
                            result.AddError(error);
                        }
                    }
                }
            }

            // Validate new start time if provided
            if (newStartTime.HasValue)
            {
                if (newStartTime.Value < DateTime.Now)
                {
                    result.AddError("Cannot reschedule appointment to a past time");
                }

                // Calculate end time for conflict checking
                var effectiveDuration = durationMinutes ?? (int)(appointment.End - appointment.Start).TotalMinutes;
                var endTime = newStartTime.Value.AddMinutes(effectiveDuration);

                // Check for all scheduling conflicts (overlap, double-booking, business hours, duration)
                // using centralized conflict detection - no manual business hours checks needed
                var proposedAppointment = new AppointmentTimeInterval(
                    newStartTime.Value,
                    endTime,
                    appointment.PatientId,
                    appointment.PhysicianId,
                    appointment.ReasonForVisit ?? "Consultation",
                    appointment.Status);

                var conflictResult = _scheduleManager.CheckForConflicts(
                    proposedAppointment,
                    excludeAppointmentId: appointmentId.Value,
                    includeSuggestions: true);

                if (conflictResult.HasConflicts)
                {
                    foreach (var error in conflictResult.GetValidationErrors())
                    {
                        result.AddError(error);
                    }
                }
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var appointmentId = parameters.GetRequiredParameter<Guid>(Parameters.AppointmentId);
                var reason = parameters.GetParameter<string?>(Parameters.ReasonForVisit);
                var notes = parameters.GetParameter<string?>(Parameters.Notes);
                var durationMinutes = parameters.GetParameter<int?>(Parameters.DurationMinutes);
                var newStartTime = parameters.GetParameter<DateTime?>(Parameters.NewStartTime);

                // Delegate to SchedulerService for business logic and conflict validation
                var result = _scheduleManager.UpdateAppointment(
                    appointmentId,
                    reason,
                    notes,
                    durationMinutes,
                    newStartTime);

                if (!result.Success)
                {
                    // Return failure with conflict details if available
                    if (result.Conflicts.Any())
                    {
                        var errorMessage = result.Message;

                        // Add alternative suggestions if available
                        if (result.AlternativeSuggestions.Any())
                        {
                            errorMessage += "\n\nSuggested alternative times:";
                            foreach (var suggestion in result.AlternativeSuggestions.Take(3))
                            {
                                errorMessage += $"\n  - {suggestion.Start:yyyy-MM-dd HH:mm} to {suggestion.End:HH:mm} ({suggestion.Reason})";
                            }
                        }

                        return CommandResult.Fail(errorMessage);
                    }

                    return CommandResult.Fail(result.Message);
                }

                var appointment = _scheduleManager.FindAppointmentById(appointmentId);
                return CommandResult.Ok($"Appointment updated successfully (ID: {appointmentId})", appointment);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to update appointment: {ex.Message}", ex);
            }
        }
    }
}
