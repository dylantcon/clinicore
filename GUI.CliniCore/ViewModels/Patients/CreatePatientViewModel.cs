using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Profile;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;

namespace GUI.CliniCore.ViewModels.Patients
{
    /// <summary>
    /// ViewModel for creating new patients.
    /// Uses CreatePatientCommand for saving.
    /// Includes username/password fields for account creation.
    /// </summary>
    public class CreatePatientViewModel : PatientFormViewModelBase
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

        public CreatePatientViewModel(
            CommandFactory commandFactory,
            CommandInvoker commandInvoker,
            INavigationService navigationService,
            SessionManager sessionManager)
            : base(commandFactory, navigationService, sessionManager)
        {
            _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
            Title = "Create Patient";
        }

        protected override MauiCommandAdapter CreateSaveCommand()
        {
            var coreCommand = _commandFactory.CreateCommand(CreatePatientCommand.Key);
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
                .SetParameter(CreatePatientCommand.Parameters.Username, Username)
                .SetParameter(CreatePatientCommand.Parameters.Password, Password)
                .SetParameter(CreatePatientCommand.Parameters.Name, Name)
                .SetParameter(CreatePatientCommand.Parameters.Address, Address)
                .SetParameter(CreatePatientCommand.Parameters.Birthdate, BirthDate)
                .SetParameter(CreatePatientCommand.Parameters.Gender, SelectedGender)
                .SetParameter(CreatePatientCommand.Parameters.Race, Race);
        }
    }
}
