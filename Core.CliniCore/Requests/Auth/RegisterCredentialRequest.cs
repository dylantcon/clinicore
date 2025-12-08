using System;
using System.ComponentModel.DataAnnotations;

namespace Core.CliniCore.Requests.Auth
{
    /// <summary>
    /// Request DTO for registering new user credentials
    /// </summary>
    public class RegisterCredentialRequest
    {
        /// <summary>
        /// The Id of the associated user profile (shared identity)
        /// </summary>
        [Required(ErrorMessage = "Profile Id is required")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = string.Empty;
    }
}
