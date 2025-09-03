using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.CliniCore.Domain;

namespace Core.CliniCore.Domain
{
    internal class UserProfile
    {
        private IUser user;
        private List<ProfileEntry> profileData;

        UserProfile(IUser user, List<ProfileEntry> profileData)
        {
            this.user = user;
            this.profileData = profileData;
        }
    }
}
