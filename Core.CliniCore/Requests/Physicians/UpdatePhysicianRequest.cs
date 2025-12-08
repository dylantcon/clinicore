using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Core.CliniCore.Requests.Physicians
{
    /// <summary>
    /// Request DTO for updating an existing physician
    /// </summary>
    public class UpdatePhysicianRequest
    {
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be at least 1 character")]
        public string? Name { get; set; }

        public string? Address { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? LicenseNumber { get; set; }

        public DateTime? GraduationDate { get; set; }

        public List<string>? Specializations { get; set; }

        // Relationship IDs for assignment operations
        public List<Guid>? PatientIds { get; set; }
        public List<Guid>? AppointmentIds { get; set; }
    }
}
