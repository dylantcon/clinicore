using Core.CliniCore.Domain.Authentication.Representation;
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
        public const string Key = "setphysicianavailability";
        public override string CommandKey => Key;
        public static class Parameters
        {
            public const string PhysicianId = "physician_id";
            public const string DayOfWeek = "day_of_week";
            public const string StartTime = "start_time";
            public const string EndTime = "end_time";
        }

        public override string Description => "Set the availability schedule for a physician";

        public override Permission? GetRequiredPermission()
        {
            return Permission.EditOwnAvailability;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            // TODO: Implement when SchedulerService supports physician availability
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
