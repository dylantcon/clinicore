using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.ProfileTemplates;
using Core.CliniCore.Service;
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
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "updateprofile";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="UpdateProfileCommand"/>.
        /// Includes common fields shared by all profile types and role-specific fields.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the unique identifier of the profile to update.
            /// </summary>
            public const string ProfileId = "profileId";

            /// <summary>
            /// Parameter key for the user's full name (all profile types).
            /// </summary>
            public const string Name = "name";

            /// <summary>
            /// Parameter key for the user's address (all profile types).
            /// </summary>
            public const string Address = "address";

            /// <summary>
            /// Parameter key for the user's birthdate (all profile types).
            /// </summary>
            public const string BirthDate = "birthdate";

            /// <summary>
            /// Parameter key for the patient's gender (patient-specific).
            /// </summary>
            public const string Gender = "patient_gender";

            /// <summary>
            /// Parameter key for the patient's race/ethnicity (patient-specific).
            /// </summary>
            public const string Race = "patient_race";

            /// <summary>
            /// Parameter key for the physician's medical license number (physician-specific).
            /// </summary>
            public const string LicenseNumber = "physician_license";

            /// <summary>
            /// Parameter key for the physician's graduation date (physician-specific).
            /// </summary>
            public const string GraduationDate = "physician_graduation";

            /// <summary>
            /// Parameter key for the physician's medical specializations (physician-specific).
            /// </summary>
            public const string Specializations = "physician_specializations";

            /// <summary>
            /// Parameter key for the administrator's email address (administrator-specific).
            /// </summary>
            public const string Email = "email";
        }

        private readonly ProfileService _registry;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateProfileCommand"/> class using the specified profile
        /// service.
        /// </summary>
        /// <param name="profileService">The <see cref="ProfileService"/> instance used to perform profile updates. Cannot be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="profileService"/> is <see langword="null"/>.</exception>
        public UpdateProfileCommand(ProfileService profileService)
        {
            _registry = profileService ?? throw new ArgumentNullException(nameof(profileService));
        }

        /// <inheritdoc />
        public override string Description => "Updates an existing user profile (routes to appropriate profile-specific command)";

        /// <inheritdoc />
        public override bool CanUndo => false; // Profile updates are not undoable for data integrity

        /// <inheritdoc />
        public override Permission? GetRequiredPermission()
            => Permission.UpdatePatientProfile; // Base permission - concrete commands will check specific permissions

        /// <summary>
        /// Validates the specified command parameters to ensure that a valid profile identifier is provided and that
        /// the corresponding profile exists.
        /// </summary>
        /// <remarks>This method checks for the presence and validity of the <c>ProfileId</c> parameter
        /// and verifies that a profile with the specified identifier exists in the registry. Additional parameter
        /// validation may be performed by derived command implementations.</remarks>
        /// <param name="parameters">The <see cref="CommandParameters"/> instance containing the parameters to validate. Must include a non-empty
        /// <c>ProfileId</c> parameter.</param>
        /// <returns>A <see cref="CommandValidationResult"/> indicating the outcome of the validation. The result contains errors
        /// if required parameters are missing, the profile identifier is invalid, or the profile does not exist.</returns>
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

        /// <inheritdoc />
        protected override CommandValidationResult ValidateSpecific(CommandParameters parameters, SessionContext? session)
        {
            // Delegate session validation to the concrete commands
            return CommandValidationResult.Success();
        }

        /// <inheritdoc />
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
                    UserRole.Patient => new UpdatePatientProfileCommand(_registry),
                    UserRole.Physician => new UpdatePhysicianProfileCommand(_registry),
                    UserRole.Administrator => new UpdateAdministratorProfileCommand(_registry),
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