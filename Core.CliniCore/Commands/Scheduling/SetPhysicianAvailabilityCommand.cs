using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Commands.Scheduling
{
    /// <summary>
    /// Command that configures a physician's recurring availability schedule.
    /// </summary>
    public class SetPhysicianAvailabilityCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "setphysicianavailability";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="SetPhysicianAvailabilityCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the physician identifier whose availability is being set.
            /// </summary>
            public const string PhysicianId = "physician_id";

            /// <summary>
            /// Parameter key for the day of week for the availability window.
            /// </summary>
            public const string DayOfWeek = "day_of_week";

            /// <summary>
            /// Parameter key for the start time of the availability window.
            /// </summary>
            public const string StartTime = "start_time";

            /// <summary>
            /// Parameter key for the end time of the availability window.
            /// </summary>
            public const string EndTime = "end_time";
        }

        /// <inheritdoc />
        public override string Description => "Set the availability schedule for a physician";

        /// <inheritdoc />
        public override Permission? GetRequiredPermission()
        {
            return Permission.EditOwnAvailability;
        }

        /// <inheritdoc />
        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            // TODO: Implement when SchedulerService supports physician availability
            return CommandResult.Fail("SetPhysicianAvailability is not yet implemented.");
        }

        /// <inheritdoc />
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
