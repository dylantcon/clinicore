// Core.CliniCore/Commands/Authentication/LogoutCommand.cs
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;

namespace Core.CliniCore.Commands.Authentication
{
    public class LogoutCommand : AbstractCommand
    {
        public LogoutCommand()
        {
        }

        public override string Description => "Ends the current user session";

        public override bool CanUndo => false; // Logout cannot be undone for security

        public override Permission? GetRequiredPermission() => null; // Anyone logged in can logout

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            // No parameters needed for logout
            return CommandValidationResult.Success();
        }

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
