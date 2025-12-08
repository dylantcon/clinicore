using System;
using System.Linq;
using System.Text;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.Service;

namespace Core.CliniCore.Commands.Profile
{
    /// <summary>
    /// Command that retrieves and displays detailed information for a specific physician profile.
    /// Includes credentials, specializations, patient roster, and appointment statistics.
    /// </summary>
    public class ViewPhysicianProfileCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "viewphysicianprofile";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="ViewPhysicianProfileCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the unique identifier of the physician profile to view.
            /// </summary>
            public const string ProfileId = "profile_id";

            /// <summary>
            /// Parameter key indicating whether to include extended details like patient
            /// and appointment counts, and validation errors if present.
            /// </summary>
            public const string ShowDetails = "show_details";
        }

        /// <summary>
        /// Defines the result data keys returned by this command.
        /// </summary>
        public static class Results
        {
            /// <summary>
            /// Result key for the number of patients assigned to this physician.
            /// </summary>
            public const string PatientCount = "patient_count";

            /// <summary>
            /// Result key for the total number of appointments for this physician.
            /// </summary>
            public const string AppointmentCount = "appointment_count";
        }

        private readonly ProfileService _registry;
        private readonly SchedulerService _schedulerService;

        public ViewPhysicianProfileCommand(
            ProfileService profileService,
            SchedulerService schedulerService)
        {
            _registry = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
        }

        public override string Description => "Views detailed information for a specific physician profile";

        public override bool CanUndo => false;

        public override Permission? GetRequiredPermission()
            => Permission.ViewPhysicianProfile;

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Check required parameters exist
            var missingParams = parameters.GetMissingRequired(Parameters.ProfileId);

            if (missingParams.Any())
            {
                foreach (var error in missingParams)
                    result.AddError(error);
                return result;
            }

            // Validate profile_id is a valid GUID
            var profileId = parameters.GetParameter<Guid>(Parameters.ProfileId);
            if (profileId == Guid.Empty)
            {
                result.AddError($"Invalid profile ID format: '{profileId}'. Expected a valid GUID.");
                return result;
            }

            // Check if profile exists and is a physician
            var profile = _registry.GetProfileById(profileId);
            if (profile == null)
            {
                result.AddError($"Profile with ID {profileId} not found in the registry.");
                return result;
            }

            if (profile.Role != UserRole.Physician)
            {
                result.AddError($"Selected profile is not a physician. Please select a valid physician profile.");
                return result;
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var profileId = parameters.GetRequiredParameter<Guid>(Parameters.ProfileId);
                var showDetails = parameters.GetParameter<bool?>(Parameters.ShowDetails) ?? false;

                var profile = _registry.GetProfileById(profileId) as PhysicianProfile;
                if (profile == null)
                {
                    return CommandResult.Fail($"Physician profile with ID {profileId} not found.");
                }

                var sb = new StringBuilder();
                sb.AppendLine("=== PHYSICIAN PROFILE ===");
                sb.AppendLine($"ID: {profile.Id:N}");
                sb.AppendLine($"Username: {profile.Username}");
                sb.AppendLine($"Name: Dr. {profile.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty}");
                sb.AppendLine($"License Number: {profile.GetValue<string>(PhysicianEntryType.LicenseNumber.GetKey()) ?? string.Empty}");
                sb.AppendLine($"Graduation Date: {profile.GetValue<DateTime>(PhysicianEntryType.GraduationDate.GetKey()):yyyy-MM-dd}");
                sb.AppendLine($"Valid Profile: {(profile.IsValid ? "Yes" : "No")}");

                // Specializations
                var specializations = profile.GetValue<List<MedicalSpecialization>>(PhysicianEntryType.Specializations.GetKey()) ?? new();
                if (specializations.Any())
                {
                    sb.AppendLine($"Specializations: {string.Join(", ", specializations.Select(s => s.GetDisplayName()))}");
                }
                else
                {
                    sb.AppendLine("Specializations: None listed");
                }

                // Get actual counts from services
                var patientCount = _registry.GetPhysicianPatients(profileId).Count();
                var appointmentCount = _schedulerService.GetPhysicianSchedule(profileId).Appointments.Count();

                if (showDetails)
                {
                    sb.AppendLine($"Patient Count: {patientCount}");
                    sb.AppendLine($"Appointment Count: {appointmentCount}");

                    // Show validation errors if any
                    if (!profile.IsValid)
                    {
                        var errors = profile.GetValidationErrors();
                        sb.AppendLine("Validation Errors:");
                        foreach (var error in errors)
                        {
                            sb.AppendLine($"  - {error}");
                        }
                    }
                }

                var result = CommandResult.Ok(sb.ToString().TrimEnd(), profile);
                result.SetData(Results.PatientCount, patientCount);
                result.SetData(Results.AppointmentCount, appointmentCount);
                return result;
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to view physician profile: {ex.Message}", ex);
            }
        }
    }
}