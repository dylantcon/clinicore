using Core.CliniCore.Domain.Enumerations;

namespace Core.CliniCore.Requests.ClinicalDocuments
{
    /// <summary>
    /// Request to create a new assessment entry
    /// </summary>
    public class CreateAssessmentRequest
    {
        public string Content { get; set; } = string.Empty;  // Clinical impression
        public PatientCondition Condition { get; set; } = PatientCondition.Stable;
        public Prognosis Prognosis { get; set; } = Prognosis.Good;
        public ConfidenceLevel Confidence { get; set; } = ConfidenceLevel.Moderate;
        public EntrySeverity Severity { get; set; } = EntrySeverity.Routine;
        public bool RequiresImmediateAction { get; set; }
        public List<string>? DifferentialDiagnoses { get; set; }
        public List<string>? RiskFactors { get; set; }
    }

    /// <summary>
    /// Request to update an existing assessment entry
    /// </summary>
    public class UpdateAssessmentRequest
    {
        public string? Content { get; set; }
        public PatientCondition? Condition { get; set; }
        public Prognosis? Prognosis { get; set; }
        public ConfidenceLevel? Confidence { get; set; }
        public EntrySeverity? Severity { get; set; }
        public bool? RequiresImmediateAction { get; set; }
        public List<string>? DifferentialDiagnoses { get; set; }
        public List<string>? RiskFactors { get; set; }
        public bool? IsActive { get; set; }
    }
}
