namespace Core.CliniCore.Domain.Enumerations.Extensions
{
    public static class ConfidenceLevelExtensions
    {
        public static ConfidenceLevel[] All => Enum.GetValues<ConfidenceLevel>();
        public static int TypeCount => All.Length;

        public static ConfidenceLevel? GetByIndex(int oneBasedIndex)
        {
            var all = All;
            if (oneBasedIndex < 1 || oneBasedIndex > all.Length)
                return null;
            return all[oneBasedIndex - 1];
        }

        public static ConfidenceLevel? FindByDisplayName(string display)
        {
            return All.FirstOrDefault(c => c
                .GetDisplayName()
                .Equals(display, StringComparison.OrdinalIgnoreCase)
            );
        }

        public static string GetDisplayName(this ConfidenceLevel level)
        {
            return level.ToString();
        }

        public static bool IsHighConfidence(this ConfidenceLevel level)
        {
            return level >= ConfidenceLevel.High;
        }

        public static List<(int Index, string Display)> GetNumberedList()
        {
            return All.Select((c, index) => (index + 1, c.GetDisplayName())).ToList();
        }
    }
}
