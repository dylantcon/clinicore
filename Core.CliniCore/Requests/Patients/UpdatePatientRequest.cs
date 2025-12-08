using System;
using System.ComponentModel.DataAnnotations;

namespace Core.CliniCore.Requests.Patients
{
    /// <summary>
    /// Request DTO for updating an existing patient
    /// </summary>
    public class UpdatePatientRequest
    {
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be at least 1 character")]
        public string? Name { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? Gender { get; set; }

        public string? Race { get; set; }

        public string? Address { get; set; }

        public Guid? PrimaryPhysicianId { get; set; }
    }
}
