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
    /// Command that updates an existing patient profile.
    /// Supports updating common fields (name, address, birthdate) and patient-specific fields (gender, race).
    /// Patients can update their own profile; physicians can update their assigned patients.
    /// </summary>
    public class UpdatePatientProfileCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "updatepatientprofile";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="UpdatePatientProfileCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the unique identifier of the patient profile to update.
            /// </summary>
            public const string ProfileId = "profileId";

            /// <summary>
            /// Parameter key for the patient's full name.
            /// </summary>
            public const string Name = "name";

            /// <summary>
            /// Parameter key for the patient's address.
            /// </summary>
            public const string Address = "address";

            /// <summary>
            /// Parameter key for the patient's birthdate.
            /// </summary>
            public const string BirthDate = "birthdate";

            /// <summary>
            /// Parameter key for the patient's gender.
            /// </summary>
            public const string Gender = "patient_gender";

            /// <summary>
            /// Parameter key for the patient's race/ethnicity.
            /// </summary>
            public const string Race = "patient_race";
        }

        private readonly ProfileService _registry;

        public UpdatePatientProfileCommand(ProfileService profileService)
        {
            _registry = profileService ?? throw new ArgumentNullException(nameof(profileService));
        }

        public override string Description => "Updates an existing patient profile";

        public override bool CanUndo => false;

        public override Permission? GetRequiredPermission()
            => Permission.UpdatePatientProfile;

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

            if (profile.Role != UserRole.Patient)
            {
                result.AddError($"Profile {profileId} is not a patient profile");
                return result;
            }

            // Check that at least one update field is provided
            var hasUpdate = parameters.HasParameter(Parameters.Name) ||
                          parameters.HasParameter(Parameters.Address) ||
                          parameters.HasParameter(Parameters.BirthDate) ||
                          parameters.HasParameter(Parameters.Gender) ||
                          parameters.HasParameter(Parameters.Race);

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
            if (session.UserRole == UserRole.Patient)
            {
                // Patients can only update their own profile
                if (profileId.HasValue && profileId.Value != session.UserId)
                {
                    result.AddError("Patients can only update their own profile");
                }
            }
            else if (session.UserRole == UserRole.Physician)
            {
                // Physicians can update their patients
                var physician = _registry.GetProfileById(session.UserId) as PhysicianProfile;
                if (physician != null && profileId.HasValue)
                {
                    if (!physician.PatientIds.Contains(profileId.Value))
                    {
                        result.AddError("You can only update profiles of your patients");
                    }
                }
            }
            // Administrators can update any patient profile

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var profileId = parameters.GetRequiredParameter<Guid>(Parameters.ProfileId);
                var profile = _registry.GetProfileById(profileId) as PatientProfile;

                if (profile == null)
                {
                    return CommandResult.Fail("Patient profile not found");
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

                // Update patient-specific fields
                if (parameters.HasParameter(Parameters.Gender))
                {
                    var gender = parameters.GetParameter<string>(Parameters.Gender);
                    if (!string.IsNullOrWhiteSpace(gender))
                    {
                        profile.SetValue("patient_gender", gender);
                        fieldsUpdated.Add("gender");
                    }
                }

                if (parameters.HasParameter(Parameters.Race))
                {
                    var race = parameters.GetParameter<string>(Parameters.Race);
                    if (!string.IsNullOrWhiteSpace(race))
                    {
                        profile.SetValue("patient_race", race);
                        fieldsUpdated.Add("race");
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
                        $"Patient profile updated successfully. Fields changed: {string.Join(", ", fieldsUpdated)}",
                        profile);
                }
                else
                {
                    return CommandResult.Ok("No changes were made to the patient profile", profile);
                }
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to update patient profile: {ex.Message}", ex);
            }
        }
    }
}