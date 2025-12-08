using System;
using System.Collections.Generic;

namespace API.CliniCore.Data.Entities.User
{
    /// <summary>
    /// EF Core entity for patient persistence.
    /// Inherits common fields (Id, Username, CreatedAt, Name, Address, BirthDate) from ProfileEntityBase.
    /// </summary>
    public class PatientEntity : ProfileEntityBase
    {
        // Patient-specific ProfileEntries
        public string Gender { get; set; } = string.Empty;
        public string? Race { get; set; }

        // Relationships
        public Guid? PrimaryPhysicianId { get; set; }

        // Stored as JSON
        public string AppointmentIdsJson { get; set; } = "[]";
        public string ClinicalDocumentIdsJson { get; set; } = "[]";
    }
}
