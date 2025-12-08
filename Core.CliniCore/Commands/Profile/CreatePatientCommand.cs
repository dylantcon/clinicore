// Core.CliniCore/Commands/Profile/CreatePatientCommand.cs
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.Domain.Validation;
using Core.CliniCore.Service;

namespace Core.CliniCore.Commands.Profile
{
    /// <summary>
    /// Command that creates a new patient profile and associated authentication account.
    /// </summary>
    public class CreatePatientCommand(IAuthenticationService authService, ProfileService profileService) : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "createpatient";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="CreatePatientCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the username of the new patient.
            /// </summary>
            public const string Username = "username";

            /// <summary>
            /// Parameter key for the password of the new patient.
            /// </summary>
            public const string Password = "password";

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
            public const string Birthdate = "birthdate";

            /// <summary>
            /// Parameter key for the patient's gender.
            /// </summary>
            public const string Gender = "gender";

            /// <summary>
            /// Parameter key for the patient's race.
            /// </summary>
            public const string Race = "race";
        }

        private readonly ProfileService _registry = profileService ?? throw new ArgumentNullException(nameof(profileService));
        private readonly IAuthenticationService _authService = authService ?? throw new ArgumentNullException(nameof(authService));

        /// <inheritdoc />
        public override string Description => "Creates a new patient profile in the system";

        /// <inheritdoc />
        public override bool CanUndo => true;

        /// <inheritdoc />
        public override Permission? GetRequiredPermission()
            => Permission.CreatePatientProfile;

        /// <inheritdoc />
        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Check required parameters exist
            var missingParams = parameters.GetMissingRequired(
                Parameters.Username, Parameters.Password, Parameters.Name, Parameters.Address,
                Parameters.Birthdate, Parameters.Gender, Parameters.Race);

            if (missingParams.Count != 0)
            {
                foreach (var error in missingParams)
                    result.AddError(error);
                return result;
            }

            // Validate username uniqueness
            var username = parameters.GetParameter<string>(Parameters.Username)!;
            if (_registry.UsernameExists(username) || _authService.UserExists(username))
            {
                result.AddError($"Username '{username}' already exists");
            }

            // Validate password strength
            var password = parameters.GetParameter<string>(Parameters.Password)!;
            if (!_authService.ValidatePasswordStrength(password))
            {
                result.AddError("Password does not meet security requirements (minimum 6 characters)");
            }

            return result;
        }

        /// <inheritdoc />
        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                // Create the patient profile
                var patient = new PatientProfile
                {
                    Username = parameters.GetRequiredParameter<string>(Parameters.Username)
                };

                // Set profile values
                patient.SetValue(CommonEntryType.Name.GetKey(), parameters.GetRequiredParameter<string>(Parameters.Name));
                patient.SetValue(CommonEntryType.Address.GetKey(), parameters.GetRequiredParameter<string>(Parameters.Address));
                patient.SetValue(CommonEntryType.BirthDate.GetKey(), parameters.GetRequiredParameter<DateTime>(Parameters.Birthdate));
                
                patient.SetValue(PatientEntryType.Gender.GetKey(), parameters.GetRequiredParameter<Gender>(Parameters.Gender));
                
                patient.SetValue(PatientEntryType.Race.GetKey(), parameters.GetRequiredParameter<string>(Parameters.Race));

                // Validate the profile
                if (!patient.IsValid)
                {
                    var errors = patient.GetValidationErrors();
                    return CommandResult.ValidationFailed(errors);
                }

                // Add to registry first (fail-secure: profile before credentials)
                var password = parameters.GetRequiredParameter<string>(Parameters.Password);
                if (!_registry.AddProfile(patient))
                {
                    return CommandResult.Fail("Failed to add patient to registry");
                }

                // Register with authentication service
                if (!_authService.Register(patient, password))
                {
                    return CommandResult.Fail("Failed to register patient account");
                }

                return CommandResult.Ok(
                    $"Patient '{patient.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty}' created successfully with ID {patient.Id}",
                    patient);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to create patient: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        protected override object? CaptureStateForUndo(CommandParameters parameters, SessionContext? session)
        {
            // Capture the username for undo
            return parameters.GetParameter<string>(Parameters.Username);
        }

        /// <inheritdoc />
        protected override CommandResult UndoCore(object previousState, SessionContext? session)
        {
            if (previousState is string username)
            {
                var profile = _registry.GetProfileByUsername(username);
                if (profile != null)
                {
                    _registry.RemoveProfile(profile.Id);
                    // Note: In production, would also need to remove from auth service
                    return CommandResult.Ok($"Patient creation for '{username}' has been undone");
                }
            }
            return CommandResult.Fail("Unable to undo patient creation");
        }
    }
}
