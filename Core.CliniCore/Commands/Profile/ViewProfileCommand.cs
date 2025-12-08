using System;
using System.Linq;
using System.Text;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.Service;

namespace Core.CliniCore.Commands.Profile
{
    public class ViewProfileCommand : AbstractCommand
    {
        public const string Key = "viewprofile";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string ProfileId = "profile_id";
            public const string ShowDetails = "show_details";
        }

        private readonly ProfileService _registry;

        public ViewProfileCommand(ProfileService profileService)
        {
            _registry = profileService ?? throw new ArgumentNullException(nameof(profileService));
        }

        public override string Description => "Views detailed information for a specific patient or physician profile";

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

            // Check if profile exists
            var profile = _registry.GetProfileById(profileId);
            if (profile == null)
            {
                result.AddError($"Profile with ID {profileId} not found in the registry.");
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

                var profile = _registry.GetProfileById(profileId);
                if (profile == null)
                {
                    return CommandResult.Fail($"Profile with ID {profileId} not found.");
                }

                // Build the profile display based on type
                var sb = new StringBuilder();
                
                if (profile is PatientProfile patient)
                {
                    sb.AppendLine(FormatPatientProfile(patient, showDetails));
                }
                else if (profile is PhysicianProfile physician)
                {
                    sb.AppendLine(FormatPhysicianProfile(physician, showDetails));
                }
                else if (profile is AdministratorProfile admin)
                {
                    sb.AppendLine(FormatAdministratorProfile(admin, showDetails));
                }
                else
                {
                    sb.AppendLine(FormatGenericProfile(profile, showDetails));
                }

                return CommandResult.Ok(sb.ToString().TrimEnd(), profile);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to view profile: {ex.Message}", ex);
            }
        }

        private string FormatPatientProfile(PatientProfile patient, bool showDetails)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== PATIENT PROFILE ===");
            sb.AppendLine($"ID: {patient.Id:N}");
            sb.AppendLine($"Username: {patient.Username}");
            sb.AppendLine($"Name: {patient.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty}");
            sb.AppendLine($"Gender: {patient.GetValue<Gender>(PatientEntryType.Gender.GetKey()).GetDisplayName()}");
            var birthDate = patient.GetValue<DateTime>(CommonEntryType.BirthDate.GetKey());
            sb.AppendLine($"Birth Date: {birthDate:yyyy-MM-dd} (Age: {DateTime.Now.Year - birthDate.Year})");
            sb.AppendLine($"Race: {patient.GetValue<string>(PatientEntryType.Race.GetKey()) ?? string.Empty}");
            sb.AppendLine($"Address: {patient.GetValue<string>(CommonEntryType.Address.GetKey()) ?? string.Empty}");
            sb.AppendLine($"Valid Profile: {(patient.IsValid ? "Yes" : "No")}");

            // Primary physician info
            if (patient.PrimaryPhysicianId.HasValue)
            {
                var primaryPhysician = _registry.GetProfileById(patient.PrimaryPhysicianId.Value) as PhysicianProfile;
                sb.AppendLine($"Primary Physician: {(primaryPhysician != null ? $"Dr. {primaryPhysician.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty}" : "Not found")}");
            }
            else
            {
                sb.AppendLine("Primary Physician: None assigned");
            }

            if (showDetails)
            {
                sb.AppendLine($"Appointment Count: {patient.AppointmentIds.Count}");
                sb.AppendLine($"Clinical Document Count: {patient.ClinicalDocumentIds.Count}");
                
                // Show validation errors if any
                if (!patient.IsValid)
                {
                    var errors = patient.GetValidationErrors();
                    sb.AppendLine("Validation Errors:");
                    foreach (var error in errors)
                    {
                        sb.AppendLine($"  - {error}");
                    }
                }
            }

            return sb.ToString();
        }

        private string FormatPhysicianProfile(PhysicianProfile physician, bool showDetails)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== PHYSICIAN PROFILE ===");
            sb.AppendLine($"ID: {physician.Id:N}");
            sb.AppendLine($"Username: {physician.Username}");
            sb.AppendLine($"Name: Dr. {physician.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty}");
            sb.AppendLine($"License Number: {physician.GetValue<string>(PhysicianEntryType.LicenseNumber.GetKey()) ?? string.Empty}");
            sb.AppendLine($"Graduation Date: {physician.GetValue<DateTime>(PhysicianEntryType.GraduationDate.GetKey()):yyyy-MM-dd}");
            sb.AppendLine($"Valid Profile: {(physician.IsValid ? "Yes" : "No")}");

            // Specializations
            var specializations = physician.GetValue<List<MedicalSpecialization>>(PhysicianEntryType.Specializations.GetKey()) ?? new();
            if (specializations.Any())
            {
                sb.AppendLine($"Specializations: {string.Join(", ", specializations.Select(s => s.GetDisplayName()))}");
            }
            else
            {
                sb.AppendLine("Specializations: None listed");
            }

            if (showDetails)
            {
                sb.AppendLine($"Patient Count: {physician.PatientIds.Count}");
                sb.AppendLine($"Appointment Count: {physician.AppointmentIds.Count}");
                
                // Show validation errors if any
                if (!physician.IsValid)
                {
                    var errors = physician.GetValidationErrors();
                    sb.AppendLine("Validation Errors:");
                    foreach (var error in errors)
                    {
                        sb.AppendLine($"  - {error}");
                    }
                }
            }

            return sb.ToString();
        }

        private string FormatAdministratorProfile(AdministratorProfile admin, bool showDetails)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== ADMINISTRATOR PROFILE ===");
            sb.AppendLine($"ID: {admin.Id:N}");
            sb.AppendLine($"Username: {admin.Username}");
            sb.AppendLine($"Role: {admin.Role}");
            sb.AppendLine($"Valid Profile: {(admin.IsValid ? "Yes" : "No")}");

            if (showDetails && !admin.IsValid)
            {
                var errors = admin.GetValidationErrors();
                sb.AppendLine("Validation Errors:");
                foreach (var error in errors)
                {
                    sb.AppendLine($"  - {error}");
                }
            }

            return sb.ToString();
        }

        private string FormatGenericProfile(IUserProfile profile, bool showDetails)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== USER PROFILE ===");
            sb.AppendLine($"ID: {profile.Id:N}");
            sb.AppendLine($"Username: {profile.Username}");
            sb.AppendLine($"Role: {profile.Role}");
            sb.AppendLine($"Valid Profile: {(profile.IsValid ? "Yes" : "No")}");

            if (showDetails && !profile.IsValid)
            {
                var errors = profile.GetValidationErrors();
                sb.AppendLine("Validation Errors:");
                foreach (var error in errors)
                {
                    sb.AppendLine($"  - {error}");
                }
            }

            return sb.ToString();
        }
    }
}