using System;
using System.ComponentModel.DataAnnotations;

namespace Core.CliniCore.Requests.Physicians
{
    /// <summary>
    /// Request DTO for creating a new physician
    /// </summary>
    public class CreatePhysicianRequest
    {
        /// <summary>
        /// Optional: Client-provided ID for the profile. If not provided, server generates one.
        /// Required for client-server ID synchronization (e.g., credential linking).
        /// </summary>
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Birth date is required")]
        public DateTime BirthDate { get; set; }

        public string? Address { get; set; }

        [Required(ErrorMessage = "License number is required")]
        public string LicenseNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Graduation date is required")]
        public DateTime GraduationDate { get; set; }

        [Required(ErrorMessage = "At least one specialization is required")]
        [MinLength(1, ErrorMessage = "At least one specialization is required")]
        public List<string> Specializations { get; set; } = new();
    }
}
