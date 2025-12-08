using Core.CliniCore.Domain.ClinicalDocumentation;
using Core.CliniCore.Domain.ClinicalDocumentation.ClinicalEntries;

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

        #region Entry-Level CRUD Operations

        // Observation operations
        public ObservationEntry? GetObservation(Guid documentId, Guid entryId)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(documentId, out var doc))
                    return null;
                return doc.GetObservations().FirstOrDefault(o => o.Id == entryId);
            }
        }

        public void AddObservation(Guid documentId, ObservationEntry entry)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(documentId, out var doc))
                    throw new InvalidOperationException($"Document {documentId} not found");
                doc.AddEntry(entry);
            }
        }

        public void UpdateObservation(Guid documentId, ObservationEntry entry)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(documentId, out var doc))
                    throw new InvalidOperationException($"Document {documentId} not found");
                var existing = doc.GetObservations().FirstOrDefault(o => o.Id == entry.Id);
                if (existing != null)
                {
                    existing.Content = entry.Content;
                    existing.Type = entry.Type;
                    existing.BodySystem = entry.BodySystem;
                    existing.IsAbnormal = entry.IsAbnormal;
                    existing.Severity = entry.Severity;
                    existing.ReferenceRange = entry.ReferenceRange;
                    existing.Code = entry.Code;
                    existing.NumericValue = entry.NumericValue;
                    existing.Unit = entry.Unit;
                    existing.ModifiedAt = DateTime.Now;
                }
            }
        }

        public void DeleteObservation(Guid documentId, Guid entryId)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(documentId, out var doc))
                    return;
                var entry = doc.GetObservations().FirstOrDefault(o => o.Id == entryId);
                if (entry != null)
                    entry.IsActive = false;
            }
        }

        // Assessment operations
        public AssessmentEntry? GetAssessment(Guid documentId, Guid entryId)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(documentId, out var doc))
                    return null;
                return doc.GetAssessments().FirstOrDefault(a => a.Id == entryId);
            }
        }

        public void AddAssessment(Guid documentId, AssessmentEntry entry)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(documentId, out var doc))
                    throw new InvalidOperationException($"Document {documentId} not found");
                doc.AddEntry(entry);
            }
        }

        public void UpdateAssessment(Guid documentId, AssessmentEntry entry)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(documentId, out var doc))
                    throw new InvalidOperationException($"Document {documentId} not found");
                var existing = doc.GetAssessments().FirstOrDefault(a => a.Id == entry.Id);
                if (existing != null)
                {
                    existing.ClinicalImpression = entry.ClinicalImpression;
                    existing.Condition = entry.Condition;
                    existing.Prognosis = entry.Prognosis;
                    existing.Confidence = entry.Confidence;
                    existing.Severity = entry.Severity;
                    existing.RequiresImmediateAction = entry.RequiresImmediateAction;
                    existing.ModifiedAt = DateTime.Now;
                }
            }
        }

        public void DeleteAssessment(Guid documentId, Guid entryId)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(documentId, out var doc))
                    return;
                var entry = doc.GetAssessments().FirstOrDefault(a => a.Id == entryId);
                if (entry != null)
                    entry.IsActive = false;
            }
        }

        // Diagnosis operations
        public DiagnosisEntry? GetDiagnosis(Guid documentId, Guid entryId)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(documentId, out var doc))
                    return null;
                return doc.GetDiagnoses().FirstOrDefault(d => d.Id == entryId);
            }
        }

        public void AddDiagnosis(Guid documentId, DiagnosisEntry entry)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(documentId, out var doc))
                    throw new InvalidOperationException($"Document {documentId} not found");
                doc.AddEntry(entry);
            }
        }

        public void UpdateDiagnosis(Guid documentId, DiagnosisEntry entry)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(documentId, out var doc))
                    throw new InvalidOperationException($"Document {documentId} not found");
                var existing = doc.GetDiagnoses().FirstOrDefault(d => d.Id == entry.Id);
                if (existing != null)
                {
                    existing.Content = entry.Content;
                    existing.ICD10Code = entry.ICD10Code;
                    existing.Type = entry.Type;
                    existing.Status = entry.Status;
                    existing.Severity = entry.Severity;
                    existing.IsPrimary = entry.IsPrimary;
                    existing.OnsetDate = entry.OnsetDate;
                    existing.ModifiedAt = DateTime.Now;
                }
            }
        }

        public void DeleteDiagnosis(Guid documentId, Guid entryId)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(documentId, out var doc))
                    return;

                // Check if any prescriptions reference this diagnosis
                if (doc.GetPrescriptions().Any(p => p.DiagnosisId == entryId && p.IsActive))
                    throw new InvalidOperationException("Cannot delete diagnosis with associated prescriptions");

                var entry = doc.GetDiagnoses().FirstOrDefault(d => d.Id == entryId);
                if (entry != null)
                    entry.IsActive = false;
            }
        }

        // Plan operations
        public PlanEntry? GetPlan(Guid documentId, Guid entryId)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(documentId, out var doc))
                    return null;
                return doc.GetPlans().FirstOrDefault(p => p.Id == entryId);
            }
        }

        public void AddPlan(Guid documentId, PlanEntry entry)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(documentId, out var doc))
                    throw new InvalidOperationException($"Document {documentId} not found");
                doc.AddEntry(entry);
            }
        }

        public void UpdatePlan(Guid documentId, PlanEntry entry)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(documentId, out var doc))
                    throw new InvalidOperationException($"Document {documentId} not found");
                var existing = doc.GetPlans().FirstOrDefault(p => p.Id == entry.Id);
                if (existing != null)
                {
                    existing.Content = entry.Content;
                    existing.Type = entry.Type;
                    existing.Priority = entry.Priority;
                    existing.Severity = entry.Severity;
                    existing.TargetDate = entry.TargetDate;
                    existing.FollowUpInstructions = entry.FollowUpInstructions;
                    if (entry.IsCompleted && !existing.IsCompleted)
                        existing.MarkCompleted();
                    existing.ModifiedAt = DateTime.Now;
                }
            }
        }

        public void DeletePlan(Guid documentId, Guid entryId)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(documentId, out var doc))
                    return;
                var entry = doc.GetPlans().FirstOrDefault(p => p.Id == entryId);
                if (entry != null)
                    entry.IsActive = false;
            }
        }

        // Prescription operations
        public PrescriptionEntry? GetPrescription(Guid documentId, Guid entryId)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(documentId, out var doc))
                    return null;
                return doc.GetPrescriptions().FirstOrDefault(p => p.Id == entryId);
            }
        }

        public void AddPrescription(Guid documentId, PrescriptionEntry entry)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(documentId, out var doc))
                    throw new InvalidOperationException($"Document {documentId} not found");

                // Verify the diagnosis exists
                var diagnosisExists = doc.GetDiagnoses().Any(d => d.Id == entry.DiagnosisId && d.IsActive);
                if (!diagnosisExists)
                    throw new InvalidOperationException("Prescription requires a valid diagnosis in the same document");

                doc.AddEntry(entry);
            }
        }

        public void UpdatePrescription(Guid documentId, PrescriptionEntry entry)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(documentId, out var doc))
                    throw new InvalidOperationException($"Document {documentId} not found");
                var existing = doc.GetPrescriptions().FirstOrDefault(p => p.Id == entry.Id);
                if (existing != null)
                {
                    existing.Dosage = entry.Dosage;
                    existing.Frequency = entry.Frequency;
                    existing.Route = entry.Route;
                    existing.Duration = entry.Duration;
                    existing.Refills = entry.Refills;
                    existing.GenericAllowed = entry.GenericAllowed;
                    existing.DEASchedule = entry.DEASchedule;
                    existing.ExpirationDate = entry.ExpirationDate;
                    existing.NDCCode = entry.NDCCode;
                    existing.Instructions = entry.Instructions;
                    existing.Severity = entry.Severity;
                    existing.ModifiedAt = DateTime.Now;
                }
            }
        }

        public void DeletePrescription(Guid documentId, Guid entryId)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(documentId, out var doc))
                    return;
                var entry = doc.GetPrescriptions().FirstOrDefault(p => p.Id == entryId);
                if (entry != null)
                    entry.IsActive = false;
            }
        }

        #endregion
    }
}
