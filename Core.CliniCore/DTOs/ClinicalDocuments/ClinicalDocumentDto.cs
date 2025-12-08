using System;
using System.Collections.Generic;

namespace Core.CliniCore.DTOs.ClinicalDocuments
{
    /// <summary>
    /// Response DTO representing a clinical document with full details
    /// </summary>
    public class ClinicalDocumentDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public Guid PhysicianId { get; set; }
        public string? PhysicianName { get; set; }
        public Guid AppointmentId { get; set; }
        public string? ChiefComplaint { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; }

        // SOAP Note components - typed entry collections
        public List<ObservationDto> Observations { get; set; } = new();
        public List<AssessmentDto> Assessments { get; set; } = new();
        public List<DiagnosisDto> Diagnoses { get; set; } = new();
        public List<PrescriptionDto> Prescriptions { get; set; } = new();
        public List<PlanDto> Plans { get; set; } = new();
    }

    /// <summary>
    /// Summary DTO for listing clinical documents
    /// </summary>
    public class ClinicalDocumentSummaryDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public Guid PhysicianId { get; set; }
        public string? PhysicianName { get; set; }
        public string? ChiefComplaint { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsCompleted { get; set; }
        public int DiagnosisCount { get; set; }
        public int PrescriptionCount { get; set; }
    }

    /// <summary>
    /// Generic clinical entry DTO (for simple/legacy use cases)
    /// </summary>
    public class ClinicalEntryDto
    {
        public Guid Id { get; set; }
        public string EntryType { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime RecordedAt { get; set; }
    }
}
