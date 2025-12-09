namespace Core.CliniCore.Domain.Enumerations.Extensions
{
    public static class BodySystemExtensions
    {
        public static BodySystem[] All => Enum.GetValues<BodySystem>();
        public static int TypeCount => All.Length;

        public static BodySystem? GetByIndex(int oneBasedIndex)
        {
            var all = All;
            if (oneBasedIndex < 1 || oneBasedIndex > all.Length)
                return null;
            return all[oneBasedIndex - 1];
        }

        public static BodySystem? FindByDisplayName(string display)
        {
            return All.FirstOrDefault(s => s
                .GetDisplayName()
                .Equals(display, StringComparison.OrdinalIgnoreCase)
            );
        }

        public static string GetDisplayName(this BodySystem system)
        {
            return system switch
            {
                BodySystem.General => "General",
                BodySystem.HEENT => "HEENT (Head, Eyes, Ears, Nose, Throat)",
                BodySystem.Cardiovascular => "Cardiovascular",
                BodySystem.Respiratory => "Respiratory",
                BodySystem.Gastrointestinal => "Gastrointestinal",
                BodySystem.Genitourinary => "Genitourinary",
                BodySystem.Musculoskeletal => "Musculoskeletal",
                BodySystem.Neurological => "Neurological",
                BodySystem.Integumentary => "Integumentary (Skin)",
                BodySystem.Endocrine => "Endocrine",
                BodySystem.Hematologic => "Hematologic",
                BodySystem.Immunologic => "Immunologic",
                BodySystem.Psychiatric => "Psychiatric",
                _ => system.ToString()
            };
        }

        public static string GetAbbreviation(this BodySystem system)
        {
            return system switch
            {
                BodySystem.General => "Gen",
                BodySystem.HEENT => "HEENT",
                BodySystem.Cardiovascular => "CV",
                BodySystem.Respiratory => "Resp",
                BodySystem.Gastrointestinal => "GI",
                BodySystem.Genitourinary => "GU",
                BodySystem.Musculoskeletal => "MSK",
                BodySystem.Neurological => "Neuro",
                BodySystem.Integumentary => "Skin",
                BodySystem.Endocrine => "Endo",
                BodySystem.Hematologic => "Heme",
                BodySystem.Immunologic => "Imm",
                BodySystem.Psychiatric => "Psych",
                _ => system.ToString()
            };
        }

        public static List<(int Index, string Display)> GetNumberedList()
        {
            return [.. All.Select((s, index) => (index + 1, s.GetDisplayName()))];
        }
    }
}
