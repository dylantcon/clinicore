namespace Core.CliniCore.Domain.Enumerations.Extensions
{
    public static class DosageFrequencyExtensions
    {
        public static DosageFrequency[] All => Enum.GetValues<DosageFrequency>();
        public static int TypeCount => All.Length;

        public static DosageFrequency? GetByIndex(int oneBasedIndex)
        {
            var all = All;
            if (oneBasedIndex < 1 || oneBasedIndex > all.Length)
                return null;
            return all[oneBasedIndex - 1];
        }

        public static DosageFrequency? FindByDisplayName(string display)
        {
            return All.FirstOrDefault(f => f
                .GetDisplayName()
                .Equals(display, StringComparison.OrdinalIgnoreCase)
            );
        }

        public static DosageFrequency? FindByAbbreviation(string abbrev)
        {
            return All.FirstOrDefault(f => f
                .GetAbbreviation()
                .Equals(abbrev, StringComparison.OrdinalIgnoreCase)
            );
        }

        public static string GetDisplayName(this DosageFrequency frequency)
        {
            return frequency switch
            {
                DosageFrequency.OnceDaily => "Once daily",
                DosageFrequency.TwiceDaily => "Twice daily",
                DosageFrequency.ThreeTimesDaily => "Three times daily",
                DosageFrequency.FourTimesDaily => "Four times daily",
                DosageFrequency.EveryFourHours => "Every 4 hours",
                DosageFrequency.EverySixHours => "Every 6 hours",
                DosageFrequency.EveryEightHours => "Every 8 hours",
                DosageFrequency.EveryTwelveHours => "Every 12 hours",
                DosageFrequency.AtBedtime => "At bedtime",
                DosageFrequency.BeforeMeals => "Before meals",
                DosageFrequency.AfterMeals => "After meals",
                DosageFrequency.AsNeeded => "As needed",
                DosageFrequency.Weekly => "Weekly",
                DosageFrequency.BiWeekly => "Every two weeks",
                DosageFrequency.Monthly => "Monthly",
                DosageFrequency.Once => "Once (single dose)",
                _ => frequency.ToString()
            };
        }

        /// <summary>
        /// Gets the standard medical abbreviation (Sig codes)
        /// </summary>
        public static string GetAbbreviation(this DosageFrequency frequency)
        {
            return frequency switch
            {
                DosageFrequency.OnceDaily => "QD",
                DosageFrequency.TwiceDaily => "BID",
                DosageFrequency.ThreeTimesDaily => "TID",
                DosageFrequency.FourTimesDaily => "QID",
                DosageFrequency.EveryFourHours => "Q4H",
                DosageFrequency.EverySixHours => "Q6H",
                DosageFrequency.EveryEightHours => "Q8H",
                DosageFrequency.EveryTwelveHours => "Q12H",
                DosageFrequency.AtBedtime => "HS",
                DosageFrequency.BeforeMeals => "AC",
                DosageFrequency.AfterMeals => "PC",
                DosageFrequency.AsNeeded => "PRN",
                DosageFrequency.Weekly => "QW",
                DosageFrequency.BiWeekly => "Q2W",
                DosageFrequency.Monthly => "QM",
                DosageFrequency.Once => "x1",
                _ => frequency.ToString()
            };
        }

        public static List<(int Index, string Display)> GetNumberedList()
        {
            return All.Select((f, index) => (index + 1, f.GetDisplayName())).ToList();
        }

        public static List<(int Index, string Display, string Abbreviation)> GetNumberedListWithAbbreviations()
        {
            return All.Select((f, index) => (index + 1, f.GetDisplayName(), f.GetAbbreviation())).ToList();
        }
    }
}
