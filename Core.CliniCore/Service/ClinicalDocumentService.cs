// Core.CliniCore/Service/ClinicalDocumentService.cs
using Core.CliniCore.Domain.ClinicalDocumentation;
using Core.CliniCore.Domain.ClinicalDocumentation.ClinicalEntries;
using Core.CliniCore.Repositories;

namespace Core.CliniCore.Service
{
    /// <summary>
    /// Service for managing clinical documents in the system.
    /// Provides document storage, retrieval, and search operations.
    /// Delegates persistence to IClinicalDocumentRepository.
    /// </summary>
    public class ClinicalDocumentService(IClinicalDocumentRepository repository)
    {
        private readonly IClinicalDocumentRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

        /// <summary>
        /// Adds a document to the repository
        /// </summary>
        public bool AddDocument(ClinicalDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);

            // Check for duplicate
            if (_repository.GetById(document.Id) != null)
                return false;

            // Check if appointment already has a document
            if (_repository.GetByAppointment(document.AppointmentId) != null)
                return false;

            _repository.Add(document);
            return true;
        }

        /// <summary>
        /// Removes a document from the repository
        /// </summary>
        public bool RemoveDocument(Guid documentId)
        {
            var document = _repository.GetById(documentId);
            if (document == null)
                return false;

            _repository.Delete(documentId);
            return true;
        }

        /// <summary>
        /// Gets a document by ID
        /// </summary>
        public ClinicalDocument? GetDocumentById(Guid documentId)
        {
            return _repository.GetById(documentId);
        }

        /// <summary>
        /// Gets a document by appointment ID
        /// </summary>
        public ClinicalDocument? GetDocumentByAppointment(Guid appointmentId)
        {
            return _repository.GetByAppointment(appointmentId);
        }

        /// <summary>
        /// Gets all documents for a patient
        /// </summary>
        public IEnumerable<ClinicalDocument> GetPatientDocuments(Guid patientId)
        {
            return _repository.GetByPatient(patientId);
        }

        /// <summary>
        /// Gets all documents created by a physician
        /// </summary>
        public IEnumerable<ClinicalDocument> GetPhysicianDocuments(Guid physicianId)
        {
            return _repository.GetByPhysician(physicianId);
        }

        /// <summary>
        /// Gets documents within a date range
        /// </summary>
        public IEnumerable<ClinicalDocument> GetDocumentsInDateRange(
            DateTime startDate,
            DateTime endDate,
            Guid? patientId = null,
            Guid? physicianId = null)
        {
            var documents = _repository.GetByDateRange(startDate, endDate);

            if (patientId.HasValue)
            {
                documents = documents.Where(d => d.PatientId == patientId.Value);
            }

            if (physicianId.HasValue)
            {
                documents = documents.Where(d => d.PhysicianId == physicianId.Value);
            }

            return documents.OrderByDescending(d => d.CreatedAt);
        }

        /// <summary>
        /// Searches documents by diagnosis
        /// </summary>
        public IEnumerable<ClinicalDocument> SearchByDiagnosis(string diagnosisText)
        {
            if (string.IsNullOrWhiteSpace(diagnosisText))
                return [];

            return _repository.SearchByDiagnosis(diagnosisText);
        }

        /// <summary>
        /// Searches documents by prescription/medication
        /// </summary>
        public IEnumerable<ClinicalDocument> SearchByMedication(string medicationName)
        {
            if (string.IsNullOrWhiteSpace(medicationName))
                return [];

            return _repository.SearchByMedication(medicationName);
        }

        /// <summary>
        /// Gets all incomplete documents (not completed)
        /// </summary>
        public IEnumerable<ClinicalDocument> GetIncompleteDocuments(Guid? physicianId = null)
        {
            var documents = _repository.GetIncomplete();

            if (physicianId.HasValue)
            {
                documents = documents.Where(d => d.PhysicianId == physicianId.Value);
            }

            return documents.OrderBy(d => d.CreatedAt);
        }

        /// <summary>
        /// Gets the most recent document for a patient
        /// </summary>
        public ClinicalDocument? GetMostRecentPatientDocument(Guid patientId)
        {
            return _repository.GetByPatient(patientId)
                .OrderByDescending(d => d.CreatedAt)
                .FirstOrDefault();
        }

        /// <summary>
        /// Checks if a document exists
        /// </summary>
        public bool DocumentExists(Guid documentId)
        {
            return _repository.GetById(documentId) != null;
        }

        /// <summary>
        /// Checks if an appointment already has a document
        /// </summary>
        public bool AppointmentHasDocument(Guid appointmentId)
        {
            return _repository.GetByAppointment(appointmentId) != null;
        }

        /// <summary>
        /// Gets all clinical documents in the system
        /// </summary>
        public IEnumerable<ClinicalDocument> GetAllDocuments()
        {
            return _repository.GetAll().OrderByDescending(d => d.CreatedAt);
        }

        /// <summary>
        /// Updates a document in the repository
        /// </summary>
        public void UpdateDocument(ClinicalDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);

            _repository.Update(document);
        }

        #region Entry-Level Operations

        /// <summary>
        /// Adds an observation entry to a document and persists it
        /// </summary>
        public void AddObservation(Guid documentId, ObservationEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);
            _repository.AddObservation(documentId, entry);
        }

        /// <summary>
        /// Adds an assessment entry to a document and persists it
        /// </summary>
        public void AddAssessment(Guid documentId, AssessmentEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);
            _repository.AddAssessment(documentId, entry);
        }

        /// <summary>
        /// Adds a diagnosis entry to a document and persists it
        /// </summary>
        public void AddDiagnosis(Guid documentId, DiagnosisEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);
            _repository.AddDiagnosis(documentId, entry);
        }

        /// <summary>
        /// Adds a plan entry to a document and persists it
        /// </summary>
        public void AddPlan(Guid documentId, PlanEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);
            _repository.AddPlan(documentId, entry);
        }

        /// <summary>
        /// Adds a prescription entry to a document and persists it
        /// </summary>
        public void AddPrescription(Guid documentId, PrescriptionEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);
            _repository.AddPrescription(documentId, entry);
        }

        #endregion

        /// <summary>
        /// Gets statistics about clinical documents
        /// </summary>
        public ClinicalDocumentStatistics GetStatistics()
        {
            var allDocs = _repository.GetAll().ToList();

            return new ClinicalDocumentStatistics
            {
                TotalDocuments = allDocs.Count,
                CompletedDocuments = allDocs.Count(d => d.IsCompleted),
                IncompleteDocuments = allDocs.Count(d => !d.IsCompleted),
                UniquePatients = allDocs.Select(d => d.PatientId).Distinct().Count(),
                UniquePhysicians = allDocs.Select(d => d.PhysicianId).Distinct().Count(),
                TotalDiagnoses = allDocs.Sum(d => d.GetDiagnoses().Count()),
                TotalPrescriptions = allDocs.Sum(d => d.GetPrescriptions().Count()),
                DocumentsToday = allDocs.Count(d => d.CreatedAt.Date == DateTime.Today),
                DocumentsThisWeek = allDocs.Count(d => d.CreatedAt >= DateTime.Today.AddDays(-7))
            };
        }
    }

    /// <summary>
    /// Statistics about clinical documents
    /// </summary>
    public class ClinicalDocumentStatistics
    {
        public int TotalDocuments { get; set; }
        public int CompletedDocuments { get; set; }
        public int IncompleteDocuments { get; set; }
        public int UniquePatients { get; set; }
        public int UniquePhysicians { get; set; }
        public int TotalDiagnoses { get; set; }
        public int TotalPrescriptions { get; set; }
        public int DocumentsToday { get; set; }
        public int DocumentsThisWeek { get; set; }

        public double CompletionRate => TotalDocuments > 0
            ? (double)CompletedDocuments / TotalDocuments * 100
            : 0;

        public override string ToString()
        {
            return $"Documents: {TotalDocuments} total ({CompletedDocuments} completed, {IncompleteDocuments} pending)\n" +
                   $"Patients: {UniquePatients}, Physicians: {UniquePhysicians}\n" +
                   $"Diagnoses: {TotalDiagnoses}, Prescriptions: {TotalPrescriptions}\n" +
                   $"Completion Rate: {CompletionRate:F1}%";
        }
    }
}
