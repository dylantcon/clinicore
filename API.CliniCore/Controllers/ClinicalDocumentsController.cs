using System;
using System.Collections.Generic;
using System.Linq;
using API.CliniCore.Common;
using Core.CliniCore.Api;
using Core.CliniCore.Domain.ClinicalDocumentation;
using Core.CliniCore.Domain.ClinicalDocumentation.ClinicalEntries;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.DTOs.ClinicalDocuments;
using Core.CliniCore.Mapping;
using Core.CliniCore.Requests.ClinicalDocuments;
using Core.CliniCore.Service;
using Microsoft.AspNetCore.Mvc;

namespace API.CliniCore.Controllers
{
    [ApiController]
    [Route(ApiRoutes.ClinicalDocuments.BasePath)]
    public class ClinicalDocumentsController : ControllerBase
    {
        private readonly ClinicalDocumentService _documentService;
        private readonly ProfileService _profileService;
        private readonly SchedulerService _schedulerService;

        public ClinicalDocumentsController(
            ClinicalDocumentService documentService,
            ProfileService profileService,
            SchedulerService schedulerService)
        {
            _documentService = documentService;
            _profileService = profileService;
            _schedulerService = schedulerService;
        }

        /// <summary>
        /// Get all clinical documents
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<ClinicalDocumentSummaryDto>> GetAll(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] bool? completed = null)
        {
            IEnumerable<ClinicalDocument> documents;

            if (fromDate.HasValue && toDate.HasValue)
            {
                documents = _documentService.GetDocumentsInDateRange(fromDate.Value, toDate.Value);
            }
            else
            {
                documents = _documentService.GetAllDocuments();
            }

            if (completed.HasValue)
            {
                documents = documents.Where(d => d.IsCompleted == completed.Value);
            }

            var dtos = documents.Select(d => d.ToSummaryDto(
                GetPatientName(d.PatientId),
                GetPhysicianName(d.PhysicianId)));

            return Ok(dtos);
        }

        /// <summary>
        /// Get a clinical document by ID
        /// </summary>
        [HttpGet(ApiRoutes.ClinicalDocuments.ById)]
        public ActionResult<ClinicalDocumentDto> GetById(Guid id)
        {
            var document = _documentService.GetDocumentById(id);
            if (document == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {id} not found"));
            }

            return Ok(document.ToDto(
                GetPatientName(document.PatientId),
                GetPhysicianName(document.PhysicianId)));
        }

        /// <summary>
        /// Get documents for a patient
        /// </summary>
        [HttpGet(ApiRoutes.ClinicalDocuments.ByPatient)]
        public ActionResult<IEnumerable<ClinicalDocumentSummaryDto>> GetByPatient(Guid patientId)
        {
            var patient = _profileService.GetProfileById(patientId) as PatientProfile;
            if (patient == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Patient with ID {patientId} not found"));
            }

            var documents = _documentService.GetPatientDocuments(patientId);
            var dtos = documents.Select(d => d.ToSummaryDto(
                patient.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty,
                GetPhysicianName(d.PhysicianId)));

            return Ok(dtos);
        }

        /// <summary>
        /// Get documents created by a physician
        /// </summary>
        [HttpGet(ApiRoutes.ClinicalDocuments.ByPhysician)]
        public ActionResult<IEnumerable<ClinicalDocumentSummaryDto>> GetByPhysician(Guid physicianId)
        {
            var physician = _profileService.GetProfileById(physicianId) as PhysicianProfile;
            if (physician == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Physician with ID {physicianId} not found"));
            }

            var documents = _documentService.GetPhysicianDocuments(physicianId);
            var dtos = documents.Select(d => d.ToSummaryDto(
                GetPatientName(d.PatientId),
                physician.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty));

            return Ok(dtos);
        }

        /// <summary>
        /// Get document for a specific appointment
        /// </summary>
        [HttpGet(ApiRoutes.ClinicalDocuments.ByAppointment)]
        public ActionResult<ClinicalDocumentDto> GetByAppointment(Guid appointmentId)
        {
            var document = _documentService.GetDocumentByAppointment(appointmentId);
            if (document == null)
            {
                return NotFound(ApiErrorResponse.FromMessage(
                    $"No clinical document found for appointment {appointmentId}"));
            }

            return Ok(document.ToDto(
                GetPatientName(document.PatientId),
                GetPhysicianName(document.PhysicianId)));
        }

        /// <summary>
        /// Create a new clinical document
        /// </summary>
        [HttpPost("")]
        public ActionResult<ClinicalDocumentDto> Create([FromBody] CreateClinicalDocumentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiErrorResponse.FromErrors("Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            // Validate patient exists
            var patient = _profileService.GetProfileById(request.PatientId) as PatientProfile;
            if (patient == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Patient with ID {request.PatientId} not found"));
            }

            // Validate physician exists
            var physician = _profileService.GetProfileById(request.PhysicianId) as PhysicianProfile;
            if (physician == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Physician with ID {request.PhysicianId} not found"));
            }

            // Validate appointment exists
            var appointment = _schedulerService.FindAppointmentById(request.AppointmentId);
            if (appointment == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Appointment with ID {request.AppointmentId} not found"));
            }

            // Check if appointment already has a document
            if (_documentService.AppointmentHasDocument(request.AppointmentId))
            {
                return Conflict(ApiErrorResponse.FromMessage(
                    $"Appointment {request.AppointmentId} already has a clinical document"));
            }

            // Validate appointment matches patient and physician
            if (appointment.PatientId != request.PatientId)
            {
                return BadRequest(ApiErrorResponse.FromMessage(
                    "Patient ID does not match the appointment's patient"));
            }
            if (appointment.PhysicianId != request.PhysicianId)
            {
                return BadRequest(ApiErrorResponse.FromMessage(
                    "Physician ID does not match the appointment's physician"));
            }

            // Create the document (use client-provided ID if available)
            var document = request.Id.HasValue && request.Id.Value != Guid.Empty
                ? new ClinicalDocument(request.Id.Value, request.PatientId, request.PhysicianId, request.AppointmentId)
                : new ClinicalDocument(request.PatientId, request.PhysicianId, request.AppointmentId);

            document.ChiefComplaint = request.ChiefComplaint;

            // Add to registry
            var success = _documentService.AddDocument(document);
            if (!success)
            {
                return BadRequest(ApiErrorResponse.FromMessage("Failed to create clinical document"));
            }

            // Update appointment with document reference and persist
            _schedulerService.LinkClinicalDocument(appointment.Id, document.Id);

            // Add to patient's clinical document list
            patient.ClinicalDocumentIds.Add(document.Id);

            return CreatedAtAction(nameof(GetById), new { id = document.Id },
                document.ToDto(patient.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty, physician.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty));
        }

        /// <summary>
        /// Add an observation to a clinical document
        /// </summary>
        [HttpPost("{id}/observations")]
        public ActionResult<ClinicalDocumentDto> AddObservation(Guid id,
            [FromBody] CreateObservationRequest request)
        {
            var document = _documentService.GetDocumentById(id);
            if (document == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {id} not found"));
            }

            if (document.IsCompleted)
            {
                return BadRequest(ApiErrorResponse.FromMessage("Cannot modify a completed document"));
            }

            var observation = new ObservationEntry(
                request.AuthorId ?? document.PhysicianId,
                request.Content)
            {
                Type = request.Type,
                BodySystem = request.BodySystem,
                IsAbnormal = request.IsAbnormal,
                Severity = request.Severity,
                ReferenceRange = request.ReferenceRange,
                Code = request.Code,
                NumericValue = request.NumericValue,
                Unit = request.Unit
            };

            // Use client-provided ID if present
            if (request.Id.HasValue && request.Id.Value != Guid.Empty)
                MappingExtensions.SetEntryId(observation, request.Id.Value);

            document.AddEntry(observation);
            _documentService.UpdateDocument(document);

            return Ok(document.ToDto(
                GetPatientName(document.PatientId),
                GetPhysicianName(document.PhysicianId)));
        }

        /// <summary>
        /// Add a diagnosis to a clinical document
        /// </summary>
        [HttpPost("{id}/diagnoses")]
        public ActionResult<ClinicalDocumentDto> AddDiagnosis(Guid id,
            [FromBody] CreateDiagnosisRequest request)
        {
            var document = _documentService.GetDocumentById(id);
            if (document == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {id} not found"));
            }

            if (document.IsCompleted)
            {
                return BadRequest(ApiErrorResponse.FromMessage("Cannot modify a completed document"));
            }

            var diagnosis = new DiagnosisEntry(
                request.AuthorId ?? document.PhysicianId,
                request.Content)
            {
                ICD10Code = request.ICD10Code,
                Type = request.Type,
                Status = request.Status,
                Severity = request.Severity,
                IsPrimary = request.IsPrimary,
                OnsetDate = request.OnsetDate
            };

            // Use client-provided ID if present
            if (request.Id.HasValue && request.Id.Value != Guid.Empty)
                MappingExtensions.SetEntryId(diagnosis, request.Id.Value);

            document.AddEntry(diagnosis);
            _documentService.UpdateDocument(document);

            return Ok(document.ToDto(
                GetPatientName(document.PatientId),
                GetPhysicianName(document.PhysicianId)));
        }

        /// <summary>
        /// Add a prescription to a clinical document
        /// </summary>
        [HttpPost("{id}/prescriptions")]
        public ActionResult<ClinicalDocumentDto> AddPrescription(Guid id,
            [FromBody] CreatePrescriptionRequest request)
        {
            var document = _documentService.GetDocumentById(id);
            if (document == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {id} not found"));
            }

            if (document.IsCompleted)
            {
                return BadRequest(ApiErrorResponse.FromMessage("Cannot modify a completed document"));
            }

            // Validate diagnosis exists in the document
            var diagnosis = document.GetDiagnoses().FirstOrDefault(d => d.Id == request.DiagnosisId);
            if (diagnosis == null)
            {
                return BadRequest(ApiErrorResponse.FromMessage(
                    $"Diagnosis with ID {request.DiagnosisId} not found in document"));
            }

            var prescription = new PrescriptionEntry(
                request.AuthorId ?? document.PhysicianId,
                request.DiagnosisId,
                request.MedicationName)
            {
                Dosage = request.Dosage,
                Frequency = request.Frequency,
                Route = request.Route,
                Duration = request.Duration,
                Refills = request.Refills,
                GenericAllowed = request.GenericAllowed,
                DEASchedule = request.DEASchedule,
                NDCCode = request.NDCCode,
                Instructions = request.Instructions,
                Severity = request.Severity
            };

            // Use client-provided ID if present
            if (request.Id.HasValue && request.Id.Value != Guid.Empty)
                MappingExtensions.SetEntryId(prescription, request.Id.Value);

            document.AddEntry(prescription);
            _documentService.UpdateDocument(document);

            return Ok(document.ToDto(
                GetPatientName(document.PatientId),
                GetPhysicianName(document.PhysicianId)));
        }

        /// <summary>
        /// Add an assessment to a clinical document
        /// </summary>
        [HttpPost("{id}/assessments")]
        public ActionResult<ClinicalDocumentDto> AddAssessment(Guid id,
            [FromBody] CreateAssessmentRequest request)
        {
            var document = _documentService.GetDocumentById(id);
            if (document == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {id} not found"));
            }

            if (document.IsCompleted)
            {
                return BadRequest(ApiErrorResponse.FromMessage("Cannot modify a completed document"));
            }

            var assessment = new AssessmentEntry(
                request.AuthorId ?? document.PhysicianId,
                request.Content)
            {
                Condition = request.Condition,
                Prognosis = request.Prognosis,
                Confidence = request.Confidence,
                Severity = request.Severity,
                RequiresImmediateAction = request.RequiresImmediateAction
            };

            // Use client-provided ID if present
            if (request.Id.HasValue && request.Id.Value != Guid.Empty)
                MappingExtensions.SetEntryId(assessment, request.Id.Value);

            // Add differential diagnoses if provided
            if (request.DifferentialDiagnoses != null)
            {
                foreach (var diff in request.DifferentialDiagnoses)
                {
                    assessment.DifferentialDiagnoses.Add(diff);
                }
            }

            // Add risk factors if provided
            if (request.RiskFactors != null)
            {
                foreach (var risk in request.RiskFactors)
                {
                    assessment.RiskFactors.Add(risk);
                }
            }

            document.AddEntry(assessment);
            _documentService.UpdateDocument(document);

            return Ok(document.ToDto(
                GetPatientName(document.PatientId),
                GetPhysicianName(document.PhysicianId)));
        }

        /// <summary>
        /// Add a plan entry to a clinical document
        /// </summary>
        [HttpPost("{id}/plans")]
        public ActionResult<ClinicalDocumentDto> AddPlan(Guid id,
            [FromBody] CreatePlanRequest request)
        {
            var document = _documentService.GetDocumentById(id);
            if (document == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {id} not found"));
            }

            if (document.IsCompleted)
            {
                return BadRequest(ApiErrorResponse.FromMessage("Cannot modify a completed document"));
            }

            var plan = new PlanEntry(
                request.AuthorId ?? document.PhysicianId,
                request.Content)
            {
                Type = request.Type,
                Priority = request.Priority,
                Severity = request.Severity,
                TargetDate = request.TargetDate,
                FollowUpInstructions = request.FollowUpInstructions
            };

            // Use client-provided ID if present
            if (request.Id.HasValue && request.Id.Value != Guid.Empty)
                MappingExtensions.SetEntryId(plan, request.Id.Value);

            // Link to diagnoses if provided
            if (request.RelatedDiagnosisIds != null)
            {
                var validDiagnoses = document.GetDiagnoses().Select(d => d.Id).ToHashSet();
                foreach (var diagId in request.RelatedDiagnosisIds.Where(id => validDiagnoses.Contains(id)))
                {
                    plan.RelatedDiagnoses.Add(diagId);
                }
            }

            document.AddEntry(plan);
            _documentService.UpdateDocument(document);

            return Ok(document.ToDto(
                GetPatientName(document.PatientId),
                GetPhysicianName(document.PhysicianId)));
        }

        #region Entry-Level GET/PUT/DELETE Endpoints

        // Observation endpoints
        [HttpGet(ApiRoutes.ClinicalDocuments.ObservationById)]
        public ActionResult<ObservationDto> GetObservation(Guid docId, Guid entryId)
        {
            var document = _documentService.GetDocumentById(docId);
            if (document == null)
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {docId} not found"));

            var entry = document.GetObservations().FirstOrDefault(o => o.Id == entryId);
            if (entry == null)
                return NotFound(ApiErrorResponse.FromMessage($"Observation with ID {entryId} not found"));

            return Ok(entry.ToObservationDto(docId));
        }

        [HttpPut(ApiRoutes.ClinicalDocuments.ObservationById)]
        public ActionResult<ObservationDto> UpdateObservation(Guid docId, Guid entryId, [FromBody] ObservationDto dto)
        {
            var document = _documentService.GetDocumentById(docId);
            if (document == null)
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {docId} not found"));

            if (document.IsCompleted)
                return BadRequest(ApiErrorResponse.FromMessage("Cannot modify a completed document"));

            var entry = document.GetObservations().FirstOrDefault(o => o.Id == entryId);
            if (entry == null)
                return NotFound(ApiErrorResponse.FromMessage($"Observation with ID {entryId} not found"));

            entry.Content = dto.Content;
            entry.Type = dto.Type;
            entry.BodySystem = dto.BodySystem;
            entry.IsAbnormal = dto.IsAbnormal;
            entry.Severity = dto.Severity;
            entry.ReferenceRange = dto.ReferenceRange;
            entry.Code = dto.Code;
            entry.NumericValue = dto.NumericValue;
            entry.Unit = dto.Unit;
            entry.ModifiedAt = DateTime.Now;

            _documentService.UpdateDocument(document);
            return Ok(entry.ToObservationDto(docId));
        }

        [HttpDelete(ApiRoutes.ClinicalDocuments.ObservationById)]
        public IActionResult DeleteObservation(Guid docId, Guid entryId)
        {
            var document = _documentService.GetDocumentById(docId);
            if (document == null)
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {docId} not found"));

            if (document.IsCompleted)
                return BadRequest(ApiErrorResponse.FromMessage("Cannot modify a completed document"));

            var entry = document.GetObservations().FirstOrDefault(o => o.Id == entryId);
            if (entry == null)
                return NotFound(ApiErrorResponse.FromMessage($"Observation with ID {entryId} not found"));

            entry.IsActive = false;
            _documentService.UpdateDocument(document);
            return NoContent();
        }

        // Assessment endpoints
        [HttpGet(ApiRoutes.ClinicalDocuments.AssessmentById)]
        public ActionResult<AssessmentDto> GetAssessment(Guid docId, Guid entryId)
        {
            var document = _documentService.GetDocumentById(docId);
            if (document == null)
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {docId} not found"));

            var entry = document.GetAssessments().FirstOrDefault(a => a.Id == entryId);
            if (entry == null)
                return NotFound(ApiErrorResponse.FromMessage($"Assessment with ID {entryId} not found"));

            return Ok(entry.ToAssessmentDto(docId));
        }

        [HttpPut(ApiRoutes.ClinicalDocuments.AssessmentById)]
        public ActionResult<AssessmentDto> UpdateAssessment(Guid docId, Guid entryId, [FromBody] AssessmentDto dto)
        {
            var document = _documentService.GetDocumentById(docId);
            if (document == null)
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {docId} not found"));

            if (document.IsCompleted)
                return BadRequest(ApiErrorResponse.FromMessage("Cannot modify a completed document"));

            var entry = document.GetAssessments().FirstOrDefault(a => a.Id == entryId);
            if (entry == null)
                return NotFound(ApiErrorResponse.FromMessage($"Assessment with ID {entryId} not found"));

            entry.ClinicalImpression = dto.Content;
            entry.Condition = dto.Condition;
            entry.Prognosis = dto.Prognosis;
            entry.Confidence = dto.Confidence;
            entry.Severity = dto.Severity;
            entry.RequiresImmediateAction = dto.RequiresImmediateAction;
            entry.ModifiedAt = DateTime.Now;

            _documentService.UpdateDocument(document);
            return Ok(entry.ToAssessmentDto(docId));
        }

        [HttpDelete(ApiRoutes.ClinicalDocuments.AssessmentById)]
        public IActionResult DeleteAssessment(Guid docId, Guid entryId)
        {
            var document = _documentService.GetDocumentById(docId);
            if (document == null)
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {docId} not found"));

            if (document.IsCompleted)
                return BadRequest(ApiErrorResponse.FromMessage("Cannot modify a completed document"));

            var entry = document.GetAssessments().FirstOrDefault(a => a.Id == entryId);
            if (entry == null)
                return NotFound(ApiErrorResponse.FromMessage($"Assessment with ID {entryId} not found"));

            entry.IsActive = false;
            _documentService.UpdateDocument(document);
            return NoContent();
        }

        // Diagnosis endpoints
        [HttpGet(ApiRoutes.ClinicalDocuments.DiagnosisById)]
        public ActionResult<DiagnosisDto> GetDiagnosis(Guid docId, Guid entryId)
        {
            var document = _documentService.GetDocumentById(docId);
            if (document == null)
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {docId} not found"));

            var entry = document.GetDiagnoses().FirstOrDefault(d => d.Id == entryId);
            if (entry == null)
                return NotFound(ApiErrorResponse.FromMessage($"Diagnosis with ID {entryId} not found"));

            return Ok(entry.ToDiagnosisDto(docId));
        }

        [HttpPut(ApiRoutes.ClinicalDocuments.DiagnosisById)]
        public ActionResult<DiagnosisDto> UpdateDiagnosis(Guid docId, Guid entryId, [FromBody] DiagnosisDto dto)
        {
            var document = _documentService.GetDocumentById(docId);
            if (document == null)
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {docId} not found"));

            if (document.IsCompleted)
                return BadRequest(ApiErrorResponse.FromMessage("Cannot modify a completed document"));

            var entry = document.GetDiagnoses().FirstOrDefault(d => d.Id == entryId);
            if (entry == null)
                return NotFound(ApiErrorResponse.FromMessage($"Diagnosis with ID {entryId} not found"));

            entry.Content = dto.Content;
            entry.ICD10Code = dto.ICD10Code;
            entry.Type = dto.Type;
            entry.Status = dto.Status;
            entry.Severity = dto.Severity;
            entry.IsPrimary = dto.IsPrimary;
            entry.OnsetDate = dto.OnsetDate;
            entry.ModifiedAt = DateTime.Now;

            _documentService.UpdateDocument(document);
            return Ok(entry.ToDiagnosisDto(docId));
        }

        [HttpDelete(ApiRoutes.ClinicalDocuments.DiagnosisById)]
        public IActionResult DeleteDiagnosis(Guid docId, Guid entryId)
        {
            var document = _documentService.GetDocumentById(docId);
            if (document == null)
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {docId} not found"));

            if (document.IsCompleted)
                return BadRequest(ApiErrorResponse.FromMessage("Cannot modify a completed document"));

            var entry = document.GetDiagnoses().FirstOrDefault(d => d.Id == entryId);
            if (entry == null)
                return NotFound(ApiErrorResponse.FromMessage($"Diagnosis with ID {entryId} not found"));

            // Check for dependent prescriptions
            var hasPrescriptions = document.GetPrescriptions().Any(p => p.DiagnosisId == entryId && p.IsActive);
            if (hasPrescriptions)
                return BadRequest(ApiErrorResponse.FromMessage("Cannot delete diagnosis with active prescriptions"));

            entry.IsActive = false;
            _documentService.UpdateDocument(document);
            return NoContent();
        }

        // Plan endpoints
        [HttpGet(ApiRoutes.ClinicalDocuments.PlanById)]
        public ActionResult<PlanDto> GetPlan(Guid docId, Guid entryId)
        {
            var document = _documentService.GetDocumentById(docId);
            if (document == null)
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {docId} not found"));

            var entry = document.GetPlans().FirstOrDefault(p => p.Id == entryId);
            if (entry == null)
                return NotFound(ApiErrorResponse.FromMessage($"Plan with ID {entryId} not found"));

            return Ok(entry.ToPlanDto(docId));
        }

        [HttpPut(ApiRoutes.ClinicalDocuments.PlanById)]
        public ActionResult<PlanDto> UpdatePlan(Guid docId, Guid entryId, [FromBody] PlanDto dto)
        {
            var document = _documentService.GetDocumentById(docId);
            if (document == null)
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {docId} not found"));

            if (document.IsCompleted)
                return BadRequest(ApiErrorResponse.FromMessage("Cannot modify a completed document"));

            var entry = document.GetPlans().FirstOrDefault(p => p.Id == entryId);
            if (entry == null)
                return NotFound(ApiErrorResponse.FromMessage($"Plan with ID {entryId} not found"));

            entry.Content = dto.Content;
            entry.Type = dto.Type;
            entry.Priority = dto.Priority;
            entry.Severity = dto.Severity;
            entry.TargetDate = dto.TargetDate;
            entry.FollowUpInstructions = dto.FollowUpInstructions;
            if (dto.IsCompleted && !entry.IsCompleted)
                entry.MarkCompleted();
            entry.ModifiedAt = DateTime.Now;

            _documentService.UpdateDocument(document);
            return Ok(entry.ToPlanDto(docId));
        }

        [HttpDelete(ApiRoutes.ClinicalDocuments.PlanById)]
        public IActionResult DeletePlan(Guid docId, Guid entryId)
        {
            var document = _documentService.GetDocumentById(docId);
            if (document == null)
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {docId} not found"));

            if (document.IsCompleted)
                return BadRequest(ApiErrorResponse.FromMessage("Cannot modify a completed document"));

            var entry = document.GetPlans().FirstOrDefault(p => p.Id == entryId);
            if (entry == null)
                return NotFound(ApiErrorResponse.FromMessage($"Plan with ID {entryId} not found"));

            entry.IsActive = false;
            _documentService.UpdateDocument(document);
            return NoContent();
        }

        // Prescription endpoints
        [HttpGet(ApiRoutes.ClinicalDocuments.PrescriptionById)]
        public ActionResult<PrescriptionDto> GetPrescription(Guid docId, Guid entryId)
        {
            var document = _documentService.GetDocumentById(docId);
            if (document == null)
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {docId} not found"));

            var entry = document.GetPrescriptions().FirstOrDefault(p => p.Id == entryId);
            if (entry == null)
                return NotFound(ApiErrorResponse.FromMessage($"Prescription with ID {entryId} not found"));

            return Ok(entry.ToPrescriptionDto(docId));
        }

        [HttpPut(ApiRoutes.ClinicalDocuments.PrescriptionById)]
        public ActionResult<PrescriptionDto> UpdatePrescription(Guid docId, Guid entryId, [FromBody] PrescriptionDto dto)
        {
            var document = _documentService.GetDocumentById(docId);
            if (document == null)
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {docId} not found"));

            if (document.IsCompleted)
                return BadRequest(ApiErrorResponse.FromMessage("Cannot modify a completed document"));

            var entry = document.GetPrescriptions().FirstOrDefault(p => p.Id == entryId);
            if (entry == null)
                return NotFound(ApiErrorResponse.FromMessage($"Prescription with ID {entryId} not found"));

            entry.Dosage = dto.Dosage;
            entry.Frequency = dto.Frequency;
            entry.Route = dto.Route;
            entry.Duration = dto.Duration;
            entry.Refills = dto.Refills;
            entry.GenericAllowed = dto.GenericAllowed;
            entry.DEASchedule = dto.DEASchedule;
            entry.ExpirationDate = dto.ExpirationDate;
            entry.NDCCode = dto.NDCCode;
            entry.Instructions = dto.Instructions;
            entry.Severity = dto.Severity;
            entry.ModifiedAt = DateTime.Now;

            _documentService.UpdateDocument(document);
            return Ok(entry.ToPrescriptionDto(docId));
        }

        [HttpDelete(ApiRoutes.ClinicalDocuments.PrescriptionById)]
        public IActionResult DeletePrescription(Guid docId, Guid entryId)
        {
            var document = _documentService.GetDocumentById(docId);
            if (document == null)
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {docId} not found"));

            if (document.IsCompleted)
                return BadRequest(ApiErrorResponse.FromMessage("Cannot modify a completed document"));

            var entry = document.GetPrescriptions().FirstOrDefault(p => p.Id == entryId);
            if (entry == null)
                return NotFound(ApiErrorResponse.FromMessage($"Prescription with ID {entryId} not found"));

            entry.IsActive = false;
            _documentService.UpdateDocument(document);
            return NoContent();
        }

        #endregion

        /// <summary>
        /// Complete a clinical document
        /// </summary>
        [HttpPost(ApiRoutes.ClinicalDocuments.Complete)]
        public ActionResult<ClinicalDocumentDto> Complete(Guid id)
        {
            var document = _documentService.GetDocumentById(id);
            if (document == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {id} not found"));
            }

            if (document.IsCompleted)
            {
                return BadRequest(ApiErrorResponse.FromMessage("Document is already completed"));
            }

            var errors = document.GetValidationErrors();
            if (errors.Any())
            {
                return BadRequest(ApiErrorResponse.FromErrors(
                    "Cannot complete document with validation errors", errors));
            }

            try
            {
                document.Complete();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiErrorResponse.FromMessage(ex.Message));
            }

            return Ok(document.ToDto(
                GetPatientName(document.PatientId),
                GetPhysicianName(document.PhysicianId)));
        }

        /// <summary>
        /// Update a clinical document
        /// </summary>
        [HttpPut("{id}")]
        public ActionResult<ClinicalDocumentDto> Update(Guid id, [FromBody] UpdateClinicalDocumentRequest request)
        {
            var document = _documentService.GetDocumentById(id);
            if (document == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {id} not found"));
            }

            if (document.IsCompleted)
            {
                return BadRequest(ApiErrorResponse.FromMessage("Cannot modify a completed document"));
            }

            // Update fields
            if (!string.IsNullOrEmpty(request.ChiefComplaint))
            {
                document.ChiefComplaint = request.ChiefComplaint;
            }

            // Handle completion
            if (request.IsCompleted == true && !document.IsCompleted)
            {
                var errors = document.GetValidationErrors();
                if (errors.Any())
                {
                    return BadRequest(ApiErrorResponse.FromErrors(
                        "Cannot complete document with validation errors", errors));
                }
                document.Complete();
            }

            // Persist updates
            _documentService.UpdateDocument(document);

            return Ok(document.ToDto(
                GetPatientName(document.PatientId),
                GetPhysicianName(document.PhysicianId)));
        }

        /// <summary>
        /// Delete a clinical document
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var document = _documentService.GetDocumentById(id);
            if (document == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {id} not found"));
            }

            if (document.IsCompleted)
            {
                return BadRequest(ApiErrorResponse.FromMessage("Cannot delete a completed document"));
            }

            // Remove from patient's list
            var patient = _profileService.GetProfileById(document.PatientId) as PatientProfile;
            patient?.ClinicalDocumentIds.Remove(id);

            // Remove from appointment and persist
            _schedulerService.LinkClinicalDocument(document.AppointmentId, null);

            var success = _documentService.RemoveDocument(id);
            if (!success)
            {
                return BadRequest(ApiErrorResponse.FromMessage("Failed to delete document"));
            }

            return NoContent();
        }

        /// <summary>
        /// Search documents by diagnosis
        /// </summary>
        [HttpGet(ApiRoutes.ClinicalDocuments.SearchDiagnosis)]
        public ActionResult<IEnumerable<ClinicalDocumentSummaryDto>> SearchByDiagnosis([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(ApiErrorResponse.FromMessage("Search query is required"));
            }

            var documents = _documentService.SearchByDiagnosis(q);
            var dtos = documents.Select(d => d.ToSummaryDto(
                GetPatientName(d.PatientId),
                GetPhysicianName(d.PhysicianId)));

            return Ok(dtos);
        }

        /// <summary>
        /// Search documents by medication
        /// </summary>
        [HttpGet(ApiRoutes.ClinicalDocuments.SearchMedication)]
        public ActionResult<IEnumerable<ClinicalDocumentSummaryDto>> SearchByMedication([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(ApiErrorResponse.FromMessage("Search query is required"));
            }

            var documents = _documentService.SearchByMedication(q);
            var dtos = documents.Select(d => d.ToSummaryDto(
                GetPatientName(d.PatientId),
                GetPhysicianName(d.PhysicianId)));

            return Ok(dtos);
        }

        /// <summary>
        /// Get document statistics
        /// </summary>
        [HttpGet(ApiRoutes.ClinicalDocuments.Statistics)]
        public ActionResult<object> GetStatistics()
        {
            var stats = _documentService.GetStatistics();
            return Ok(new
            {
                stats.TotalDocuments,
                stats.CompletedDocuments,
                stats.IncompleteDocuments,
                stats.CompletionRate,
                stats.UniquePatients,
                stats.UniquePhysicians,
                stats.TotalDiagnoses,
                stats.TotalPrescriptions,
                stats.DocumentsToday,
                stats.DocumentsThisWeek
            });
        }

        /// <summary>
        /// Get SOAP note for a document
        /// </summary>
        [HttpGet(ApiRoutes.ClinicalDocuments.SoapNote)]
        public ActionResult<string> GetSOAPNote(Guid id)
        {
            var document = _documentService.GetDocumentById(id);
            if (document == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Clinical document with ID {id} not found"));
            }

            return Ok(document.GenerateSOAPNote());
        }

        #region Helper Methods

        private string? GetPatientName(Guid patientId)
        {
            var patient = _profileService.GetProfileById(patientId) as PatientProfile;
            return patient?.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty;
        }

        private string? GetPhysicianName(Guid physicianId)
        {
            var physician = _profileService.GetProfileById(physicianId) as PhysicianProfile;
            return physician?.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty;
        }

        #endregion
    }

}
