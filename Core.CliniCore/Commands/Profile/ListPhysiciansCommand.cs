// Core.CliniCore/Commands/Profile/ListPhysiciansCommand.cs
using System;
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
    /// Command that retrieves and displays a list of physician profiles.
    /// Supports filtering by medical specialization and search terms.
    /// </summary>
    public class ListPhysiciansCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "listphysicians";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="ListPhysiciansCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for filtering physicians by medical specialization.
            /// </summary>
            public const string Specialization = "specialization";

            /// <summary>
            /// Parameter key for searching physicians by name or license number.
            /// </summary>
            public const string Search = "search";
        }

        private readonly ProfileService _registry;

        /// <summary>
        /// Constructs a ListPhysicianCommand that will source its data from the supplied ProfileService
        /// </summary>
        /// <param name="profileService"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ListPhysiciansCommand(ProfileService profileService)
        {
            _registry = profileService ?? throw new ArgumentNullException(nameof(profileService));
        }

        /// <inheritdoc />
        public override string Description => "Lists all physicians in the system";

        /// <inheritdoc />
        public override bool CanUndo => false;

        /// <inheritdoc />
        public override Permission? GetRequiredPermission() => null; // Anyone can view physician list

        /// <inheritdoc />
        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            // No validation needed for listing
            return CommandValidationResult.Success();
        }

        /// <inheritdoc />
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
                    {
                        var specializations = p.GetValue<List<MedicalSpecialization>>(PhysicianEntryType.Specializations.GetKey()) ?? new List<MedicalSpecialization>();
                        return specializations.Contains(specialization.Value);
                    });
                }

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    physicians = physicians.Where(p =>
                    {
                        var name = p.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty;
                        var licenseNumber = p.GetValue<string>(PhysicianEntryType.LicenseNumber.GetKey()) ?? string.Empty;
                        return name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                               licenseNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
                    });
                }

                var physicianList = physicians.OrderBy(p => p.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty).ToList();

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
            var graduationDate = physician.GetValue<DateTime>(PhysicianEntryType.GraduationDate.GetKey());
            var yearsExperience = DateTime.Now.Year - graduationDate.Year;

            // Insert years of experience after graduation date
            var graduationLine = $"  Graduation Date: {graduationDate:yyyy-MM-dd}";
            var graduationWithExperience = $"{graduationLine}\n  Years Experience: {yearsExperience}";

            physicianInfo = physicianInfo.Replace(graduationLine, graduationWithExperience);

            return physicianInfo.TrimEnd('\r', '\n');
        }
    }
}
