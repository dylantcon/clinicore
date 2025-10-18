using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.ProfileTemplates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.CliniCore.Commands.Profile
{
    /// <summary>
    /// Router command that delegates to the appropriate profile-specific update command
    /// based on the profile type being updated. This allows for a single entry point
    /// while maintaining type-specific validation and field handling.
    /// </summary>
    public class UpdateProfileCommand : AbstractCommand
    {
        public const string Key = "updateprofile";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string ProfileId = "profileId";
            // Common fields (all profile types)
            public const string Name = "name";
            public const string Address = "address";
            public const string BirthDate = "birthdate";
            // Patient-specific fields
            public const string Gender = "patient_gender";
            public const string Race = "patient_race";
            // Physician-specific fields
            public const string LicenseNumber = "physician_license";
            public const string GraduationDate = "physician_graduation";
            public const string Specializations = "physician_specializations";
            // Administrator-specific field
            public const string Email = "email";
        }

        private readonly ProfileRegistry _registry = ProfileRegistry.Instance;

        public override string Description => "Updates an existing user profile (routes to appropriate profile-specific command)";

        public override bool CanUndo => false; // Profile updates are not undoable for data integrity

        public override Permission? GetRequiredPermission()
            => Permission.UpdatePatientProfile; // Base permission - concrete commands will check specific permissions

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Check required ProfileId parameter
            var missingParams = parameters.GetMissingRequired(Parameters.ProfileId);
            if (missingParams.Any())
            {
                foreach (var error in missingParams)
                    result.AddError(error);
                return result;
            }

            var profileId = parameters.GetParameter<Guid?>(Parameters.ProfileId);
            if (!profileId.HasValue || profileId.Value == Guid.Empty)
            {
                result.AddError("Profile ID is required");
                return result;
            }

            // Verify profile exists
            var profile = _registry.GetProfileById(profileId.Value);
            if (profile == null)
            {
                result.AddError($"Profile with ID {profileId} not found");
            }

            // Note: Further validation is delegated to the concrete commands

            return result;
        }

        protected override CommandValidationResult ValidateSpecific(CommandParameters parameters, SessionContext? session)
        {
            // Delegate session validation to the concrete commands
            return CommandValidationResult.Success();
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var profileId = parameters.GetRequiredParameter<Guid>(Parameters.ProfileId);
                var profile = _registry.GetProfileById(profileId);

                if (profile == null)
                {
                    return CommandResult.Fail("Profile not found");
                }

                // Create and execute the appropriate concrete command based on profile type
                AbstractCommand concreteCommand = profile.Role switch
                {
                    UserRole.Patient => new UpdatePatientProfileCommand(),
                    UserRole.Physician => new UpdatePhysicianProfileCommand(),
                    UserRole.Administrator => new UpdateAdministratorProfileCommand(),
                    _ => throw new InvalidOperationException($"Unknown profile role: {profile.Role}")
                };

                // Delegate execution to the concrete command
                // The concrete command will handle all validation and execution
                return concreteCommand.Execute(parameters, session);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to route profile update: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Helper method to get editable fields for a specific profile type.
        /// Used by the console layer to determine which fields to prompt for.
        /// </summary>
        public static List<string> GetEditableFieldsForProfileType(UserRole role)
        {
            return role switch
            {
                UserRole.Patient => new List<string>
                {
                    Parameters.Name, Parameters.Address, Parameters.BirthDate,
                    Parameters.Gender, Parameters.Race
                },
                UserRole.Physician => new List<string>
                {
                    Parameters.Name, Parameters.Address, Parameters.BirthDate,
                    Parameters.LicenseNumber, Parameters.GraduationDate, Parameters.Specializations
                },
                UserRole.Administrator => new List<string>
                {
                    Parameters.Name, Parameters.Address, Parameters.BirthDate,
                    Parameters.Email
                },
                _ => new List<string> { Parameters.Name, Parameters.Address, Parameters.BirthDate }
            };
        }
    }
}