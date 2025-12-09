// Core.CliniCore/Commands/Scheduling/UpdateAppointmentCommand.cs
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Scheduling;
using Core.CliniCore.Service;

namespace Core.CliniCore.Commands.Scheduling
{
    /// <summary>
    /// Command that updates appointment details such as time, duration, reason, and notes.
    /// </summary>
    public class UpdateAppointmentCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "updateappointment";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="UpdateAppointmentCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the appointment identifier to update.
            /// </summary>
            public const string AppointmentId = "appointment_id";

            /// <summary>
            /// Parameter key for the updated reason for visit.
            /// </summary>
            public const string ReasonForVisit = "reason";

            /// <summary>
            /// Parameter key for updated appointment notes.
            /// </summary>
            public const string Notes = "notes";

            /// <summary>
            /// Parameter key for the updated appointment duration in minutes.
            /// </summary>
            public const string DurationMinutes = "duration_minutes";

            /// <summary>
            /// Parameter key for the new appointment start time.
            /// </summary>
            public const string NewStartTime = "new_start_time";

            /// <summary>
            /// Parameter key for the optional room number (1-999).
            /// </summary>
            public const string RoomNumber = "room_number";
        }

        private readonly SchedulerService _scheduleManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateAppointmentCommand"/> class.
        /// </summary>
        /// <param name="scheduleManager">The scheduler service responsible for managing appointments.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="scheduleManager"/> is <c>null</c>.</exception>
        public UpdateAppointmentCommand(SchedulerService scheduleManager)
        {
            _scheduleManager = scheduleManager ?? throw new ArgumentNullException(nameof(scheduleManager));
        }

        /// <inheritdoc />
        public override string Description => "Updates appointment details (time, duration, reason, and notes)";

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

        /// <inheritdoc />
        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var appointmentId = parameters.GetRequiredParameter<Guid>(Parameters.AppointmentId);
                var reason = parameters.GetParameter<string?>(Parameters.ReasonForVisit);
                var notes = parameters.GetParameter<string?>(Parameters.Notes);
                var durationMinutes = parameters.GetParameter<int?>(Parameters.DurationMinutes);
                var newStartTime = parameters.GetParameter<DateTime?>(Parameters.NewStartTime);
                var roomNumber = parameters.GetParameter<int?>(Parameters.RoomNumber);

                // Delegate to SchedulerService for business logic and conflict validation
                var result = _scheduleManager.UpdateAppointment(
                    appointmentId,
                    reason,
                    notes,
                    durationMinutes,
                    newStartTime,
                    roomNumber);

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
