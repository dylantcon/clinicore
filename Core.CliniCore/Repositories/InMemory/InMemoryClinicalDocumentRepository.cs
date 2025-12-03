using Core.CliniCore.ClinicalDoc;

namespace Core.CliniCore.Repositories.InMemory
{
    /// <summary>
    /// In-memory implementation of IClinicalDocumentRepository.
    /// Provides clinical document-specific query operations.
    /// </summary>
    public class InMemoryClinicalDocumentRepository : InMemoryRepositoryBase<ClinicalDocument>, IClinicalDocumentRepository
    {
        /// <summary>
        /// Gets all clinical documents for a specific patient
        /// </summary>
        public IEnumerable<ClinicalDocument> GetByPatient(Guid patientId)
        {
            lock (_lock)
            {
                return _entities.Values
                    .Where(d => d.PatientId == patientId)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToList();
            }
        }

        /// <summary>
        /// Gets all clinical documents authored by a specific physician
        /// </summary>
        public IEnumerable<ClinicalDocument> GetByPhysician(Guid physicianId)
        {
            lock (_lock)
            {
                return _entities.Values
                    .Where(d => d.PhysicianId == physicianId)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToList();
            }
        }

        /// <summary>
        /// Gets the clinical document associated with a specific appointment
        /// </summary>
        public ClinicalDocument? GetByAppointment(Guid appointmentId)
        {
            lock (_lock)
            {
                return _entities.Values
                    .FirstOrDefault(d => d.AppointmentId == appointmentId);
            }
        }

        /// <summary>
        /// Gets all incomplete (draft) clinical documents
        /// </summary>
        public IEnumerable<ClinicalDocument> GetIncomplete()
        {
            lock (_lock)
            {
                return _entities.Values
                    .Where(d => !d.IsCompleted)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToList();
            }
        }

        /// <summary>
        /// Gets clinical documents within a date range
        /// </summary>
        public IEnumerable<ClinicalDocument> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            lock (_lock)
            {
                return _entities.Values
                    .Where(d => d.CreatedAt.Date >= startDate.Date && d.CreatedAt.Date <= endDate.Date)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToList();
            }
        }

        /// <summary>
        /// Searches clinical documents by diagnosis code (ICD-10)
        /// </summary>
        public IEnumerable<ClinicalDocument> SearchByDiagnosis(string icd10Code)
        {
            if (string.IsNullOrWhiteSpace(icd10Code))
                return Enumerable.Empty<ClinicalDocument>();

            var lowerCode = icd10Code.ToLowerInvariant();

            lock (_lock)
            {
                return _entities.Values
                    .Where(d => d.GetDiagnoses()
                        .Any(diag => (diag.ICD10Code ?? "").ToLowerInvariant().Contains(lowerCode)))
                    .OrderByDescending(d => d.CreatedAt)
                    .ToList();
            }
        }

        /// <summary>
        /// Searches clinical documents by medication name
        /// </summary>
        public IEnumerable<ClinicalDocument> SearchByMedication(string medicationName)
        {
            if (string.IsNullOrWhiteSpace(medicationName))
                return Enumerable.Empty<ClinicalDocument>();

            var lowerName = medicationName.ToLowerInvariant();

            lock (_lock)
            {
                return _entities.Values
                    .Where(d => d.GetPrescriptions()
                        .Any(rx => rx.MedicationName.ToLowerInvariant().Contains(lowerName)))
                    .OrderByDescending(d => d.CreatedAt)
                    .ToList();
            }
        }

        /// <summary>
        /// Searches clinical documents by chief complaint, observations, or assessments
        /// </summary>
        public override IEnumerable<ClinicalDocument> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return GetAll();

            var lowerQuery = query.ToLowerInvariant();

            lock (_lock)
            {
                return _entities.Values
                    .Where(d =>
                        (d.ChiefComplaint?.ToLowerInvariant().Contains(lowerQuery) ?? false) ||
                        d.GetDiagnoses().Any(diag =>
                            diag.Content.ToLowerInvariant().Contains(lowerQuery) ||
                            (diag.ICD10Code ?? "").ToLowerInvariant().Contains(lowerQuery)) ||
                        d.GetPrescriptions().Any(rx =>
                            rx.MedicationName.ToLowerInvariant().Contains(lowerQuery)))
                    .OrderByDescending(d => d.CreatedAt)
                    .ToList();
            }
        }

        /// <summary>
        /// Gets completed clinical documents
        /// </summary>
        public IEnumerable<ClinicalDocument> GetCompleted()
        {
            lock (_lock)
            {
                return _entities.Values
                    .Where(d => d.IsCompleted)
                    .OrderByDescending(d => d.CompletedAt)
                    .ToList();
            }
        }
    }
}
