using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Enumerations;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;
using GUI.CliniCore.ViewModels.Base;
using GUI.CliniCore.Views.Patients;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels.Patients
{
    /// <summary>
    /// Base class for patient form ViewModels.
    /// Contains shared properties for Common and Patient entry types,
    /// and navigation commands. Derived classes implement specific save logic.
    /// </summary>
    public abstract class PatientFormViewModelBase : BaseViewModel
    {
        protected readonly CommandFactory _commandFactory;
        protected readonly INavigationService _navigationService;
        protected readonly SessionManager _sessionManager;

        #region Common Entry Type Properties

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    RaiseSaveCanExecuteChanged();
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
                    RaiseSaveCanExecuteChanged();
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
                    RaiseSaveCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Patient Entry Type Properties

        private Gender _selectedGender = Gender.PreferNotToSay;
        public Gender SelectedGender
        {
            get => _selectedGender;
            set
            {
                if (SetProperty(ref _selectedGender, value))
                {
                    RaiseSaveCanExecuteChanged();
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
                    RaiseSaveCanExecuteChanged();
                }
            }
        }

        public List<Gender> GenderOptions { get; } = [.. Enum.GetValues(typeof(Gender)).Cast<Gender>()];

        #endregion

        #region Commands

        private MauiCommandAdapter? _saveCommand;

        /// <summary>
        /// Lazy-initialized save command. Created on first access to ensure
        /// derived class is fully constructed before command creation.
        /// </summary>
        public MauiCommand SaveCommand => _saveCommand ??= CreateSaveCommand();

        /// <summary>
        /// Factory method for derived classes to create their specific save command.
        /// </summary>
        protected abstract MauiCommandAdapter CreateSaveCommand();

        public MauiCommand CancelCommand { get; }

        #endregion

        protected PatientFormViewModelBase(
            CommandFactory commandFactory,
            INavigationService navigationService,
            SessionManager sessionManager)
        {
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));

            // Navigation commands shared by all derived classes
            CancelCommand = new AsyncRelayCommand(NavigateBackAsync);
        }

        #region Validation

        /// <summary>
        /// Validates that common fields are filled.
        /// </summary>
        protected bool AreCommonFieldsValid()
        {
            return !string.IsNullOrWhiteSpace(Name) &&
                   !string.IsNullOrWhiteSpace(Address);
        }

        /// <summary>
        /// Validates that patient-specific fields are filled.
        /// </summary>
        protected bool ArePatientFieldsValid()
        {
            return !string.IsNullOrWhiteSpace(Race);
        }

        /// <summary>
        /// Notifies that the save command's CanExecute may have changed.
        /// </summary>
        protected void RaiseSaveCanExecuteChanged()
        {
            (_saveCommand as MauiCommandAdapter)?.RaiseCanExecuteChanged();
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Default back navigation. Override in derived classes if needed.
        /// </summary>
        protected virtual async Task NavigateBackAsync()
        {
            await _navigationService.NavigateToAsync(nameof(PatientListPage));
        }

        #endregion

        #region Command Handling

        /// <summary>
        /// Handles the result of the save command execution.
        /// Override in derived classes for specific behavior.
        /// </summary>
        protected virtual void HandleSaveResult(CommandResult result)
        {
            if (result.Success)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await NavigateBackAsync();
                });
            }
        }

        #endregion
    }
}
