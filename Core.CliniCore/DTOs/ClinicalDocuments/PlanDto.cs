using Core.CliniCore.Domain.Enumerations;

namespace Core.CliniCore.DTOs.ClinicalDocuments
{
    /// <summary>
    /// DTO for treatment plan entries with full field support
    /// </summary>
    public class PlanDto
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public Guid AuthorId { get; set; }

        // Core content
        public string Content { get; set; } = string.Empty;  // Plan description

        // Plan classification
        public PlanType Type { get; set; } = PlanType.Treatment;
        public PlanPriority Priority { get; set; } = PlanPriority.Routine;
        public EntrySeverity Severity { get; set; } = EntrySeverity.Routine;

        // Timeline
        public DateTime? TargetDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }

        // Instructions
        public string? FollowUpInstructions { get; set; }

        // Relationships
        public List<Guid> RelatedDiagnosisIds { get; set; } = new();

        // Timestamps and status
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
