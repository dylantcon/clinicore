using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.CliniCore.Domain.Enumerations.EntryTypes;

namespace Core.CliniCore.Domain.Enumerations.Extensions
{
    public static class AdministratorEntryTypeExtensions
    {
        public static string GetKey(this AdministratorEntryType entryType)
        {
            return entryType switch
            {
                AdministratorEntryType.Email => "user_email",
                _ => throw new ArgumentException($"Unknown AdministratorEntryType: {entryType}")
            };
        }

        public static string GetDisplayName(this AdministratorEntryType entryType)
        {
            return entryType switch
            {
                AdministratorEntryType.Email => "Email",
                _ => throw new ArgumentException($"Unknown AdministratorEntryType: {entryType}")
            };
        }
    }
}
