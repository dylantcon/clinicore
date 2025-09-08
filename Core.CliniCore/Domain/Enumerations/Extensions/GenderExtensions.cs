using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using Core.CliniCore.Domain.Enumerations;

namespace Core.CliniCore.Domain.Enumerations.Extensions
{
    public static class GenderExtensions
    {
        
        public static Gender[] All => Enum.GetValues<Gender>();
        public static int TypeCount => All.Length;

        public static Gender? GetByIndex(int oneBasedIndex)
        {
            Gender[] all = All;
            if (oneBasedIndex < 0 || oneBasedIndex > All.Length)
                return null;

            return all[oneBasedIndex - 1];
        }

        public static Gender? FindByDisplayName(string display)
        {
            return All.FirstOrDefault(g => g
                .GetDisplayName()
                .Equals(display, StringComparison.OrdinalIgnoreCase)
            );
        }

        public static string GetDisplayName(this Gender gender)
        {
            return gender switch
            {
                Gender.Man => "Man",
                Gender.Woman => "Woman",
                Gender.NonBinary => "Non-binary",
                Gender.GenderQueer => "Genderqueer",
                Gender.GenderFluid => "Genderfluid",
                Gender.AGender => "Agender",
                Gender.Other => "Other",
                Gender.PreferNotToSay => "Prefer not to say",
                _ => gender.ToString()
            };
        }

        public static List<(int Index, string Display)> GetNumberedList()
        {
            return Enum.GetValues<Gender>()
                .Select((g, index) => (index + 1, g.GetDisplayName()))
                .ToList();
        }

        public static List<string> GetAlphabetizedList()
        {
            return Enum.GetValues<Gender>()
                .Select((g) => g.GetDisplayName())
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
