using System;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;

namespace Core.CliniCore.Domain.Enumerations
{
    public static class EntryKeyResolver
    {
        public static string GetKey(Enum entryType)
        {
            return entryType switch
            {
                CommonEntryType common => common.GetKey(),
                PatientEntryType patient => patient.GetKey(),
                PhysicianEntryType physician => physician.GetKey(),
                AdministratorEntryType administrator => administrator.GetKey(),
                _ => throw new ArgumentException($"Unknown entry type: {entryType}")
            };
        }

        public static string GetDisplayName(Enum entryType)
        {
            return entryType switch
            {
                CommonEntryType common => common.GetDisplayName(),
                PatientEntryType patient => patient.GetDisplayName(),
                PhysicianEntryType physician => physician.GetDisplayName(),
                AdministratorEntryType administrator => administrator.GetDisplayName(),
                _ => throw new ArgumentException($"Unknown entry type: {entryType}")
            };
        }
    }
}