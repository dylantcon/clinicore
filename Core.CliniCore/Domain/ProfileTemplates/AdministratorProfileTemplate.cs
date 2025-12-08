using Core.CliniCore.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Domain.ProfileTemplates
{
    public class AdministratorProfileTemplate : AbstractProfileTemplate
    {
        protected override void AddSpecificEntries(List<ProfileEntry> entries)
        {
            entries.Add(ProfileEntryFactory.CreateEmail());
        }
    }
}
