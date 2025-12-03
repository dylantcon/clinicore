using Core.CliniCore.ClinicalDoc;

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
    }
}
