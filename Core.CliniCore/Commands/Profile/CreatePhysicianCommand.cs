// Core.CliniCore/Commands/Profile/CreatePhysicianCommand.cs
using System;
using System.Collections.Generic;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.Service;

namespace Core.CliniCore.Commands.Profile
{
    public class CreatePhysicianCommand(IAuthenticationService authService, ProfileService profileService) : AbstractCommand
    {
        public const string Key = "createphysician";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string Username = "username";
            public const string Password = "password";
            public const string Name = "name";
            public const string Address = "address";
            public const string Birthdate = "birthdate";
            public const string LicenseNumber = "license_number";
            public const string GraduationDate = "graduation_date";
            public const string Specializations = "specializations";
        }

        private readonly ProfileService _registry = profileService ?? throw new ArgumentNullException(nameof(profileService));
        private readonly IAuthenticationService _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        public static readonly int MEDSPECMAXCOUNT = 5;

        public override string Description => "Creates a new physician profile in the system";

        public override bool CanUndo => true;

        public override Permission? GetRequiredPermission() 
            => Permission.CreatePhysicianProfile;

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();
            
            // Check required parameters
            var missingParams = parameters.GetMissingRequired(
                Parameters.Username, Parameters.Password, Parameters.Name, Parameters.Address, Parameters.Birthdate,
                Parameters.LicenseNumber, Parameters.GraduationDate, Parameters.Specializations);
            
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

            // Validate password
            var password = parameters.GetParameter<string>(Parameters.Password)!;
            if (!_authService.ValidatePasswordStrength(password))
            {
                result.AddError("Password does not meet security requirements");
            }

            // Validate specializations
            var specializations = parameters.GetParameter<List<MedicalSpecialization>>(Parameters.Specializations);
            if (specializations == null || specializations.Count == 0)
            {
                result.AddError("At least one specialization is required");
            }
            else if (specializations.Count > MEDSPECMAXCOUNT)
            {
                result.AddError($"Maximum of {MEDSPECMAXCOUNT} specializations allowed");
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                // Create physician profile
                var physician = new PhysicianProfile
                {
                    Username = parameters.GetRequiredParameter<string>(Parameters.Username)
                };

                // Set profile values
                physician.SetValue(CommonEntryType.Name.GetKey(), parameters.GetRequiredParameter<string>(Parameters.Name));
                physician.SetValue(CommonEntryType.Address.GetKey(), parameters.GetRequiredParameter<string>(Parameters.Address));
                physician.SetValue(CommonEntryType.BirthDate.GetKey(), parameters.GetRequiredParameter<DateTime>(Parameters.Birthdate));
                physician.SetValue(PhysicianEntryType.LicenseNumber.GetKey(), parameters.GetRequiredParameter<string>(Parameters.LicenseNumber));
                physician.SetValue(PhysicianEntryType.GraduationDate.GetKey(), parameters.GetRequiredParameter<DateTime>(Parameters.GraduationDate));
                physician.SetValue(PhysicianEntryType.Specializations.GetKey(), 
                    parameters.GetRequiredParameter<List<MedicalSpecialization>>(Parameters.Specializations));

                // Validate profile
                if (!physician.IsValid)
                {
                    var errors = physician.GetValidationErrors();
                    return CommandResult.ValidationFailed(errors);
                }

                // Add to registry first (fail-secure: profile before credentials)
                var password = parameters.GetRequiredParameter<string>(Parameters.Password);
                if (!_registry.AddProfile(physician))
                {
                    return CommandResult.Fail("Failed to add physician to registry");
                }

                // Register with auth service
                if (!_authService.Register(physician, password))
                {
                    return CommandResult.Fail("Failed to register physician account");
                }

                return CommandResult.Ok(
                    $"Physician Dr. {physician.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty} created successfully with ID {physician.Id}",
                    physician);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to create physician: {ex.Message}", ex);
            }
        }

        protected override object? CaptureStateForUndo(CommandParameters parameters, SessionContext? session)
        {
            return parameters.GetParameter<string>(Parameters.Username);
        }

        protected override CommandResult UndoCore(object previousState, SessionContext? session)
        {
            if (previousState is string username)
            {
                var profile = _registry.GetProfileByUsername(username);
                if (profile != null)
                {
                    _registry.RemoveProfile(profile.Id);
                    return CommandResult.Ok($"Physician creation for '{username}' has been undone");
                }
            }
            return CommandResult.Fail("Unable to undo physician creation");
        }
    }
}