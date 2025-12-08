using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Commands.Authentication
{
    /// <summary>
    /// Command that allows an authenticated user to change their password.
    /// </summary>
    /// <remarks>
    /// This command validates the old password, enforces password strength rules for the new password,
    /// and typically requires the user to be authenticated.
    /// </remarks>
    public class ChangePasswordCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "changepassword";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="ChangePasswordCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the current (old) password.
            /// </summary>
            public const string OldPassword = "oldPassword";

            /// <summary>
            /// Parameter key for the new password.
            /// </summary>
            public const string NewPassword = "newPassword";

            /// <summary>
            /// Parameter key for the confirmation of the new password.
            /// </summary>
            public const string ConfirmPassword = "confirmPassword";
        }

        /// <inheritdoc />
        public override string Description => throw new NotImplementedException();

        /// <inheritdoc />
        public override Permission? GetRequiredPermission()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            throw new NotImplementedException();
        }
    }
}
