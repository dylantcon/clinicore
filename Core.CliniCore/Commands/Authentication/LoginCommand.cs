using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Authentication.Byproducts;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Enumerations;

namespace Core.CliniCore.Commands.Authentication
{
    /// <summary>
    /// Command that authenticates a user and creates a new session.
    /// </summary>
    public class LoginCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "login";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="LoginCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the username.
            /// </summary>
            public const string Username = "username";

            /// <summary>
            /// Parameter key for the password.
            /// </summary>
            public const string Password = "password";
        }

        private readonly IAuthenticationService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginCommand"/> class.
        /// </summary>
        /// <param name="authService">The authentication service used to validate credentials.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="authService"/> is <c>null</c>.</exception>
        public LoginCommand(IAuthenticationService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        /// <inheritdoc />
        public override string Description => "Authenticates a user and creates a session";

        /// <inheritdoc />
        public override bool CanUndo => false; // Login cannot be undone, use logout instead

        /// <inheritdoc />
        public override Permission? GetRequiredPermission() => null; // Anyone can attempt login

        /// <inheritdoc />
        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Check for required parameters
            var missingParams = parameters.GetMissingRequired(Parameters.Username, Parameters.Password);
            if (missingParams.Any())
            {
                foreach (var error in missingParams)
                    result.AddError(error);
                return result;
            }

            // Validate username is not empty
            var username = parameters.GetParameter<string>(Parameters.Username);
            if (string.IsNullOrWhiteSpace(username))
            {
                result.AddError("Username cannot be empty");
            }

            // Validate password is not empty
            var password = parameters.GetParameter<string>(Parameters.Password);
            if (string.IsNullOrWhiteSpace(password))
            {
                result.AddError("Password cannot be empty");
            }

            return result;
        }

        /// <inheritdoc />
        protected override CommandValidationResult ValidateSpecific(CommandParameters parameters, SessionContext? session)
        {
            var result = CommandValidationResult.Success();

            // Check if already logged in
            if (session != null && !session.IsExpired())
            {
                result.AddWarning($"Already logged in as {session.Username}. Previous session will be replaced.");
            }

            return result;
        }

        /// <inheritdoc />
        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var username = parameters.GetRequiredParameter<string>(Parameters.Username);
                var password = parameters.GetRequiredParameter<string>(Parameters.Password);

                // Attempt authentication
                var authResult = _authService.Authenticate(username, password);

                if (!authResult.Success)
                {
                    LogFailedAttempt(username);
                    return CommandResult.Fail("Invalid username or password");
                }

                var userProfile = authResult.Profile!;

                // Check if the profile is valid
                if (!userProfile.IsValid)
                {
                    var errors = userProfile.GetValidationErrors();
                    return CommandResult.Fail(
                        $"User profile is invalid: {string.Join(", ", errors)}");
                }

                // Create session
                var newSession = SessionContext.CreateSession(userProfile);

                // Get last login time for welcome message
                var lastLogin = _authService.GetLastLoginTime(username);
                var welcomeMessage = lastLogin.HasValue
                    ? $"Welcome back, {userProfile.GetValue<string>("name")}! Last login: {lastLogin:yyyy-MM-dd HH:mm}"
                    : $"Welcome, {userProfile.GetValue<string>("name")}! This is your first login.";

                return CommandResult.Ok(welcomeMessage, newSession);
            }
            catch (AuthenticationException ex)
            {
                return CommandResult.Fail($"Authentication failed: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Login failed: {ex.Message}", ex);
            }
        }

        private void LogFailedAttempt(string username)
        {
            // In production, this would write to security audit log
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine($"[{timestamp}] Failed login attempt for user: {username}");
        }
    }
}