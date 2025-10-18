// Core.CliniCore/Commands/Scheduling/ScheduleAppointmentCommand.cs
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Scheduling;
using Core.CliniCore.Scheduling.Management;

namespace Core.CliniCore.Commands.Scheduling
{
    public class ScheduleAppointmentCommand : AbstractCommand
    {
        public const string Key = "scheduleappointment";
        public override string CommandKey => Key;
        public static class Parameters
        {
            public const string PatientId = "patient_id";
            public const string PhysicianId = "physician_id";
            public const string StartTime = "start_time";
            public const string DurationMinutes = "duration_minutes";
            public const string Reason = "reason";
            public const string Notes = "notes";
        }

        private readonly ProfileRegistry _registry = ProfileRegistry.Instance;
        private readonly ScheduleManager _scheduleManager;
        private AppointmentTimeInterval? _createdAppointment;

        public ScheduleAppointmentCommand(ScheduleManager scheduleManager)
        {
            _scheduleManager = scheduleManager ?? throw new ArgumentNullException(nameof(scheduleManager));
        }

        public override string Description => "Schedules a new appointment between a patient and physician";

        public override bool CanUndo => true;

        public override Permission? GetRequiredPermission()
            => Permission.ScheduleAnyAppointment;

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Check required parameters
            var missingParams = parameters.GetMissingRequired(
                Parameters.PatientId, Parameters.PhysicianId, Parameters.StartTime, Parameters.DurationMinutes);

            if (missingParams.Any())
            {
                foreach (var error in missingParams)
                    result.AddError(error);
                return result;
            }

            // Validate patient exists - use nullable type
            var patientId = parameters.GetParameter<Guid?>(Parameters.PatientId);
            if (!patientId.HasValue || patientId.Value == Guid.Empty)
            {
                result.AddError("Invalid patient ID");
            }
            else
            {
                var patient = _registry.GetProfileById(patientId.Value);
                if (patient == null || patient.Role != UserRole.Patient)
                {
                    result.AddError($"Patient with ID {patientId.Value} not found");
                }
            }

            // Validate physician exists - use nullable type
            var physicianId = parameters.GetParameter<Guid?>(Parameters.PhysicianId);
            if (!physicianId.HasValue || physicianId.Value == Guid.Empty)
            {
                result.AddError("Invalid physician ID");
            }
            else
            {
                var physician = _registry.GetProfileById(physicianId.Value);
                if (physician == null || physician.Role != UserRole.Physician)
                {
                    result.AddError($"Physician with ID {physicianId.Value} not found");
                }
            }

            // Validate time is in the future - use nullable type
            var startTime = parameters.GetParameter<DateTime?>(Parameters.StartTime);
            if (startTime.HasValue && startTime.Value < DateTime.Now)
            {
                result.AddError("Cannot schedule appointments in the past");
            }

            // Validate duration - use nullable type
            var durationMinutes = parameters.GetParameter<int?>(Parameters.DurationMinutes);
            if (!durationMinutes.HasValue || durationMinutes.Value < 15)
            {
                result.AddError("Appointment must be at least 15 minutes");
            }
            else if (durationMinutes.Value > 180)
            {
                result.AddError("Appointment cannot exceed 3 hours (180 minutes)");
            }

            // Validate business hours (M-F 8am-5pm)
            if (startTime.HasValue && durationMinutes.HasValue && durationMinutes.Value > 0)
            {
                var endTime = startTime.Value.AddMinutes(durationMinutes.Value);

                // Check day of week
                if (startTime.Value.DayOfWeek == DayOfWeek.Saturday ||
                    startTime.Value.DayOfWeek == DayOfWeek.Sunday)
                {
                    result.AddError("Appointments can only be scheduled Monday through Friday");
                }

                // Check hours (8am-5pm)
                var startHour = startTime.Value.TimeOfDay;
                var endHour = endTime.TimeOfDay;
                var businessStart = new TimeSpan(8, 0, 0);
                var businessEnd = new TimeSpan(17, 0, 0);

                if (startHour < businessStart || endHour > businessEnd)
                {
                    result.AddError("Appointments must be scheduled between 8:00 AM and 5:00 PM");
                }
            }

            return result;
        }

        protected override CommandValidationResult ValidateSpecific(CommandParameters parameters, SessionContext? session)
        {
            var result = CommandValidationResult.Success();

            // Additional permission check - patients can only schedule their own appointments
            if (session != null && session.UserRole == UserRole.Patient)
            {
                var patientId = parameters.GetParameter<Guid?>(Parameters.PatientId);
                if (patientId.HasValue && patientId.Value != session.UserId)
                {
                    result.AddError("Patients can only schedule their own appointments");
                }
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                // Extract parameters - these are validated already so we can use GetRequiredParameter
                var patientId = parameters.GetRequiredParameter<Guid>(Parameters.PatientId);
                var physicianId = parameters.GetRequiredParameter<Guid>(Parameters.PhysicianId);
                var startTime = parameters.GetRequiredParameter<DateTime>(Parameters.StartTime);
                var durationMinutes = parameters.GetRequiredParameter<int>(Parameters.DurationMinutes);
                var reason = parameters.GetParameter<string>(Parameters.Reason) ?? "General Consultation";

                // Calculate end time
                var endTime = startTime.AddMinutes(durationMinutes);

                // Create the appointment
                _createdAppointment = new AppointmentTimeInterval(
                    startTime,
                    endTime,
                    patientId,
                    physicianId,
                    reason,
                    AppointmentStatus.Scheduled)
                {
                    ReasonForVisit = reason,
                    Notes = parameters.GetParameter<string>(Parameters.Notes)
                };

                // Try to schedule
                var scheduleResult = _scheduleManager.ScheduleAppointment(_createdAppointment);

                if (!scheduleResult.Success)
                {
                    // Check for double-booking
                    var hasDoubleBooking = scheduleResult.Conflicts
                        .Any(c => c.Type == ConflictType.DoubleBooking);

                    if (hasDoubleBooking)
                    {
                        return CommandResult.Fail(
                            "Cannot schedule appointment: Physician is already booked at this time. " +
                            scheduleResult.Message);
                    }

                    // Return with suggestions if available
                    if (scheduleResult.AlternativeSuggestions.Any())
                    {
                        var suggestions = string.Join("\n",
                            scheduleResult.AlternativeSuggestions
                                .Take(3)
                                .Select(s => $"  - {s.Start:yyyy-MM-dd HH:mm} to {s.End:HH:mm}"));

                        return CommandResult.Fail(
                            scheduleResult.Message + "\n\nSuggested alternative times:\n" + suggestions);
                    }

                    return CommandResult.Fail(scheduleResult.Message);
                }

                // Get patient and physician names for confirmation
                var patient = _registry.GetProfileById(patientId) as PatientProfile;
                var physician = _registry.GetProfileById(physicianId) as PhysicianProfile;

                // Establish relationship if one does not already exist
                if (physician != null && !physician.PatientIds.Contains(patientId))
                    _registry.AssignPatientToPhysician(patientId, physicianId, setPrimary: false);

                return CommandResult.Ok(
                    $"Appointment scheduled successfully:\n" +
                    $"  Patient: {patient?.Name ?? "Unknown"}\n" +
                    $"  Physician: Dr. {physician?.Name ?? "Unknown"}\n" +
                    $"  Date/Time: {startTime:yyyy-MM-dd HH:mm}\n" +
                    $"  Duration: {durationMinutes} minutes\n" +
                    $"  ID: {_createdAppointment.Id}",
                    _createdAppointment);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to schedule appointment: {ex.Message}", ex);
            }
        }

        protected override object? CaptureStateForUndo(CommandParameters parameters, SessionContext? session)
        {
            return _createdAppointment;
        }

        protected override CommandResult UndoCore(object previousState, SessionContext? session)
        {
            if (previousState is AppointmentTimeInterval appointment)
            {
                var success = _scheduleManager.CancelAppointment(
                    appointment.PhysicianId,
                    appointment.Id,
                    "Undo scheduling command");

                if (success)
                {
                    return CommandResult.Ok($"Appointment {appointment.Id} has been cancelled");
                }
            }
            return CommandResult.Fail("Unable to undo appointment scheduling");
        }
    }
}