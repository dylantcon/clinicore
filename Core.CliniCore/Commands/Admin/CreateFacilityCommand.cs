using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Enumerations;

namespace Core.CliniCore.Commands.Admin
{
    /// <summary>
    /// Command to create a new healthcare facility.
    /// </summary>
    public class CreateFacilityCommand : AbstractCommand
    {
        // private readonly IFacilityRepository _facilityRepository;
        private readonly string _name;
        private readonly string _address;
        private readonly string _phone;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateFacilityCommand"/> class.
        /// </summary>
        /// <param name="name">The name of the new facility.</param>
        /// <param name="address">The physical address of the new facility.</param>
        /// <param name="phone">The contact phone number for the new facility.</param>
        public CreateFacilityCommand(string name, string address, string phone)
        {
            _name = name;
            _address = address;
            _phone = phone;
        }

        /// <summary>
        /// The unique key for the CreateFacility command.
        /// </summary>
        public const string Key = "createfacility";

        /// <summary>
        /// Gets the unique key identifier for this command type.
        /// </summary>
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys for the CreateFacility command.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the facility name.
            /// </summary>
            public const string FacilityName = "facility_name";
            /// <summary>
            /// Parameter key for the facility address.
            /// </summary>
            public const string FacilityAddress = "facility_address";
            /// <summary>
            /// Parameter key for the facility phone number.
            /// </summary>
            public const string FacilityPhone = "facility_phone";
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
