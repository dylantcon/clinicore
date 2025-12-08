using API.CliniCore.Data.Entities.Clinical;
using System;

namespace API.CliniCore.Data.Entities.ClinicalEntries
{
    /// <summary>
    /// EF Core entity for clinical observations
    /// </summary>
    public class ObservationEntity
    {
        public Guid Id { get; set; }
        public Guid ClinicalDocumentId { get; set; }
        public Guid AuthorId { get; set; }

        // Core content
        public string Content { get; set; } = string.Empty;
        public string Type { get; set; } = "PhysicalExam";  // ObservationType enum as string

        // Clinical details
        public string? BodySystem { get; set; }
        public bool IsAbnormal { get; set; }
        public string Severity { get; set; } = "Routine";  // EntrySeverity enum as string
        public string? ReferenceRange { get; set; }
        public string? Code { get; set; }  // LOINC or other coding

        // Numeric measurement
        public double? NumericValue { get; set; }
        public string? Unit { get; set; }

        // Vital signs stored as JSON
        public string? VitalSignsJson { get; set; }

        // Timestamps and status
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation property
        public ClinicalDocumentEntity ClinicalDocument { get; set; } = null!;
    }
}
