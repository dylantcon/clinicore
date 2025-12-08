using Core.CliniCore.Domain.Enumerations;

namespace Core.CliniCore.DTOs.ClinicalDocuments
{
    /// <summary>
    /// DTO for prescription entries with full domain field support
    /// </summary>
    public class PrescriptionDto
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public Guid AuthorId { get; set; }

        // Required: Link to supporting diagnosis
        public Guid DiagnosisId { get; set; }

        // Medication details
        public string MedicationName { get; set; } = string.Empty;
        public string? Dosage { get; set; }
        public DosageFrequency? Frequency { get; set; }
        public MedicationRoute Route { get; set; } = MedicationRoute.Oral;
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

        // Severity (for drug interactions, allergies)
        public EntrySeverity Severity { get; set; } = EntrySeverity.Routine;

        // Timestamps and status
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
