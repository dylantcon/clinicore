// Core.CliniCore/Commands/Profile/ListPatientsCommand.cs
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.Service;
using System;
using System.Text;

namespace Core.CliniCore.Commands.Profile
{
    public class ListPatientsCommand : AbstractCommand
    {
        public const string Key = "listpatients";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string PhysicianId = "physician_id";
            public const string Search = "search";
            public const string IncludeInactive = "include_inactive";
        }

        private readonly ProfileService _registry;

        public ListPatientsCommand(ProfileService profileService)
        {
            _registry = profileService ?? throw new ArgumentNullException(nameof(profileService));
        }

        public override string Description => "Lists all patients or patients for a specific physician";

        public override bool CanUndo => false;

        public override Permission? GetRequiredPermission()
            => Permission.ViewAllPatients;

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Optional physician filter
            var physicianId = parameters.GetParameter<Guid?>(Parameters.PhysicianId);
            if (physicianId.HasValue && physicianId.Value != Guid.Empty)
            {
                var physician = _registry.GetProfileById(physicianId.Value);
                if (physician == null || physician.Role != UserRole.Physician)
                {
                    result.AddWarning($"Physician {physicianId} not found - showing all patients");
                }
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var physicianId = parameters.GetParameter<Guid?>(Parameters.PhysicianId);
                var searchTerm = parameters.GetParameter<string>(Parameters.Search);
                var includeInactive = parameters.GetParameter<bool?>(Parameters.IncludeInactive) ?? false;

                IEnumerable<PatientProfile> patients;

                // Get patients based on filter
                if (physicianId.HasValue && physicianId.Value != Guid.Empty)
                {
                    patients = _registry.GetPhysicianPatients(physicianId.Value);
                }
                else
                {
                    patients = _registry.GetAllPatients();
                }

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    patients = patients.Where(p =>
                        (p.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty).Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        p.Username.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                }

                // Filter inactive if needed
                if (!includeInactive)
                {
                    patients = patients.Where(p => p.IsValid);
                }

                // Build result
                var patientList = patients.OrderBy(p => p.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty).ToList();

                if (!patientList.Any())
                {
                    return CommandResult.Ok("No patients found matching criteria.", patientList);
                }

                var sb = new StringBuilder();
                sb.AppendLine($"Found {patientList.Count} patient(s):");
                sb.AppendLine(new string('-', 80));

                foreach (var patient in patientList)
                {
                    sb.AppendLine(FormatPatientInfo(patient));
                    sb.AppendLine("  ---");
                }

                return CommandResult.Ok(sb.ToString(), patientList);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to list patients: {ex.Message}", ex);
            }
        }

        private string FormatPatientInfo(PatientProfile patient)
        {
            // Get the primary physician info and replace the generic ID with physician name
            var patientInfo = patient.ToString();

            if (patient.PrimaryPhysicianId.HasValue)
            {
                var primaryPhysician = _registry.GetProfileById(patient.PrimaryPhysicianId.Value) as PhysicianProfile;
                if (primaryPhysician != null)
                {
                    // Replace the generic "Primary Physician ID" line with physician name
                    patientInfo = patientInfo.Replace(
                        $"  Primary Physician ID: {patient.PrimaryPhysicianId.Value:N}",
                        $"  Primary Physician: Dr. {primaryPhysician.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty}"
                    );
                }
            }

            return patientInfo.TrimEnd('\r', '\n');
        }
    }
}