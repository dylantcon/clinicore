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
                "appointment_id", "physician_id", "new_start_time", "duration_minutes");

            if (missingParams.Any())
            {
                foreach (var error in missingParams)
                    result.AddError(error);
            }

            // Validate new time is in future
            var newStart = parameters.GetParameter<DateTime?>("new_start_time");
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
                var appointmentId = parameters.GetRequiredParameter<Guid>("appointment_id");
                var physicianId = parameters.GetRequiredParameter<Guid>("physician_id");
                var newStart = parameters.GetRequiredParameter<DateTime>("new_start_time");
                var durationMinutes = parameters.GetRequiredParameter<int>("duration_minutes");

                var newEnd = newStart.AddMinutes(durationMinutes);

                var result = _scheduleManager.RescheduleAppointment(
                    physicianId, appointmentId, newStart, newEnd);

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
