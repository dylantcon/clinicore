namespace Core.CliniCore.Domain.Authentication.Representation
{
    /// <summary>
    /// Domain model representing user authentication credentials.
    /// Implements IIdentifiable with Id equal to the associated profile's Id (1:1 relationship).
    /// </summary>
    public class UserCredential : IIdentifiable
    {
        /// <summary>
        /// Unique identifier - same as the associated profile's Id (shared identity).
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Username for login (must be unique across all users).
        /// </summary>
        public string Username { get; init; } = string.Empty;

        /// <summary>
        /// Hashed password (never store plaintext).
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// User role: "Patient", "Physician", or "Administrator".
        /// </summary>
        public string Role { get; init; } = string.Empty;

        /// <summary>
        /// When the credential was created.
        /// </summary>
        public DateTime CreatedAt { get; init; }

        /// <summary>
        /// Last successful login time.
        /// </summary>
        public DateTime? LastLoginAt { get; set; }
    }
}
