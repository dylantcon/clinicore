using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Commands.Admin
{
    /// <summary>
    /// Command to manage user roles. (Not yet implemented)
    /// </summary>
    public class ManageUserRolesCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key for the ManageUserRoles command.
        /// </summary>
        public const string Key = "manageuserroles";
        /// <summary>
        /// Gets the unique key identifier for this command type.
        /// </summary>
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys for the ManageUserRoles command.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the user ID.
            /// </summary>
            public const string UserId = "userid";
            /// <summary>
            /// Parameter key for the user role.
            /// </summary>
            public const string UserRole = "userrole";
        }

        /// <summary>
        /// Gets the description of what this command does.
        /// </summary>
        public override string Description => throw new NotImplementedException();

        /// <summary>
        /// Gets the required permission to execute this command.
        /// </summary>
        /// <returns>The required permission.</returns>
        public override Permission? GetRequiredPermission()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Executes the core logic of the command.
        /// </summary>
        /// <param name="parameters">The parameters for the command.</param>
        /// <param name="session">The user session context.</param>
        /// <returns>The result of the command execution.</returns>
        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Validates the parameters for the command.
        /// </summary>
        /// <param name="parameters">The parameters to validate.</param>
        /// <returns>The result of the validation.</returns>
        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            throw new NotImplementedException();
        }
    }
}
