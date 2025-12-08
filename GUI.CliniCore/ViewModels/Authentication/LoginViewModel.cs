using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Authentication;
using Core.CliniCore.Domain.Authentication.Representation;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;
using GUI.CliniCore.ViewModels.Base;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels.Authentication
{
    /// <summary>
    /// ViewModel for the Login page
    /// Demonstrates the MauiCommandAdapter pattern for integrating Core commands with MAUI UI
    /// </summary>
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly CommandFactory _commandFactory;
        private readonly CommandInvoker _commandInvoker;
        private readonly INavigationService _navigationService;
        private readonly SessionManager _sessionManager;

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set
            {
                System.Diagnostics.Debug.WriteLine($"Username changed to: '{value}'");
                if (SetProperty(ref _username, value))
                {
                    // Trigger validation when username changes
                    (LoginCommand as MauiCommandAdapter)?.RaiseCanExecuteChanged();
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
                    // Trigger validation when password changes
                    (LoginCommand as MauiCommandAdapter)?.RaiseCanExecuteChanged();
                }
            }
        }

        public MauiCommand LoginCommand { get; }
        public MauiCommand CancelCommand { get; }

        public LoginViewModel(
            CommandFactory commandFactory,
            CommandInvoker commandInvoker,
            INavigationService navigationService,
            SessionManager sessionManager)
        {
            System.Diagnostics.Debug.WriteLine("LoginViewModel constructor called");

            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));

            Title = "CliniCore Login";

            // Create the login command using MauiCommandAdapter with type-safe Key
            var loginCoreCommand = _commandFactory.CreateCommand(Core.CliniCore.Commands.Authentication.LoginCommand.Key);

            System.Diagnostics.Debug.WriteLine($"LoginCommand from factory: {loginCoreCommand?.GetType().Name ?? "NULL"}");

            if (loginCoreCommand == null)
                throw new InvalidOperationException("LoginCommand not found in CommandFactory");

            LoginCommand = new MauiCommandAdapter(
                _commandInvoker,
                loginCoreCommand,
                parameterBuilder: BuildLoginParameters,
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleLoginResult
            );

            System.Diagnostics.Debug.WriteLine("LoginCommand adapter created successfully");

            // Simple relay command for cancel action
            CancelCommand = new RelayCommand(
                execute: () => ClearLoginForm()
            );
        }

        /// <summary>
        /// Builds CommandParameters from the current ViewModel state
        /// </summary>
        private CommandParameters BuildLoginParameters()
        {
            return new CommandParameters()
                .SetParameter(Core.CliniCore.Commands.Authentication.LoginCommand.Parameters.Username, Username)
                .SetParameter(Core.CliniCore.Commands.Authentication.LoginCommand.Parameters.Password, Password);
        }

        /// <summary>
        /// Handles the result of the login command execution.
        /// ViewModel is responsible for updating UI state based on CommandResult.
        /// </summary>
        private void HandleLoginResult(CommandResult result)
        {
            ClearValidation();

            if (result.Success)
            {
                if (result.Data is SessionContext session)
                {
                    _sessionManager.CurrentSession = session;
                    ClearLoginForm();

                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await _navigationService.NavigateToHomeAsync();
                    });
                }
                else
                {
                    SetValidationError("Login succeeded but session was not created properly.");
                }
            }
            else
            {
                // ViewModel handles error display - use CommandResult's built-in message
                SetValidationError(result.GetDisplayMessage());

                // Clear password for security
                Password = string.Empty;
            }
        }

        private void ClearLoginForm()
        {
            System.Diagnostics.Debug.WriteLine("ClearLoginForm called - STACK TRACE:");
            System.Diagnostics.Debug.WriteLine(new System.Diagnostics.StackTrace().ToString());
            Username = string.Empty;
            Password = string.Empty;
            ClearValidation();
        }
    }
}
