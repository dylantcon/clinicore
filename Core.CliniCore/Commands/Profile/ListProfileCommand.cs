using System;
using System.Linq;
using System.Text;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Services;

namespace Core.CliniCore.Commands.Profile
{
    public class ListProfileCommand : AbstractCommand
    {
        public const string Key = "listprofiles";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string IncludeInvalid = "include_invalid";
        }

        private readonly ProfileService _registry;

        public ListProfileCommand(ProfileService profileService)
        {
            _registry = profileService ?? throw new ArgumentNullException(nameof(profileService));
        }

        public override string Description => "Lists all profiles in the system";

        public override bool CanUndo => false;

        public override Permission? GetRequiredPermission()
            => Permission.ViewAllProfiles;

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            // No required parameters for listing all profiles
            return CommandValidationResult.Success();
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var includeInvalid = parameters.GetParameter<bool?>(Parameters.IncludeInvalid) ?? false;

                var allProfiles = _registry.GetAllProfiles();

                if (!includeInvalid)
                {
                    allProfiles = allProfiles.Where(p => p.IsValid);
                }

                if (!allProfiles.Any())
                {
                    return CommandResult.Ok("No profiles found in the registry.", allProfiles);
                }

                var sb = new StringBuilder();
                sb.AppendLine($"Found {allProfiles.Count()} profile(s) in the registry:");
                sb.AppendLine();

                // Group by role for better organization
                var groupedProfiles = allProfiles.GroupBy(p => p.Role).OrderBy(g => g.Key.ToString());

                foreach (var group in groupedProfiles)
                {
                    sb.AppendLine($"=== {group.Key} PROFILES ({group.Count()}) ===");

                    foreach (var profile in group.OrderBy(p => p.Username))
                    {
                        var name = profile.GetValue<string>("name") ?? profile.Username;
                        var validStatus = profile.IsValid ? "" : " [INVALID]";

                        sb.AppendLine($"  • {profile.Id:N} - {name} ({profile.Username}){validStatus}");

                        // Add role-specific details
                        switch (profile)
                        {
                            case PhysicianProfile physician:
                                if (physician.Specializations.Any())
                                {
                                    var specs = string.Join(", ", physician.Specializations.Select(s => s.ToString()));
                                    sb.AppendLine($"    Specializations: {specs}");
                                }
                                sb.AppendLine($"    Patients: {physician.PatientIds.Count}");
                                break;
                            case PatientProfile patient:
                                if (patient.PrimaryPhysicianId.HasValue)
                                {
                                    var primaryPhysician = _registry.GetProfileById(patient.PrimaryPhysicianId.Value) as PhysicianProfile;
                                    if (primaryPhysician != null)
                                    {
                                        sb.AppendLine($"    Primary Physician: Dr. {primaryPhysician.Name}");
                                    }
                                }
                                break;
                        }
                    }
                    sb.AppendLine();
                }

                return CommandResult.Ok(sb.ToString().TrimEnd(), allProfiles);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to list profiles: {ex.Message}", ex);
            }
        }
    }
}