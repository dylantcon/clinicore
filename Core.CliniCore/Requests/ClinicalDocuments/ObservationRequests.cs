using Core.CliniCore.Domain.Enumerations;

namespace Core.CliniCore.Requests.ClinicalDocuments
{
    /// <summary>
    /// Request to create a new observation entry
    /// </summary>
    public class CreateObservationRequest
    {
        public Guid? Id { get; set; }
        public Guid? AuthorId { get; set; }
        public string Content { get; set; } = string.Empty;
        public ObservationType Type { get; set; } = ObservationType.PhysicalExam;
        public BodySystem? BodySystem { get; set; }
        public bool IsAbnormal { get; set; }
        public EntrySeverity Severity { get; set; } = EntrySeverity.Routine;
        public string? ReferenceRange { get; set; }
        public string? Code { get; set; }
        public double? NumericValue { get; set; }
        public string? Unit { get; set; }
        public Dictionary<string, string>? VitalSigns { get; set; }
    }

    /// <summary>
    /// Request to update an existing observation entry
    /// </summary>
    public class UpdateObservationRequest
    {
        public string? Content { get; set; }
        public ObservationType? Type { get; set; }
        public BodySystem? BodySystem { get; set; }
        public bool? IsAbnormal { get; set; }
        public EntrySeverity? Severity { get; set; }
        public string? ReferenceRange { get; set; }
        public string? Code { get; set; }
        public double? NumericValue { get; set; }
        public string? Unit { get; set; }
        public Dictionary<string, string>? VitalSigns { get; set; }
        public bool? IsActive { get; set; }
    }
}
