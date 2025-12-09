using Core.CliniCore.Api;
using Core.CliniCore.Domain.ClinicalDocumentation;
using Core.CliniCore.Domain.ClinicalDocumentation.ClinicalEntries;
using Core.CliniCore.DTOs.ClinicalDocuments;
using Core.CliniCore.Mapping;
using Core.CliniCore.Requests.ClinicalDocuments;

namespace Core.CliniCore.Repositories.Remote
{
    /// <summary>
    /// Remote repository implementation that calls the API for clinical document operations.
    /// Uses ApiRoutes for all endpoint paths (single source of truth).
    /// </summary>
    public class RemoteClinicalDocumentRepository : RemoteRepositoryBase, IClinicalDocumentRepository
    {
        public RemoteClinicalDocumentRepository(HttpClient httpClient) : base(httpClient)
        {
        }

        public ClinicalDocument? GetById(Guid id)
        {
            var dto = Get<ClinicalDocumentDto>(ApiRoutes.ClinicalDocuments.GetById(id));
            return dto?.ToDomain();
        }

        public IEnumerable<ClinicalDocument> GetAll()
        {
            var dtos = GetList<ClinicalDocumentDto>(ApiRoutes.ClinicalDocuments.GetAll());
            return dtos.Select(d => d.ToDomain());
        }

        public void Add(ClinicalDocument entity)
        {
            var request = new CreateClinicalDocumentRequest
            {
                Id = entity.Id,  // Include client-generated ID so API uses the same ID
                PatientId = entity.PatientId,
                PhysicianId = entity.PhysicianId,
                AppointmentId = entity.AppointmentId,
                ChiefComplaint = entity.ChiefComplaint ?? string.Empty
            };

            var result = Post<CreateClinicalDocumentRequest, ClinicalDocumentDto>(ApiRoutes.ClinicalDocuments.GetAll(), request);
            if (result == null)
            {
                throw new RepositoryOperationException("Add", "ClinicalDocument", entity.Id,
                    LastError ?? "Remote server failed to create the clinical document");
            }
        }

        public void Update(ClinicalDocument entity)
        {
            var request = new UpdateClinicalDocumentRequest
            {
                ChiefComplaint = entity.ChiefComplaint,
                IsCompleted = entity.IsCompleted
            };

            if (!Put(ApiRoutes.ClinicalDocuments.GetById(entity.Id), request))
            {
                throw new RepositoryOperationException("Update", "ClinicalDocument", entity.Id,
                    "Remote server failed to update the clinical document");
            }
        }

        public void Delete(Guid id)
        {
            if (!Delete(ApiRoutes.ClinicalDocuments.GetById(id)))
            {
                throw new RepositoryOperationException("Delete", "ClinicalDocument", id,
                    "Remote server failed to delete the clinical document");
            }
        }

        public IEnumerable<ClinicalDocument> Search(string query)
        {
            var dtos = GetList<ClinicalDocumentDto>(ApiRoutes.ClinicalDocuments.SearchByQuery(query));
            return dtos.Select(d => d.ToDomain());
        }

        public IEnumerable<ClinicalDocument> GetByPatient(Guid patientId)
        {
            var dtos = GetList<ClinicalDocumentDto>(ApiRoutes.ClinicalDocuments.GetByPatient(patientId));
            return dtos.Select(d => d.ToDomain());
        }

        public IEnumerable<ClinicalDocument> GetByPhysician(Guid physicianId)
        {
            var dtos = GetList<ClinicalDocumentDto>(ApiRoutes.ClinicalDocuments.GetByPhysician(physicianId));
            return dtos.Select(d => d.ToDomain());
        }

        public ClinicalDocument? GetByAppointment(Guid appointmentId)
        {
            var dto = Get<ClinicalDocumentDto>(ApiRoutes.ClinicalDocuments.GetByAppointment(appointmentId));
            return dto?.ToDomain();
        }

        public IEnumerable<ClinicalDocument> GetIncomplete()
        {
            var dtos = GetList<ClinicalDocumentDto>(ApiRoutes.ClinicalDocuments.GetIncomplete());
            return dtos.Select(d => d.ToDomain());
        }

        public IEnumerable<ClinicalDocument> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            var dtos = GetList<ClinicalDocumentDto>(ApiRoutes.ClinicalDocuments.GetByDateRange(startDate, endDate));
            return dtos.Select(d => d.ToDomain());
        }

        public IEnumerable<ClinicalDocument> SearchByDiagnosis(string icd10Code)
        {
            var dtos = GetList<ClinicalDocumentDto>(ApiRoutes.ClinicalDocuments.SearchByDiagnosis(icd10Code));
            return dtos.Select(d => d.ToDomain());
        }

        public IEnumerable<ClinicalDocument> SearchByMedication(string medicationName)
        {
            var dtos = GetList<ClinicalDocumentDto>(ApiRoutes.ClinicalDocuments.SearchByMedication(medicationName));
            return dtos.Select(d => d.ToDomain());
        }

        #region Entry-Level CRUD Operations

        // Observation operations
        public ObservationEntry? GetObservation(Guid documentId, Guid entryId)
        {
            var dto = Get<ObservationDto>(ApiRoutes.ClinicalDocuments.GetObservation(documentId, entryId));
            return dto?.ToDomain();
        }

        public void AddObservation(Guid documentId, ObservationEntry entry)
        {
            if (!Post(ApiRoutes.ClinicalDocuments.GetObservations(documentId), entry.ToCreateRequest()))
            {
                throw new RepositoryOperationException("AddObservation", "ClinicalDocument", documentId,
                    LastError ?? "Remote server failed to add observation");
            }
        }

        public void UpdateObservation(Guid documentId, ObservationEntry entry)
        {
            Put(ApiRoutes.ClinicalDocuments.GetObservation(documentId, entry.Id), entry.ToUpdateRequest());
        }

        public void DeleteObservation(Guid documentId, Guid entryId)
        {
            Delete(ApiRoutes.ClinicalDocuments.GetObservation(documentId, entryId));
        }

        // Assessment operations
        public AssessmentEntry? GetAssessment(Guid documentId, Guid entryId)
        {
            var dto = Get<AssessmentDto>(ApiRoutes.ClinicalDocuments.GetAssessment(documentId, entryId));
            return dto?.ToDomain();
        }

        public void AddAssessment(Guid documentId, AssessmentEntry entry)
        {
            if (!Post(ApiRoutes.ClinicalDocuments.GetAssessments(documentId), entry.ToCreateRequest()))
            {
                throw new RepositoryOperationException("AddAssessment", "ClinicalDocument", documentId,
                    LastError ?? "Remote server failed to add assessment");
            }
        }

        public void UpdateAssessment(Guid documentId, AssessmentEntry entry)
        {
            Put(ApiRoutes.ClinicalDocuments.GetAssessment(documentId, entry.Id), entry.ToUpdateRequest());
        }

        public void DeleteAssessment(Guid documentId, Guid entryId)
        {
            Delete(ApiRoutes.ClinicalDocuments.GetAssessment(documentId, entryId));
        }

        // Diagnosis operations
        public DiagnosisEntry? GetDiagnosis(Guid documentId, Guid entryId)
        {
            var dto = Get<DiagnosisDto>(ApiRoutes.ClinicalDocuments.GetDiagnosis(documentId, entryId));
            return dto?.ToDomain();
        }

        public void AddDiagnosis(Guid documentId, DiagnosisEntry entry)
        {
            if (!Post(ApiRoutes.ClinicalDocuments.GetDiagnoses(documentId), entry.ToCreateRequest()))
            {
                throw new RepositoryOperationException("AddDiagnosis", "ClinicalDocument", documentId,
                    LastError ?? "Remote server failed to add diagnosis");
            }
        }

        public void UpdateDiagnosis(Guid documentId, DiagnosisEntry entry)
        {
            Put(ApiRoutes.ClinicalDocuments.GetDiagnosis(documentId, entry.Id), entry.ToUpdateRequest());
        }

        public void DeleteDiagnosis(Guid documentId, Guid entryId)
        {
            Delete(ApiRoutes.ClinicalDocuments.GetDiagnosis(documentId, entryId));
        }

        // Plan operations
        public PlanEntry? GetPlan(Guid documentId, Guid entryId)
        {
            var dto = Get<PlanDto>(ApiRoutes.ClinicalDocuments.GetPlan(documentId, entryId));
            return dto?.ToDomain();
        }

        public void AddPlan(Guid documentId, PlanEntry entry)
        {
            if (!Post(ApiRoutes.ClinicalDocuments.GetPlans(documentId), entry.ToCreateRequest()))
            {
                throw new RepositoryOperationException("AddPlan", "ClinicalDocument", documentId,
                    LastError ?? "Remote server failed to add plan");
            }
        }

        public void UpdatePlan(Guid documentId, PlanEntry entry)
        {
            Put(ApiRoutes.ClinicalDocuments.GetPlan(documentId, entry.Id), entry.ToUpdateRequest());
        }

        public void DeletePlan(Guid documentId, Guid entryId)
        {
            Delete(ApiRoutes.ClinicalDocuments.GetPlan(documentId, entryId));
        }

        // Prescription operations
        public PrescriptionEntry? GetPrescription(Guid documentId, Guid entryId)
        {
            var dto = Get<PrescriptionDto>(ApiRoutes.ClinicalDocuments.GetPrescription(documentId, entryId));
            return dto?.ToDomain();
        }

        public void AddPrescription(Guid documentId, PrescriptionEntry entry)
        {
            if (!Post(ApiRoutes.ClinicalDocuments.GetPrescriptions(documentId), entry.ToCreateRequest()))
            {
                throw new RepositoryOperationException("AddPrescription", "ClinicalDocument", documentId,
                    LastError ?? "Remote server failed to add prescription");
            }
        }

        public void UpdatePrescription(Guid documentId, PrescriptionEntry entry)
        {
            Put(ApiRoutes.ClinicalDocuments.GetPrescription(documentId, entry.Id), entry.ToUpdateRequest());
        }

        public void DeletePrescription(Guid documentId, Guid entryId)
        {
            Delete(ApiRoutes.ClinicalDocuments.GetPrescription(documentId, entryId));
        }

        #endregion
    }
}
