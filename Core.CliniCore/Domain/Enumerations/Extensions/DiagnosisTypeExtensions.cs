namespace Core.CliniCore.Domain.Enumerations.Extensions
{
    public static class DiagnosisTypeExtensions
    {
        public static DiagnosisType[] All => Enum.GetValues<DiagnosisType>();
        public static int TypeCount => All.Length;

        public static DiagnosisType? GetByIndex(int oneBasedIndex)
        {
            var all = All;
            if (oneBasedIndex < 1 || oneBasedIndex > all.Length)
                return null;
            return all[oneBasedIndex - 1];
        }

        public static DiagnosisType? FindByDisplayName(string display)
        {
            return All.FirstOrDefault(t => t
                .GetDisplayName()
                .Equals(display, StringComparison.OrdinalIgnoreCase)
            );
        }

        public static string GetDisplayName(this DiagnosisType type)
        {
            return type switch
            {
                DiagnosisType.Differential => "Differential",
                DiagnosisType.Working => "Working",
                DiagnosisType.Final => "Final",
                DiagnosisType.RuledOut => "Ruled Out",
                _ => type.ToString()
            };
        }

        public static bool IsConfirmed(this DiagnosisType type)
        {
            return type == DiagnosisType.Final;
        }

        public static bool RequiresICD10Code(this DiagnosisType type)
        {
            return type == DiagnosisType.Final;
        }

        public static List<(int Index, string Display)> GetNumberedList()
        {
            return All.Select((t, index) => (index + 1, t.GetDisplayName())).ToList();
        }
    }
}
