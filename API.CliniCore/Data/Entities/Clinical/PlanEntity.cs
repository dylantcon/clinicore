using API.CliniCore.Data.Entities.Clinical;
using System;

namespace API.CliniCore.Data.Entities.ClinicalEntries
{
    /// <summary>
    /// EF Core entity for treatment plans
    /// </summary>
    public class PlanEntity
    {
        public Guid Id { get; set; }
        public Guid ClinicalDocumentId { get; set; }
        public Guid AuthorId { get; set; }

        // Core content (plan description)
        public string Content { get; set; } = string.Empty;

        // Plan classification (enums stored as strings)
        public string Type { get; set; } = "Treatment";  // PlanType
        public string Priority { get; set; } = "Routine";  // PlanPriority
        public string Severity { get; set; } = "Routine";  // EntrySeverity

        // Timeline
        public DateTime? TargetDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }

        // Instructions
        public string? FollowUpInstructions { get; set; }

        // Related diagnoses stored as JSON array of Guids
        public string? RelatedDiagnosisIdsJson { get; set; }

        // Timestamps and status
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation property
        public ClinicalDocumentEntity ClinicalDocument { get; set; } = null!;
    }
}
