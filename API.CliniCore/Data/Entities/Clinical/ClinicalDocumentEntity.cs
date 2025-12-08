using API.CliniCore.Data.Entities.ClinicalEntries;
using System;
using System.Collections.Generic;

namespace API.CliniCore.Data.Entities.Clinical
{
    /// <summary>
    /// EF Core entity for clinical document persistence.
    /// </summary>
    public class ClinicalDocumentEntity
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid PhysicianId { get; set; }
        public Guid AppointmentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? ChiefComplaint { get; set; }

        // Navigation properties - relational entry collections
        public ICollection<ObservationEntity> Observations { get; set; } = [];
        public ICollection<AssessmentEntity> Assessments { get; set; } = [];
        public ICollection<DiagnosisEntity> Diagnoses { get; set; } = [];
        public ICollection<PlanEntity> Plans { get; set; } = [];
        public ICollection<PrescriptionEntity> Prescriptions { get; set; } = [];
    }
}
