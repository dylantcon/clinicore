using System.Security.Cryptography;
using System.Text;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Authentication.Byproducts;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Users;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.Repositories;

namespace Core.CliniCore.Service
{
    /// <summary>
    /// Authentication service implementation that delegates to ICredentialRepository.
    /// Provides password hashing and verification utilities.
    /// </summary>
    public class BasicAuthenticationService(
        ICredentialRepository credentialRepository,
        IPatientRepository patientRepository,
        IPhysicianRepository physicianRepository,
        IAdministratorRepository administratorRepository) : IAuthenticationService
    {
        private readonly ICredentialRepository _credentialRepository = credentialRepository ?? throw new ArgumentNullException(nameof(credentialRepository));
        private readonly IPatientRepository _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
        private readonly IPhysicianRepository _physicianRepository = physicianRepository ?? throw new ArgumentNullException(nameof(physicianRepository));
        private readonly IAdministratorRepository _administratorRepository = administratorRepository ?? throw new ArgumentNullException(nameof(administratorRepository));

        public AuthenticationResult Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return AuthenticationResult.Fail(AuthenticationFailureReason.InvalidCredentials);

            var credential = _credentialRepository.GetByUsername(username);
            if (credential == null)
                return AuthenticationResult.Fail(AuthenticationFailureReason.InvalidCredentials);

            if (!VerifyPassword(password, credential.PasswordHash))
                return AuthenticationResult.Fail(AuthenticationFailureReason.InvalidCredentials);

            // Update last login time
            credential.LastLoginAt = DateTime.UtcNow;
            _credentialRepository.Update(credential);

            // Resolve the profile based on role
            var profile = ResolveProfile(credential);
            if (profile == null)
                return AuthenticationResult.Fail(AuthenticationFailureReason.InvalidCredentials);

            return AuthenticationResult.Ok(profile);
        }

        public bool Register(IUserProfile profile, string password)
        {
            if (profile == null || string.IsNullOrWhiteSpace(password))
                return false;

            if (!profile.IsValid)
                return false;

            if (!ValidatePasswordStrength(password))
                return false;

            var role = profile switch
            {
                PatientProfile => nameof(UserRole.Patient),
                PhysicianProfile => nameof(UserRole.Physician),
                AdministratorProfile => nameof(UserRole.Administrator),
                _ => throw new ArgumentException($"Unknown profile type: {profile.GetType().Name}")
            };

            // Use the repository's Register method which handles password hashing
            // and works correctly for both local and remote repositories
            var credential = _credentialRepository.Register(profile.Id, profile.Username, password, role);
            return credential != null;
        }

        public bool ChangePassword(string username, string currentPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(currentPassword) ||
                string.IsNullOrWhiteSpace(newPassword))
                return false;

            if (!ValidatePasswordStrength(newPassword))
                return false;

            var credential = _credentialRepository.GetByUsername(username);
            if (credential == null)
                return false;

            if (!VerifyPassword(currentPassword, credential.PasswordHash))
                return false;

            credential.PasswordHash = HashPassword(newPassword);
            _credentialRepository.Update(credential);
            return true;
        }

        public bool ValidatePasswordStrength(string password)
        {
            // Simple validation for Assignment 1
            // Production would have more complex requirements
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (password.Length < 6)
                return false;

            return true;
        }

        public bool UserExists(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            return _credentialRepository.Exists(username);
        }

        public DateTime? GetLastLoginTime(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            var credential = _credentialRepository.GetByUsername(username);
            return credential?.LastLoginAt;
        }

        #region Profile Resolution

        /// <summary>
        /// Resolves the user profile from the credential's Id and Role.
        /// </summary>
        private IUserProfile? ResolveProfile(UserCredential credential)
        {
            return credential.Role switch
            {
                nameof(UserRole.Patient) => _patientRepository.GetById(credential.Id),
                nameof(UserRole.Physician) => _physicianRepository.GetById(credential.Id),
                nameof(UserRole.Administrator) => _administratorRepository.GetById(credential.Id),
                _ => null
            };
        }

        #endregion

        #region Password Hashing Helpers

        /// <summary>
        /// Simple password hashing for Assignment 1.
        /// Production would use BCrypt, Argon2, or similar.
        /// </summary>
        public static string HashPassword(string password)
        {
            // Add a simple salt for basic security
            var saltedPassword = $"CliniCore_{password}_Salt2025";
            var bytes = Encoding.UTF8.GetBytes(saltedPassword);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Verifies a password against a hash
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            var passwordHash = HashPassword(password);
            return passwordHash == hash;
        }

        #endregion

        #region Helper Methods for Testing

        /// <summary>
        /// Gets all registered usernames (for debugging/testing only)
        /// </summary>
        public IEnumerable<string> GetAllUsernames()
        {
            return _credentialRepository.GetAll().Select(c => c.Username);
        }

        #endregion
    }
}
