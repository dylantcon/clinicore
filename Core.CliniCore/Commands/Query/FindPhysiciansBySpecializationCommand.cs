using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
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
    public class FindPhysiciansBySpecializationCommand : AbstractCommand
    {
        public const string Key = "findphysiciansbyspecialization";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string Specialization = "specialization";
            public const string IncludeAvailability = "includeAvailability";
        }

        private readonly ProfileService _profileRegistry;

        public FindPhysiciansBySpecializationCommand(ProfileService profileService)
        {
            _profileRegistry = profileService ?? throw new ArgumentNullException(nameof(profileService));
        }

        public override string Description => "Find physicians by their medical specialization";

        public override Permission? GetRequiredPermission()
            => Permission.ViewAllPatients; // Using existing permission as there's no specific "ViewPhysicians" permission

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            var missingParams = parameters.GetMissingRequired(Parameters.Specialization);
            if (missingParams.Count != 0)
            {
                foreach (var error in missingParams)
                    result.AddError(error);
                return result;
            }

            var specializationInput = parameters.GetParameter<string>(Parameters.Specialization);
            if (string.IsNullOrWhiteSpace(specializationInput))
            {
                result.AddError("Specialization cannot be empty");
                return result;
            }

            // Try to parse the specialization
            if (!TryParseSpecialization(specializationInput, out var _))
            {
                var validSpecializations = Enum.GetValues<MedicalSpecialization>()
                    .Select(s => s.GetDisplayName())
                    .OrderBy(s => s);

                result.AddError($"Invalid specialization '{specializationInput}'. Valid specializations are: {string.Join(", ", validSpecializations)}");
            }

            return result;
        }

        protected override CommandValidationResult ValidateSpecific(CommandParameters parameters, SessionContext? session)
        {
            var result = CommandValidationResult.Success();

            if (session == null)
            {
                result.AddError("Must be logged in to search physicians");
                return result;
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var specializationInput = parameters.GetRequiredParameter<string>(Parameters.Specialization);
                var includeAvailability = parameters.GetParameter<bool>(Parameters.IncludeAvailability);

                // Parse the specialization
                if (!TryParseSpecialization(specializationInput, out var targetSpecialization))
                {
                    return CommandResult.Fail($"Invalid specialization: {specializationInput}");
                }

                // Find physicians with the specified specialization
                var allPhysicians = _profileRegistry.GetAllPhysicians();
                var matchingPhysicians = allPhysicians
                    .Where(p => (p.GetValue<List<MedicalSpecialization>>(PhysicianEntryType.Specializations.GetKey()) ?? new List<MedicalSpecialization>()).Contains(targetSpecialization))
                    .OrderBy(p => p.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty)
                    .ToList();

                if (!matchingPhysicians.Any())
                {
                    return CommandResult.Ok($"No physicians found with specialization '{targetSpecialization.GetDisplayName()}'");
                }

                var output = FormatSearchResults(matchingPhysicians, targetSpecialization, includeAvailability, session);
                return CommandResult.Ok(output, matchingPhysicians);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to search physicians by specialization: {ex.Message}", ex);
            }
        }

        private bool TryParseSpecialization(string input, out MedicalSpecialization specialization)
        {
            specialization = default;

            // Try direct enum parsing first
            if (Enum.TryParse<MedicalSpecialization>(input, true, out specialization))
            {
                return true;
            }

            // Try by display name
            var allSpecializations = Enum.GetValues<MedicalSpecialization>();
            foreach (var spec in allSpecializations)
            {
                if (string.Equals(spec.GetDisplayName(), input, StringComparison.OrdinalIgnoreCase))
                {
                    specialization = spec;
                    return true;
                }
            }

            // Try partial matching
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

        private string FormatSearchResults(List<PhysicianProfile> physicians, MedicalSpecialization specialization, bool includeAvailability, SessionContext? session)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== PHYSICIANS BY SPECIALIZATION ===");
            sb.AppendLine($"Specialization: {specialization.GetDisplayName()}");
            sb.AppendLine($"Found {physicians.Count} physician(s)");
            sb.AppendLine();

            foreach (var physician in physicians)
            {
                var name = physician.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty;
                var licenseNumber = physician.GetValue<string>(PhysicianEntryType.LicenseNumber.GetKey()) ?? string.Empty;
                var graduationDate = physician.GetValue<DateTime>(PhysicianEntryType.GraduationDate.GetKey());
                var specializations = physician.GetValue<List<MedicalSpecialization>>(PhysicianEntryType.Specializations.GetKey()) ?? new List<MedicalSpecialization>();

                sb.AppendLine($"Physician ID: {physician.Id:N}");
                sb.AppendLine($"Name: Dr. {name}");
                sb.AppendLine($"Username: {physician.Username}");
                sb.AppendLine($"License Number: {licenseNumber}");
                sb.AppendLine($"Graduation Date: {graduationDate:yyyy-MM-dd}");

                // Show all specializations
                if (specializations.Any())
                {
                    var specializationNames = specializations.Select(s => s.GetDisplayName());
                    sb.AppendLine($"All Specializations: {string.Join(", ", specializationNames)}");
                }

                // Show patient count
                sb.AppendLine($"Patients Under Care: {physician.PatientIds.Count}");

                // Show availability information if requested and user has appropriate permissions
                if (includeAvailability && (session?.UserRole == UserRole.Administrator || session?.UserRole == UserRole.Physician))
                {
                    if (physician.StandardAvailability.Any())
                    {
                        sb.AppendLine("Standard Availability:");
                        foreach (var availability in physician.StandardAvailability.OrderBy(kvp => (int)kvp.Key))
                        {
                            sb.AppendLine($"  {availability.Key}: Available");
                        }
                    }
                    else
                    {
                        sb.AppendLine("Standard Availability: Not set");
                    }
                }

                // Show additional details for administrators
                if (session?.UserRole == UserRole.Administrator)
                {
                    sb.AppendLine($"Scheduled Appointments: {physician.AppointmentIds.Count}");

                    if (physician.PatientIds.Any())
                    {
                        var patientNames = physician.PatientIds
                            .Take(5) // Show first 5 patients
                            .Select(id => _profileRegistry.GetProfileById(id) as PatientProfile)
                            .Where(p => p != null)
                            .Select(p => p!.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty);

                        sb.AppendLine($"Sample Patients: {string.Join(", ", patientNames)}" +
                            (physician.PatientIds.Count > 5 ? $" and {physician.PatientIds.Count - 5} more" : ""));
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
