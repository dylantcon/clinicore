using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.ProfileTemplates;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.CliniCore.Commands.Profile
{
    /// <summary>
    /// Command that updates an existing physician profile.
    /// Supports updating common fields (name, address, birthdate) and physician-specific fields
    /// (license number, graduation date, specializations). Physicians can update their own profile.
    /// </summary>
    public class UpdatePhysicianProfileCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "updatephysicianprofile";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="UpdatePhysicianProfileCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the unique identifier of the physician profile to update.
            /// </summary>
            public const string ProfileId = "profileId";

            /// <summary>
            /// Parameter key for the physician's full name.
            /// </summary>
            public const string Name = "name";

            /// <summary>
            /// Parameter key for the physician's address.
            /// </summary>
            public const string Address = "address";

            /// <summary>
            /// Parameter key for the physician's birthdate.
            /// </summary>
            public const string BirthDate = "birthdate";

            /// <summary>
            /// Parameter key for the physician's medical license number.
            /// </summary>
            public const string LicenseNumber = "physician_license";

            /// <summary>
            /// Parameter key for the physician's medical school graduation date.
            /// </summary>
            public const string GraduationDate = "physician_graduation";

            /// <summary>
            /// Parameter key for the physician's list of medical specializations.
            /// </summary>
            public const string Specializations = "physician_specializations";
        }

        private readonly ProfileService _registry;

        public UpdatePhysicianProfileCommand(ProfileService profileService)
        {
            _registry = profileService ?? throw new ArgumentNullException(nameof(profileService));
        }

        public override string Description => "Updates an existing physician profile";

        public override bool CanUndo => false;

        public override Permission? GetRequiredPermission()
            => Permission.UpdatePhysicianProfile;

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

            if (profile.Role != UserRole.Physician)
            {
                result.AddError($"Profile {profileId} is not a physician profile");
                return result;
            }

            // Check that at least one update field is provided
            var hasUpdate = parameters.HasParameter(Parameters.Name) ||
                          parameters.HasParameter(Parameters.Address) ||
                          parameters.HasParameter(Parameters.BirthDate) ||
                          parameters.HasParameter(Parameters.LicenseNumber) ||
                          parameters.HasParameter(Parameters.GraduationDate) ||
                          parameters.HasParameter(Parameters.Specializations);

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

            // Check permissions based on role
            if (session.UserRole == UserRole.Physician)
            {
                // Physicians can only update their own profile
                if (profileId.HasValue && profileId.Value != session.UserId)
                {
                    result.AddError("Physicians can only update their own profile");
                }
            }
            else if (session.UserRole == UserRole.Patient)
            {
                result.AddError("Patients cannot update physician profiles");
            }
            // Administrators can update any physician profile

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var profileId = parameters.GetRequiredParameter<Guid>(Parameters.ProfileId);
                var profile = _registry.GetProfileById(profileId) as PhysicianProfile;

                if (profile == null)
                {
                    return CommandResult.Fail("Physician profile not found");
                }

                var fieldsUpdated = new List<string>();

                // Update common fields
                if (parameters.HasParameter(Parameters.Name))
                {
                    var name = parameters.GetParameter<string>(Parameters.Name);
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        profile.SetValue("name", name);
                        fieldsUpdated.Add("name");
                    }
                }

                if (parameters.HasParameter(Parameters.Address))
                {
                    var address = parameters.GetParameter<string>(Parameters.Address);
                    if (!string.IsNullOrWhiteSpace(address))
                    {
                        profile.SetValue("address", address);
                        fieldsUpdated.Add("address");
                    }
                }

                if (parameters.HasParameter(Parameters.BirthDate))
                {
                    var birthDate = parameters.GetParameter<DateTime?>(Parameters.BirthDate);
                    if (birthDate.HasValue)
                    {
                        profile.SetValue("birthdate", birthDate.Value);
                        fieldsUpdated.Add("birthdate");
                    }
                }

                // Update physician-specific fields
                if (parameters.HasParameter(Parameters.LicenseNumber))
                {
                    var license = parameters.GetParameter<string>(Parameters.LicenseNumber);
                    if (!string.IsNullOrWhiteSpace(license))
                    {
                        profile.SetValue("physician_license", license);
                        fieldsUpdated.Add("license number");
                    }
                }

                if (parameters.HasParameter(Parameters.GraduationDate))
                {
                    var gradDate = parameters.GetParameter<DateTime?>(Parameters.GraduationDate);
                    if (gradDate.HasValue)
                    {
                        profile.SetValue("physician_graduation", gradDate.Value);
                        fieldsUpdated.Add("graduation date");
                    }
                }

                if (parameters.HasParameter(Parameters.Specializations))
                {
                    var specializations = parameters.GetParameter<List<MedicalSpecialization>>(Parameters.Specializations);
                    if (specializations != null && specializations.Any())
                    {
                        profile.SetValue("physician_specializations", specializations);
                        fieldsUpdated.Add("specializations");
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
                        $"Physician profile updated successfully. Fields changed: {string.Join(", ", fieldsUpdated)}",
                        profile);
                }
                else
                {
                    return CommandResult.Ok("No changes were made to the physician profile", profile);
                }
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to update physician profile: {ex.Message}", ex);
            }
        }
    }
}