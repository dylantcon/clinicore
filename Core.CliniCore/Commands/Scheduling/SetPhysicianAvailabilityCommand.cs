using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Commands.Scheduling
{
    public class SetPhysicianAvailabilityCommand : AbstractCommand
    {
        public static class Parameters
        {
            public const string PhysicianId = CommandParameterKeys.PhysicianId;
            public const string DayOfWeek = CommandParameterKeys.DayOfWeek;
            public const string StartTime = CommandParameterKeys.StartTime;
            public const string EndTime = CommandParameterKeys.EndTime;
        }

        public override string Description => "Set the availability schedule for a physician";

        public override Permission? GetRequiredPermission()
        {
            return Permission.EditOwnAvailability;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            // TODO: Implement when ScheduleManager supports physician availability
            return CommandResult.Fail("SetPhysicianAvailability is not yet implemented.");
        }

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Check required parameters exist
            var missingParams = parameters.GetMissingRequired(
                Parameters.PhysicianId, 
                Parameters.DayOfWeek, 
                Parameters.StartTime, 
                Parameters.EndTime);

            if (missingParams.Any())
            {
                foreach (var error in missingParams)
                    result.AddError(error);
                return result;
            }

            // Validate physician_id is a valid GUID
            var physicianIdStr = parameters.GetParameter<string>(Parameters.PhysicianId);
            if (!Guid.TryParse(physicianIdStr, out var physicianId) || physicianId == Guid.Empty)
            {
                result.AddError($"Invalid physician ID format: '{physicianIdStr}'. Expected a valid GUID.");
                return result;
            }

            return result;
        }
    }
}
