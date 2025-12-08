// Core.CliniCore/Commands/Authentication/LogoutCommand.cs
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Enumerations;

namespace Core.CliniCore.Commands.Authentication
{
    /// <summary>
    /// Command that ends the current user session.
    /// </summary>
    public class LogoutCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "logout";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="LogoutCommand"/>.
        /// </summary>
        public static class Parameters
        {
            // LogoutCommand requires no parameters
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogoutCommand"/> class.
        /// </summary>
        public LogoutCommand()
        {
        }

        /// <inheritdoc />
        public override string Description => "Ends the current user session";

        /// <inheritdoc />
        public override bool CanUndo => false; // Logout cannot be undone for security

        /// <inheritdoc />
        public override Permission? GetRequiredPermission() => null; // Anyone logged in can logout

        /// <inheritdoc />
        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            // No parameters needed for logout
            return CommandValidationResult.Success();
        }

        /// <inheritdoc />
        protected override CommandValidationResult ValidateSpecific(CommandParameters parameters, SessionContext? session)
        {
            var result = CommandValidationResult.Success();

            if (session == null)
            {
                result.AddError("No active session to logout from");
            }
            else if (session.IsExpired())
            {
                result.AddWarning("Session has already expired");
            }

            return result;
        }

        /// <inheritdoc />
        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                if (session == null)
                {
                    return CommandResult.Fail("No active session to logout from");
                }

                // Capture session info for logout message
                var username = session.Username;
                var sessionDuration = session.SessionDuration;
                var logoutTime = DateTime.Now;

                // In a real system, we might:
                // - Invalidate any auth tokens
                // - Clear session from cache/database
                // - Log the logout event
                // - Save any pending work

                LogLogoutEvent(session, logoutTime);

                // Format nice logout message
                var durationStr = FormatDuration(sessionDuration);
                var message = $"Goodbye, {username}! You were logged in for {durationStr}.";

                // Return success with null data to signal session should be cleared
                return CommandResult.Ok(message, null);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Logout failed: {ex.Message}", ex);
            }
        }

        private void LogLogoutEvent(SessionContext session, DateTime logoutTime)
        {
            // In production, this would write to audit log
            var timestamp = logoutTime.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine($"[{timestamp}] User '{session.Username}' logged out after {session.SessionDuration:hh\\:mm\\:ss}");
        }

        private string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalDays >= 1)
            {
                return $"{(int)duration.TotalDays} day(s), {duration.Hours} hour(s), and {duration.Minutes} minute(s)";
            }
            else if (duration.TotalHours >= 1)
            {
                return $"{duration.Hours} hour(s) and {duration.Minutes} minute(s)";
            }
            else if (duration.TotalMinutes >= 1)
            {
                return $"{duration.Minutes} minute(s)";
            }
            else
            {
                return "less than a minute";
            }
        }
    }
}
