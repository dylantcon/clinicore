using System;
using System.Collections.Generic;

namespace Core.CliniCore.DTOs.Patients
{
    /// <summary>
    /// Response DTO representing a patient profile.
    /// Inherits common fields (Id, Username, CreatedAt, Name, Address, BirthDate) from ProfileDtoBase.
    /// </summary>
    public class PatientDto : ProfileDtoBase
    {
        // Patient-specific ProfileEntries
        public string Gender { get; set; } = string.Empty;
        public string? Race { get; set; }

        // Relationships
        public Guid? PrimaryPhysicianId { get; set; }
        public string? PrimaryPhysicianName { get; set; }
        public List<Guid> AppointmentIds { get; set; } = new();
        public List<Guid> ClinicalDocumentIds { get; set; } = new();

        // Computed properties (convenience for display)
        public int AppointmentCount { get; set; }
        public int ClinicalDocumentCount { get; set; }
    }
}
