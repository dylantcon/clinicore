// Core.CliniCore/ClinicalDoc/ObservationEntry.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.ClinicalDoc
{
    /// <summary>
    /// Represents a clinical observation (objective finding)
    /// </summary>
    public class ObservationEntry : AbstractClinicalEntry
    {
        public ObservationEntry(Guid authorId, string observation)
            : base(authorId, observation)
        {
            VitalSigns = new Dictionary<string, string>();
        }

        public override ClinicalEntryType EntryType => ClinicalEntryType.Observation;

        /// <summary>
        /// Type of observation
        /// </summary>
        public ObservationType Type { get; set; } = ObservationType.PhysicalExam;

        /// <summary>
        /// Body system or area examined
        /// </summary>
        public string? BodySystem { get; set; }

        /// <summary>
        /// Whether this is a normal or abnormal finding
        /// </summary>
        public bool IsAbnormal { get; set; }

        /// <summary>
        /// Vital signs if applicable (BP, HR, Temp, etc.)
        /// </summary>
        public Dictionary<string, string> VitalSigns { get; private set; }

        /// <summary>
        /// Numeric value if this is a measurement
        /// </summary>
        public double? NumericValue { get; set; }

        /// <summary>
        /// Unit of measurement
        /// </summary>
        public string? Unit { get; set; }

        /// <summary>
        /// Reference range for this observation
        /// </summary>
        public string? ReferenceRange { get; set; }

        /// <summary>
        /// Adds a vital sign measurement
        /// </summary>
        public void AddVitalSign(string name, string value)
        {
            VitalSigns[name] = value;
        }

        public override string GetDisplayString()
        {
            var abnormalStr = IsAbnormal ? "[ABNORMAL] " : "";
            var systemStr = !string.IsNullOrEmpty(BodySystem) ? $"[{BodySystem}] " : "";

            var valueStr = "";
            if (NumericValue.HasValue)
            {
                valueStr = $": {NumericValue}";
                if (!string.IsNullOrEmpty(Unit))
                    valueStr += $" {Unit}";
                if (!string.IsNullOrEmpty(ReferenceRange))
                    valueStr += $" (Ref: {ReferenceRange})";
            }

            return $"{abnormalStr}{systemStr}{base.Content}{valueStr}";
        }

        /// <summary>
        /// Formats vital signs for display
        /// </summary>
        public string GetVitalSignsDisplay()
        {
            if (!VitalSigns.Any())
                return "No vital signs recorded";

            return string.Join(", ", VitalSigns.Select(vs => $"{vs.Key}: {vs.Value}"));
        }
    }

    public enum ObservationType
    {
        ChiefComplaint,
        HistoryOfPresentIllness,
        PhysicalExam,
        VitalSigns,
        LabResult,
        ImagingResult,
        ReviewOfSystems,
        SocialHistory,
        FamilyHistory,
        Allergy
    }
}