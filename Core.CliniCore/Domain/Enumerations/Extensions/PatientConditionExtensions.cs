namespace Core.CliniCore.Domain.Enumerations.Extensions
{
    public static class PatientConditionExtensions
    {
        public static PatientCondition[] All => Enum.GetValues<PatientCondition>();
        public static int TypeCount => All.Length;

        public static PatientCondition? GetByIndex(int oneBasedIndex)
        {
            var all = All;
            if (oneBasedIndex < 1 || oneBasedIndex > all.Length)
                return null;
            return all[oneBasedIndex - 1];
        }

        public static PatientCondition? FindByDisplayName(string display)
        {
            return All.FirstOrDefault(c => c
                .GetDisplayName()
                .Equals(display, StringComparison.OrdinalIgnoreCase)
            );
        }

        public static string GetDisplayName(this PatientCondition condition)
        {
            return condition.ToString();
        }

        public static bool RequiresEscalation(this PatientCondition condition)
        {
            return condition == PatientCondition.Critical || condition == PatientCondition.Worsening;
        }

        public static List<(int Index, string Display)> GetNumberedList()
        {
            return All.Select((c, index) => (index + 1, c.GetDisplayName())).ToList();
        }
    }
}
