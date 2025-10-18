using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.ProfileTemplates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.CliniCore.Commands.Profile
{
    public class CreateAdministratorCommand : AbstractCommand
    {
        public static class Parameters
        {
            public const string Username = "username";
            public const string Password = "password";
            public const string Name = "name";
            public const string Address = "address";
            public const string BirthDate = "birthdate";
            public const string Email = "email";
        }

        private readonly IAuthenticationService _authService;
        private readonly ProfileRegistry _registry = ProfileRegistry.Instance;

        public CreateAdministratorCommand(IAuthenticationService authenticationService)
        {
            _authService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        }

        public const string Key = "createadministrator";
        public override string CommandKey => Key;

        public override string Description => "Creates a new administrator profile";

        public override bool CanUndo => false;

        public override Permission? GetRequiredPermission()
            => Permission.CreatePhysicianProfile; // Using similar permission - only admins can create admins

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Check required parameters
            var requiredParams = new[] { Parameters.Username, Parameters.Password, Parameters.Name };
            var missingParams = parameters.GetMissingRequired(requiredParams);

            if (missingParams.Any())
            {
                foreach (var error in missingParams)
                    result.AddError(error);
                return result;
            }

            // Validate username
            var username = parameters.GetParameter<string>(Parameters.Username);
            if (string.IsNullOrWhiteSpace(username))
            {
                result.AddError("Username cannot be empty");
            }
            else if (username.Length < 3)
            {
                result.AddError("Username must be at least 3 characters long");
            }
            else
            {
                // Check if username already exists across all user types
                var allProfiles = new List<IUserProfile>();
                allProfiles.AddRange(_registry.GetAllAdministrators());
                allProfiles.AddRange(_registry.GetAllPhysicians());
                allProfiles.AddRange(_registry.GetAllPatients());

                if (allProfiles.Any(p => p.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                {
                    result.AddError($"Username '{username}' is already taken");
                }
            }

            // Validate password
            var password = parameters.GetParameter<string>(Parameters.Password);
            if (string.IsNullOrWhiteSpace(password))
            {
                result.AddError("Password cannot be empty");
            }
            else if (password.Length < 6)
            {
                result.AddError("Password must be at least 6 characters long");
            }

            // Validate name
            var name = parameters.GetParameter<string>(Parameters.Name);
            if (string.IsNullOrWhiteSpace(name))
            {
                result.AddError("Name cannot be empty");
            }

            // Validate email if provided
            if (parameters.HasParameter(Parameters.Email))
            {
                var email = parameters.GetParameter<string>(Parameters.Email);
                if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
                {
                    result.AddError("Invalid email format");
                }
            }

            // Validate birthdate if provided
            if (parameters.HasParameter(Parameters.BirthDate))
            {
                var birthDate = parameters.GetParameter<DateTime?>(Parameters.BirthDate);
                if (birthDate.HasValue)
                {
                    if (birthDate.Value > DateTime.Now)
                    {
                        result.AddError("Birth date cannot be in the future");
                    }
                    else if (birthDate.Value < DateTime.Now.AddYears(-120))
                    {
                        result.AddError("Invalid birth date");
                    }
                }
            }

            return result;
        }

        protected override CommandValidationResult ValidateSpecific(CommandParameters parameters, SessionContext? session)
        {
            var result = CommandValidationResult.Success();

            if (session == null)
            {
                result.AddError("Must be logged in to create administrator profiles");
                return result;
            }

            // Only administrators can create other administrators
            if (session.UserRole != UserRole.Administrator)
            {
                result.AddError("Only administrators can create administrator profiles");
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var username = parameters.GetRequiredParameter<string>(Parameters.Username);
                var password = parameters.GetRequiredParameter<string>(Parameters.Password);
                var name = parameters.GetRequiredParameter<string>(Parameters.Name);

                // Create the administrator profile
                var adminProfile = new AdministratorProfile();
                adminProfile.Username = username;
                adminProfile.SetValue(CommonEntryType.Name.GetKey(), name);

                // Set optional fields
                if (parameters.HasParameter(Parameters.Address))
                {
                    var address = parameters.GetParameter<string>(Parameters.Address);
                    if (!string.IsNullOrWhiteSpace(address))
                    {
                        adminProfile.SetValue(CommonEntryType.Address.GetKey(), address);
                    }
                }

                if (parameters.HasParameter(Parameters.BirthDate))
                {
                    var birthDate = parameters.GetParameter<DateTime?>(Parameters.BirthDate);
                    if (birthDate.HasValue)
                    {
                        adminProfile.SetValue(CommonEntryType.BirthDate.GetKey(), birthDate.Value);
                    }
                }

                if (parameters.HasParameter(Parameters.Email))
                {
                    var email = parameters.GetParameter<string>(Parameters.Email);
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        adminProfile.SetValue(AdministratorEntryType.Email.GetKey(), email);
                    }
                }

                // Add the profile to the registry
                if (!_registry.AddProfile(adminProfile))
                {
                    return CommandResult.Fail("Failed to add administrator profile to registry");
                }

                // Register with authentication service
                _authService.Register(adminProfile, password);

                return CommandResult.Ok(
                    $"Administrator profile created successfully for '{username}' (ID: {adminProfile.Id})",
                    adminProfile);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to create administrator profile: {ex.Message}", ex);
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}