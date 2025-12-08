// Core.CliniCore/Commands/Scheduling/DeleteAppointmentCommand.cs
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Service;

namespace Core.CliniCore.Commands.Scheduling
{
    /// <summary>
    /// Command that permanently deletes an appointment from the schedule.
    /// </summary>
    public class DeleteAppointmentCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "deleteappointment";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="DeleteAppointmentCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the appointment identifier to delete.
            /// </summary>
            public const string AppointmentId = "appointment_id";
        }

        private readonly SchedulerService _scheduleManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteAppointmentCommand"/> class.
        /// </summary>
        /// <param name="scheduleManager">The scheduler service responsible for managing appointments.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="scheduleManager"/> is <c>null</c>.</exception>
        public DeleteAppointmentCommand(SchedulerService scheduleManager)
        {
            _scheduleManager = scheduleManager ?? throw new ArgumentNullException(nameof(scheduleManager));
        }

        /// <inheritdoc />
        public override string Description => "Deletes an appointment from the schedule";

        /// <inheritdoc />
        public override bool CanUndo => false;

        /// <inheritdoc />
        public override Permission? GetRequiredPermission()
            => Permission.ScheduleAnyAppointment;

        /// <inheritdoc />
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

            // Add warning for completed appointments
            if (appointment.Status == AppointmentStatus.Completed)
            {
                result.AddWarning("Deleting a completed appointment will remove it from historical records");
            }

            return result;
        }

        /// <inheritdoc />
        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var appointmentId = parameters.GetRequiredParameter<Guid>(Parameters.AppointmentId);

                // Appointment existence already validated in ValidateParameters
                var appointment = _scheduleManager.FindAppointmentById(appointmentId)!;

                // Delete from physician's schedule
                bool deleted = _scheduleManager.DeleteAppointment(appointment.PhysicianId, appointmentId);

                if (deleted)
                {
                    return CommandResult.Ok($"Appointment deleted successfully (ID: {appointmentId})", appointmentId);
                }

                return CommandResult.Fail("Failed to delete appointment");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to delete appointment: {ex.Message}", ex);
            }
        }
    }
}
