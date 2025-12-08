using API.CliniCore.Data.Entities.Clinical;
using System;
using System.Collections.Generic;

namespace API.CliniCore.Data.Entities.ClinicalEntries
{
    /// <summary>
    /// EF Core entity for clinical diagnoses
    /// </summary>
    public class DiagnosisEntity
    {
        public Guid Id { get; set; }
        public Guid ClinicalDocumentId { get; set; }
        public Guid AuthorId { get; set; }

        // Core content (diagnosis description)
        public string Content { get; set; } = string.Empty;

        // ICD-10 coding
        public string? ICD10Code { get; set; }

        // Diagnosis classification (enums stored as strings)
        public string Type { get; set; } = "Working";  // DiagnosisType
        public string Status { get; set; } = "Active";  // DiagnosisStatus
        public string Severity { get; set; } = "Routine";  // EntrySeverity
        public bool IsPrimary { get; set; }

        // Clinical timeline
        public DateTime? OnsetDate { get; set; }

        // Timestamps and status
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ClinicalDocumentEntity ClinicalDocument { get; set; } = null!;
        public ICollection<PrescriptionEntity> Prescriptions { get; set; } = new List<PrescriptionEntity>();
    }
}
