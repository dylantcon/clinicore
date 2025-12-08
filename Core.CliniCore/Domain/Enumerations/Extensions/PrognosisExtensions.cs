namespace Core.CliniCore.Domain.Enumerations.Extensions
{
    public static class PrognosisExtensions
    {
        public static Prognosis[] All => Enum.GetValues<Prognosis>();
        public static int TypeCount => All.Length;

        public static Prognosis? GetByIndex(int oneBasedIndex)
        {
            var all = All;
            if (oneBasedIndex < 1 || oneBasedIndex > all.Length)
                return null;
            return all[oneBasedIndex - 1];
        }

        public static Prognosis? FindByDisplayName(string display)
        {
            return All.FirstOrDefault(p => p
                .GetDisplayName()
                .Equals(display, StringComparison.OrdinalIgnoreCase)
            );
        }

        public static string GetDisplayName(this Prognosis prognosis)
        {
            return prognosis.ToString();
        }

        public static bool IsPositive(this Prognosis prognosis)
        {
            return prognosis == Prognosis.Excellent || prognosis == Prognosis.Good;
        }

        public static List<(int Index, string Display)> GetNumberedList()
        {
            return All.Select((p, index) => (index + 1, p.GetDisplayName())).ToList();
        }
    }
}
