using System;
using System.Collections.Generic;

namespace Core.CliniCore.DTOs.Physicians
{
    /// <summary>
    /// Response DTO representing a physician profile.
    /// Inherits common fields (Id, Username, CreatedAt, Name, Address, BirthDate) from ProfileDtoBase.
    /// </summary>
    public class PhysicianDto : ProfileDtoBase
    {
        // Physician-specific ProfileEntries
        public string LicenseNumber { get; set; } = string.Empty;
        public DateTime GraduationDate { get; set; }
        public List<string> Specializations { get; set; } = new();

        // Computed properties
        public int PatientCount { get; set; }
        public int AppointmentCount { get; set; }
    }
}
