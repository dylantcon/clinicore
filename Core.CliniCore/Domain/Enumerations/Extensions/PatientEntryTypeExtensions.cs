using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.CliniCore.Domain.Enumerations.EntryTypes;

namespace Core.CliniCore.Domain.Enumerations.Extensions
{
    public static class PatientEntryTypeExtensions
    {
        public static string GetKey(this PatientEntryType entryType)
        {
            return entryType switch
            {
                PatientEntryType.Gender => "patient_gender",
                PatientEntryType.Race => "patient_race",
                _ => throw new ArgumentException($"Unknown PatientEntryType: {entryType}")
            };
        }

        public static string GetDisplayName(this PatientEntryType entryType)
        {
            return entryType switch
            {
                PatientEntryType.Gender => "Gender",
                PatientEntryType.Race => "Race",
                _ => throw new ArgumentException($"Unknown PatientEntryType: {entryType}")
            };
        }
    }
}
