// Core.CliniCore/ClinicalDoc/ClinicalDocumentService.cs
using Core.CliniCore.ClinicalDoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Service
{
    /// <summary>
    /// Registry for managing all clinical documents in the system.
    /// Thread-safe operations for document storage and retrieval.
    /// Note: Currently uses in-memory storage via internal dictionaries.
    /// Future: Will integrate with IClinicalDocumentRepository for database persistence.
    /// </summary>
    public class ClinicalDocumentService
    {
        private readonly Dictionary<Guid, ClinicalDocument> _documentsById;
        private readonly Dictionary<Guid, List<ClinicalDocument>> _documentsByPatient;
        private readonly Dictionary<Guid, List<ClinicalDocument>> _documentsByPhysician;
        private readonly Dictionary<Guid, ClinicalDocument> _documentsByAppointment;
        private readonly object _lock = new object();

        public ClinicalDocumentService()
        {
            _documentsById = new Dictionary<Guid, ClinicalDocument>();
            _documentsByPatient = new Dictionary<Guid, List<ClinicalDocument>>();
            _documentsByPhysician = new Dictionary<Guid, List<ClinicalDocument>>();
            _documentsByAppointment = new Dictionary<Guid, ClinicalDocument>();
        }

        /// <summary>
        /// Adds a document to the registry
        /// </summary>
        public bool AddDocument(ClinicalDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            lock (_lock)
            {
                // Check for duplicate
                if (_documentsById.ContainsKey(document.Id))
                    return false;

                // Check if appointment already has a document
                if (_documentsByAppointment.ContainsKey(document.AppointmentId))
                    return false;

                // Add to main registry
                _documentsById[document.Id] = document;

                // Add to appointment index
                _documentsByAppointment[document.AppointmentId] = document;

                // Add to patient index
                if (!_documentsByPatient.ContainsKey(document.PatientId))
                {
                    _documentsByPatient[document.PatientId] = new List<ClinicalDocument>();
                }
                _documentsByPatient[document.PatientId].Add(document);

                // Add to physician index
                if (!_documentsByPhysician.ContainsKey(document.PhysicianId))
                {
                    _documentsByPhysician[document.PhysicianId] = new List<ClinicalDocument>();
                }
                _documentsByPhysician[document.PhysicianId].Add(document);

                return true;
            }
        }

        /// <summary>
        /// Removes a document from the registry
        /// </summary>
        public bool RemoveDocument(Guid documentId)
        {
            lock (_lock)
            {
                if (!_documentsById.TryGetValue(documentId, out var document))
                    return false;

                // Remove from all indices
                _documentsById.Remove(documentId);
                _documentsByAppointment.Remove(document.AppointmentId);

                if (_documentsByPatient.TryGetValue(document.PatientId, out var patientDocs))
                {
                    patientDocs.Remove(document);
                }

                if (_documentsByPhysician.TryGetValue(document.PhysicianId, out var physicianDocs))
                {
                    physicianDocs.Remove(document);
                }

                return true;
            }
        }

        /// <summary>
        /// Gets a document by ID
        /// </summary>
        public ClinicalDocument? GetDocumentById(Guid documentId)
        {
            lock (_lock)
            {
                return _documentsById.TryGetValue(documentId, out var document)
                    ? document
                    : null;
            }
        }

        /// <summary>
        /// Gets a document by appointment ID
        /// </summary>
        public ClinicalDocument? GetDocumentByAppointment(Guid appointmentId)
        {
            lock (_lock)
            {
                return _documentsByAppointment.TryGetValue(appointmentId, out var document)
                    ? document
                    : null;
            }
        }

        /// <summary>
        /// Gets all documents for a patient
        /// </summary>
        public IEnumerable<ClinicalDocument> GetPatientDocuments(Guid patientId)
        {
            lock (_lock)
            {
                if (_documentsByPatient.TryGetValue(patientId, out var documents))
                {
                    return documents
                        .OrderByDescending(d => d.CreatedAt)
                        .ToList();
                }
                return Enumerable.Empty<ClinicalDocument>();
            }
        }

        /// <summary>
        /// Gets all documents created by a physician
        /// </summary>
        public IEnumerable<ClinicalDocument> GetPhysicianDocuments(Guid physicianId)
        {
            lock (_lock)
            {
                if (_documentsByPhysician.TryGetValue(physicianId, out var documents))
                {
                    return documents
                        .OrderByDescending(d => d.CreatedAt)
                        .ToList();
                }
                return Enumerable.Empty<ClinicalDocument>();
            }
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
            lock (_lock)
            {
                var query = _documentsById.Values.AsEnumerable();

                // Filter by date range
                query = query.Where(d => d.CreatedAt >= startDate && d.CreatedAt <= endDate);

                // Optional patient filter
                if (patientId.HasValue)
                {
                    query = query.Where(d => d.PatientId == patientId.Value);
                }

                // Optional physician filter
                if (physicianId.HasValue)
                {
                    query = query.Where(d => d.PhysicianId == physicianId.Value);
                }

                return query.OrderByDescending(d => d.CreatedAt).ToList();
            }
        }

        /// <summary>
        /// Searches documents by diagnosis
        /// </summary>
        public IEnumerable<ClinicalDocument> SearchByDiagnosis(string diagnosisText)
        {
            if (string.IsNullOrWhiteSpace(diagnosisText))
                return Enumerable.Empty<ClinicalDocument>();

            lock (_lock)
            {
                return _documentsById.Values
                    .Where(doc => doc.GetDiagnoses()
                        .Any(d => d.Content.Contains(diagnosisText, StringComparison.OrdinalIgnoreCase) ||
                                 d.ICD10Code != null && d.ICD10Code.Contains(diagnosisText, StringComparison.OrdinalIgnoreCase)))
                    .OrderByDescending(d => d.CreatedAt)
                    .ToList();
            }
        }

        /// <summary>
        /// Searches documents by prescription
        /// </summary>
        public IEnumerable<ClinicalDocument> SearchByMedication(string medicationName)
        {
            if (string.IsNullOrWhiteSpace(medicationName))
                return Enumerable.Empty<ClinicalDocument>();

            lock (_lock)
            {
                return _documentsById.Values
                    .Where(doc => doc.GetPrescriptions()
                        .Any(p => p.MedicationName.Contains(medicationName, StringComparison.OrdinalIgnoreCase)))
                    .OrderByDescending(d => d.CreatedAt)
                    .ToList();
            }
        }

        /// <summary>
        /// Gets all incomplete documents (not completed)
        /// </summary>
        public IEnumerable<ClinicalDocument> GetIncompleteDocuments(Guid? physicianId = null)
        {
            lock (_lock)
            {
                var query = _documentsById.Values.Where(d => !d.IsCompleted);

                if (physicianId.HasValue)
                {
                    query = query.Where(d => d.PhysicianId == physicianId.Value);
                }

                return query.OrderBy(d => d.CreatedAt).ToList();
            }
        }

        /// <summary>
        /// Gets the most recent document for a patient
        /// </summary>
        public ClinicalDocument? GetMostRecentPatientDocument(Guid patientId)
        {
            lock (_lock)
            {
                if (_documentsByPatient.TryGetValue(patientId, out var documents))
                {
                    return documents
                        .OrderByDescending(d => d.CreatedAt)
                        .FirstOrDefault();
                }
                return null;
            }
        }

        /// <summary>
        /// Checks if a document exists
        /// </summary>
        public bool DocumentExists(Guid documentId)
        {
            lock (_lock)
            {
                return _documentsById.ContainsKey(documentId);
            }
        }

        /// <summary>
        /// Checks if an appointment already has a document
        /// </summary>
        public bool AppointmentHasDocument(Guid appointmentId)
        {
            lock (_lock)
            {
                return _documentsByAppointment.ContainsKey(appointmentId);
            }
        }

        /// <summary>
        /// Gets statistics about the registry
        /// </summary>
        public ClinicalDocumentStatistics GetStatistics()
        {
            lock (_lock)
            {
                var allDocs = _documentsById.Values.ToList();

                return new ClinicalDocumentStatistics
                {
                    TotalDocuments = allDocs.Count,
                    CompletedDocuments = allDocs.Count(d => d.IsCompleted),
                    IncompleteDocuments = allDocs.Count(d => !d.IsCompleted),
                    UniquePatients = _documentsByPatient.Keys.Count,
                    UniquePhysicians = _documentsByPhysician.Keys.Count,
                    TotalDiagnoses = allDocs.Sum(d => d.GetDiagnoses().Count()),
                    TotalPrescriptions = allDocs.Sum(d => d.GetPrescriptions().Count()),
                    DocumentsToday = allDocs.Count(d => d.CreatedAt.Date == DateTime.Today),
                    DocumentsThisWeek = allDocs.Count(d => d.CreatedAt >= DateTime.Today.AddDays(-7))
                };
            }
        }

        /// <summary>
        /// Gets all clinical documents in the system
        /// </summary>
        public IEnumerable<ClinicalDocument> GetAllDocuments()
        {
            lock (_lock)
            {
                return _documentsById.Values.OrderByDescending(d => d.CreatedAt).ToList();
            }
        }

        /// <summary>
        /// Clears all documents (for testing purposes)
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _documentsById.Clear();
                _documentsByPatient.Clear();
                _documentsByPhysician.Clear();
                _documentsByAppointment.Clear();
            }
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