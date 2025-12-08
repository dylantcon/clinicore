using System.ComponentModel.DataAnnotations;

namespace Core.CliniCore.Requests.Auth
{
    /// <summary>
    /// Request DTO for validating user credentials (login)
    /// </summary>
    public class ValidateCredentialRequest
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}
