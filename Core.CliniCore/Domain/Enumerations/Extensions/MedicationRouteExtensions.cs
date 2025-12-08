namespace Core.CliniCore.Domain.Enumerations.Extensions
{
    public static class MedicationRouteExtensions
    {
        public static MedicationRoute[] All => Enum.GetValues<MedicationRoute>();
        public static int TypeCount => All.Length;

        public static MedicationRoute? GetByIndex(int oneBasedIndex)
        {
            var all = All;
            if (oneBasedIndex < 1 || oneBasedIndex > all.Length)
                return null;
            return all[oneBasedIndex - 1];
        }

        public static MedicationRoute? FindByDisplayName(string display)
        {
            return All.FirstOrDefault(r => r
                .GetDisplayName()
                .Equals(display, StringComparison.OrdinalIgnoreCase)
            );
        }

        public static string GetDisplayName(this MedicationRoute route)
        {
            return route switch
            {
                MedicationRoute.Oral => "Oral",
                MedicationRoute.Intravenous => "Intravenous (IV)",
                MedicationRoute.Intramuscular => "Intramuscular (IM)",
                MedicationRoute.Subcutaneous => "Subcutaneous (SubQ)",
                MedicationRoute.Topical => "Topical",
                MedicationRoute.Sublingual => "Sublingual",
                MedicationRoute.Transdermal => "Transdermal",
                MedicationRoute.Inhaled => "Inhaled",
                MedicationRoute.Rectal => "Rectal",
                MedicationRoute.Ophthalmic => "Ophthalmic (Eye)",
                MedicationRoute.Otic => "Otic (Ear)",
                MedicationRoute.Nasal => "Nasal",
                _ => route.ToString()
            };
        }

        public static string GetAbbreviation(this MedicationRoute route)
        {
            return route switch
            {
                MedicationRoute.Oral => "PO",
                MedicationRoute.Intravenous => "IV",
                MedicationRoute.Intramuscular => "IM",
                MedicationRoute.Subcutaneous => "SubQ",
                MedicationRoute.Topical => "TOP",
                MedicationRoute.Sublingual => "SL",
                MedicationRoute.Transdermal => "TD",
                MedicationRoute.Inhaled => "INH",
                MedicationRoute.Rectal => "PR",
                MedicationRoute.Ophthalmic => "OU",
                MedicationRoute.Otic => "AU",
                MedicationRoute.Nasal => "NAS",
                _ => route.ToString()
            };
        }

        public static List<(int Index, string Display)> GetNumberedList()
        {
            return All.Select((r, index) => (index + 1, r.GetDisplayName())).ToList();
        }
    }
}
