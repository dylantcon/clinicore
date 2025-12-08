namespace Core.CliniCore.Domain.Enumerations.Extensions
{
    public static class ClinicalEntryTypeExtensions
    {
        public static ClinicalEntryType[] All => Enum.GetValues<ClinicalEntryType>();
        public static int TypeCount => All.Length;

        public static ClinicalEntryType? GetByIndex(int oneBasedIndex)
        {
            var all = All;
            if (oneBasedIndex < 1 || oneBasedIndex > all.Length)
                return null;
            return all[oneBasedIndex - 1];
        }

        public static ClinicalEntryType? FindByDisplayName(string display)
        {
            return All.FirstOrDefault(t => t
                .GetDisplayName()
                .Equals(display, StringComparison.OrdinalIgnoreCase)
            );
        }

        public static string GetDisplayName(this ClinicalEntryType type)
        {
            return type switch
            {
                ClinicalEntryType.ChiefComplaint => "Chief Complaint",
                ClinicalEntryType.Observation => "Observation",
                ClinicalEntryType.Assessment => "Assessment",
                ClinicalEntryType.Diagnosis => "Diagnosis",
                ClinicalEntryType.Plan => "Plan",
                ClinicalEntryType.Prescription => "Prescription",
                ClinicalEntryType.ProgressNote => "Progress Note",
                ClinicalEntryType.Procedure => "Procedure",
                ClinicalEntryType.LabResult => "Lab Result",
                ClinicalEntryType.VitalSigns => "Vital Signs",
                _ => type.ToString()
            };
        }

        public static string GetAbbreviation(this ClinicalEntryType type)
        {
            return type switch
            {
                ClinicalEntryType.ChiefComplaint => "CC",
                ClinicalEntryType.Observation => "O",
                ClinicalEntryType.Assessment => "A",
                ClinicalEntryType.Diagnosis => "Dx",
                ClinicalEntryType.Plan => "P",
                ClinicalEntryType.Prescription => "Rx",
                ClinicalEntryType.ProgressNote => "PN",
                ClinicalEntryType.Procedure => "Proc",
                ClinicalEntryType.LabResult => "Lab",
                ClinicalEntryType.VitalSigns => "VS",
                _ => type.ToString()
            };
        }

        public static List<(int Index, string Display)> GetNumberedList()
        {
            return All.Select((t, index) => (index + 1, t.GetDisplayName())).ToList();
        }
    }
}
