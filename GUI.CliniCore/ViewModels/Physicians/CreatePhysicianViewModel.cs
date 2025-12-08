using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Profile;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;

namespace GUI.CliniCore.ViewModels.Physicians
{
    /// <summary>
    /// ViewModel for creating new physicians.
    /// Uses CreatePhysicianCommand for saving.
    /// Includes username/password fields for account creation.
    /// </summary>
    public class CreatePhysicianViewModel : PhysicianFormViewModelBase
    {
        private readonly CommandInvoker _commandInvoker;

        #region Account Creation Properties

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    RaiseSaveCanExecuteChanged();
                }
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    RaiseSaveCanExecuteChanged();
                }
            }
        }

        #endregion

        public CreatePhysicianViewModel(
            CommandFactory commandFactory,
            CommandInvoker commandInvoker,
            INavigationService navigationService,
            SessionManager sessionManager)
            : base(commandFactory, navigationService, sessionManager)
        {
            _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
            Title = "Create Physician";
        }

        protected override MauiCommandAdapter CreateSaveCommand()
        {
            var coreCommand = _commandFactory.CreateCommand(CreatePhysicianCommand.Key);
            return new MauiCommandAdapter(
                _commandInvoker,
                coreCommand!,
                parameterBuilder: BuildParameters,
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleSaveResult
            );
        }

        private CommandParameters BuildParameters()
        {
            return new CommandParameters()
                .SetParameter(CreatePhysicianCommand.Parameters.Username, Username)
                .SetParameter(CreatePhysicianCommand.Parameters.Password, Password)
                .SetParameter(CreatePhysicianCommand.Parameters.Name, Name)
                .SetParameter(CreatePhysicianCommand.Parameters.Address, Address)
                .SetParameter(CreatePhysicianCommand.Parameters.Birthdate, BirthDate)
                .SetParameter(CreatePhysicianCommand.Parameters.LicenseNumber, LicenseNumber)
                .SetParameter(CreatePhysicianCommand.Parameters.GraduationDate, GraduationDate)
                .SetParameter(CreatePhysicianCommand.Parameters.Specializations, GetSelectedSpecializations());
        }
    }
}
