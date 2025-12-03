using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.CliniCore.Services;

namespace Core.CliniCore.Commands.Query
{
    public class SearchPatientsCommand : AbstractCommand
    {
        public const string Key = "searchpatients";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string SearchTerm = "searchTerm";
        }

        private readonly ProfileService _profileRegistry;

        public SearchPatientsCommand(ProfileService profileService)
        {
            _profileRegistry = profileService ?? throw new ArgumentNullException(nameof(profileService));
        }

        public override string Description => "Search patients by name";

        public override Permission? GetRequiredPermission()
            => Permission.ViewAllPatients;

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            var missingParams = parameters.GetMissingRequired(Parameters.SearchTerm);
            if (missingParams.Count != 0)
            {
                foreach (var error in missingParams)
                    result.AddError(error);
                return result;
            }

            var searchTerm = parameters.GetParameter<string>(Parameters.SearchTerm);
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                result.AddError("Search term cannot be empty");
            }
            else if (searchTerm.Length < 2)
            {
                result.AddError("Search term must be at least 2 characters long");
            }

            return result;
        }

        protected override CommandValidationResult ValidateSpecific(CommandParameters parameters, SessionContext? session)
        {
            var result = CommandValidationResult.Success();

            if (session == null)
            {
                result.AddError("Must be logged in to search patients");
                return result;
            }

            // Only physicians and administrators can search all patients
            if (session.UserRole == UserRole.Patient)
            {
                result.AddError("Patients cannot search for other patients");
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var searchTerm = parameters.GetRequiredParameter<string>(Parameters.SearchTerm);

                // Search for patients by name
                var matchingProfiles = _profileRegistry.SearchByName(searchTerm)
                    .OfType<PatientProfile>()
                    .ToList();

                // Apply role-based filtering for physicians
                if (session?.UserRole == UserRole.Physician)
                {
                    var physician = _profileRegistry.GetProfileById(session.UserId) as PhysicianProfile;
                    if (physician != null)
                    {
                        // Physicians can only see their own patients
                        matchingProfiles = matchingProfiles
                            .Where(p => physician.PatientIds.Contains(p.Id))
                            .ToList();
                    }
                    else
                    {
                        // If physician profile not found, return empty results
                        matchingProfiles.Clear();
                    }
                }

                if (!matchingProfiles.Any())
                {
                    return CommandResult.Ok($"No patients found matching search term '{searchTerm}'");
                }

                var output = FormatSearchResults(matchingProfiles, searchTerm, session);
                return CommandResult.Ok(output, matchingProfiles);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to search patients: {ex.Message}", ex);
            }
        }

        private string FormatSearchResults(List<PatientProfile> patients, string searchTerm, SessionContext? session)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== PATIENT SEARCH RESULTS ===");
            sb.AppendLine($"Search Term: '{searchTerm}'");
            sb.AppendLine($"Found {patients.Count} patient(s)");
            sb.AppendLine();

            foreach (var patient in patients.OrderBy(p => p.Name))
            {
                sb.AppendLine($"Patient ID: {patient.Id:N}");
                sb.AppendLine($"Name: {patient.Name}");
                sb.AppendLine($"Username: {patient.Username}");

                var birthDate = patient.GetValue<DateTime>("birthdate");
                if (birthDate != default)
                {
                    var age = DateTime.Now.Year - birthDate.Year;
                    if (DateTime.Now.DayOfYear < birthDate.DayOfYear)
                        age--;
                    sb.AppendLine($"Birth Date: {birthDate:yyyy-MM-dd} (Age: {age})");
                }

                var gender = patient.GetValue<Gender>("gender");
                if (gender != default)
                {
                    sb.AppendLine($"Gender: {gender}");
                }

                var address = patient.GetValue<string>("address");
                if (!string.IsNullOrEmpty(address))
                {
                    sb.AppendLine($"Address: {address}");
                }

                // Show primary physician if available
                if (patient.PrimaryPhysicianId.HasValue && patient.PrimaryPhysicianId.Value != Guid.Empty)
                {
                    var primaryPhysician = _profileRegistry.GetProfileById(patient.PrimaryPhysicianId.Value) as PhysicianProfile;
                    if (primaryPhysician != null)
                    {
                        sb.AppendLine($"Primary Physician: Dr. {primaryPhysician.Name}");
                    }
                }

                // Show additional information for administrators
                if (session?.UserRole == UserRole.Administrator)
                {
                    var allPhysicians = _profileRegistry.GetPatientPhysicians(patient.Id);
                    if (allPhysicians.Any())
                    {
                        sb.AppendLine($"All Physicians: {string.Join(", ", allPhysicians.Select(p => "Dr. " + p.Name))}");
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
