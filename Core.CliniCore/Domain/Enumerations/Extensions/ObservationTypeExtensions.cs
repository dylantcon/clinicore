namespace Core.CliniCore.Domain.Enumerations.Extensions
{
    public static class ObservationTypeExtensions
    {
        public static ObservationType[] All => Enum.GetValues<ObservationType>();
        public static int TypeCount => All.Length;

        public static ObservationType? GetByIndex(int oneBasedIndex)
        {
            var all = All;
            if (oneBasedIndex < 1 || oneBasedIndex > all.Length)
                return null;
            return all[oneBasedIndex - 1];
        }

        public static ObservationType? FindByDisplayName(string display)
        {
            return All.FirstOrDefault(t => t
                .GetDisplayName()
                .Equals(display, StringComparison.OrdinalIgnoreCase)
            );
        }

        public static string GetDisplayName(this ObservationType type)
        {
            return type switch
            {
                ObservationType.ChiefComplaint => "Chief Complaint",
                ObservationType.HistoryOfPresentIllness => "History of Present Illness",
                ObservationType.PhysicalExam => "Physical Exam",
                ObservationType.VitalSigns => "Vital Signs",
                ObservationType.LabResult => "Lab Result",
                ObservationType.ImagingResult => "Imaging Result",
                ObservationType.ReviewOfSystems => "Review of Systems",
                ObservationType.SocialHistory => "Social History",
                ObservationType.FamilyHistory => "Family History",
                ObservationType.Allergy => "Allergy",
                _ => type.ToString()
            };
        }

        public static string GetAbbreviation(this ObservationType type)
        {
            return type switch
            {
                ObservationType.ChiefComplaint => "CC",
                ObservationType.HistoryOfPresentIllness => "HPI",
                ObservationType.PhysicalExam => "PE",
                ObservationType.VitalSigns => "VS",
                ObservationType.LabResult => "Lab",
                ObservationType.ImagingResult => "Img",
                ObservationType.ReviewOfSystems => "ROS",
                ObservationType.SocialHistory => "SH",
                ObservationType.FamilyHistory => "FH",
                ObservationType.Allergy => "Alg",
                _ => type.ToString()
            };
        }

        public static List<(int Index, string Display)> GetNumberedList()
        {
            return All.Select((t, index) => (index + 1, t.GetDisplayName())).ToList();
        }

        /// <summary>
        /// Returns true if this observation type is subjective (patient-reported).
        /// Subjective: ChiefComplaint, HPI, SocialHistory, FamilyHistory, Allergy
        /// </summary>
        public static bool IsSubjective(this ObservationType type)
        {
            return type is ObservationType.ChiefComplaint
                or ObservationType.HistoryOfPresentIllness
                or ObservationType.SocialHistory
                or ObservationType.FamilyHistory
                or ObservationType.Allergy;
        }

        /// <summary>
        /// Returns true if this observation type is objective (clinician-observed/measured).
        /// Objective: PhysicalExam, VitalSigns, LabResult, ImagingResult, ReviewOfSystems
        /// </summary>
        public static bool IsObjective(this ObservationType type) => !type.IsSubjective();
    }
}
