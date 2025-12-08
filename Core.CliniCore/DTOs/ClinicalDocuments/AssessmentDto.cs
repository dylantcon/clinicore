using Core.CliniCore.Domain.Enumerations;

namespace Core.CliniCore.DTOs.ClinicalDocuments
{
    /// <summary>
    /// DTO for clinical assessments with full field support
    /// </summary>
    public class AssessmentDto
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public Guid AuthorId { get; set; }

        // Core content (clinical impression)
        public string Content { get; set; } = string.Empty;

        // Clinical status
        public PatientCondition Condition { get; set; } = PatientCondition.Stable;
        public Prognosis Prognosis { get; set; } = Prognosis.Good;
        public ConfidenceLevel Confidence { get; set; } = ConfidenceLevel.Moderate;
        public EntrySeverity Severity { get; set; } = EntrySeverity.Routine;

        // Flags
        public bool RequiresImmediateAction { get; set; }

        // Related items
        public List<string> DifferentialDiagnoses { get; set; } = new();
        public List<string> RiskFactors { get; set; } = new();

        // Timestamps and status
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
