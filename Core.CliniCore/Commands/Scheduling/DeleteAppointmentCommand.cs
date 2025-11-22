// Core.CliniCore/Commands/Scheduling/DeleteAppointmentCommand.cs
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Scheduling.Management;

namespace Core.CliniCore.Commands.Scheduling
{
    public class DeleteAppointmentCommand : AbstractCommand
    {
        public const string Key = "deleteappointment";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string AppointmentId = "appointment_id";
        }

        private readonly ScheduleManager _scheduleManager;

        public DeleteAppointmentCommand(ScheduleManager scheduleManager)
        {
            _scheduleManager = scheduleManager ?? throw new ArgumentNullException(nameof(scheduleManager));
        }

        public override string Description => "Deletes an appointment from the schedule";

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

            // Add warning for completed appointments
            if (appointment.Status == AppointmentStatus.Completed)
            {
                result.AddWarning("Deleting a completed appointment will remove it from historical records");
            }

            return result;
        }

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
