using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Commands.Admin
{
    public class ViewAuditLogCommand : AbstractCommand
    {
        public const string Key = "viewauditlog";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string StartDate = "startdate";
            public const string EndDate = "enddate";
            public const string UserId = "userid";
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
