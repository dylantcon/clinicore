using System;
using System.Linq;
using System.Text;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Services;

namespace Core.CliniCore.Commands.Profile
{
    public class ViewPatientProfileCommand : AbstractCommand
    {
        public const string Key = "viewpatientprofile";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string ProfileId = "profile_id";
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
                sb.AppendLine($"Name: {profile.Name}");
                sb.AppendLine($"Gender: {profile.Gender.GetDisplayName()}");
                sb.AppendLine($"Birth Date: {profile.BirthDate:yyyy-MM-dd} (Age: {DateTime.Now.Year - profile.BirthDate.Year})");
                sb.AppendLine($"Race: {profile.Race}");
                sb.AppendLine($"Address: {profile.Address}");
                sb.AppendLine($"Valid Profile: {(profile.IsValid ? "Yes" : "No")}");

                // Primary physician info
                if (profile.PrimaryPhysicianId.HasValue)
                {
                    var primaryPhysician = _registry.GetProfileById(profile.PrimaryPhysicianId.Value) as PhysicianProfile;
                    sb.AppendLine($"Primary Physician: {(primaryPhysician != null ? $"Dr. {primaryPhysician.Name}" : "Not found")}");
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