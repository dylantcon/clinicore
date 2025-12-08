// Core.CliniCore/Commands/Scheduling/CancelAppointmentCommand.cs
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Service;

namespace Core.CliniCore.Commands.Scheduling
{
    /// <summary>
    /// Command that cancels an existing appointment.
    /// </summary>
    public class CancelAppointmentCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "cancelappointment";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="CancelAppointmentCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the appointment identifier to cancel.
            /// </summary>
            public const string AppointmentId = "appointment_id";

            /// <summary>
            /// Parameter key for the human-readable reason for cancellation.
            /// </summary>
            public const string Reason = "reason";
        }

        private readonly SchedulerService _scheduleManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CancelAppointmentCommand"/> class.
        /// </summary>
        /// <param name="scheduleManager">The scheduler service responsible for managing appointments.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="scheduleManager"/> is <c>null</c>.</exception>
        public CancelAppointmentCommand(SchedulerService scheduleManager)
        {
            _scheduleManager = scheduleManager ?? throw new ArgumentNullException(nameof(scheduleManager));
        }

        /// <inheritdoc />
        public override string Description => "Cancels an existing appointment";

        /// <inheritdoc />
        public override bool CanUndo => false; // Cancellation creates audit trail, don't undo

        /// <inheritdoc />
        public override Permission? GetRequiredPermission()
            => Permission.ViewOwnAppointments;

        /// <inheritdoc />
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

        /// <inheritdoc />
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