using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Domain.Authentication
{
    /// <summary>
    /// Defines the contract for authentication operations in the CliniCore system
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Authenticates a user with username and password
        /// </summary>
        /// <param name="username">The username to authenticate</param>
        /// <param name="password">The password to verify</param>
        /// <returns>An authenticated user profile if successful, null otherwise</returns>
        IUserProfile? Authenticate(string username, string password);

        /// <summary>
        /// Registers a new user in the system
        /// </summary>
        /// <param name="profile">The user profile to register</param>
        /// <param name="password">The password for the new user</param>
        /// <returns>True if registration successful, false otherwise</returns>
        bool Register(IUserProfile profile, string password);

        /// <summary>
        /// Changes a user's password
        /// </summary>
        /// <param name="username">The username whose password to change</param>
        /// <param name="currentPassword">The current password for verification</param>
        /// <param name="newPassword">The new password to set</param>
        /// <returns>True if password change successful, false otherwise</returns>
        bool ChangePassword(string username, string currentPassword, string newPassword);

        /// <summary>
        /// Validates if a password meets security requirements
        /// </summary>
        /// <param name="password">The password to validate</param>
        /// <returns>True if password meets requirements, false otherwise</returns>
        bool ValidatePasswordStrength(string password);

        /// <summary>
        /// Checks if a username already exists in the system
        /// </summary>
        /// <param name="username">The username to check</param>
        /// <returns>True if username exists, false otherwise</returns>
        bool UserExists(string username);

        /// <summary>
        /// Locks a user account (admin function)
        /// </summary>
        /// <param name="username">The username to lock</param>
        /// <returns>True if account locked successfully, false otherwise</returns>
        bool LockAccount(string username);

        /// <summary>
        /// Unlocks a user account (admin function)
        /// </summary>
        /// <param name="username">The username to unlock</param>
        /// <returns>True if account unlocked successfully, false otherwise</returns>
        bool UnlockAccount(string username);

        /// <summary>
        /// Gets the last login time for a user
        /// </summary>
        /// <param name="username">The username to check</param>
        /// <returns>The last login DateTime, or null if never logged in</returns>
        DateTime? GetLastLoginTime(string username);
    }
}
