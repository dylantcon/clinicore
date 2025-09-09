using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Domain.Authentication
{
    /// <summary>
    /// Exception thrown when authentication fails
    /// </summary>
    public class AuthenticationException : Exception
    {
        public enum FailureReason
        {
            InvalidCredentials,
            AccountLocked,
            AccountNotFound,
            SessionExpired,
            PasswordExpired,
            InvalidPassword,
            Unknown
        }

        public FailureReason Reason { get; }
        public string? Username { get; }
        public DateTime OccurredAt { get; }

        public AuthenticationException(
            FailureReason reason,
            string? username = null,
            string? message = null)
            : base(message ?? GetDefaultMessage(reason))
        {
            Reason = reason;
            Username = username;
            OccurredAt = DateTime.Now;
        }

        public AuthenticationException(
            FailureReason reason,
            string? username,
            string message,
            Exception innerException)
            : base(message, innerException)
        {
            Reason = reason;
            Username = username;
            OccurredAt = DateTime.Now;
        }

        private static string GetDefaultMessage(FailureReason reason)
        {
            return reason switch
            {
                FailureReason.InvalidCredentials => "Invalid username or password.",
                FailureReason.AccountLocked => "Account is locked. Please contact an administrator.",
                FailureReason.AccountNotFound => "Account not found.",
                FailureReason.SessionExpired => "Session has expired. Please log in again.",
                FailureReason.PasswordExpired => "Password has expired. Please change your password.",
                FailureReason.InvalidPassword => "Password does not meet security requirements.",
                FailureReason.Unknown => "An unknown authentication error occurred.",
                _ => "Authentication failed."
            };
        }
    }
}