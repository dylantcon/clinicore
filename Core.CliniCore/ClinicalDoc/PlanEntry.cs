// Core.CliniCore/ClinicalDoc/PlanEntry.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.ClinicalDoc
{
    /// <summary>
    /// Represents a treatment plan entry
    /// </summary>
    public class PlanEntry : AbstractClinicalEntry
    {
        public PlanEntry(Guid authorId, string planDescription)
            : base(authorId, planDescription)
        {
            RelatedDiagnoses = new List<Guid>();
        }

        public override ClinicalEntryType EntryType => ClinicalEntryType.Plan;

        /// <summary>
        /// Type of plan item
        /// </summary>
        public PlanType Type { get; set; } = PlanType.Treatment;

        /// <summary>
        /// When this plan item should be completed
        /// </summary>
        public DateTime? TargetDate { get; set; }

        /// <summary>
        /// Whether this plan item has been completed
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// When the plan item was completed
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// Priority of this plan item
        /// </summary>
        public PlanPriority Priority { get; set; } = PlanPriority.Routine;

        /// <summary>
        /// Diagnoses this plan addresses
        /// </summary>
        public List<Guid> RelatedDiagnoses { get; private set; }

        /// <summary>
        /// Follow-up instructions
        /// </summary>
        public string? FollowUpInstructions { get; set; }

        /// <summary>
        /// Marks the plan item as completed
        /// </summary>
        public void MarkCompleted()
        {
            IsCompleted = true;
            CompletedDate = DateTime.Now;
            ModifiedAt = DateTime.Now;
        }

        public override string GetDisplayString()
        {
            var priorityStr = Priority != PlanPriority.Routine ? $"[{Priority}] " : "";
            var typeStr = $"[{Type}] ";
            var statusStr = IsCompleted ? " ✓ COMPLETED" : "";
            var targetStr = TargetDate.HasValue && !IsCompleted
                ? $" (Due: {TargetDate:yyyy-MM-dd})"
                : "";

            return $"{priorityStr}{typeStr}{base.Content}{targetStr}{statusStr}";
        }

        public override List<string> GetValidationErrors()
        {
            var errors = base.GetValidationErrors();

            if (IsCompleted && !CompletedDate.HasValue)
            {
                errors.Add("Completed plan items must have a completion date");
            }

            if (TargetDate.HasValue && TargetDate < CreatedAt)
            {
                errors.Add("Target date cannot be before creation date");
            }

            return errors;
        }
    }

    public enum PlanType
    {
        Treatment,          // Medical treatment plan
        Diagnostic,         // Diagnostic tests to order
        Referral,          // Referral to specialist
        FollowUp,          // Follow-up appointment
        PatientEducation,  // Education provided/needed
        Procedure,         // Procedure to perform
        Monitoring,        // Ongoing monitoring
        Prevention         // Preventive care
    }

    public enum PlanPriority
    {
        Routine,
        High,
        Urgent,
        Emergency
    }
}
