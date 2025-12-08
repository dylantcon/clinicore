using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Profile;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users.Concrete;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;
using GUI.CliniCore.ViewModels.Base;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels.Users
{
    /// <summary>
    /// ViewModel for Administrator Edit/Create page
    /// Handles both creation of new administrators and updating existing ones
    /// </summary>
    [QueryProperty(nameof(UserIdString), "userId")]
    public partial class AdministratorEditViewModel : BaseViewModel
    {
        private readonly CommandFactory _commandFactory;
        private readonly CommandInvoker _commandInvoker;
        private readonly INavigationService _navigationService;
        private readonly SessionManager _sessionManager;

        private Guid? _userId;
        public Guid? UserId
        {
            get => _userId;
            private set
            {
                if (SetProperty(ref _userId, value) && value.HasValue && value.Value != Guid.Empty)
                {
                    IsEditMode = true;
                    LoadAdministratorCommand.Execute(null);
                }
                else
                {
                    IsEditMode = false;
                }
            }
        }

        public string UserIdString
        {
            set
            {
                if (Guid.TryParse(value, out var guid))
                {
                    UserId = guid;
                }
                else
                {
                    UserId = null;
                }
            }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if (SetProperty(ref _isEditMode, value))
                {
                    Title = value ? "Edit Administrator" : "Create Administrator";
                    OnPropertyChanged(nameof(IsPasswordVisible));
                }
            }
        }

        // Password is only shown in create mode
        public bool IsPasswordVisible => !IsEditMode;

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
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
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _address = string.Empty;
        public string Address
        {
            get => _address;
            set
            {
                if (SetProperty(ref _address, value))
                {
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private DateTime _birthDate = DateTime.Now.AddYears(-30);
        public DateTime BirthDate
        {
            get => _birthDate;
            set
            {
                if (SetProperty(ref _birthDate, value))
                {
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public MauiCommand LoadAdministratorCommand { get; }
        public MauiCommand SaveCommand { get; }
        public MauiCommand CancelCommand { get; }

        public AdministratorEditViewModel(
            CommandFactory commandFactory,
            CommandInvoker commandInvoker,
            INavigationService navigationService,
            SessionManager sessionManager)
        {
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));

            Title = "Create Administrator";

            // Load command for edit mode
            var viewCoreCommand = _commandFactory.CreateCommand(ViewAdministratorProfileCommand.Key);
            LoadAdministratorCommand = new MauiCommandAdapter(
                _commandInvoker,
                viewCoreCommand!,
                parameterBuilder: BuildLoadParameters,
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleLoadResult
            );

            // Save command (dynamically creates Create or Update command)
            SaveCommand = new RelayCommand(
                execute: () => ExecuteSave(),
                canExecute: () => CanSave()
            );

            // Cancel command
            CancelCommand = new AsyncRelayCommand(CancelEditAsync);
        }

        private CommandParameters BuildLoadParameters()
        {
            return new CommandParameters()
                .SetParameter(ViewAdministratorProfileCommand.Parameters.ProfileId, UserId);
        }

        private void HandleLoadResult(CommandResult result)
        {
            ClearValidation();

            if (result.Success && result.Data is AdministratorProfile admin)
            {
                Username = admin.Username;
                Name = admin.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty;
                Address = admin.GetValue<string>(CommonEntryType.Address.GetKey()) ?? string.Empty;

                var birthDate = admin.GetValue<DateTime?>(CommonEntryType.BirthDate.GetKey());
                if (birthDate.HasValue && birthDate.Value != default)
                {
                    BirthDate = birthDate.Value;
                }

                Email = admin.GetValue<string>(AdministratorEntryType.Email.GetKey()) ?? string.Empty;
            }
            else if (!result.Success)
            {
                SetValidationError(result.GetDisplayMessage());
            }
        }

        private bool CanSave()
        {
            if (IsEditMode)
            {
                // For edit mode, just need valid data
                return !string.IsNullOrWhiteSpace(Name);
            }
            else
            {
                // For create mode, need username, password, and name
                return !string.IsNullOrWhiteSpace(Username) &&
                       !string.IsNullOrWhiteSpace(Password) &&
                       !string.IsNullOrWhiteSpace(Name);
            }
        }

        private void ExecuteSave()
        {
            if (IsEditMode)
            {
                ExecuteUpdate();
            }
            else
            {
                ExecuteCreate();
            }
        }

        private void ExecuteCreate()
        {
            var createCoreCommand = _commandFactory.CreateCommand(CreateAdministratorCommand.Key);
            var createCommand = new MauiCommandAdapter(
                _commandInvoker,
                createCoreCommand!,
                parameterBuilder: BuildCreateParameters,
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleSaveResult
            );

            createCommand.Execute(null);
        }

        private void ExecuteUpdate()
        {
            var updateCoreCommand = _commandFactory.CreateCommand(UpdateAdministratorProfileCommand.Key);
            var updateCommand = new MauiCommandAdapter(
                _commandInvoker,
                updateCoreCommand!,
                parameterBuilder: BuildUpdateParameters,
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleSaveResult
            );

            updateCommand.Execute(null);
        }

        private CommandParameters BuildCreateParameters()
        {
            var parameters = new CommandParameters()
                .SetParameter(CreateAdministratorCommand.Parameters.Username, Username)
                .SetParameter(CreateAdministratorCommand.Parameters.Password, Password)
                .SetParameter(CreateAdministratorCommand.Parameters.Name, Name);

            // Add optional fields if provided
            if (!string.IsNullOrWhiteSpace(Address))
            {
                parameters.SetParameter(CreateAdministratorCommand.Parameters.Address, Address);
            }

            if (BirthDate != default && BirthDate < DateTime.Now)
            {
                parameters.SetParameter(CreateAdministratorCommand.Parameters.BirthDate, BirthDate);
            }

            if (!string.IsNullOrWhiteSpace(Email))
            {
                parameters.SetParameter(CreateAdministratorCommand.Parameters.Email, Email);
            }

            return parameters;
        }

        private CommandParameters BuildUpdateParameters()
        {
            var parameters = new CommandParameters()
                .SetParameter(UpdateAdministratorProfileCommand.Parameters.ProfileId, UserId);

            // Add fields to update
            if (!string.IsNullOrWhiteSpace(Name))
            {
                parameters.SetParameter(UpdateAdministratorProfileCommand.Parameters.Name, Name);
            }

            if (!string.IsNullOrWhiteSpace(Address))
            {
                parameters.SetParameter(UpdateAdministratorProfileCommand.Parameters.Address, Address);
            }

            if (BirthDate != default && BirthDate < DateTime.Now)
            {
                parameters.SetParameter(UpdateAdministratorProfileCommand.Parameters.BirthDate, BirthDate);
            }

            if (!string.IsNullOrWhiteSpace(Email))
            {
                parameters.SetParameter(UpdateAdministratorProfileCommand.Parameters.Email, Email);
            }

            return parameters;
        }

        private void HandleSaveResult(CommandResult result)
        {
            ClearValidation();

            if (result.Success)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await _navigationService.NavigateToAsync("UserListPage");
                });
            }
            else
            {
                SetValidationError(result.GetDisplayMessage());
            }
        }

        private async Task CancelEditAsync()
        {
            // Go back to user list
            await _navigationService.NavigateToAsync("UserListPage");
        }
    }
}
