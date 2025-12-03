// Core.CliniCore/Commands/Profile/ListPhysiciansCommand.cs
using System;
using System.Text;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Services;

namespace Core.CliniCore.Commands.Profile
{
    public class ListPhysiciansCommand : AbstractCommand
    {
        public const string Key = "listphysicians";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string Specialization = "specialization";
            public const string Search = "search";
        }

        private readonly ProfileService _registry;

        public ListPhysiciansCommand(ProfileService profileService)
        {
            _registry = profileService ?? throw new ArgumentNullException(nameof(profileService));
        }

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
                    sb.AppendLine("  ---");
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
            // Get the base physician info from ToString and add years of experience
            var physicianInfo = physician.ToString();
            var yearsExperience = DateTime.Now.Year - physician.GraduationDate.Year;

            // Insert years of experience after graduation date
            var graduationLine = $"  Graduation Date: {physician.GraduationDate:yyyy-MM-dd}";
            var graduationWithExperience = $"{graduationLine}\n  Years Experience: {yearsExperience}";

            physicianInfo = physicianInfo.Replace(graduationLine, graduationWithExperience);

            return physicianInfo.TrimEnd('\r', '\n');
        }
    }
}
