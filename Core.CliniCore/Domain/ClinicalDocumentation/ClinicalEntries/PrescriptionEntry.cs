using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.Extensions;

namespace Core.CliniCore.Domain.ClinicalDocumentation.ClinicalEntries
{
    /// <summary>
    /// Represents a medication prescription
    /// Must be linked to a supporting diagnosis
    /// </summary>
    public class PrescriptionEntry : AbstractClinicalEntry
    {
        public PrescriptionEntry(Guid authorId, Guid diagnosisId, string medicationName)
            : base(authorId, medicationName)
        {
            DiagnosisId = diagnosisId;
            MedicationName = medicationName;
        }

        public override ClinicalEntryType EntryType => ClinicalEntryType.Prescription;

        /// <summary>
        /// REQUIRED: The diagnosis this prescription treats
        /// </summary>
        public Guid DiagnosisId { get; private set; }

        /// <summary>
        /// Name of the medication
        /// </summary>
        public string MedicationName { get; set; }

        /// <summary>
        /// Dosage amount and unit (e.g., "500mg")
        /// </summary>
        public string? Dosage { get; set; }

        /// <summary>
        /// Frequency of administration
        /// </summary>
        public DosageFrequency? Frequency { get; set; }

        /// <summary>
        /// Route of administration
        /// </summary>
        public MedicationRoute Route { get; set; } = MedicationRoute.Oral;

        /// <summary>
        /// Duration of treatment
        /// </summary>
        public string? Duration { get; set; }

        /// <summary>
        /// Number of refills authorized
        /// </summary>
        public int Refills { get; set; } = 0;

        /// <summary>
        /// Whether generic substitution is allowed
        /// </summary>
        public bool GenericAllowed { get; set; } = true;

        /// <summary>
        /// DEA schedule if controlled substance
        /// </summary>
        public int? DEASchedule { get; set; }

        /// <summary>
        /// Date prescription expires
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Special instructions or warnings
        /// </summary>
        public string? Instructions { get; set; }

        /// <summary>
        /// NDC (National Drug Code) if available
        /// </summary>
        public string? NDCCode
        {
            get => Code;
            set => Code = value;
        }

        /// <summary>
        /// Generates standard prescription signature (Sig)
        /// </summary>
        public string GenerateSig()
        {
            var sig = $"Take {Dosage ?? "as directed"}";

            sig += $" by {Route.GetDisplayName()}";

            if (Frequency.HasValue)
                sig += $" {Frequency.Value.GetDisplayName()}";

            if (!string.IsNullOrEmpty(Duration))
                sig += $" for {Duration}";

            if (!string.IsNullOrEmpty(Instructions))
                sig += $". {Instructions}";

            return sig;
        }

        public override string GetDisplayString()
        {
            var sig = GenerateSig();
            var refillStr = Refills > 0 ? $" (Refills: {Refills})" : "";
            var deaStr = DEASchedule.HasValue ? $" [DEA Schedule {DEASchedule}]" : "";

            return $"{MedicationName}: {sig}{refillStr}{deaStr}";
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   DiagnosisId != Guid.Empty &&
                   !string.IsNullOrWhiteSpace(MedicationName);
        }

        public override List<string> GetValidationErrors()
        {
            var errors = base.GetValidationErrors();

            if (DiagnosisId == Guid.Empty)
            {
                errors.Add("Prescription must be linked to a diagnosis");
            }

            if (string.IsNullOrWhiteSpace(MedicationName))
            {
                errors.Add("Prescription must specify medication name");
            }

            if (string.IsNullOrWhiteSpace(Dosage))
            {
                errors.Add("Prescription should specify dosage");
            }

            if (!Frequency.HasValue)
            {
                errors.Add("Prescription should specify frequency");
            }

            if (DEASchedule.HasValue && (DEASchedule < 1 || DEASchedule > 5))
            {
                errors.Add("DEA Schedule must be between 1 and 5");
            }

            return errors;
        }
    }
}