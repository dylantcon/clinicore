// Core.CliniCore/Commands/Profile/ListPhysiciansCommand.cs
using System;
using System.Text;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.Extensions;

namespace Core.CliniCore.Commands.Profile
{
    public class ListPhysiciansCommand : AbstractCommand
    {
        public static class Parameters
        {
            public const string Specialization = "specialization";
            public const string Search = "search";
        }

        private readonly ProfileRegistry _registry = ProfileRegistry.Instance;

        public override string Description => "Lists all physicians in the system";

        public override bool CanUndo => false;

        public override Permission? GetRequiredPermission() => null; // Anyone can view physician list

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            // No validation needed for listing
            return CommandValidationResult.Success();
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var specialization = parameters.GetParameter<MedicalSpecialization?>(Parameters.Specialization);
                var searchTerm = parameters.GetParameter<string>(Parameters.Search);

                var physicians = _registry.GetAllPhysicians();

                // Filter by specialization if provided
                if (specialization.HasValue)
                {
                    physicians = physicians.Where(p =>
                        p.Specializations.Contains(specialization.Value));
                }

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    physicians = physicians.Where(p =>
                        p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        p.LicenseNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                }

                var physicianList = physicians.OrderBy(p => p.Name).ToList();

                if (!physicianList.Any())
                {
                    return CommandResult.Ok("No physicians found matching criteria.", physicianList);
                }

                var sb = new StringBuilder();
                sb.AppendLine($"Found {physicianList.Count} physician(s):");
                sb.AppendLine(new string('-', 80));

                foreach (var physician in physicianList)
                {
                    sb.AppendLine(FormatPhysicianInfo(physician));
                }

                return CommandResult.Ok(sb.ToString(), physicianList);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to list physicians: {ex.Message}", ex);
            }
        }

        private string FormatPhysicianInfo(PhysicianProfile physician)
        {
            var patientCount = physician.PatientIds.Count;
            var specializations = string.Join(", ",
                physician.Specializations.Select(s => s.GetDisplayName()));

            var yearsExperience = DateTime.Now.Year - physician.GraduationDate.Year;

            return $"  ID: {physician.Id:N}\n" +
                   $"  Name: Dr. {physician.Name}\n" +
                   $"  License: {physician.LicenseNumber}\n" +
                   $"  Specializations: {specializations}\n" +
                   $"  Years Experience: {yearsExperience}\n" +
                   $"  Active Patients: {patientCount}\n" +
                   $"  ---";
        }
    }
}
