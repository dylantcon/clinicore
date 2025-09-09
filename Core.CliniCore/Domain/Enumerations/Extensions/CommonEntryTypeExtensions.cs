using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.CliniCore.Domain.Enumerations.EntryTypes;

namespace Core.CliniCore.Domain.Enumerations.Extensions
{
    public static class CommonEntryTypeExtensions
    {
        public static string GetKey(this CommonEntryType entryType)
        {
            return entryType switch
            {
                CommonEntryType.Name => "name",
                CommonEntryType.Address => "address",
                CommonEntryType.BirthDate => "birthdate",
                _ => throw new ArgumentException($"Unknown CommonEntryType: {entryType}")
            };
        }

        public static string GetDisplayName(this CommonEntryType entryType)
        {
            return entryType switch
            {
                CommonEntryType.Name => "Full Name",
                CommonEntryType.Address => "Address", 
                CommonEntryType.BirthDate => "Date of Birth",
                _ => throw new ArgumentException($"Unknown CommonEntryType: {entryType}")
            };
        }
    }
}