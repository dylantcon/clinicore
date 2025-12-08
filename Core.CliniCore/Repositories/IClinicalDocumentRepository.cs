using Core.CliniCore.Domain.ClinicalDocumentation;
using Core.CliniCore.Domain.ClinicalDocumentation.ClinicalEntries;

namespace Core.CliniCore.Repositories
{
    /// <summary>
    /// Repository interface for clinical document operations.
    /// Extends generic repository with clinical documentation-specific queries.
    /// </summary>
    public interface IClinicalDocumentRepository : IRepository<ClinicalDocument>
    {
        /// <summary>
        /// Gets all clinical documents for a specific patient
        /// </summary>
        IEnumerable<ClinicalDocument> GetByPatient(Guid patientId);

        /// <summary>
        /// Gets all clinical documents authored by a specific physician
        /// </summary>
        IEnumerable<ClinicalDocument> GetByPhysician(Guid physicianId);

        /// <summary>
        /// Gets the clinical document associated with a specific appointment
        /// </summary>
        ClinicalDocument? GetByAppointment(Guid appointmentId);

        /// <summary>
        /// Gets all incomplete (draft) clinical documents
        /// </summary>
        IEnumerable<ClinicalDocument> GetIncomplete();

        /// <summary>
        /// Gets clinical documents within a date range
        /// </summary>
        IEnumerable<ClinicalDocument> GetByDateRange(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Searches clinical documents by diagnosis code (ICD-10)
        /// </summary>
        IEnumerable<ClinicalDocument> SearchByDiagnosis(string icd10Code);

        /// <summary>
        /// Searches clinical documents by medication name
        /// </summary>
        IEnumerable<ClinicalDocument> SearchByMedication(string medicationName);

        #region Entry-Level CRUD Operations

        // Observation operations
        ObservationEntry? GetObservation(Guid documentId, Guid entryId);
        void AddObservation(Guid documentId, ObservationEntry entry);
        void UpdateObservation(Guid documentId, ObservationEntry entry);
        void DeleteObservation(Guid documentId, Guid entryId);

        // Assessment operations
        AssessmentEntry? GetAssessment(Guid documentId, Guid entryId);
        void AddAssessment(Guid documentId, AssessmentEntry entry);
        void UpdateAssessment(Guid documentId, AssessmentEntry entry);
        void DeleteAssessment(Guid documentId, Guid entryId);

        // Diagnosis operations
        DiagnosisEntry? GetDiagnosis(Guid documentId, Guid entryId);
        void AddDiagnosis(Guid documentId, DiagnosisEntry entry);
        void UpdateDiagnosis(Guid documentId, DiagnosisEntry entry);
        void DeleteDiagnosis(Guid documentId, Guid entryId);

        // Plan operations
        PlanEntry? GetPlan(Guid documentId, Guid entryId);
        void AddPlan(Guid documentId, PlanEntry entry);
        void UpdatePlan(Guid documentId, PlanEntry entry);
        void DeletePlan(Guid documentId, Guid entryId);

        // Prescription operations
        PrescriptionEntry? GetPrescription(Guid documentId, Guid entryId);
        void AddPrescription(Guid documentId, PrescriptionEntry entry);
        void UpdatePrescription(Guid documentId, PrescriptionEntry entry);
        void DeletePrescription(Guid documentId, Guid entryId);

        #endregion
    }
}
