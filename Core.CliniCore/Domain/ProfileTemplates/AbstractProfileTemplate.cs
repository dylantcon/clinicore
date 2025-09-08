using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Domain.ProfileTemplates
{
    public abstract class AbstractProfileTemplate : IProfileTemplate
    {
        public List<ProfileEntry> GetRequiredEntries()
        {
            var entries = new List<ProfileEntry>();

            AddCommonEntries(entries);
            AddSpecificEntries(entries);

            return entries;
        }

        protected virtual void AddCommonEntries(List<ProfileEntry> entries)
        {
            // invariant human data needed by both
            entries.Add(ProfileEntryFactory.CreateName());
            entries.Add(ProfileEntryFactory.CreateAddress());
            entries.Add(ProfileEntryFactory.CreateBirthDate());
        }

        protected abstract void AddSpecificEntries(List<ProfileEntry> entries);
    }
}
