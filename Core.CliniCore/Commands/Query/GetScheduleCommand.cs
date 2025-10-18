using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain;
using Core.CliniCore.Scheduling.Management;
using Core.CliniCore.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Commands.Query
{
    public class GetScheduleCommand : AbstractCommand
    {
        public const string Key = "getschedule";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string PhysicianId = "physicianId";
            public const string StartDate = "startDate";
            public const string EndDate = "endDate";
            public const string Date = "date";
            public const string ViewType = "viewType";
        }

        private readonly ProfileRegistry _profileRegistry = ProfileRegistry.Instance;
        private readonly ScheduleManager _scheduleManager = ScheduleManager.Instance;

        public override string Description => "Get schedule for a physician for a specific date or date range";

        public override Permission? GetRequiredPermission()
            => Permission.ViewAllAppointments;

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            var missingParams = parameters.GetMissingRequired(Parameters.PhysicianId);
            if (missingParams.Count != 0)
            {
                foreach (var error in missingParams)
                    result.AddError(error);
                return result;
            }

            var physicianId = parameters.GetParameter<Guid?>(Parameters.PhysicianId);
            if (!physicianId.HasValue || physicianId.Value == Guid.Empty)
            {
                result.AddError("Invalid physician ID");
            }
            else if (_profileRegistry.GetProfileById(physicianId.Value) is not PhysicianProfile)
            {
                result.AddError($"Physician with ID {physicianId} not found");
            }

            // Validate date parameters - require either single date or date range
            var hasDate = parameters.HasParameter(Parameters.Date);
            var hasDateRange = parameters.HasParameter(Parameters.StartDate) && parameters.HasParameter(Parameters.EndDate);

            if (!hasDate && !hasDateRange)
            {
                result.AddError("Must provide either 'date' parameter or both 'startDate' and 'endDate' parameters");
                return result;
            }

            if (hasDate && hasDateRange)
            {
                result.AddError("Provide either 'date' OR date range ('startDate' and 'endDate'), not both");
                return result;
            }

            if (hasDate)
            {
                var date = parameters.GetParameter<DateTime?>(Parameters.Date);
                if (!date.HasValue)
                {
                    result.AddError("Invalid date format");
                }
            }

            if (hasDateRange)
            {
                var startDate = parameters.GetParameter<DateTime?>(Parameters.StartDate);
                var endDate = parameters.GetParameter<DateTime?>(Parameters.EndDate);

                if (!startDate.HasValue)
                {
                    result.AddError("Invalid start date format");
                }

                if (!endDate.HasValue)
                {
                    result.AddError("Invalid end date format");
                }

                if (startDate.HasValue && endDate.HasValue)
                {
                    if (startDate.Value.Date > endDate.Value.Date)
                    {
                        result.AddError("Start date must be before or equal to end date");
                    }

                    var daysDifference = (endDate.Value.Date - startDate.Value.Date).Days;
                    if (daysDifference > 90)
                    {
                        result.AddError("Date range cannot exceed 90 days");
                    }
                }
            }

            // Validate optional view type
            var viewType = parameters.GetParameter<string>(Parameters.ViewType) ?? "summary";
            var validViewTypes = new[] { "summary", "detailed", "compact", "statistics" };
            if (!validViewTypes.Contains(viewType.ToLowerInvariant()))
            {
                result.AddError($"Invalid view type '{viewType}'. Valid types are: {string.Join(", ", validViewTypes)}");
            }

            return result;
        }

        protected override CommandValidationResult ValidateSpecific(CommandParameters parameters, SessionContext? session)
        {
            var result = CommandValidationResult.Success();

            if (session == null)
            {
                result.AddError("Must be logged in to view physician schedules");
                return result;
            }

            var physicianId = parameters.GetParameter<Guid?>(Parameters.PhysicianId);

            // Role-based access control
            if (session.UserRole == UserRole.Patient)
            {
                result.AddError("Patients cannot view physician schedules");
            }
            else if (session.UserRole == UserRole.Physician)
            {
                // Physicians can only view their own schedules
                if (physicianId.HasValue && physicianId.Value != session.UserId)
                {
                    result.AddError("Physicians can only view their own schedules");
                }
            }
            // Administrators can view any physician's schedule

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var physicianId = parameters.GetRequiredParameter<Guid>(Parameters.PhysicianId);
                var viewType = parameters.GetParameter<string>(Parameters.ViewType)?.ToLowerInvariant() ?? "summary";

                var physician = _profileRegistry.GetProfileById(physicianId) as PhysicianProfile;
                if (physician == null)
                {
                    return CommandResult.Fail("Physician not found");
                }

                IEnumerable<AppointmentTimeInterval> appointments;
                DateTime startDate, endDate;

                // Determine date range
                if (parameters.HasParameter(Parameters.Date))
                {
                    var date = parameters.GetRequiredParameter<DateTime>(Parameters.Date);
                    startDate = date.Date;
                    endDate = date.Date;
                    appointments = _scheduleManager.GetDailySchedule(physicianId, date);
                }
                else
                {
                    startDate = parameters.GetRequiredParameter<DateTime>(Parameters.StartDate);
                    endDate = parameters.GetRequiredParameter<DateTime>(Parameters.EndDate);
                    appointments = _scheduleManager.GetScheduleInRange(physicianId, startDate, endDate);
                }

                var appointmentsList = appointments.ToList();

                // Generate output based on view type
                string output = viewType switch
                {
                    "detailed" => FormatDetailedSchedule(physician, appointmentsList, startDate, endDate, session),
                    "compact" => FormatCompactSchedule(physician, appointmentsList, startDate, endDate),
                    "statistics" => FormatScheduleStatistics(physician, appointmentsList, startDate, endDate),
                    "summary" or _ => FormatSummarySchedule(physician, appointmentsList, startDate, endDate, session)
                };

                return CommandResult.Ok(output, new { Physician = physician, Appointments = appointmentsList });
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to get physician schedule: {ex.Message}", ex);
            }
        }

        private string FormatSummarySchedule(PhysicianProfile physician, List<AppointmentTimeInterval> appointments, DateTime startDate, DateTime endDate, SessionContext? session)
        {
            var sb = new StringBuilder();
            var isDateRange = startDate.Date != endDate.Date;

            sb.AppendLine($"=== PHYSICIAN SCHEDULE - SUMMARY ===");
            sb.AppendLine($"Physician: Dr. {physician.Name} (ID: {physician.Id:N})");
            sb.AppendLine($"License: {physician.LicenseNumber}");

            if (isDateRange)
            {
                sb.AppendLine($"Date Range: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
            }
            else
            {
                sb.AppendLine($"Date: {startDate:yyyy-MM-dd} ({startDate.DayOfWeek})");
            }

            sb.AppendLine($"Total Appointments: {appointments.Count}");
            sb.AppendLine();

            if (!appointments.Any())
            {
                sb.AppendLine("No appointments scheduled for this period.");
                return sb.ToString();
            }

            // Group appointments by date if date range
            if (isDateRange)
            {
                var appointmentsByDate = appointments.GroupBy(a => a.Start.Date).OrderBy(g => g.Key);

                foreach (var dateGroup in appointmentsByDate)
                {
                    sb.AppendLine($"--- {dateGroup.Key:yyyy-MM-dd} ({dateGroup.Key.DayOfWeek}) ---");

                    foreach (var appointment in dateGroup.OrderBy(a => a.Start))
                    {
                        var patient = _profileRegistry.GetProfileById(appointment.PatientId) as PatientProfile;
                        sb.AppendLine($"{appointment.Start:HH:mm} - {appointment.End:HH:mm} | {patient?.Name ?? "Unknown Patient"} | {appointment.Status}");
                    }

                    sb.AppendLine();
                }
            }
            else
            {
                // Single day view
                foreach (var appointment in appointments.OrderBy(a => a.Start))
                {
                    var patient = _profileRegistry.GetProfileById(appointment.PatientId) as PatientProfile;
                    sb.AppendLine($"{appointment.Start:HH:mm} - {appointment.End:HH:mm} | {patient?.Name ?? "Unknown Patient"} | {appointment.Status}");

                    if (appointment.Status == AppointmentStatus.Cancelled && !string.IsNullOrEmpty(appointment.CancellationReason))
                    {
                        sb.AppendLine($"  Cancellation Reason: {appointment.CancellationReason}");
                    }
                }
            }

            return sb.ToString();
        }

        private string FormatDetailedSchedule(PhysicianProfile physician, List<AppointmentTimeInterval> appointments, DateTime startDate, DateTime endDate, SessionContext? session)
        {
            var sb = new StringBuilder();
            var isDateRange = startDate.Date != endDate.Date;

            sb.AppendLine($"=== PHYSICIAN SCHEDULE - DETAILED ===");
            sb.AppendLine($"Physician: Dr. {physician.Name}");
            sb.AppendLine($"License: {physician.LicenseNumber}");

            if (physician.Specializations.Any())
            {
                sb.AppendLine($"Specializations: {string.Join(", ", physician.Specializations.Select(s => s.ToString()))}");
            }

            if (isDateRange)
            {
                sb.AppendLine($"Date Range: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
            }
            else
            {
                sb.AppendLine($"Date: {startDate:yyyy-MM-dd} ({startDate.DayOfWeek})");
            }

            sb.AppendLine($"Total Appointments: {appointments.Count}");
            sb.AppendLine();

            if (!appointments.Any())
            {
                sb.AppendLine("No appointments scheduled for this period.");
                return sb.ToString();
            }

            foreach (var appointment in appointments.OrderBy(a => a.Start))
            {
                var patient = _profileRegistry.GetProfileById(appointment.PatientId) as PatientProfile;

                sb.AppendLine($"=== APPOINTMENT {appointment.Id:N} ===");
                sb.AppendLine($"Time: {appointment.Start:yyyy-MM-dd HH:mm} - {appointment.End:HH:mm}");
                sb.AppendLine($"Duration: {appointment.Duration.TotalMinutes} minutes");
                sb.AppendLine($"Patient: {patient?.Name ?? "Unknown Patient"}");

                if (session?.UserRole == UserRole.Administrator || session?.UserRole == UserRole.Physician)
                {
                    sb.AppendLine($"Patient ID: {appointment.PatientId:N}");
                    if (patient != null)
                    {
                        var birthDate = patient.GetValue<DateTime>("birthdate");
                        if (birthDate != default)
                        {
                            var age = DateTime.Now.Year - birthDate.Year;
                            if (DateTime.Now.DayOfYear < birthDate.DayOfYear) age--;
                            sb.AppendLine($"Patient Age: {age}");
                        }
                    }
                }

                sb.AppendLine($"Status: {appointment.Status}");
                sb.AppendLine($"Appointment Type: {appointment.AppointmentType}");

                if (!string.IsNullOrEmpty(appointment.ReasonForVisit))
                {
                    sb.AppendLine($"Reason for Visit: {appointment.ReasonForVisit}");
                }

                if (appointment.Status == AppointmentStatus.Cancelled && !string.IsNullOrEmpty(appointment.CancellationReason))
                {
                    sb.AppendLine($"Cancellation Reason: {appointment.CancellationReason}");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string FormatCompactSchedule(PhysicianProfile physician, List<AppointmentTimeInterval> appointments, DateTime startDate, DateTime endDate)
        {
            var sb = new StringBuilder();
            var isDateRange = startDate.Date != endDate.Date;

            sb.AppendLine($"=== SCHEDULE - Dr. {physician.Name} ===");

            if (isDateRange)
            {
                sb.AppendLine($"{startDate:MMM dd} - {endDate:MMM dd, yyyy} | {appointments.Count} appointments");
            }
            else
            {
                sb.AppendLine($"{startDate:MMM dd, yyyy} ({startDate.DayOfWeek}) | {appointments.Count} appointments");
            }

            sb.AppendLine();

            if (!appointments.Any())
            {
                sb.AppendLine("No appointments.");
                return sb.ToString();
            }

            foreach (var appointment in appointments.OrderBy(a => a.Start))
            {
                var patient = _profileRegistry.GetProfileById(appointment.PatientId) as PatientProfile;
                var statusIndicator = appointment.Status switch
                {
                    AppointmentStatus.Scheduled => "●",
                    AppointmentStatus.Completed => "✓",
                    AppointmentStatus.Cancelled => "✗",
                    AppointmentStatus.NoShow => "○",
                    _ => "?"
                };

                sb.AppendLine($"{statusIndicator} {appointment.Start:MM/dd HH:mm} - {patient?.Name ?? "Unknown"} ({appointment.Duration.TotalMinutes}m)");
            }

            return sb.ToString();
        }

        private string FormatScheduleStatistics(PhysicianProfile physician, List<AppointmentTimeInterval> appointments, DateTime startDate, DateTime endDate)
        {
            var sb = new StringBuilder();
            var isDateRange = startDate.Date != endDate.Date;

            sb.AppendLine($"=== SCHEDULE STATISTICS - Dr. {physician.Name} ===");

            if (isDateRange)
            {
                sb.AppendLine($"Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd} ({(endDate.Date - startDate.Date).Days + 1} days)");
            }
            else
            {
                sb.AppendLine($"Date: {startDate:yyyy-MM-dd} ({startDate.DayOfWeek})");
            }

            sb.AppendLine();

            if (!appointments.Any())
            {
                sb.AppendLine("No appointments scheduled for this period.");
                return sb.ToString();
            }

            var totalAppointments = appointments.Count;
            var scheduledCount = appointments.Count(a => a.Status == AppointmentStatus.Scheduled);
            var completedCount = appointments.Count(a => a.Status == AppointmentStatus.Completed);
            var cancelledCount = appointments.Count(a => a.Status == AppointmentStatus.Cancelled);
            var noShowCount = appointments.Count(a => a.Status == AppointmentStatus.NoShow);

            var totalScheduledMinutes = appointments.Where(a => a.Status != AppointmentStatus.Cancelled).Sum(a => a.Duration.TotalMinutes);
            var avgAppointmentLength = appointments.Any() ? appointments.Average(a => a.Duration.TotalMinutes) : 0;

            sb.AppendLine($"Total Appointments: {totalAppointments}");
            sb.AppendLine($"  Scheduled: {scheduledCount}");
            sb.AppendLine($"  Completed: {completedCount}");
            sb.AppendLine($"  Cancelled: {cancelledCount}");
            sb.AppendLine($"  No-Shows: {noShowCount}");
            sb.AppendLine();

            if (totalAppointments > 0)
            {
                sb.AppendLine($"Completion Rate: {(double)completedCount / totalAppointments * 100:F1}%");
                sb.AppendLine($"Cancellation Rate: {(double)cancelledCount / totalAppointments * 100:F1}%");
                sb.AppendLine($"No-Show Rate: {(double)noShowCount / totalAppointments * 100:F1}%");
            }

            sb.AppendLine();
            sb.AppendLine($"Total Scheduled Time: {totalScheduledMinutes / 60:F1} hours");
            sb.AppendLine($"Average Appointment Length: {avgAppointmentLength:F0} minutes");

            if (isDateRange && appointments.Any())
            {
                var appointmentsByDay = appointments.GroupBy(a => a.Start.Date).OrderBy(g => g.Key);
                sb.AppendLine();
                sb.AppendLine("Daily Breakdown:");

                foreach (var dayGroup in appointmentsByDay)
                {
                    var dayCount = dayGroup.Count();
                    var dayHours = dayGroup.Sum(a => a.Duration.TotalMinutes) / 60;
                    sb.AppendLine($"  {dayGroup.Key:MM/dd} ({dayGroup.Key.DayOfWeek}): {dayCount} appointments, {dayHours:F1} hours");
                }
            }

            return sb.ToString();
        }
    }
}
