namespace Core.CliniCore.Domain.Enumerations.Extensions
{
    public static class EntrySeverityExtensions
    {
        public static EntrySeverity[] All => Enum.GetValues<EntrySeverity>();
        public static int TypeCount => All.Length;

        public static EntrySeverity? GetByIndex(int oneBasedIndex)
        {
            var all = All;
            if (oneBasedIndex < 1 || oneBasedIndex > all.Length)
                return null;
            return all[oneBasedIndex - 1];
        }

        public static EntrySeverity? FindByDisplayName(string display)
        {
            return All.FirstOrDefault(s => s
                .GetDisplayName()
                .Equals(display, StringComparison.OrdinalIgnoreCase)
            );
        }

        public static string GetDisplayName(this EntrySeverity severity)
        {
            return severity.ToString();
        }

        public static bool RequiresImmediateAttention(this EntrySeverity severity)
        {
            return severity >= EntrySeverity.Urgent;
        }

        public static List<(int Index, string Display)> GetNumberedList()
        {
            return All.Select((s, index) => (index + 1, s.GetDisplayName())).ToList();
        }
    }
}
