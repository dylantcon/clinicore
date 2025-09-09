using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.CliniCore.Domain.Enumerations.EntryTypes;

namespace Core.CliniCore.Domain.Enumerations.Extensions
{
    public static class PhysicianEntryTypeExtensions
    {
        public static string GetKey(this PhysicianEntryType entryType)
        {
            return entryType switch
            {
                PhysicianEntryType.LicenseNumber => "physician_license",
                PhysicianEntryType.GraduationDate => "physician_graduation",
                PhysicianEntryType.Specializations => "physician_specializations",
                _ => throw new ArgumentException($"Unknown PhysicianEntryType: {entryType}")
            };
        }

        public static string GetDisplayName(this PhysicianEntryType entryType)
        {
            return entryType switch
            {
                PhysicianEntryType.LicenseNumber => "License Number",
                PhysicianEntryType.GraduationDate => "Graduation Date",
                PhysicianEntryType.Specializations => "Clinical Specializations",
                _ => throw new ArgumentException($"Unknown PhysicianEntryType: {entryType}")
            };
        }
    }
}
