using System.ComponentModel.DataAnnotations;

namespace Core.CliniCore.Requests.Auth
{
    /// <summary>
    /// Request DTO for changing user password
    /// </summary>
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "New password must be at least 6 characters")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
