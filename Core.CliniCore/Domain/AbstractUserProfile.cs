using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.ProfileTemplates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Domain
{
    public abstract class AbstractUserProfile : IUserProfile
    {
        public Guid Id { get; }
        public string Username { get; set; } = string.Empty;
        public DateTime CreatedAt { get; }
        public List<ProfileEntry> Entries { get; }

        public abstract UserRole Role { get; }

        protected AbstractUserProfile()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.Now;
            Entries = GetProfileTemplate().GetRequiredEntries();
        }

        public bool IsValid => 
            !string.IsNullOrWhiteSpace(Username) && 
            Entries.All(e => e.IsValid);

        public List<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Username))
                errors.Add("Username is required");

            errors.AddRange(Entries
                .Where(e => !e.IsValid)
                .Select(e => $"{e.DisplayName}: {e.ErrorMessage}"));

            return errors;
        }

        public ProfileEntry? GetEntry(string key)
            => Entries.FirstOrDefault(e => e.Key == key);

        public T? GetValue<T>(string key)
        {
            var entry = GetEntry(key);
            if (entry is ProfileEntry<T> typedEntry)
                return typedEntry.Value;
            return default;
        }

        public bool SetValue<T>(string key, T value)
        {
            var entry = GetEntry(key);
            if (entry is ProfileEntry<T> typedEntry)
            {
                typedEntry.Value = value;
                return true;
            }
            return false;
        }
        protected abstract IProfileTemplate GetProfileTemplate();
    }
}
