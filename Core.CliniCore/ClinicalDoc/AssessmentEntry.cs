// Core.CliniCore/ClinicalDoc/AssessmentEntry.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.ClinicalDoc
{
    /// <summary>
    /// Represents a clinical assessment/impression
    /// </summary>
    public class AssessmentEntry : AbstractClinicalEntry
    {
        public AssessmentEntry(Guid authorId, string assessment)
            : base(authorId, assessment)
        {
            DifferentialDiagnoses = new List<string>();
            RiskFactors = new List<string>();
        }

        public override ClinicalEntryType EntryType => ClinicalEntryType.Assessment;

        /// <summary>
        /// Clinical impression summary
        /// </summary>
        public string ClinicalImpression
        {
            get => Content;
            set => Content = value;
        }

        /// <summary>
        /// Patient's overall condition
        /// </summary>
        public PatientCondition Condition { get; set; } = PatientCondition.Stable;

        /// <summary>
        /// Prognosis assessment
        /// </summary>
        public Prognosis Prognosis { get; set; } = Prognosis.Good;

        /// <summary>
        /// List of differential diagnoses being considered
        /// </summary>
        public List<string> DifferentialDiagnoses { get; private set; }

        /// <summary>
        /// Identified risk factors
        /// </summary>
        public List<string> RiskFactors { get; private set; }

        /// <summary>
        /// Whether immediate intervention is needed
        /// </summary>
        public bool RequiresImmediateAction { get; set; }

        /// <summary>
        /// Confidence level in the assessment
        /// </summary>
        public ConfidenceLevel Confidence { get; set; } = ConfidenceLevel.Moderate;

        public override string GetDisplayString()
        {
            var conditionStr = $"[{Condition}] ";
            var actionStr = RequiresImmediateAction ? "[IMMEDIATE ACTION REQUIRED] " : "";

            var assessment = $"{actionStr}{conditionStr}{base.Content}";

            if (DifferentialDiagnoses.Any())
            {
                assessment += $"\nDifferential: {string.Join(", ", DifferentialDiagnoses)}";
            }

            if (RiskFactors.Any())
            {
                assessment += $"\nRisk Factors: {string.Join(", ", RiskFactors)}";
            }

            return assessment;
        }

        public override List<string> GetValidationErrors()
        {
            var errors = base.GetValidationErrors();

            if (RequiresImmediateAction && Severity < EntrySeverity.Urgent)
            {
                errors.Add("Entries requiring immediate action should have Urgent or higher severity");
            }

            if (Condition == PatientCondition.Critical && Prognosis == Prognosis.Excellent)
            {
                errors.Add("Critical condition is inconsistent with excellent prognosis");
            }

            return errors;
        }
    }

    public enum PatientCondition
    {
        Stable,
        Improving,
        Unchanged,
        Worsening,
        Critical
    }

    public enum Prognosis
    {
        Excellent,
        Good,
        Fair,
        Guarded,
        Poor
    }

    public enum ConfidenceLevel
    {
        Low,
        Moderate,
        High,
        Certain
    }
}
