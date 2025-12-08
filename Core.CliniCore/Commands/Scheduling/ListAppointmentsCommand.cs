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
    /// <summary>
    /// Command that lists appointments using optional filters such as date, physician, and patient.
    /// </summary>
    public class ListAppointmentsCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "listappointments";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="ListAppointmentsCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the date on which to filter appointments.
            /// </summary>
            public const string Date = "date";

            /// <summary>
            /// Parameter key for filtering by physician identifier.
            /// </summary>
            public const string PhysicianId = "physician_id";

            /// <summary>
            /// Parameter key for filtering by patient identifier.
            /// </summary>
            public const string PatientId = "patient_id";
        }

        private readonly SchedulerService _scheduleManager;
        private readonly ProfileService _profileRegistry;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListAppointmentsCommand"/> class.
        /// </summary>
        /// <param name="scheduleManager">The scheduler service responsible for managing appointments.</param>
        /// <param name="profileService">The profile service used to resolve patient and physician details.</param>
        /// <exception cref="ArgumentNullException">Thrown when any dependency is <c>null</c>.</exception>
        public ListAppointmentsCommand(SchedulerService scheduleManager, ProfileService profileService)
        {
            _scheduleManager = scheduleManager ?? throw new ArgumentNullException(nameof(scheduleManager));
            _profileRegistry = profileService ?? throw new ArgumentNullException(nameof(profileService));
        }

        /// <inheritdoc />
        public override string Description => "Lists appointments with various filters";

        /// <inheritdoc />
        public override bool CanUndo => false;

        /// <inheritdoc />
        public override Permission? GetRequiredPermission()
            => Permission.ViewOwnAppointments;

        /// <inheritdoc />
        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            return CommandValidationResult.Success();
        }

        /// <inheritdoc />
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
