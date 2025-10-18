using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Profile;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Enumerations;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels
{
    /// <summary>
    /// ViewModel for Patient Edit/Create page
    /// Handles both creation of new patients and updating existing ones
    /// </summary>
    [QueryProperty(nameof(PatientIdString), "patientId")]
    public partial class PatientEditViewModel : BaseViewModel
    {
        private readonly CommandFactory _commandFactory;
        private readonly INavigationService _navigationService;
        private readonly SessionManager _sessionManager;

        private Guid? _patientId;
        public Guid? PatientId
        {
            get => _patientId;
            private set
            {
                if (SetProperty(ref _patientId, value) && value.HasValue && value.Value != Guid.Empty)
                {
                    IsEditMode = true;
                    LoadPatientCommand.Execute(null);
                }
                else
                {
                    IsEditMode = false;
                }
            }
        }

        public string PatientIdString
        {
            set
            {
                if (Guid.TryParse(value, out var guid))
                {
                    PatientId = guid;
                }
                else
                {
                    PatientId = null;
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
                    Title = value ? "Edit Patient" : "Create Patient";
                }
            }
        }

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

        private Gender _selectedGender = Gender.PreferNotToSay;
        public Gender SelectedGender
        {
            get => _selectedGender;
            set
            {
                if (SetProperty(ref _selectedGender, value))
                {
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _race = string.Empty;
        public string Race
        {
            get => _race;
            set
            {
                if (SetProperty(ref _race, value))
                {
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public List<Gender> GenderOptions { get; } = [.. Enum.GetValues(typeof(Gender)).Cast<Gender>()];

        public MauiCommand LoadPatientCommand { get; }
        public MauiCommand SaveCommand { get; }
        public MauiCommand CancelCommand { get; }

        public PatientEditViewModel(
            CommandFactory commandFactory,
            INavigationService navigationService,
            SessionManager sessionManager)
        {
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));

            Title = "Create Patient";

            // Load command for edit mode
            var viewCoreCommand = _commandFactory.CreateCommand(ViewPatientProfileCommand.Key);
            LoadPatientCommand = new MauiCommandAdapter(
                viewCoreCommand!,
                parameterBuilder: BuildLoadParameters,
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleLoadResult,
                viewModel: this
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
                .SetParameter(ViewPatientProfileCommand.Parameters.ProfileId, PatientId);
        }

        private void HandleLoadResult(CommandResult result)
        {
            if (result.Success && result.Data is PatientProfile patient)
            {
                Username = patient.Username;
                Name = patient.Name;
                Address = patient.Address;
                BirthDate = patient.BirthDate;
                SelectedGender = patient.Gender;
                Race = patient.Race;
                ClearValidation();
            }
        }

        private bool CanSave()
        {
            if (IsEditMode)
            {
                // For edit mode, just need valid data
                return !string.IsNullOrWhiteSpace(Name) &&
                       !string.IsNullOrWhiteSpace(Address) &&
                       !string.IsNullOrWhiteSpace(Race);
            }
            else
            {
                // For create mode, need username and password too
                return !string.IsNullOrWhiteSpace(Username) &&
                       !string.IsNullOrWhiteSpace(Password) &&
                       !string.IsNullOrWhiteSpace(Name) &&
                       !string.IsNullOrWhiteSpace(Address) &&
                       !string.IsNullOrWhiteSpace(Race);
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
            var createCoreCommand = _commandFactory.CreateCommand(CreatePatientCommand.Key);
            var createCommand = new MauiCommandAdapter(
                createCoreCommand!,
                parameterBuilder: BuildCreateParameters,
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleSaveResult,
                viewModel: this
            );

            createCommand.Execute(null);
        }

        private void ExecuteUpdate()
        {
            var updateCoreCommand = _commandFactory.CreateCommand(UpdatePatientProfileCommand.Key);
            var updateCommand = new MauiCommandAdapter(
                updateCoreCommand!,
                parameterBuilder: BuildUpdateParameters,
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleSaveResult,
                viewModel: this
            );

            updateCommand.Execute(null);
        }

        private CommandParameters BuildCreateParameters()
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

        private CommandParameters BuildUpdateParameters()
        {
            return new CommandParameters()
                .SetParameter(UpdatePatientProfileCommand.Parameters.ProfileId, PatientId)
                .SetParameter(UpdatePatientProfileCommand.Parameters.Name, Name)
                .SetParameter(UpdatePatientProfileCommand.Parameters.Address, Address)
                .SetParameter(UpdatePatientProfileCommand.Parameters.BirthDate, BirthDate)
                .SetParameter(UpdatePatientProfileCommand.Parameters.Gender, SelectedGender.ToString())
                .SetParameter(UpdatePatientProfileCommand.Parameters.Race, Race);
        }

        private void HandleSaveResult(CommandResult result)
        {
            if (result.Success)
            {
                // Navigate to appropriate page after successful save (must be on UI thread)
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (IsEditMode && PatientId.HasValue)
                    {
                        // After editing, navigate back to detail page
                        await _navigationService.NavigateToAsync($"PatientDetailPage?patientId={PatientId.Value}");
                    }
                    else
                    {
                        // After creating, navigate to list page
                        await _navigationService.NavigateToAsync("PatientListPage");
                    }
                });
            }
            // Errors are already populated by the adapter
        }

        private async Task CancelEditAsync()
        {
            if (IsEditMode && PatientId.HasValue)
            {
                // If editing, go back to detail page
                await _navigationService.NavigateToAsync($"PatientDetailPage?patientId={PatientId.Value}");
            }
            else
            {
                // If creating, go back to list
                await _navigationService.NavigateToAsync("PatientListPage");
            }
        }
    }
}
