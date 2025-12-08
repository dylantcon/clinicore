using System;

namespace Core.CliniCore.DTOs.Auth
{
    /// <summary>
    /// DTO representing user credentials for API communication.
    /// </summary>
    public class UserCredentialDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}
