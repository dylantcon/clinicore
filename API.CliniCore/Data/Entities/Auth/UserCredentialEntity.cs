using System;

namespace API.CliniCore.Data.Entities.Auth
{
    /// <summary>
    /// EF Core entity for user authentication credentials.
    /// Id is the same as the associated profile's Id (1:1 relationship, shared identity).
    /// </summary>
    public class UserCredentialEntity
    {
        /// <summary>
        /// Primary key - same value as the associated profile's Id.
        /// </summary>
        public Guid Id { get; set; }

        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}
