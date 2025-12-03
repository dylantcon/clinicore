// Core.CliniCore/Commands/Scheduling/CancelAppointmentCommand.cs
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Services;

namespace Core.CliniCore.Commands.Scheduling
{
    public class CancelAppointmentCommand : AbstractCommand
    {
        public const string Key = "cancelappointment";
        public override string CommandKey => Key;
        public static class Parameters
        {
            public const string AppointmentId = "appointment_id";
            public const string Reason = "reason";
        }

        private readonly SchedulerService _scheduleManager;

        public CancelAppointmentCommand(SchedulerService scheduleManager)
        {
            _scheduleManager = scheduleManager ?? throw new ArgumentNullException(nameof(scheduleManager));
        }

        public override string Description => "Cancels an existing appointment";

        public override bool CanUndo => false; // Cancellation creates audit trail, don't undo

        public override Permission? GetRequiredPermission()
            => Permission.ViewOwnAppointments;

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            var missingParams = parameters.GetMissingRequired(Parameters.AppointmentId);
            if (missingParams.Any())
            {
                foreach (var error in missingParams)
                    result.AddError(error);
            }

            var reason = parameters.GetParameter<string>(Parameters.Reason);
            if (string.IsNullOrWhiteSpace(reason))
            {
                result.AddWarning("No cancellation reason provided");
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var appointmentId = parameters.GetRequiredParameter<Guid>(Parameters.AppointmentId);
                var reason = parameters.GetParameter<string>(Parameters.Reason) ?? "Cancelled by user";

                // Get existing appointment to determine physician
                var existingAppointment = _scheduleManager.FindAppointmentById(appointmentId);
                if (existingAppointment == null)
                {
                    return CommandResult.Fail("Appointment not found");
                }

                if (_scheduleManager.CancelAppointment(existingAppointment.PhysicianId, appointmentId, reason))
                {
                    return CommandResult.Ok($"Appointment {appointmentId} cancelled successfully");
                }

                return CommandResult.Fail("Failed to cancel appointment. It may not exist or is already cancelled.");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to cancel appointment: {ex.Message}", ex);
            }
        }
    }
}