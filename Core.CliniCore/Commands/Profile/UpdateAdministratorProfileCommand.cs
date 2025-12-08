using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.ProfileTemplates;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.CliniCore.Commands.Profile
{
    /// <summary>
    /// Command that updates an existing administrator profile.
    /// Supports updating common fields (name, address, birthdate) and administrator-specific fields (email).
    /// Only administrators can execute this command.
    /// </summary>
    public class UpdateAdministratorProfileCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "updateadministratorprofile";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="UpdateAdministratorProfileCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the unique identifier of the administrator profile to update.
            /// </summary>
            public const string ProfileId = "profileId";

            /// <summary>
            /// Parameter key for the administrator's full name.
            /// </summary>
            public const string Name = "name";

            /// <summary>
            /// Parameter key for the administrator's address.
            /// </summary>
            public const string Address = "address";

            /// <summary>
            /// Parameter key for the administrator's birthdate.
            /// </summary>
            public const string BirthDate = "birthdate";

            /// <summary>
            /// Parameter key for the administrator's email address.
            /// </summary>
            public const string Email = "email";
        }

        private readonly ProfileService _registry;

        public UpdateAdministratorProfileCommand(ProfileService profileService)
        {
            _registry = profileService ?? throw new ArgumentNullException(nameof(profileService));
        }

        public override string Description => "Updates an existing administrator profile";

        public override bool CanUndo => false;

        public override Permission? GetRequiredPermission()
            => Permission.UpdateAdministratorProfile;

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

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

            var profile = _registry.GetProfileById(profileId.Value);
            if (profile == null)
            {
                result.AddError($"Profile with ID {profileId} not found");
                return result;
            }

            if (profile.Role != UserRole.Administrator)
            {
                result.AddError($"Profile {profileId} is not an administrator profile");
                return result;
            }

            // Check that at least one update field is provided
            var hasUpdate = parameters.HasParameter(Parameters.Name) ||
                          parameters.HasParameter(Parameters.Address) ||
                          parameters.HasParameter(Parameters.BirthDate) ||
                          parameters.HasParameter(Parameters.Email);

            if (!hasUpdate)
            {
                result.AddError("At least one field to update must be provided");
            }

            return result;
        }

        protected override CommandValidationResult ValidateSpecific(CommandParameters parameters, SessionContext? session)
        {
            var result = CommandValidationResult.Success();

            if (session == null)
            {
                result.AddError("Must be logged in to update profiles");
                return result;
            }

            var profileId = parameters.GetParameter<Guid?>(Parameters.ProfileId);

            // Only administrators can update administrator profiles
            if (session.UserRole != UserRole.Administrator)
            {
                result.AddError("Only administrators can update administrator profiles");
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var profileId = parameters.GetRequiredParameter<Guid>(Parameters.ProfileId);
                var profile = _registry.GetProfileById(profileId) as AdministratorProfile;

                if (profile == null)
                {
                    return CommandResult.Fail("Administrator profile not found");
                }

                var fieldsUpdated = new List<string>();

                // Update common fields
                if (parameters.HasParameter(Parameters.Name))
                {
                    var name = parameters.GetParameter<string>(Parameters.Name);
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        profile.SetValue(CommonEntryType.Name.GetKey(), name);
                        fieldsUpdated.Add(CommonEntryType.Name.GetDisplayName());
                    }
                }

                if (parameters.HasParameter(Parameters.Address))
                {
                    var address = parameters.GetParameter<string>(Parameters.Address);
                    if (!string.IsNullOrWhiteSpace(address))
                    {
                        profile.SetValue(CommonEntryType.Address.GetKey(), address);
                        fieldsUpdated.Add(CommonEntryType.Address.GetDisplayName());
                    }
                }

                if (parameters.HasParameter(Parameters.BirthDate))
                {
                    var birthDate = parameters.GetParameter<DateTime?>(Parameters.BirthDate);
                    if (birthDate.HasValue)
                    {
                        profile.SetValue(CommonEntryType.BirthDate.GetKey(), birthDate.Value);
                        fieldsUpdated.Add(CommonEntryType.BirthDate.GetDisplayName());
                    }
                }

                // Update administrator-specific field
                if (parameters.HasParameter(Parameters.Email))
                {
                    var email = parameters.GetParameter<string>(Parameters.Email);
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        profile.SetValue(AdministratorEntryType.Email.GetKey(), email);
                        fieldsUpdated.Add(AdministratorEntryType.Email.GetDisplayName());
                    }
                }

                // Validate the updated profile
                if (!profile.IsValid)
                {
                    var errors = profile.GetValidationErrors();
                    return CommandResult.ValidationFailed(errors);
                }

                if (fieldsUpdated.Any())
                {
                    // Persist the changes to the repository
                    _registry.UpdateProfile(profile);

                    return CommandResult.Ok(
                        $"Administrator profile updated successfully. Fields changed: {string.Join(", ", fieldsUpdated)}",
                        profile);
                }
                else
                {
                    return CommandResult.Ok("No changes were made to the administrator profile", profile);
                }
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to update administrator profile: {ex.Message}", ex);
            }
        }
    }
}