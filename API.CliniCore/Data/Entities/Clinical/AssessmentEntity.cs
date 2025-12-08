using System;

namespace API.CliniCore.Data.Entities.Clinical
{
    /// <summary>
    /// EF Core entity for clinical assessments
    /// </summary>
    public class AssessmentEntity
    {
        public Guid Id { get; set; }
        public Guid ClinicalDocumentId { get; set; }
        public Guid AuthorId { get; set; }

        // Core content (clinical impression)
        public string Content { get; set; } = string.Empty;

        // Clinical status (enums stored as strings)
        public string Condition { get; set; } = "Stable";  // PatientCondition
        public string Prognosis { get; set; } = "Good";  // Prognosis
        public string Confidence { get; set; } = "Moderate";  // ConfidenceLevel
        public string Severity { get; set; } = "Routine";  // EntrySeverity

        // Flags
        public bool RequiresImmediateAction { get; set; }

        // Related items stored as JSON arrays
        public string? DifferentialDiagnosesJson { get; set; }
        public string? RiskFactorsJson { get; set; }

        // Timestamps and status
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation property
        public ClinicalDocumentEntity ClinicalDocument { get; set; } = null!;
    }
}
