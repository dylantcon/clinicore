using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Domain.Authentication
{
    /// <summary>
    /// Basic in-memory authentication service implementation for Assignment 1
    /// Note: This is a simplified implementation. Production would use proper database and hashing
    /// </summary>
    public class BasicAuthenticationService : IAuthenticationService
    {
        private class UserCredential
        {
            public string Username { get; set; } = string.Empty;
            public string PasswordHash { get; set; } = string.Empty;
            public IUserProfile Profile { get; set; } = null!;
            public bool IsLocked { get; set; }
            public DateTime? LastLoginTime { get; set; }
        }

        private readonly Dictionary<string, UserCredential> _users;
        private readonly object _lock = new object();

        public BasicAuthenticationService()
        {
            _users = new Dictionary<string, UserCredential>(StringComparer.OrdinalIgnoreCase);
            InitializeDefaultUsers();
        }

        /// <summary>
        /// Initializes system with default admin account for testing
        /// </summary>
        private void InitializeDefaultUsers()
        {
            // Create a default admin for initial system access
            var adminProfile = new AdministratorProfile
            {
                Username = "admin"
            };
            adminProfile.SetValue("name", "System Administrator");
            adminProfile.SetValue("address", "123 Admin St, Medical Center, MC 12345");
            adminProfile.SetValue("birthdate", new DateTime(1980, 1, 1));
            adminProfile.SetValue("user_email", "admin@clinicore.local");

            Register(adminProfile, "admin123"); // Default password for assignment
        }

        public IUserProfile? Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            lock (_lock)
            {
                if (!_users.TryGetValue(username, out var credential))
                    return null;

                if (credential.IsLocked)
                    return null;

                if (!VerifyPassword(password, credential.PasswordHash))
                    return null;

                credential.LastLoginTime = DateTime.Now;
                return credential.Profile;
            }
        }

        public bool Register(IUserProfile profile, string password)
        {
            if (profile == null || string.IsNullOrWhiteSpace(password))
                return false;

            if (!profile.IsValid)
                return false;

            if (!ValidatePasswordStrength(password))
                return false;

            lock (_lock)
            {
                if (_users.ContainsKey(profile.Username))
                    return false;

                var credential = new UserCredential
                {
                    Username = profile.Username,
                    PasswordHash = HashPassword(password),
                    Profile = profile,
                    IsLocked = false
                };

                _users[profile.Username] = credential;
                return true;
            }
        }

        public bool ChangePassword(string username, string currentPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(currentPassword) ||
                string.IsNullOrWhiteSpace(newPassword))
                return false;

            if (!ValidatePasswordStrength(newPassword))
                return false;

            lock (_lock)
            {
                if (!_users.TryGetValue(username, out var credential))
                    return false;

                if (!VerifyPassword(currentPassword, credential.PasswordHash))
                    return false;

                credential.PasswordHash = HashPassword(newPassword);
                return true;
            }
        }

        public bool ValidatePasswordStrength(string password)
        {
            // Simple validation for Assignment 1
            // Production would have more complex requirements
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (password.Length < 6)
                return false;

            // Could add more rules: uppercase, lowercase, numbers, special chars
            return true;
        }

        public bool UserExists(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            lock (_lock)
            {
                return _users.ContainsKey(username);
            }
        }

        public bool LockAccount(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            lock (_lock)
            {
                if (!_users.TryGetValue(username, out var credential))
                    return false;

                credential.IsLocked = true;
                return true;
            }
        }

        public bool UnlockAccount(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            lock (_lock)
            {
                if (!_users.TryGetValue(username, out var credential))
                    return false;

                credential.IsLocked = false;
                return true;
            }
        }

        public DateTime? GetLastLoginTime(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            lock (_lock)
            {
                if (!_users.TryGetValue(username, out var credential))
                    return null;

                return credential.LastLoginTime;
            }
        }

        #region Password Hashing Helpers

        /// <summary>
        /// Simple password hashing for Assignment 1
        /// Production would use BCrypt, Argon2, or similar
        /// </summary>
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                // Add a simple salt for basic security
                var saltedPassword = $"CliniCore_{password}_Salt2024";
                var bytes = Encoding.UTF8.GetBytes(saltedPassword);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Verifies a password against a hash
        /// </summary>
        private bool VerifyPassword(string password, string hash)
        {
            var passwordHash = HashPassword(password);
            return passwordHash == hash;
        }

        #endregion

        #region Helper Methods for Testing

        /// <summary>
        /// Gets all registered users (for debugging/testing only)
        /// </summary>
        public IEnumerable<string> GetAllUsernames()
        {
            lock (_lock)
            {
                return _users.Keys.ToList();
            }
        }

        /// <summary>
        /// Clears all users except the default admin (for testing)
        /// </summary>
        public void ResetToDefaults()
        {
            lock (_lock)
            {
                _users.Clear();
                InitializeDefaultUsers();
            }
        }

        #endregion
    }
}