namespace Core.CliniCore.Domain.Enumerations.Extensions
{
    public static class PlanTypeExtensions
    {
        public static PlanType[] All => Enum.GetValues<PlanType>();
        public static int TypeCount => All.Length;

        public static PlanType? GetByIndex(int oneBasedIndex)
        {
            var all = All;
            if (oneBasedIndex < 1 || oneBasedIndex > all.Length)
                return null;
            return all[oneBasedIndex - 1];
        }

        public static PlanType? FindByDisplayName(string display)
        {
            return All.FirstOrDefault(t => t
                .GetDisplayName()
                .Equals(display, StringComparison.OrdinalIgnoreCase)
            );
        }

        public static string GetDisplayName(this PlanType type)
        {
            return type switch
            {
                PlanType.Treatment => "Treatment",
                PlanType.Diagnostic => "Diagnostic",
                PlanType.Referral => "Referral",
                PlanType.FollowUp => "Follow-Up",
                PlanType.PatientEducation => "Patient Education",
                PlanType.Procedure => "Procedure",
                PlanType.Monitoring => "Monitoring",
                PlanType.Prevention => "Prevention",
                _ => type.ToString()
            };
        }

        public static List<(int Index, string Display)> GetNumberedList()
        {
            return All.Select((t, index) => (index + 1, t.GetDisplayName())).ToList();
        }
    }
}
