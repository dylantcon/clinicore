namespace Core.CliniCore.Domain.Enumerations.Extensions
{
    public static class DiagnosisStatusExtensions
    {
        public static DiagnosisStatus[] All => Enum.GetValues<DiagnosisStatus>();
        public static int TypeCount => All.Length;

        public static DiagnosisStatus? GetByIndex(int oneBasedIndex)
        {
            var all = All;
            if (oneBasedIndex < 1 || oneBasedIndex > all.Length)
                return null;
            return all[oneBasedIndex - 1];
        }

        public static DiagnosisStatus? FindByDisplayName(string display)
        {
            return All.FirstOrDefault(s => s
                .GetDisplayName()
                .Equals(display, StringComparison.OrdinalIgnoreCase)
            );
        }

        public static string GetDisplayName(this DiagnosisStatus status)
        {
            return status.ToString();
        }

        public static bool IsActive(this DiagnosisStatus status)
        {
            return status == DiagnosisStatus.Active ||
                   status == DiagnosisStatus.Chronic ||
                   status == DiagnosisStatus.Recurrence;
        }

        public static List<(int Index, string Display)> GetNumberedList()
        {
            return All.Select((s, index) => (index + 1, s.GetDisplayName())).ToList();
        }
    }
}
