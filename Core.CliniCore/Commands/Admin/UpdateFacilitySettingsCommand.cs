using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Commands.Admin
{
    public class UpdateFacilitySettingsCommand : AbstractCommand
    {
        public const string Key = "updatefacilitysettings";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string FacilityId = "facilityid";
            public const string SettingName = "settingname";
            public const string SettingValue = "settingvalue";
        }

        public override string Description => throw new NotImplementedException();

        public override Permission? GetRequiredPermission()
        {
            throw new NotImplementedException();
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            throw new NotImplementedException();
        }

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            throw new NotImplementedException();
        }
    }
}
