// Core.CliniCore/Commands/Scheduling/ListAppointmentsCommand.cs
using System;
using System.Text;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Scheduling.Management;
using Core.CliniCore.Scheduling;

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

        private readonly ScheduleManager _scheduleManager;
        private readonly ProfileRegistry _profileRegistry = ProfileRegistry.Instance;

        public ListAppointmentsCommand(ScheduleManager scheduleManager)
        {
            _scheduleManager = scheduleManager ?? throw new ArgumentNullException(nameof(scheduleManager));
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
                var date = parameters.GetParameter<DateTime?>(Parameters.Date) ?? DateTime.Today;
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

                var appointments = physicianId.HasValue
                    ? _scheduleManager.GetDailySchedule(physicianId.Value, date)
                    : patientId.HasValue
                        ? _scheduleManager.GetPatientAppointments(patientId.Value)
                        : session?.UserRole == UserRole.Administrator
                            ? _scheduleManager.GetAllAppointments()
                            : Enumerable.Empty<AppointmentTimeInterval>();

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
                   $"  Patient: {patient?.Name ?? "Unknown"}\n" +
                   $"  Physician: Dr. {physician?.Name ?? "Unknown"}\n" +
                   $"  Reason: {apt.ReasonForVisit ?? "General Consultation"}\n" +
                   $"  ---";
        }
    }
}
