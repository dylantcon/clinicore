using Core.CliniCore.Domain.Enumerations;

namespace Core.CliniCore.Requests.ClinicalDocuments
{
    /// <summary>
    /// Request to create a new diagnosis entry
    /// </summary>
    public class CreateDiagnosisRequest
    {
        public Guid? Id { get; set; }  // Client-provided ID (optional, API will generate if null)
        public Guid? AuthorId { get; set; }  // Author/physician ID
        public string Content { get; set; } = string.Empty;  // Diagnosis description
        public string? ICD10Code { get; set; }
        public DiagnosisType Type { get; set; } = DiagnosisType.Working;
        public DiagnosisStatus Status { get; set; } = DiagnosisStatus.Active;
        public EntrySeverity Severity { get; set; } = EntrySeverity.Routine;
        public bool IsPrimary { get; set; }
        public DateTime? OnsetDate { get; set; }
    }

    /// <summary>
    /// Request to update an existing diagnosis entry
    /// </summary>
    public class UpdateDiagnosisRequest
    {
        public string? Content { get; set; }
        public string? ICD10Code { get; set; }
        public DiagnosisType? Type { get; set; }
        public DiagnosisStatus? Status { get; set; }
        public EntrySeverity? Severity { get; set; }
        public bool? IsPrimary { get; set; }
        public DateTime? OnsetDate { get; set; }
        public bool? IsActive { get; set; }
    }
}
