// Core.CliniCore/Commands/Scheduling/ListAppointmentsCommand.cs
using System;
using System.Text;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.Scheduling;
using Core.CliniCore.Service;

namespace Core.CliniCore.Commands.Scheduling
{
    public class ListAppointmentsCommand : AbstractCommand
    {
        public const string Key = "listappointments";
        public override string CommandKey => Key;
        public static class Parameters
        {
            public const string Date = "date";
            public const string PhysicianId = "physician_id";
            public const string PatientId = "patient_id";
        }

        private readonly SchedulerService _scheduleManager;
        private readonly ProfileService _profileRegistry;

        public ListAppointmentsCommand(SchedulerService scheduleManager, ProfileService profileService)
        {
            _scheduleManager = scheduleManager ?? throw new ArgumentNullException(nameof(scheduleManager));
            _profileRegistry = profileService ?? throw new ArgumentNullException(nameof(profileService));
        }

        public override string Description => "Lists appointments with various filters";

        public override bool CanUndo => false;

        public override Permission? GetRequiredPermission()
            => Permission.ViewOwnAppointments;

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            return CommandValidationResult.Success();
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var date = parameters.GetParameter<DateTime?>(Parameters.Date);
                var physicianId = parameters.GetParameter<Guid?>(Parameters.PhysicianId);
                var patientId = parameters.GetParameter<Guid?>(Parameters.PatientId);

                // Role-based filtering
                if (session?.UserRole == UserRole.Patient)
                {
                    patientId = session.UserId;
                }
                else if (session?.UserRole == UserRole.Physician && !physicianId.HasValue)
                {
                    physicianId = session.UserId;
                }

                IEnumerable<AppointmentTimeInterval> appointments;
                if (physicianId.HasValue)
                {
                    // If a specific date is provided, show only that day's appointments
                    // Otherwise, show all appointments for the next 90 days (reasonable range)
                    if (date.HasValue)
                    {
                        appointments = _scheduleManager.GetDailySchedule(physicianId.Value, date.Value);
                    }
                    else
                    {
                        appointments = _scheduleManager.GetScheduleInRange(
                            physicianId.Value,
                            DateTime.Today,
                            DateTime.Today.AddDays(90));
                    }
                }
                else if (patientId.HasValue)
                {
                    appointments = _scheduleManager.GetPatientAppointments(patientId.Value);
                }
                else if (session?.UserRole == UserRole.Administrator)
                {
                    appointments = _scheduleManager.GetAllAppointments();
                }
                else
                {
                    appointments = Enumerable.Empty<AppointmentTimeInterval>();
                }

                var appointmentList = appointments.ToList();

                if (!appointmentList.Any())
                {
                    return CommandResult.Ok("No appointments found.", appointmentList);
                }

                var sb = new StringBuilder();
                sb.AppendLine($"Found {appointmentList.Count} appointment(s):");
                sb.AppendLine(new string('-', 80));

                foreach (var apt in appointmentList)
                {
                    sb.AppendLine(FormatAppointment(apt));
                }

                return CommandResult.Ok(sb.ToString(), appointmentList);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to list appointments: {ex.Message}", ex);
            }
        }

        private string FormatAppointment(AppointmentTimeInterval apt)
        {
            var patient = _profileRegistry.GetProfileById(apt.PatientId) as PatientProfile;
            var physician = _profileRegistry.GetProfileById(apt.PhysicianId) as PhysicianProfile;

            return $"  Appointment ID: {apt.Id:N}\n" +
                   $"  Date/Time: {apt.Start:yyyy-MM-dd HH:mm} - {apt.End:HH:mm}\n" +
                   $"  Status: {apt.Status}\n" +
                   $"  Patient: {patient?.GetValue<string>(CommonEntryType.Name.GetKey()) ?? "Unknown"}\n" +
                   $"  Physician: Dr. {physician?.GetValue<string>(CommonEntryType.Name.GetKey()) ?? "Unknown"}\n" +
                   $"  Reason: {apt.ReasonForVisit ?? "General Consultation"}\n" +
                   $"  ---";
        }
    }
}
