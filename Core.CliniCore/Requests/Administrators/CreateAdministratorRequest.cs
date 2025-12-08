using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Core.CliniCore.Requests.Administrators
{
    /// <summary>
    /// Request DTO for creating a new administrator
    /// </summary>
    public class CreateAdministratorRequest
    {
        /// <summary>
        /// Optional: Client-provided ID for the profile. If not provided, server generates one.
        /// Required for client-server ID synchronization (e.g., credential linking).
        /// </summary>
        public Guid? Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        // Common ProfileEntries
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Address { get; set; }

        public DateTime BirthDate { get; set; }

        // Administrator-specific ProfileEntry
        public string? Email { get; set; }

        // Direct properties
        public string Department { get; set; } = "Administration";

        public List<string> Permissions { get; set; } = new();
    }
}
