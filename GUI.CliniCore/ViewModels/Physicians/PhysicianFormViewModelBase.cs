using System.Collections.ObjectModel;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Enumerations;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;
using GUI.CliniCore.ViewModels.Base;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels.Physicians
{
    /// <summary>
    /// Base class for physician form ViewModels.
    /// Contains shared properties for Common and Physician entry types,
    /// and navigation commands. Derived classes implement specific save logic.
    /// </summary>
    public abstract class PhysicianFormViewModelBase : BaseViewModel
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

        private DateTime _birthDate = DateTime.Now.AddYears(-35);
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

        #region Physician Entry Type Properties

        private string _licenseNumber = string.Empty;
        public string LicenseNumber
        {
            get => _licenseNumber;
            set
            {
                if (SetProperty(ref _licenseNumber, value))
                {
                    RaiseSaveCanExecuteChanged();
                }
            }
        }

        private DateTime _graduationDate = DateTime.Now.AddYears(-10);
        public DateTime GraduationDate
        {
            get => _graduationDate;
            set
            {
                if (SetProperty(ref _graduationDate, value))
                {
                    RaiseSaveCanExecuteChanged();
                }
            }
        }

        // Specializations - use ObservableCollection with selection tracking
        public ObservableCollection<SpecializationItem> SpecializationItems { get; } = [];

        // Constants from Core layer
        protected const int MIN_SPECIALIZATIONS = 1;
        protected const int MAX_SPECIALIZATIONS = 5;

        // Track selected specialization count
        public int SelectedSpecializationCount => SpecializationItems.Count(s => s.IsSelected);

        // Message showing specialization selection status
        public string SpecializationCountMessage
        {
            get
            {
                var count = SelectedSpecializationCount;
                if (count == 0)
                {
                    return $"Please select {MIN_SPECIALIZATIONS}-{MAX_SPECIALIZATIONS} specializations";
                }
                else if (count > MAX_SPECIALIZATIONS)
                {
                    return $"Too many selected ({count}/{MAX_SPECIALIZATIONS}) - please deselect some";
                }
                else if (count < MIN_SPECIALIZATIONS)
                {
                    return $"Please select at least {MIN_SPECIALIZATIONS} specialization";
                }
                else
                {
                    return $"Selected: {count}/{MAX_SPECIALIZATIONS}";
                }
            }
        }

        // Whether to show warning icon
        public bool ShowSpecializationWarningIcon => SelectedSpecializationCount > MAX_SPECIALIZATIONS;

        // Color for the message based on validation state
        public string SpecializationCountColor
        {
            get
            {
                var count = SelectedSpecializationCount;
                if (count < MIN_SPECIALIZATIONS || count > MAX_SPECIALIZATIONS)
                {
                    return "#D32F2F"; // Red for error
                }
                return "#666666"; // Gray for valid
            }
        }

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

        protected PhysicianFormViewModelBase(
            CommandFactory commandFactory,
            INavigationService navigationService,
            SessionManager sessionManager)
        {
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));

            // Initialize specialization items (all available specializations)
            InitializeSpecializations();

            // Navigation commands shared by all derived classes
            CancelCommand = new AsyncRelayCommand(NavigateBackAsync);
        }

        #region Initialization

        private void InitializeSpecializations()
        {
            foreach (var spec in Enum.GetValues(typeof(MedicalSpecialization)).Cast<MedicalSpecialization>())
            {
                var item = new SpecializationItem { Specialization = spec, IsSelected = false };
                item.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(SpecializationItem.IsSelected))
                    {
                        // Notify UI of count and message changes
                        OnPropertyChanged(nameof(SelectedSpecializationCount));
                        OnPropertyChanged(nameof(SpecializationCountMessage));
                        OnPropertyChanged(nameof(SpecializationCountColor));
                        OnPropertyChanged(nameof(ShowSpecializationWarningIcon));

                        // Update Save button state
                        RaiseSaveCanExecuteChanged();
                    }
                };
                SpecializationItems.Add(item);
            }
        }

        #endregion

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
        /// Validates that physician-specific fields are filled.
        /// </summary>
        protected bool ArePhysicianFieldsValid()
        {
            var selectedCount = SelectedSpecializationCount;
            var hasValidSpecializationCount = selectedCount >= MIN_SPECIALIZATIONS && selectedCount <= MAX_SPECIALIZATIONS;

            return !string.IsNullOrWhiteSpace(LicenseNumber) &&
                   hasValidSpecializationCount;
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
            await _navigationService.NavigateToAsync("PhysicianListPage");
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

        /// <summary>
        /// Gets the list of selected specializations.
        /// </summary>
        protected List<MedicalSpecialization> GetSelectedSpecializations()
        {
            return SpecializationItems.Where(s => s.IsSelected).Select(s => s.Specialization).ToList();
        }

        #endregion
    }

    /// <summary>
    /// Helper class for specialization selection in the UI
    /// </summary>
    public class SpecializationItem : BaseViewModel
    {
        private MedicalSpecialization _specialization;
        public MedicalSpecialization Specialization
        {
            get => _specialization;
            set => SetProperty(ref _specialization, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
