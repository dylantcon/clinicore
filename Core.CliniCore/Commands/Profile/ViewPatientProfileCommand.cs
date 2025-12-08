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
    /// Command that retrieves and displays detailed information for a specific patient profile.
    /// Includes demographics, primary physician assignment, and appointment/document counts.
    /// </summary>
    public class ViewPatientProfileCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "viewpatientprofile";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="ViewPatientProfileCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the unique identifier of the patient profile to view.
            /// </summary>
            public const string ProfileId = "profile_id";

            /// <summary>
            /// Parameter key indicating whether to include extended details like appointment
            /// and clinical document counts, and validation errors if present.
            /// </summary>
            public const string ShowDetails = "show_details";
        }

        private readonly ProfileService _registry;

        public ViewPatientProfileCommand(ProfileService profileService)
        {
            _registry = profileService ?? throw new ArgumentNullException(nameof(profileService));
        }

        public override string Description => "Views detailed information for a specific patient profile";

        public override bool CanUndo => false;

        public override Permission? GetRequiredPermission()
            => Permission.ViewPatientProfile;

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

            // Check if profile exists and is a patient
            var profile = _registry.GetProfileById(profileId);
            if (profile == null)
            {
                result.AddError($"Profile with ID {profileId} not found in the registry.");
                return result;
            }

            if (profile.Role != UserRole.Patient)
            {
                result.AddError($"Selected profile is not a patient. Please select a valid patient profile.");
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

                var profile = _registry.GetProfileById(profileId) as PatientProfile;
                if (profile == null)
                {
                    return CommandResult.Fail($"Patient profile with ID {profileId} not found.");
                }

                var sb = new StringBuilder();
                sb.AppendLine("=== PATIENT PROFILE ===");
                sb.AppendLine($"ID: {profile.Id:N}");
                sb.AppendLine($"Username: {profile.Username}");
                sb.AppendLine($"Name: {profile.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty}");
                sb.AppendLine($"Gender: {profile.GetValue<Gender>(PatientEntryType.Gender.GetKey()).GetDisplayName()}");
                var birthDate = profile.GetValue<DateTime>(CommonEntryType.BirthDate.GetKey());
                sb.AppendLine($"Birth Date: {birthDate:yyyy-MM-dd} (Age: {DateTime.Now.Year - birthDate.Year})");
                sb.AppendLine($"Race: {profile.GetValue<string>(PatientEntryType.Race.GetKey()) ?? string.Empty}");
                sb.AppendLine($"Address: {profile.GetValue<string>(CommonEntryType.Address.GetKey()) ?? string.Empty}");
                sb.AppendLine($"Valid Profile: {(profile.IsValid ? "Yes" : "No")}");

                // Primary physician info
                if (profile.PrimaryPhysicianId.HasValue)
                {
                    var primaryPhysician = _registry.GetProfileById(profile.PrimaryPhysicianId.Value) as PhysicianProfile;
                    sb.AppendLine($"Primary Physician: {(primaryPhysician != null ? $"Dr. {primaryPhysician.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty}" : "Not found")}");
                }
                else
                {
                    sb.AppendLine("Primary Physician: None assigned");
                }

                if (showDetails)
                {
                    sb.AppendLine($"Appointment Count: {profile.AppointmentIds.Count}");
                    sb.AppendLine($"Clinical Document Count: {profile.ClinicalDocumentIds.Count}");

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

                return CommandResult.Ok(sb.ToString().TrimEnd(), profile);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to view patient profile: {ex.Message}", ex);
            }
        }
    }
}