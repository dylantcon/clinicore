using System;
using System.ComponentModel.DataAnnotations;

namespace Core.CliniCore.Requests.Patients
{
    /// <summary>
    /// Request DTO for creating a new patient
    /// </summary>
    public class CreatePatientRequest
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

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = string.Empty;

        public string? Race { get; set; }

        public string? Address { get; set; }

        public Guid? PrimaryPhysicianId { get; set; }
    }
}
