using Core.CliniCore.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Domain
{
    public interface IUserProfile
    {
        // identity
        Guid Id { get; }
        string Username { get; set; }
        DateTime CreatedAt { get; }

        // authorization
        UserRole Role { get; }

        // profile data
        List<ProfileEntry> Entries { get; }

        // validation
        bool IsValid { get; }
        List<string> GetValidationErrors();

        // data access
        ProfileEntry? GetEntry(string key);
        T? GetValue<T>(string key);
        bool SetValue<T>(string key, T value);
    }
}
