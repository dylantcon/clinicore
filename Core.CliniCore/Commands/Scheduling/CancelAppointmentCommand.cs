// Core.CliniCore/Commands/Scheduling/CancelAppointmentCommand.cs
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Scheduling.Management;

namespace Core.CliniCore.Commands.Scheduling
{
    public class CancelAppointmentCommand : AbstractCommand
    {
        private readonly ScheduleManager _scheduleManager;

        public CancelAppointmentCommand(ScheduleManager scheduleManager)
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

            var missingParams = parameters.GetMissingRequired("appointment_id", "physician_id");
            if (missingParams.Any())
            {
                foreach (var error in missingParams)
                    result.AddError(error);
            }

            var reason = parameters.GetParameter<string>("reason");
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
                var appointmentId = parameters.GetRequiredParameter<Guid>("appointment_id");
                var physicianId = parameters.GetRequiredParameter<Guid>("physician_id");
                var reason = parameters.GetParameter<string>("reason") ?? "Cancelled by user";

                if (_scheduleManager.CancelAppointment(physicianId, appointmentId, reason))
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