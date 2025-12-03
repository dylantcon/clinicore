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
    public class ViewPhysicianProfileCommand : AbstractCommand
    {
        public const string Key = "viewphysicianprofile";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string ProfileId = "profile_id";
            public const string ShowDetails = "show_details";
        }

        private readonly ProfileService _registry;

        public ViewPhysicianProfileCommand(ProfileService profileService)
        {
            _registry = profileService ?? throw new ArgumentNullException(nameof(profileService));
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
                sb.AppendLine($"Name: Dr. {profile.Name}");
                sb.AppendLine($"License Number: {profile.LicenseNumber}");
                sb.AppendLine($"Graduation Date: {profile.GraduationDate:yyyy-MM-dd}");
                sb.AppendLine($"Valid Profile: {(profile.IsValid ? "Yes" : "No")}");

                // Specializations
                if (profile.Specializations.Any())
                {
                    sb.AppendLine($"Specializations: {string.Join(", ", profile.Specializations.Select(s => s.GetDisplayName()))}");
                }
                else
                {
                    sb.AppendLine("Specializations: None listed");
                }

                if (showDetails)
                {
                    sb.AppendLine($"Patient Count: {profile.PatientIds.Count}");
                    sb.AppendLine($"Appointment Count: {profile.AppointmentIds.Count}");

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
                return CommandResult.Fail($"Failed to view physician profile: {ex.Message}", ex);
            }
        }
    }
}