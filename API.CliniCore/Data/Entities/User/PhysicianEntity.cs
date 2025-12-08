using System;
using System.Collections.Generic;

namespace API.CliniCore.Data.Entities.User
{
    /// <summary>
    /// EF Core entity for physician persistence.
    /// Inherits common fields (Id, Username, CreatedAt, Name, Address, BirthDate) from ProfileEntityBase.
    /// </summary>
    public class PhysicianEntity : ProfileEntityBase
    {
        // Physician-specific ProfileEntries
        public string LicenseNumber { get; set; } = string.Empty;
        public DateTime GraduationDate { get; set; }

        // Stored as JSON (comma-separated specialization names)
        public string SpecializationsJson { get; set; } = "[]";

        // Relationships stored as JSON
        public string PatientIdsJson { get; set; } = "[]";
        public string AppointmentIdsJson { get; set; } = "[]";
    }
}
