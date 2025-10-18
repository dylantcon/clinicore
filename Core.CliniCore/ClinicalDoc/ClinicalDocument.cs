// Core.CliniCore/ClinicalDoc/ClinicalDocument.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.ClinicalDoc
{
    /// <summary>
    /// Composite pattern implementation for complete medical encounter documentation
    /// Follows SOAP (Subjective, Objective, Assessment, Plan) format
    /// </summary>
    public class ClinicalDocument
    {
        private readonly List<AbstractClinicalEntry> _entries;
        private readonly Dictionary<Guid, DiagnosisEntry> _diagnoses;
        private readonly Dictionary<Guid, PrescriptionEntry> _prescriptions;

        public ClinicalDocument(Guid patientId, Guid physicianId, Guid appointmentId)
        {
            Id = Guid.NewGuid();
            PatientId = patientId;
            PhysicianId = physicianId;
            AppointmentId = appointmentId;
            CreatedAt = DateTime.Now;
            _entries = new List<AbstractClinicalEntry>();
            _diagnoses = new Dictionary<Guid, DiagnosisEntry>();
            _prescriptions = new Dictionary<Guid, PrescriptionEntry>();
        }

        public Guid Id { get; }
        public Guid PatientId { get; }
        public Guid PhysicianId { get; }
        public Guid AppointmentId { get; }
        public DateTime CreatedAt { get; }
        public DateTime? CompletedAt { get; private set; }
        public bool IsCompleted => CompletedAt.HasValue;

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
        public void AddEntry(AbstractClinicalEntry entry)
        {
            if (IsCompleted)
                throw new InvalidOperationException("Cannot modify Completed document");

            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            // Special handling for diagnoses and prescriptions
            if (entry is DiagnosisEntry diagnosis)
            {
                _diagnoses[diagnosis.Id] = diagnosis;
            }
            else if (entry is PrescriptionEntry prescription)
            {
                // Validate prescription has valid diagnosis
                if (!_diagnoses.ContainsKey(prescription.DiagnosisId))
                {
                    throw new InvalidOperationException(
                        $"Cannot add prescription without diagnosis. Diagnosis {prescription.DiagnosisId} not found.");
                }

                _prescriptions[prescription.Id] = prescription;
                
                // Link prescription to diagnosis
                _diagnoses[prescription.DiagnosisId].AddRelatedPrescription(prescription.Id);
            }

            _entries.Add(entry);
        }

        /// <summary>
        /// Gets all entries of a specific type
        /// </summary>
        public IEnumerable<T> GetEntries<T>() where T : AbstractClinicalEntry
        {
            return _entries.OfType<T>();
        }

        /// <summary>
        /// Gets all observations (can be Subjective or Objective depending on Type)
        /// </summary>
        public IEnumerable<ObservationEntry> GetObservations()
            => GetEntries<ObservationEntry>();

        /// <summary>
        /// Gets all assessments
        /// </summary>
        public IEnumerable<AssessmentEntry> GetAssessments()
            => GetEntries<AssessmentEntry>();

        /// <summary>
        /// Gets all diagnoses
        /// </summary>
        public IEnumerable<DiagnosisEntry> GetDiagnoses()
            => _diagnoses.Values;

        /// <summary>
        /// Gets all prescriptions
        /// </summary>
        public IEnumerable<PrescriptionEntry> GetPrescriptions()
            => _prescriptions.Values;

        /// <summary>
        /// Gets all plan entries
        /// </summary>
        public IEnumerable<PlanEntry> GetPlans()
            => GetEntries<PlanEntry>();

        /// <summary>
        /// Gets prescriptions for a specific diagnosis
        /// </summary>
        public IEnumerable<PrescriptionEntry> GetPrescriptionsForDiagnosis(Guid diagnosisId)
        {
            if (_diagnoses.TryGetValue(diagnosisId, out var diagnosis))
            {
                return diagnosis.RelatedPrescriptions
                    .Where(id => _prescriptions.ContainsKey(id))
                    .Select(id => _prescriptions[id]);
            }
            return Enumerable.Empty<PrescriptionEntry>();
        }

        /// <summary>
        /// Validates the document is complete
        /// </summary>
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
        public void Complete()
        {
            if (IsCompleted)
                throw new InvalidOperationException("Document already Completed");

            var errors = GetValidationErrors();
            if (errors.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot complete partial document: {string.Join("; ", errors)}");
            }

            CompletedAt = DateTime.Now;
        }

        /// <summary>
        /// Generates a formatted SOAP note
        /// </summary>
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