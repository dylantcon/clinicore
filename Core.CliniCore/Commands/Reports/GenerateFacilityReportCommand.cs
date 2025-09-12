using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Facilities;

namespace Core.CliniCore.Commands.Reports
{
    public class GenerateFacilityReportCommand : AbstractCommand
    {
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
