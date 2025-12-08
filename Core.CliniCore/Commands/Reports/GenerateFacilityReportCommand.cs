using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Facilities;

namespace Core.CliniCore.Commands.Reports
{
    public class GenerateFacilityReportCommand : AbstractCommand
    {
        public const string Key = "generatefacilityreport";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string FacilityId = "facilityId";
            public const string StartDate = "startDate";
            public const string EndDate = "endDate";
            public const string ReportType = "reportType";
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
