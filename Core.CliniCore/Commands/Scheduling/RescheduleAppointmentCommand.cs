// Core.CliniCore/Commands/Scheduling/RescheduleAppointmentCommand.cs  
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Scheduling.Management;

namespace Core.CliniCore.Commands.Scheduling
{
    public class RescheduleAppointmentCommand : AbstractCommand
    {
        public const string Key = "rescheduleappointment";
        public override string CommandKey => Key;
        public static class Parameters
        {
            public const string AppointmentId = "appointment_id";
            public const string NewDateTime = "newDateTime";
        }

        private readonly ScheduleManager _scheduleManager;

        public RescheduleAppointmentCommand(ScheduleManager scheduleManager)
        {
            _scheduleManager = scheduleManager ?? throw new ArgumentNullException(nameof(scheduleManager));
        }

        public override string Description => "Reschedules an existing appointment";

        public override bool CanUndo => false;

        public override Permission? GetRequiredPermission()
            => Permission.ScheduleAnyAppointment;

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            var missingParams = parameters.GetMissingRequired(
                Parameters.AppointmentId, Parameters.NewDateTime);

            if (missingParams.Any())
            {
                foreach (var error in missingParams)
                    result.AddError(error);
            }

            // Validate new time is in future
            var newStart = parameters.GetParameter<DateTime?>(Parameters.NewDateTime);
            if (newStart.HasValue && newStart.Value < DateTime.Now)
            {
                result.AddError("Cannot reschedule to a past time");
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var appointmentId = parameters.GetRequiredParameter<Guid>(Parameters.AppointmentId);
                var newStart = parameters.GetRequiredParameter<DateTime>(Parameters.NewDateTime);

                // Get existing appointment to determine physician and duration
                var existingAppointment = _scheduleManager.FindAppointmentById(appointmentId);
                if (existingAppointment == null)
                {
                    return CommandResult.Fail("Appointment not found");
                }

                var durationMinutes = (int)(existingAppointment.End - existingAppointment.Start).TotalMinutes;
                var newEnd = newStart.AddMinutes(durationMinutes);

                var result = _scheduleManager.RescheduleAppointment(
                    existingAppointment.PhysicianId, appointmentId, newStart, newEnd);

                if (result.Success)
                {
                    return CommandResult.Ok(
                        $"Appointment rescheduled to {newStart:yyyy-MM-dd HH:mm}",
                        result.AppointmentId);
                }

                return CommandResult.Fail(result.Message);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to reschedule: {ex.Message}", ex);
            }
        }
    }
}
