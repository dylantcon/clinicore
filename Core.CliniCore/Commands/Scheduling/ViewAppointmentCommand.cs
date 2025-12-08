using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Scheduling;
using Core.CliniCore.Scheduling.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.CliniCore.Service;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Users.Concrete;

namespace Core.CliniCore.Commands.Scheduling
{
    public class ViewAppointmentCommand : AbstractCommand
    {
        public const string Key = "viewappointment";
        public override string CommandKey => Key;
        public static class Parameters
        {
            public const string AppointmentId = "appointment_id";
        }

        private readonly SchedulerService _scheduleManager;
        private readonly ProfileService _profileRegistry;

        public ViewAppointmentCommand(SchedulerService scheduleManager, ProfileService profileService)
        {
            _scheduleManager = scheduleManager ?? throw new ArgumentNullException(nameof(scheduleManager));
            _profileRegistry = profileService ?? throw new ArgumentNullException(nameof(profileService));
        }

        public override string Description => "Views detailed information about a specific appointment";

        public override Permission? GetRequiredPermission()
            => Permission.ViewOwnAppointments;

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Check required parameters
            var missingParams = parameters.GetMissingRequired(Parameters.AppointmentId);

            if (missingParams.Any())
            {
                foreach (var error in missingParams)
                    result.AddError(error);
                return result;
            }

            // Validate appointment ID format
            var appointmentId = parameters.GetParameter<Guid?>(Parameters.AppointmentId);
            if (!appointmentId.HasValue || appointmentId.Value == Guid.Empty)
            {
                result.AddError("Invalid appointment ID format");
            }

            return result;
        }

        protected override CommandValidationResult ValidateSpecific(CommandParameters parameters, SessionContext? session)
        {
            var result = CommandValidationResult.Success();

            // Find the appointment to check permissions
            var appointmentId = parameters.GetRequiredParameter<Guid>(Parameters.AppointmentId);
            var appointment = FindAppointmentById(appointmentId);

            if (appointment == null)
            {
                result.AddError($"Appointment with ID {appointmentId} not found");
                return result;
            }

            // Role-based access control
            if (session != null)
            {
                if (session.UserRole == UserRole.Patient && appointment.PatientId != session.UserId)
                {
                    result.AddError("Patients can only view their own appointments");
                }
                else if (session.UserRole == UserRole.Physician && appointment.PhysicianId != session.UserId)
                {
                    result.AddError("Physicians can only view their own appointments");
                }
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var appointmentId = parameters.GetRequiredParameter<Guid>(Parameters.AppointmentId);
                var appointment = FindAppointmentById(appointmentId);

                if (appointment == null)
                {
                    return CommandResult.Fail($"Appointment with ID {appointmentId} not found");
                }

                // Get patient and physician details
                var patient = _profileRegistry.GetProfileById(appointment.PatientId) as PatientProfile;
                var physician = _profileRegistry.GetProfileById(appointment.PhysicianId) as PhysicianProfile;

                // Check for conflicts at the appointment time
                var conflicts = CheckAppointmentConflicts(appointment);
                var conflictInfo = conflicts.Any() ? 
                    "\n  Conflicts: " + string.Join(", ", conflicts.Select(c => c.Type.ToString())) :
                    "\n  Conflicts: None";

                // Build detailed appointment information
                var appointmentDetails = new StringBuilder();
                appointmentDetails.AppendLine("=== APPOINTMENT DETAILS ===");
                appointmentDetails.AppendLine($"  Appointment ID: {appointment.Id:N}");
                appointmentDetails.AppendLine($"  Status: {appointment.Status}");
                appointmentDetails.AppendLine($"  Date/Time: {appointment.Start:yyyy-MM-dd HH:mm} - {appointment.End:HH:mm}");
                appointmentDetails.AppendLine($"  Duration: {(appointment.End - appointment.Start).TotalMinutes} minutes");
                appointmentDetails.AppendLine();
                appointmentDetails.AppendLine("=== PATIENT INFORMATION ===");
                appointmentDetails.AppendLine($"  Name: {patient?.GetValue<string>(CommonEntryType.Name.GetKey()) ?? "Unknown"}");
                appointmentDetails.AppendLine($"  Patient ID: {appointment.PatientId:N}");
                if (patient != null)
                {
                    appointmentDetails.AppendLine($"  Address: {patient.GetValue<string>(CommonEntryType.Address.GetKey()) ?? string.Empty}");
                    appointmentDetails.AppendLine($"  Gender: {patient.GetValue<Gender>(PatientEntryType.Gender.GetKey())}");
                    appointmentDetails.AppendLine($"  Birth Date: {patient.GetValue<DateTime>(CommonEntryType.BirthDate.GetKey()):yyyy-MM-dd}");
                }
                appointmentDetails.AppendLine();
                appointmentDetails.AppendLine("=== PHYSICIAN INFORMATION ===");
                appointmentDetails.AppendLine($"  Name: Dr. {physician?.GetValue<string>(CommonEntryType.Name.GetKey()) ?? "Unknown"}");
                appointmentDetails.AppendLine($"  Physician ID: {appointment.PhysicianId:N}");
                if (physician != null)
                {
                    var specializations = physician.GetValue<List<MedicalSpecialization>>(PhysicianEntryType.Specializations.GetKey()) ?? new();
                    var specializationsText = specializations.Any() ?
                        string.Join(", ", specializations) : "General Practice";
                    appointmentDetails.AppendLine($"  Specializations: {specializationsText}");
                    appointmentDetails.AppendLine($"  License: {physician.GetValue<string>(PhysicianEntryType.LicenseNumber.GetKey()) ?? string.Empty}");
                    appointmentDetails.AppendLine($"  Graduation: {physician.GetValue<DateTime>(PhysicianEntryType.GraduationDate.GetKey()):yyyy-MM-dd}");
                }
                appointmentDetails.AppendLine();
                appointmentDetails.AppendLine("=== APPOINTMENT DETAILS ===");
                appointmentDetails.AppendLine($"  Reason: {appointment.ReasonForVisit ?? "General Consultation"}");
                appointmentDetails.AppendLine($"  Type: {appointment.AppointmentType}");
                appointmentDetails.AppendLine($"  Notes: {appointment.Notes ?? "None"}");
                appointmentDetails.AppendLine($"  Clinical Document ID: {(appointment.ClinicalDocumentId?.ToString("N") ?? "Not created")}");
                appointmentDetails.AppendLine(conflictInfo);
                appointmentDetails.AppendLine();
                appointmentDetails.AppendLine("=== BUSINESS HOURS COMPLIANCE ===");
                appointmentDetails.AppendLine(ValidateBusinessHours(appointment) ? 
                    "  ✓ Appointment is within business hours (Mon-Fri, 8 AM - 5 PM)" :
                    "  ✗ Appointment is outside business hours");

                return CommandResult.Ok(appointmentDetails.ToString(), appointment);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to view appointment: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Finds an appointment by ID across all physician schedules
        /// </summary>
        private AppointmentTimeInterval? FindAppointmentById(Guid appointmentId)
        {
            return _scheduleManager.FindAppointmentById(appointmentId);
        }

        /// <summary>
        /// Checks for scheduling conflicts at the appointment time
        /// </summary>
        private List<ScheduleConflict> CheckAppointmentConflicts(AppointmentTimeInterval appointment)
        {
            try
            {
                // Get the physician's daily schedule for the appointment date
                var dailySchedule = _scheduleManager.GetDailySchedule(appointment.PhysicianId, appointment.Start.Date);
                var conflicts = new List<ScheduleConflict>();

                foreach (var otherAppointment in dailySchedule)
                {
                    if (otherAppointment.Id != appointment.Id && 
                        appointment.Overlaps(otherAppointment))
                    {
                        conflicts.Add(new ScheduleConflict
                        {
                            Type = ConflictType.DoubleBooking,
                            ConflictingInterval = otherAppointment,
                            Description = $"Overlaps with appointment {otherAppointment.Id:N}"
                        });
                    }
                }

                return conflicts;
            }
            catch
            {
                return new List<ScheduleConflict>();
            }
        }

        /// <summary>
        /// Validates if the appointment is within business hours
        /// </summary>
        private bool ValidateBusinessHours(AppointmentTimeInterval appointment)
        {
            // Check day of week (Monday-Friday)
            if (appointment.Start.DayOfWeek == DayOfWeek.Saturday ||
                appointment.Start.DayOfWeek == DayOfWeek.Sunday)
            {
                return false;
            }

            // Check hours (8 AM - 5 PM)
            var startHour = appointment.Start.TimeOfDay;
            var endHour = appointment.End.TimeOfDay;
            var businessStart = new TimeSpan(8, 0, 0);
            var businessEnd = new TimeSpan(17, 0, 0);

            return startHour >= businessStart && endHour <= businessEnd;
        }
    }
}
