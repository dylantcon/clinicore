using Core.CliniCore.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Domain.ProfileTemplates
{
    public interface IProfileTemplate
    {
        List<ProfileEntry> GetRequiredEntries();
    }
}
