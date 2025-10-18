// Core.CliniCore/Commands/Scheduling/UpdateAppointmentCommand.cs
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Scheduling;
using Core.CliniCore.Scheduling.Management;

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
        }

        private readonly ScheduleManager _scheduleManager;

        public UpdateAppointmentCommand(ScheduleManager scheduleManager)
        {
            _scheduleManager = scheduleManager ?? throw new ArgumentNullException(nameof(scheduleManager));
        }

        public override string Description => "Updates appointment details (reason, notes, and duration)";

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
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var appointmentId = parameters.GetRequiredParameter<Guid>(Parameters.AppointmentId);

                // Find appointment
                var appointment = _scheduleManager.FindAppointmentById(appointmentId);
                if (appointment == null)
                {
                    return CommandResult.Fail("Appointment not found");
                }

                // Update fields if provided
                var reason = parameters.GetParameter<string?>(Parameters.ReasonForVisit);
                if (reason != null)
                {
                    appointment.ReasonForVisit = reason;
                }

                var notes = parameters.GetParameter<string?>(Parameters.Notes);
                if (notes != null)
                {
                    appointment.Notes = notes;
                }

                var durationMinutes = parameters.GetParameter<int?>(Parameters.DurationMinutes);
                if (durationMinutes.HasValue && durationMinutes.Value > 0)
                {
                    appointment.UpdateDuration(durationMinutes.Value);
                }

                appointment.ModifiedAt = DateTime.Now;

                return CommandResult.Ok("Appointment updated successfully", appointment);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to update appointment: {ex.Message}", ex);
            }
        }
    }
}
