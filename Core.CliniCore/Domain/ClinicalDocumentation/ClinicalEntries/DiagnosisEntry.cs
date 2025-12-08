using Core.CliniCore.Domain.Enumerations;

namespace Core.CliniCore.Domain.ClinicalDocumentation.ClinicalEntries
{
    /// <summary>
    /// Represents a clinical diagnosis
    /// </summary>
    public class DiagnosisEntry : AbstractClinicalEntry
    {
        public DiagnosisEntry(Guid authorId, string diagnosisDescription)
            : base(authorId, diagnosisDescription)
        {
            RelatedPrescriptions = new List<Guid>();
            SupportingObservations = new List<Guid>();
        }

        public override ClinicalEntryType EntryType => ClinicalEntryType.Diagnosis;

        /// <summary>
        /// Type of diagnosis (differential, working, final, etc.)
        /// </summary>
        public DiagnosisType Type { get; set; } = DiagnosisType.Working;

        /// <summary>
        /// ICD-10 diagnosis code
        /// </summary>
        public string? ICD10Code
        {
            get => Code;
            set => Code = value;
        }

        /// <summary>
        /// Whether this is the primary diagnosis
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// Date of onset for this condition
        /// </summary>
        public DateTime? OnsetDate { get; set; }

        /// <summary>
        /// IDs of prescriptions linked to this diagnosis
        /// </summary>
        public List<Guid> RelatedPrescriptions { get; private set; }

        /// <summary>
        /// IDs of observations supporting this diagnosis
        /// </summary>
        public List<Guid> SupportingObservations { get; private set; }

        /// <summary>
        /// Clinical status of the diagnosis
        /// </summary>
        public DiagnosisStatus Status { get; set; } = DiagnosisStatus.Active;

        /// <summary>
        /// Adds a prescription that treats this diagnosis
        /// </summary>
        public void AddRelatedPrescription(Guid prescriptionId)
        {
            if (!RelatedPrescriptions.Contains(prescriptionId))
            {
                RelatedPrescriptions.Add(prescriptionId);
            }
        }

        /// <summary>
        /// Adds an observation that supports this diagnosis
        /// </summary>
        public void AddSupportingObservation(Guid observationId)
        {
            if (!SupportingObservations.Contains(observationId))
            {
                SupportingObservations.Add(observationId);
            }
        }

        public override string GetDisplayString()
        {
            var typeStr = Type != DiagnosisType.Final ? $"[{Type}] " : "";
            var primaryStr = IsPrimary ? "[PRIMARY] " : "";
            var statusStr = Status != DiagnosisStatus.Active ? $" ({Status})" : "";

            return $"{primaryStr}{typeStr}{base.GetDisplayString()}{statusStr}";
        }

        public override List<string> GetValidationErrors()
        {
            var errors = base.GetValidationErrors();

            if (Type == DiagnosisType.Final && string.IsNullOrEmpty(ICD10Code))
            {
                errors.Add("Final diagnosis requires an ICD-10 code");
            }

            return errors;
        }
    }
}