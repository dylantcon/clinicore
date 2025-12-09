using Core.CliniCore.Domain.Enumerations;

namespace Core.CliniCore.Requests.ClinicalDocuments
{
    /// <summary>
    /// Request to create a new plan entry
    /// </summary>
    public class CreatePlanRequest
    {
        public Guid? Id { get; set; }
        public Guid? AuthorId { get; set; }
        public string Content { get; set; } = string.Empty;  // Plan description
        public PlanType Type { get; set; } = PlanType.Treatment;
        public PlanPriority Priority { get; set; } = PlanPriority.Routine;
        public EntrySeverity Severity { get; set; } = EntrySeverity.Routine;
        public DateTime? TargetDate { get; set; }
        public string? FollowUpInstructions { get; set; }
        public List<Guid>? RelatedDiagnosisIds { get; set; }
    }

    /// <summary>
    /// Request to update an existing plan entry
    /// </summary>
    public class UpdatePlanRequest
    {
        public string? Content { get; set; }
        public PlanType? Type { get; set; }
        public PlanPriority? Priority { get; set; }
        public EntrySeverity? Severity { get; set; }
        public DateTime? TargetDate { get; set; }
        public bool? IsCompleted { get; set; }
        public string? FollowUpInstructions { get; set; }
        public List<Guid>? RelatedDiagnosisIds { get; set; }
        public bool? IsActive { get; set; }
    }
}
