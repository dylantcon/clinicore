using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.CliniCore.Service;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Users.Concrete;

namespace Core.CliniCore.Commands.Query
{
    /// <summary>
    /// Command that finds physicians available for appointments within a specified time window.
    /// Supports filtering by medical specialization and offers flexible time specification
    /// via either explicit start/end times or date with duration.
    /// </summary>
    public class FindPhysiciansByAvailabilityCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "findphysiciansbyavailability";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="FindPhysiciansByAvailabilityCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the appointment start time (use with EndTime).
            /// </summary>
            public const string StartTime = "startTime";

            /// <summary>
            /// Parameter key for the appointment end time (use with StartTime).
            /// </summary>
            public const string EndTime = "endTime";

            /// <summary>
            /// Parameter key for the appointment duration in minutes (use with Date).
            /// </summary>
            public const string Duration = "duration";

            /// <summary>
            /// Parameter key for filtering by medical specialization.
            /// </summary>
            public const string Specialization = "specialization";

            /// <summary>
            /// Parameter key for the target date (use with Duration).
            /// </summary>
            public const string Date = "date";
        }

        private readonly ProfileService _profileRegistry;
        private readonly SchedulerService _scheduleManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="FindPhysiciansByAvailabilityCommand"/> class.
        /// </summary>
        /// <param name="profileService">The profile service for accessing physician profiles.</param>
        /// <param name="schedulerService">The scheduler service for checking availability.</param>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is <see langword="null"/>.</exception>
        public FindPhysiciansByAvailabilityCommand(ProfileService profileService, SchedulerService schedulerService)
        {
            _profileRegistry = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _scheduleManager = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
        }

        /// <inheritdoc />
        public override string Description => "Find available physicians for a specific time slot";

        /// <inheritdoc />
        public override Permission? GetRequiredPermission()
            => Permission.ViewAllAppointments;

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Require either (startTime and endTime) or (date and duration)
            var hasStartEnd = parameters.HasParameter(Parameters.StartTime) && parameters.HasParameter(Parameters.EndTime);
            var hasDateDuration = parameters.HasParameter(Parameters.Date) && parameters.HasParameter(Parameters.Duration);

            if (!hasStartEnd && !hasDateDuration)
            {
                result.AddError("Must provide either (startTime and endTime) or (date and duration) parameters");
                return result;
            }

            if (hasStartEnd && hasDateDuration)
            {
                result.AddError("Provide either (startTime and endTime) OR (date and duration), not both");
                return result;
            }

            if (hasStartEnd)
            {
                var startTime = parameters.GetParameter<DateTime?>(Parameters.StartTime);
                var endTime = parameters.GetParameter<DateTime?>(Parameters.EndTime);

                if (!startTime.HasValue)
                {
                    result.AddError("Invalid start time format");
                }

                if (!endTime.HasValue)
                {
                    result.AddError("Invalid end time format");
                }

                if (startTime.HasValue && endTime.HasValue)
                {
                    if (startTime.Value >= endTime.Value)
                    {
                        result.AddError("Start time must be before end time");
                    }

                    if (startTime.Value < DateTime.Now)
                    {
                        result.AddError("Start time cannot be in the past");
                    }

                    var duration = endTime.Value - startTime.Value;
                    if (duration.TotalMinutes < 15)
                    {
                        result.AddError("Appointment duration must be at least 15 minutes");
                    }

                    if (duration.TotalHours > 8)
                    {
                        result.AddError("Appointment duration cannot exceed 8 hours");
                    }
                }
            }

            if (hasDateDuration)
            {
                var date = parameters.GetParameter<DateTime?>(Parameters.Date);
                var durationMinutes = parameters.GetParameter<int?>(Parameters.Duration);

                if (!date.HasValue)
                {
                    result.AddError("Invalid date format");
                }

                if (!durationMinutes.HasValue || durationMinutes.Value <= 0)
                {
                    result.AddError("Duration must be a positive number of minutes");
                }

                if (date.HasValue && date.Value.Date < DateTime.Today)
                {
                    result.AddError("Date cannot be in the past");
                }

                if (durationMinutes.HasValue)
                {
                    if (durationMinutes.Value < 15)
                    {
                        result.AddError("Appointment duration must be at least 15 minutes");
                    }

                    if (durationMinutes.Value > 480) // 8 hours
                    {
                        result.AddError("Appointment duration cannot exceed 8 hours");
                    }
                }
            }

            // Validate optional specialization filter
            var specializationInput = parameters.GetParameter<string>(Parameters.Specialization);
            if (!string.IsNullOrEmpty(specializationInput))
            {
                if (!TryParseSpecialization(specializationInput, out var _))
                {
                    var validSpecializations = Enum.GetValues<MedicalSpecialization>()
                        .Select(s => s.GetDisplayName())
                        .OrderBy(s => s);

                    result.AddError($"Invalid specialization '{specializationInput}'. Valid specializations are: {string.Join(", ", validSpecializations)}");
                }
            }

            return result;
        }

        protected override CommandValidationResult ValidateSpecific(CommandParameters parameters, SessionContext? session)
        {
            var result = CommandValidationResult.Success();

            if (session == null)
            {
                result.AddError("Must be logged in to find available physicians");
                return result;
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                DateTime startTime, endTime;
                TimeSpan duration;

                // Parse time parameters
                if (parameters.HasParameter(Parameters.StartTime))
                {
                    startTime = parameters.GetRequiredParameter<DateTime>(Parameters.StartTime);
                    endTime = parameters.GetRequiredParameter<DateTime>(Parameters.EndTime);
                    duration = endTime - startTime;
                }
                else
                {
                    var date = parameters.GetRequiredParameter<DateTime>(Parameters.Date);
                    var durationMinutes = parameters.GetRequiredParameter<int>(Parameters.Duration);
                    duration = TimeSpan.FromMinutes(durationMinutes);

                    // For date-only search, we'll look for any available slot on that date
                    startTime = date.Date.AddHours(8); // Start search at 8 AM
                    endTime = date.Date.AddHours(17);   // End search at 5 PM
                }

                var specializationFilter = parameters.GetParameter<string>(Parameters.Specialization);
                MedicalSpecialization? targetSpecialization = null;

                if (!string.IsNullOrEmpty(specializationFilter))
                {
                    if (TryParseSpecialization(specializationFilter, out var spec))
                    {
                        targetSpecialization = spec;
                    }
                }

                // Get all physicians
                var allPhysicians = _profileRegistry.GetAllPhysicians();

                // Apply specialization filter if specified
                if (targetSpecialization.HasValue)
                {
                    allPhysicians = allPhysicians.Where(p =>
                    {
                        var specializations = p.GetValue<List<MedicalSpecialization>>(PhysicianEntryType.Specializations.GetKey()) ?? new List<MedicalSpecialization>();
                        return specializations.Contains(targetSpecialization.Value);
                    });
                }

                var availablePhysicians = new List<PhysicianAvailabilityInfo>();

                foreach (var physician in allPhysicians)
                {
                    // Find available slots for this physician
                    var availableSlot = _scheduleManager.FindNextAvailableSlot(
                        physician.Id,
                        duration,
                        startTime
                    );

                    // Check if the slot is within our search window
                    if (availableSlot != null && availableSlot.Start <= endTime.Subtract(duration))
                    {
                        availablePhysicians.Add(new PhysicianAvailabilityInfo
                        {
                            Physician = physician,
                            NextAvailableSlot = availableSlot,
                            MatchesTimeSlot = availableSlot.Start >= startTime && availableSlot.End <= endTime
                        });
                    }
                }

                if (!availablePhysicians.Any())
                {
                    var searchDescription = parameters.HasParameter(Parameters.StartTime)
                        ? $"between {startTime:yyyy-MM-dd HH:mm} and {endTime:yyyy-MM-dd HH:mm}"
                        : $"on {startTime:yyyy-MM-dd} for {duration.TotalMinutes} minutes";

                    var specializationText = targetSpecialization.HasValue
                        ? $" with specialization '{targetSpecialization.Value.GetDisplayName()}'"
                        : "";

                    return CommandResult.Ok($"No physicians available {searchDescription}{specializationText}");
                }

                var output = FormatAvailabilityResults(availablePhysicians, startTime, endTime, duration, targetSpecialization, session);
                return CommandResult.Ok(output, availablePhysicians.Select(a => a.Physician).ToList());
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to find available physicians: {ex.Message}", ex);
            }
        }

        private bool TryParseSpecialization(string input, out MedicalSpecialization specialization)
        {
            specialization = default;

            if (Enum.TryParse<MedicalSpecialization>(input, true, out specialization))
            {
                return true;
            }

            var allSpecializations = Enum.GetValues<MedicalSpecialization>();
            foreach (var spec in allSpecializations)
            {
                if (string.Equals(spec.GetDisplayName(), input, StringComparison.OrdinalIgnoreCase))
                {
                    specialization = spec;
                    return true;
                }
            }

            var partialMatches = allSpecializations
                .Where(s => s.GetDisplayName().Contains(input, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (partialMatches.Count == 1)
            {
                specialization = partialMatches[0];
                return true;
            }

            return false;
        }

        private string FormatAvailabilityResults(List<PhysicianAvailabilityInfo> availablePhysicians, DateTime startTime, DateTime endTime, TimeSpan duration, MedicalSpecialization? specialization, SessionContext? session)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== PHYSICIAN AVAILABILITY SEARCH ===");

            if (specialization.HasValue)
            {
                sb.AppendLine($"Specialization: {specialization.Value.GetDisplayName()}");
            }

            sb.AppendLine($"Time Period: {startTime:yyyy-MM-dd HH:mm} - {endTime:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"Requested Duration: {duration.TotalMinutes} minutes");
            sb.AppendLine($"Found {availablePhysicians.Count} available physician(s)");
            sb.AppendLine();

            // Sort by availability - exact matches first, then by earliest available time
            var sortedPhysicians = availablePhysicians
                .OrderByDescending(p => p.MatchesTimeSlot)
                .ThenBy(p => p.NextAvailableSlot?.Start)
                .ToList();

            foreach (var info in sortedPhysicians)
            {
                var physician = info.Physician;
                var slot = info.NextAvailableSlot;

                sb.AppendLine($"Physician ID: {physician.Id:N}");
                sb.AppendLine($"Name: Dr. {physician.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty}");
                sb.AppendLine($"License: {physician.GetValue<string>(PhysicianEntryType.LicenseNumber.GetKey()) ?? string.Empty}");

                var specializations = physician.GetValue<List<MedicalSpecialization>>(PhysicianEntryType.Specializations.GetKey()) ?? new List<MedicalSpecialization>();
                if (specializations.Any())
                {
                    var specializationNames = specializations.Select(s => s.GetDisplayName());
                    sb.AppendLine($"Specializations: {string.Join(", ", specializationNames)}");
                }

                if (slot != null)
                {
                    sb.AppendLine($"Available Slot: {slot.Start:yyyy-MM-dd HH:mm} - {slot.End:yyyy-MM-dd HH:mm}");

                    if (info.MatchesTimeSlot)
                    {
                        sb.AppendLine("✓ Matches requested time slot");
                    }
                    else
                    {
                        sb.AppendLine("⚠ Alternative time slot (requested time not available)");
                    }
                }

                sb.AppendLine($"Current Patients: {physician.PatientIds.Count}");

                // Show additional details for administrators
                if (session?.UserRole == UserRole.Administrator)
                {
                    sb.AppendLine($"Scheduled Appointments: {physician.AppointmentIds.Count}");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        private class PhysicianAvailabilityInfo
        {
            public PhysicianProfile Physician { get; set; } = null!;
            public AppointmentSlot? NextAvailableSlot { get; set; }
            public bool MatchesTimeSlot { get; set; }
        }
    }
}
