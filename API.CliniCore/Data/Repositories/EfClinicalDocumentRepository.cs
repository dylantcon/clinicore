using API.CliniCore.Data.Entities.Clinical;
using Core.CliniCore.Domain.ClinicalDocumentation;
using Core.CliniCore.Domain.ClinicalDocumentation.ClinicalEntries;
using Core.CliniCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace API.CliniCore.Data.Repositories
{
    /// <summary>
    /// Entity Framework Core implementation of IClinicalDocumentRepository.
    /// Provides SQLite-backed persistence for clinical documents.
    /// </summary>
    public class EfClinicalDocumentRepository : IClinicalDocumentRepository
    {
        private readonly CliniCoreDbContext _context;

        public EfClinicalDocumentRepository(CliniCoreDbContext context)
        {
            _context = context;
        }

        private IQueryable<ClinicalDocumentEntity> QueryWithIncludes()
        {
            return _context.ClinicalDocuments
                .Include(d => d.Observations)
                .Include(d => d.Assessments)
                .Include(d => d.Diagnoses)
                .Include(d => d.Plans)
                .Include(d => d.Prescriptions);
        }

        public ClinicalDocument? GetById(Guid id)
        {
            var entity = QueryWithIncludes()
                .AsNoTracking()
                .FirstOrDefault(d => d.Id == id);
            return entity?.ToDomain();
        }

        public IEnumerable<ClinicalDocument> GetAll()
        {
            return QueryWithIncludes()
                .AsNoTracking()
                .ToList()
                .Select(e => e.ToDomain());
        }

        public void Add(ClinicalDocument document)
        {
            var entity = document.ToEntity();
            _context.ClinicalDocuments.Add(entity);
            _context.SaveChanges();
        }

        public void Update(ClinicalDocument document)
        {
            var entity = document.ToEntity();
            var existing = QueryWithIncludes()
                .FirstOrDefault(d => d.Id == entity.Id);

            if (existing != null)
            {
                // Update main document properties
                _context.Entry(existing).CurrentValues.SetValues(entity);

                // Clear and re-add all entries (simplest approach for now)
                _context.Observations.RemoveRange(existing.Observations);
                _context.Assessments.RemoveRange(existing.Assessments);
                _context.Diagnoses.RemoveRange(existing.Diagnoses);
                _context.Plans.RemoveRange(existing.Plans);
                _context.Prescriptions.RemoveRange(existing.Prescriptions);

                foreach (var obs in entity.Observations)
                    _context.Observations.Add(obs);
                foreach (var assessment in entity.Assessments)
                    _context.Assessments.Add(assessment);
                foreach (var diagnosis in entity.Diagnoses)
                    _context.Diagnoses.Add(diagnosis);
                foreach (var plan in entity.Plans)
                    _context.Plans.Add(plan);
                foreach (var prescription in entity.Prescriptions)
                    _context.Prescriptions.Add(prescription);

                _context.SaveChanges();
            }
        }

        public void Delete(Guid id)
        {
            var entity = _context.ClinicalDocuments.Find(id);
            if (entity != null)
            {
                _context.ClinicalDocuments.Remove(entity);
                _context.SaveChanges();
            }
        }

        public IEnumerable<ClinicalDocument> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return GetAll();

            var lowerQuery = query.ToLowerInvariant();

            // Search across multiple entry types
            var matchingDocIds = _context.ClinicalDocuments
                .AsNoTracking()
                .Where(d => d.ChiefComplaint != null && d.ChiefComplaint.ToLower().Contains(lowerQuery))
                .Select(d => d.Id)
                .Union(_context.Observations.Where(o => o.Content.ToLower().Contains(lowerQuery)).Select(o => o.ClinicalDocumentId))
                .Union(_context.Assessments.Where(a => a.Content.ToLower().Contains(lowerQuery)).Select(a => a.ClinicalDocumentId))
                .Union(_context.Diagnoses.Where(d => d.Content.ToLower().Contains(lowerQuery)).Select(d => d.ClinicalDocumentId))
                .Union(_context.Plans.Where(p => p.Content.ToLower().Contains(lowerQuery)).Select(p => p.ClinicalDocumentId))
                .Union(_context.Prescriptions.Where(r => r.MedicationName.ToLower().Contains(lowerQuery) ||
                    (r.Instructions != null && r.Instructions.ToLower().Contains(lowerQuery))).Select(r => r.ClinicalDocumentId))
                .Distinct()
                .ToList();

            return QueryWithIncludes()
                .AsNoTracking()
                .Where(d => matchingDocIds.Contains(d.Id))
                .ToList()
                .Select(e => e.ToDomain());
        }

        public IEnumerable<ClinicalDocument> GetByPatient(Guid patientId)
        {
            return QueryWithIncludes()
                .AsNoTracking()
                .Where(d => d.PatientId == patientId)
                .OrderByDescending(d => d.CreatedAt)
                .ToList()
                .Select(e => e.ToDomain());
        }

        public IEnumerable<ClinicalDocument> GetByPhysician(Guid physicianId)
        {
            return QueryWithIncludes()
                .AsNoTracking()
                .Where(d => d.PhysicianId == physicianId)
                .OrderByDescending(d => d.CreatedAt)
                .ToList()
                .Select(e => e.ToDomain());
        }

        public ClinicalDocument? GetByAppointment(Guid appointmentId)
        {
            var entity = QueryWithIncludes()
                .AsNoTracking()
                .FirstOrDefault(d => d.AppointmentId == appointmentId);

            return entity?.ToDomain();
        }

        public IEnumerable<ClinicalDocument> GetIncomplete()
        {
            return QueryWithIncludes()
                .AsNoTracking()
                .Where(d => d.CompletedAt == null)
                .OrderByDescending(d => d.CreatedAt)
                .ToList()
                .Select(e => e.ToDomain());
        }

        public IEnumerable<ClinicalDocument> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            return QueryWithIncludes()
                .AsNoTracking()
                .Where(d => d.CreatedAt >= startDate && d.CreatedAt <= endDate)
                .OrderByDescending(d => d.CreatedAt)
                .ToList()
                .Select(e => e.ToDomain());
        }

        public IEnumerable<ClinicalDocument> SearchByDiagnosis(string icd10Code)
        {
            if (string.IsNullOrWhiteSpace(icd10Code))
                return Enumerable.Empty<ClinicalDocument>();

            var lowerCode = icd10Code.ToLowerInvariant();

            // Query diagnoses table directly
            var matchingDocIds = _context.Diagnoses
                .AsNoTracking()
                .Where(d => d.ICD10Code != null && d.ICD10Code.ToLower().Contains(lowerCode))
                .Select(d => d.ClinicalDocumentId)
                .Distinct()
                .ToList();

            return QueryWithIncludes()
                .AsNoTracking()
                .Where(d => matchingDocIds.Contains(d.Id))
                .ToList()
                .Select(e => e.ToDomain());
        }

        public IEnumerable<ClinicalDocument> SearchByMedication(string medicationName)
        {
            if (string.IsNullOrWhiteSpace(medicationName))
                return Enumerable.Empty<ClinicalDocument>();

            var lowerMed = medicationName.ToLowerInvariant();

            // Query prescriptions table directly
            var matchingDocIds = _context.Prescriptions
                .AsNoTracking()
                .Where(p => p.MedicationName.ToLower().Contains(lowerMed))
                .Select(p => p.ClinicalDocumentId)
                .Distinct()
                .ToList();

            return QueryWithIncludes()
                .AsNoTracking()
                .Where(d => matchingDocIds.Contains(d.Id))
                .ToList()
                .Select(e => e.ToDomain());
        }

        #region Entry-Level CRUD Operations

        // Observation operations
        public ObservationEntry? GetObservation(Guid documentId, Guid entryId)
        {
            var entity = _context.Observations
                .AsNoTracking()
                .FirstOrDefault(o => o.ClinicalDocumentId == documentId && o.Id == entryId);
            return entity?.ToDomain();
        }

        public void AddObservation(Guid documentId, ObservationEntry entry)
        {
            var entity = entry.ToEntity(documentId);
            _context.Observations.Add(entity);
            _context.SaveChanges();
        }

        public void UpdateObservation(Guid documentId, ObservationEntry entry)
        {
            var existing = _context.Observations
                .FirstOrDefault(o => o.ClinicalDocumentId == documentId && o.Id == entry.Id);
            if (existing != null)
            {
                var entity = entry.ToEntity(documentId);
                _context.Entry(existing).CurrentValues.SetValues(entity);
                _context.SaveChanges();
            }
        }

        public void DeleteObservation(Guid documentId, Guid entryId)
        {
            var entity = _context.Observations
                .FirstOrDefault(o => o.ClinicalDocumentId == documentId && o.Id == entryId);
            if (entity != null)
            {
                _context.Observations.Remove(entity);
                _context.SaveChanges();
            }
        }

        // Assessment operations
        public AssessmentEntry? GetAssessment(Guid documentId, Guid entryId)
        {
            var entity = _context.Assessments
                .AsNoTracking()
                .FirstOrDefault(a => a.ClinicalDocumentId == documentId && a.Id == entryId);
            return entity?.ToDomain();
        }

        public void AddAssessment(Guid documentId, AssessmentEntry entry)
        {
            var entity = entry.ToEntity(documentId);
            _context.Assessments.Add(entity);
            _context.SaveChanges();
        }

        public void UpdateAssessment(Guid documentId, AssessmentEntry entry)
        {
            var existing = _context.Assessments
                .FirstOrDefault(a => a.ClinicalDocumentId == documentId && a.Id == entry.Id);
            if (existing != null)
            {
                var entity = entry.ToEntity(documentId);
                _context.Entry(existing).CurrentValues.SetValues(entity);
                _context.SaveChanges();
            }
        }

        public void DeleteAssessment(Guid documentId, Guid entryId)
        {
            var entity = _context.Assessments
                .FirstOrDefault(a => a.ClinicalDocumentId == documentId && a.Id == entryId);
            if (entity != null)
            {
                _context.Assessments.Remove(entity);
                _context.SaveChanges();
            }
        }

        // Diagnosis operations
        public DiagnosisEntry? GetDiagnosis(Guid documentId, Guid entryId)
        {
            var entity = _context.Diagnoses
                .AsNoTracking()
                .FirstOrDefault(d => d.ClinicalDocumentId == documentId && d.Id == entryId);
            return entity?.ToDomain();
        }

        public void AddDiagnosis(Guid documentId, DiagnosisEntry entry)
        {
            var entity = entry.ToEntity(documentId);
            _context.Diagnoses.Add(entity);
            _context.SaveChanges();
        }

        public void UpdateDiagnosis(Guid documentId, DiagnosisEntry entry)
        {
            var existing = _context.Diagnoses
                .FirstOrDefault(d => d.ClinicalDocumentId == documentId && d.Id == entry.Id);
            if (existing != null)
            {
                var entity = entry.ToEntity(documentId);
                _context.Entry(existing).CurrentValues.SetValues(entity);
                _context.SaveChanges();
            }
        }

        public void DeleteDiagnosis(Guid documentId, Guid entryId)
        {
            // Check if any prescriptions reference this diagnosis
            var hasPrescriptions = _context.Prescriptions
                .Any(p => p.DiagnosisId == entryId);
            if (hasPrescriptions)
                throw new InvalidOperationException("Cannot delete diagnosis with associated prescriptions");

            var entity = _context.Diagnoses
                .FirstOrDefault(d => d.ClinicalDocumentId == documentId && d.Id == entryId);
            if (entity != null)
            {
                _context.Diagnoses.Remove(entity);
                _context.SaveChanges();
            }
        }

        // Plan operations
        public PlanEntry? GetPlan(Guid documentId, Guid entryId)
        {
            var entity = _context.Plans
                .AsNoTracking()
                .FirstOrDefault(p => p.ClinicalDocumentId == documentId && p.Id == entryId);
            return entity?.ToDomain();
        }

        public void AddPlan(Guid documentId, PlanEntry entry)
        {
            var entity = entry.ToEntity(documentId);
            _context.Plans.Add(entity);
            _context.SaveChanges();
        }

        public void UpdatePlan(Guid documentId, PlanEntry entry)
        {
            var existing = _context.Plans
                .FirstOrDefault(p => p.ClinicalDocumentId == documentId && p.Id == entry.Id);
            if (existing != null)
            {
                var entity = entry.ToEntity(documentId);
                _context.Entry(existing).CurrentValues.SetValues(entity);
                _context.SaveChanges();
            }
        }

        public void DeletePlan(Guid documentId, Guid entryId)
        {
            var entity = _context.Plans
                .FirstOrDefault(p => p.ClinicalDocumentId == documentId && p.Id == entryId);
            if (entity != null)
            {
                _context.Plans.Remove(entity);
                _context.SaveChanges();
            }
        }

        // Prescription operations
        public PrescriptionEntry? GetPrescription(Guid documentId, Guid entryId)
        {
            var entity = _context.Prescriptions
                .AsNoTracking()
                .FirstOrDefault(p => p.ClinicalDocumentId == documentId && p.Id == entryId);
            return entity?.ToDomain();
        }

        public void AddPrescription(Guid documentId, PrescriptionEntry entry)
        {
            // Verify the diagnosis exists
            var diagnosisExists = _context.Diagnoses
                .Any(d => d.ClinicalDocumentId == documentId && d.Id == entry.DiagnosisId);
            if (!diagnosisExists)
                throw new InvalidOperationException("Prescription requires a valid diagnosis in the same document");

            var entity = entry.ToEntity(documentId);
            _context.Prescriptions.Add(entity);
            _context.SaveChanges();
        }

        public void UpdatePrescription(Guid documentId, PrescriptionEntry entry)
        {
            var existing = _context.Prescriptions
                .FirstOrDefault(p => p.ClinicalDocumentId == documentId && p.Id == entry.Id);
            if (existing != null)
            {
                var entity = entry.ToEntity(documentId);
                _context.Entry(existing).CurrentValues.SetValues(entity);
                _context.SaveChanges();
            }
        }

        public void DeletePrescription(Guid documentId, Guid entryId)
        {
            var entity = _context.Prescriptions
                .FirstOrDefault(p => p.ClinicalDocumentId == documentId && p.Id == entryId);
            if (entity != null)
            {
                _context.Prescriptions.Remove(entity);
                _context.SaveChanges();
            }
        }

        #endregion
    }
}
