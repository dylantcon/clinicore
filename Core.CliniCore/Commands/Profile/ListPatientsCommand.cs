// Core.CliniCore/Commands/Profile/ListPatientsCommand.cs
using Core.CliniCore.Commands;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.Extensions;
using System;
using System.Text;

namespace Core.CliniCore.Commands.Profile
{
    public class ListPatientsCommand : AbstractCommand
    {
        public static class Parameters
        {
            public const string PhysicianId = "physician_id";
            public const string Search = "search";
            public const string IncludeInactive = "include_inactive";
        }

        private readonly ProfileRegistry _registry = ProfileRegistry.Instance;

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
                        p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        p.Username.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                }

                // Filter inactive if needed
                if (!includeInactive)
                {
                    patients = patients.Where(p => p.IsValid);
                }

                // Build result
                var patientList = patients.OrderBy(p => p.Name).ToList();

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
            var primaryPhysician = patient.PrimaryPhysicianId.HasValue
                ? _registry.GetProfileById(patient.PrimaryPhysicianId.Value) as PhysicianProfile
                : null;

            var primaryPhysicianStr = primaryPhysician != null
                ? $"Dr. {primaryPhysician.Name}"
                : "None";

            var age = DateTime.Now.Year - patient.BirthDate.Year;

            return $"  ID: {patient.Id:N}\n" +
                   $"  Name: {patient.Name} (Age: {age})\n" +
                   $"  Username: {patient.Username}\n" +
                   $"  Gender: {patient.Gender.GetDisplayName()}\n" +
                   $"  Address: {patient.Address}\n" +
                   $"  Primary Physician: {primaryPhysicianStr}\n" +
                   $"  ---";
        }
    }
}