using API.CliniCore.Data.Entities.Clinical;
using System;

namespace API.CliniCore.Data.Entities.ClinicalEntries
{
    /// <summary>
    /// EF Core entity for prescriptions
    /// </summary>
    public class PrescriptionEntity
    {
        public Guid Id { get; set; }
        public Guid ClinicalDocumentId { get; set; }
        public Guid AuthorId { get; set; }

        // Required: Link to supporting diagnosis
        public Guid DiagnosisId { get; set; }

        // Medication details
        public string MedicationName { get; set; } = string.Empty;
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public string? Route { get; set; } = "Oral";
        public string? Duration { get; set; }

        // Prescription options
        public int Refills { get; set; }
        public bool GenericAllowed { get; set; } = true;

        // Controlled substance tracking
        public int? DEASchedule { get; set; }
        public DateTime? ExpirationDate { get; set; }

        // Coding
        public string? NDCCode { get; set; }

        // Instructions
        public string? Instructions { get; set; }

        // Severity
        public string Severity { get; set; } = "Routine";  // EntrySeverity

        // Timestamps and status
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ClinicalDocumentEntity ClinicalDocument { get; set; } = null!;
        public DiagnosisEntity Diagnosis { get; set; } = null!;
    }
}
