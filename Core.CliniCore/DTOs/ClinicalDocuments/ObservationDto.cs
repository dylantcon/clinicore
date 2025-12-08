using Core.CliniCore.Domain.Enumerations;

namespace Core.CliniCore.DTOs.ClinicalDocuments
{
    /// <summary>
    /// DTO for clinical observations with full field support
    /// </summary>
    public class ObservationDto
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public Guid AuthorId { get; set; }

        // Core content
        public string Content { get; set; } = string.Empty;
        public ObservationType Type { get; set; } = ObservationType.PhysicalExam;

        // Clinical details
        public BodySystem? BodySystem { get; set; }
        public bool IsAbnormal { get; set; }
        public EntrySeverity Severity { get; set; } = EntrySeverity.Routine;
        public string? ReferenceRange { get; set; }
        public string? Code { get; set; }  // LOINC or other coding

        // Numeric measurement
        public double? NumericValue { get; set; }
        public string? Unit { get; set; }

        // Vital signs as key-value pairs
        public Dictionary<string, string>? VitalSigns { get; set; }

        // Timestamps and status
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
