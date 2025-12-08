using Core.CliniCore.Domain.Enumerations;

namespace Core.CliniCore.DTOs.ClinicalDocuments
{
    /// <summary>
    /// DTO for diagnosis entries with full domain field support
    /// </summary>
    public class DiagnosisDto
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public Guid AuthorId { get; set; }

        // Core content
        public string Content { get; set; } = string.Empty;  // Diagnosis description

        // ICD-10 coding
        public string? ICD10Code { get; set; }

        // Diagnosis classification
        public DiagnosisType Type { get; set; } = DiagnosisType.Working;
        public DiagnosisStatus Status { get; set; } = DiagnosisStatus.Active;
        public EntrySeverity Severity { get; set; } = EntrySeverity.Routine;
        public bool IsPrimary { get; set; }

        // Clinical timeline
        public DateTime? OnsetDate { get; set; }

        // Relationships
        public List<Guid> RelatedPrescriptionIds { get; set; } = new();
        public List<Guid> SupportingObservationIds { get; set; } = new();

        // Timestamps and status
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
