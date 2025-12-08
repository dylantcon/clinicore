// Core.CliniCore/ClinicalDoc/ClinicalDocument.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.ClinicalDocumentation.ClinicalEntries;
using Core.CliniCore.Domain.Enumerations;

namespace Core.CliniCore.Domain.ClinicalDocumentation
{
    /// <summary>
    /// Composite pattern implementation for complete medical encounter documentation
    /// Follows SOAP (Subjective, Objective, Assessment, Plan) format
    /// </summary>
    public class ClinicalDocument : IIdentifiable
    {
        private readonly List<AbstractClinicalEntry> _entries = [];
        private readonly Dictionary<Guid, DiagnosisEntry> _diagnoses = [];
        private readonly Dictionary<Guid, PrescriptionEntry> _prescriptions = [];

        /// <summary>
        /// Gets the unique identifier for the clinical document.
        /// </summary>
        public Guid Id { get; }
        /// <summary>
        /// Gets the unique identifier of the patient associated with this document.
        /// </summary>
        public Guid PatientId { get; }
        /// <summary>
        /// Gets the unique identifier of the physician who authored this document.
        /// </summary>
        public Guid PhysicianId { get; }
        /// <summary>
        /// Gets the unique identifier of the appointment related to this document.
        /// </summary>
        public Guid AppointmentId { get; }
        /// <summary>
        /// Gets the date and time when the document was created.
        /// </summary>
        public DateTime CreatedAt { get; }
        /// <summary>
        /// Gets the date and time when the document was completed, or null if it is not yet completed.
        /// </summary>
        public DateTime? CompletedAt { get; private set; }
        /// <summary>
        /// Gets a value indicating whether the document has been completed.
        /// </summary>
        public bool IsCompleted => CompletedAt.HasValue;

        /// <summary>
        /// Creates a new clinical document with an auto-generated ID
        /// </summary>
        /// <param name="patientId">The unique identifier of the patient.</param>
        /// <param name="physicianId">The unique identifier of the physician.</param>
        /// <param name="appointmentId">The unique identifier of the appointment.</param>
        public ClinicalDocument(Guid patientId, Guid physicianId, Guid appointmentId)
            : this(Guid.NewGuid(), patientId, physicianId, appointmentId)
        {
        }

        /// <summary>
        /// Creates a new clinical document with a specified ID (for client-server ID synchronization)
        /// </summary>
        /// <param name="id">The unique identifier for the document.</param>
        /// <param name="patientId">The unique identifier of the patient.</param>
        /// <param name="physicianId">The unique identifier of the physician.</param>
        /// <param name="appointmentId">The unique identifier of the appointment.</param>
        public ClinicalDocument(Guid id, Guid patientId, Guid physicianId, Guid appointmentId)
        {
            Id = id;
            PatientId = patientId;
            PhysicianId = physicianId;
            AppointmentId = appointmentId;
            CreatedAt = DateTime.Now;
        }

        /// <summary>
        /// All entries in this document
        /// </summary>
        public IReadOnlyList<AbstractClinicalEntry> Entries => _entries.AsReadOnly();

        /// <summary>
        /// Chief complaint/reason for visit
        /// </summary>
        public string? ChiefComplaint { get; set; }

        /// <summary>
        /// Adds an entry to the document
        /// </summary>
        /// <param name="entry">The clinical entry to add.</param>
        /// <exception cref="InvalidOperationException">Thrown when attempting to modify a completed document or add a prescription for a non-existent diagnosis.</exception>
        public void AddEntry(AbstractClinicalEntry entry)
        {
            if (IsCompleted)
                throw new InvalidOperationException("Cannot modify Completed document");

            ArgumentNullException.ThrowIfNull(entry);

            // Special handling for diagnoses and prescriptions
            if (entry is DiagnosisEntry diagnosis)
            {
                _diagnoses[diagnosis.Id] = diagnosis;
            }
            else if (entry is PrescriptionEntry prescription)
            {
                // Validate prescription has valid diagnosis
                if (!_diagnoses.TryGetValue(prescription.DiagnosisId, out DiagnosisEntry? value))
                {
                    throw new InvalidOperationException(
                        $"Cannot add prescription without diagnosis. Diagnosis {prescription.DiagnosisId} not found.");
                }

                _prescriptions[prescription.Id] = prescription;
                value.AddRelatedPrescription(prescription.Id);
            }

            _entries.Add(entry);
        }

        /// <summary>
        /// Restores an entry during deserialization/mapping from persistence.
        /// Bypasses IsCompleted check and prescription-diagnosis validation since
        /// the data was already validated when originally persisted.
        /// </summary>
        /// <param name="entry">The clinical entry to restore.</param>
        internal void RestoreEntry(AbstractClinicalEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);

            if (entry is DiagnosisEntry diagnosis)
            {
                _diagnoses[diagnosis.Id] = diagnosis;
            }
            else if (entry is PrescriptionEntry prescription)
            {
                _prescriptions[prescription.Id] = prescription;
                // Link to diagnosis if it exists (it should, since we restore diagnoses first)
                if (_diagnoses.TryGetValue(prescription.DiagnosisId, out var relatedDiagnosis))
                {
                    relatedDiagnosis.AddRelatedPrescription(prescription.Id);
                }
            }

            _entries.Add(entry);
        }

        /// <summary>
        /// Gets all entries of a specific type
        /// </summary>
        /// <typeparam name="T">The type of clinical entry to retrieve.</typeparam>
        /// <returns>An enumerable collection of entries of the specified type.</returns>
        public IEnumerable<T> GetEntries<T>() where T : AbstractClinicalEntry
        {
            return _entries.OfType<T>();
        }

        /// <summary>
        /// Gets all observations (can be Subjective or Objective depending on Type)
        /// </summary>
        /// <returns>An enumerable collection of observation entries.</returns>
        public IEnumerable<ObservationEntry> GetObservations()
            => GetEntries<ObservationEntry>();

        /// <summary>
        /// Gets all assessments
        /// </summary>
        /// <returns>An enumerable collection of assessment entries.</returns>
        public IEnumerable<AssessmentEntry> GetAssessments()
            => GetEntries<AssessmentEntry>();

        /// <summary>
        /// Gets all diagnoses
        /// </summary>
        /// <returns>An enumerable collection of diagnosis entries.</returns>
        public IEnumerable<DiagnosisEntry> GetDiagnoses()
            => _diagnoses.Values;

        /// <summary>
        /// Gets all prescriptions
        /// </summary>
        /// <returns>An enumerable collection of prescription entries.</returns>
        public IEnumerable<PrescriptionEntry> GetPrescriptions()
            => _prescriptions.Values;

        /// <summary>
        /// Gets all plan entries
        /// </summary>
        /// <returns>An enumerable collection of plan entries.</returns>
        public IEnumerable<PlanEntry> GetPlans()
            => GetEntries<PlanEntry>();

        /// <summary>
        /// Gets prescriptions for a specific diagnosis
        /// </summary>
        /// <param name="diagnosisId">The unique identifier of the diagnosis.</param>
        /// <returns>An enumerable collection of prescription entries related to the specified diagnosis.</returns>
        public IEnumerable<PrescriptionEntry> GetPrescriptionsForDiagnosis(Guid diagnosisId)
        {
            if (_diagnoses.TryGetValue(diagnosisId, out var diagnosis))
            {
                return diagnosis.RelatedPrescriptions
                    .Where(id => _prescriptions.ContainsKey(id))
                    .Select(id => _prescriptions[id]);
            }
            return [];
        }

        /// <summary>
        /// Validates the document is complete
        /// </summary>
        /// <returns>True if the document meets the minimum requirements for completion; otherwise, false.</returns>
        public bool IsComplete()
        {
            // Minimum requirements for a complete document
            return !string.IsNullOrWhiteSpace(ChiefComplaint) &&
                   GetObservations().Any() &&
                   GetAssessments().Any() &&
                   GetDiagnoses().Any() &&
                   GetPlans().Any();
        }

        /// <summary>
        /// Gets validation errors
        /// </summary>
        /// <returns>A list of strings describing any validation errors found in the document.</returns>
        public List<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(ChiefComplaint))
                errors.Add("Chief complaint is required");

            if (!GetObservations().Any())
                errors.Add("At least one observation is required");

            if (!GetAssessments().Any())
                errors.Add("At least one assessment is required");

            if (!GetDiagnoses().Any())
                errors.Add("At least one diagnosis is required");

            if (!GetPlans().Any())
                errors.Add("At least one plan entry is required");

            // Validate all entries
            foreach (var entry in _entries)
            {
                var entryErrors = entry.GetValidationErrors();
                errors.AddRange(entryErrors.Select(e => $"{entry.EntryType}: {e}"));
            }

            // Validate all prescriptions have valid diagnoses
            foreach (var prescription in _prescriptions.Values)
            {
                if (!_diagnoses.ContainsKey(prescription.DiagnosisId))
                {
                    errors.Add($"Prescription '{prescription.MedicationName}' references invalid diagnosis");
                }
            }

            return errors;
        }

        /// <summary>
        /// Completes the document, preventing further modifications
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the document is already completed or fails validation.</exception>
        public void Complete()
        {
            if (IsCompleted)
                throw new InvalidOperationException("Document already Completed");

            var errors = GetValidationErrors();
            if (errors.Count != 0)
            {
                throw new InvalidOperationException(
                    $"Cannot complete partial document: {string.Join("; ", errors)}");
            }

            CompletedAt = DateTime.Now;
        }

        /// <summary>
        /// Generates a formatted SOAP note
        /// </summary>
        /// <returns>A string containing the formatted SOAP note.</returns>
        public string GenerateSOAPNote()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== CLINICAL DOCUMENTATION ===");
            sb.AppendLine($"Date: {CreatedAt:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"Patient ID: {PatientId}");
            sb.AppendLine($"Physician ID: {PhysicianId}");
            sb.AppendLine();

            // Subjective
            sb.AppendLine("SUBJECTIVE:");
            sb.AppendLine($"Chief Complaint: {ChiefComplaint ?? "Not documented"}");
            foreach (var obs in GetObservations().Where(o =>
                o.Type == ObservationType.ChiefComplaint ||
                o.Type == ObservationType.HistoryOfPresentIllness ||
                o.Type == ObservationType.SocialHistory ||
                o.Type == ObservationType.FamilyHistory ||
                o.Type == ObservationType.Allergy))
            {
                sb.AppendLine($"  - {obs.GetDisplayString()}");
            }
            sb.AppendLine();

            // Objective
            sb.AppendLine("OBJECTIVE:");
            foreach (var obs in GetObservations().Where(o =>
                o.Type == ObservationType.PhysicalExam ||
                o.Type == ObservationType.VitalSigns ||
                o.Type == ObservationType.LabResult ||
                o.Type == ObservationType.ImagingResult ||
                o.Type == ObservationType.ReviewOfSystems))
            {
                sb.AppendLine($"  - {obs.GetDisplayString()}");
            }
            sb.AppendLine();

            // Assessment
            sb.AppendLine("ASSESSMENT:");
            foreach (var assessment in GetAssessments())
            {
                sb.AppendLine($"  {assessment.GetDisplayString()}");
            }
            
            sb.AppendLine("\nDIAGNOSES:");
            foreach (var diagnosis in GetDiagnoses())
            {
                sb.AppendLine($"  - {diagnosis.GetDisplayString()}");
            }
            sb.AppendLine();

            // Plan
            sb.AppendLine("PLAN:");
            foreach (var plan in GetPlans())
            {
                sb.AppendLine($"  - {plan.GetDisplayString()}");
            }
            
            if (GetPrescriptions().Any())
            {
                sb.AppendLine("\nPRESCRIPTIONS:");
                foreach (var rx in GetPrescriptions())
                {
                    sb.AppendLine($"  - {rx.GetDisplayString()}");
                }
            }

            if (IsCompleted)
            {
                sb.AppendLine();
                sb.AppendLine($"Document completed at: {CompletedAt:yyyy-MM-dd HH:mm}");
            }

            return sb.ToString();
        }
    }
}