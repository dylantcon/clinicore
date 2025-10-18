using System.Collections.ObjectModel;
using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Profile;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Enumerations;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Resources.Fonts;
using GUI.CliniCore.Services;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels
{
    /// <summary>
    /// ViewModel for Physician Edit/Create page
    /// Handles both creation of new physicians and updating existing ones
    /// </summary>
    [QueryProperty(nameof(PhysicianIdString), "physicianId")]
    public partial class PhysicianEditViewModel : BaseViewModel
    {
        private readonly CommandFactory _commandFactory;
        private readonly INavigationService _navigationService;
        private readonly SessionManager _sessionManager;

        private Guid? _physicianId;
        public Guid? PhysicianId
        {
            get => _physicianId;
            private set
            {
                if (SetProperty(ref _physicianId, value) && value.HasValue && value.Value != Guid.Empty)
                {
                    IsEditMode = true;
                    LoadPhysicianCommand.Execute(null);
                }
                else
                {
                    IsEditMode = false;
                }
            }
        }

        public string PhysicianIdString
        {
            set
            {
                if (Guid.TryParse(value, out var guid))
                {
                    PhysicianId = guid;
                }
                else
                {
                    PhysicianId = null;
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
                    Title = value ? "Edit Physician" : "Create Physician";
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

        private DateTime _birthDate = DateTime.Now.AddYears(-35);
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

        private string _licenseNumber = string.Empty;
        public string LicenseNumber
        {
            get => _licenseNumber;
            set
            {
                if (SetProperty(ref _licenseNumber, value))
                {
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
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
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        // Specializations - use ObservableCollection with selection tracking
        public ObservableCollection<SpecializationItem> SpecializationItems { get; } = [];

        // Constants from Core layer
        private const int MIN_SPECIALIZATIONS = 1;
        private const int MAX_SPECIALIZATIONS = 5; // Must match CreatePhysicianCommand.MEDSPECMAXCOUNT

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

        public MauiCommand LoadPhysicianCommand { get; }
        public MauiCommand SaveCommand { get; }
        public MauiCommand CancelCommand { get; }

        public PhysicianEditViewModel(
            CommandFactory commandFactory,
            INavigationService navigationService,
            SessionManager sessionManager)
        {
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));

            Title = "Create Physician";

            // Initialize specialization items (all available specializations)
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
                        (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    }
                };
                SpecializationItems.Add(item);
            }

            // Load command for edit mode
            var viewCoreCommand = _commandFactory.CreateCommand(ViewPhysicianProfileCommand.Key);
            LoadPhysicianCommand = new MauiCommandAdapter(
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
                .SetParameter(ViewPhysicianProfileCommand.Parameters.ProfileId, PhysicianId);
        }

        private void HandleLoadResult(CommandResult result)
        {
            if (result.Success && result.Data is PhysicianProfile physician)
            {
                Username = physician.Username;
                Name = physician.Name;
                Address = physician.GetValue<string>("address") ?? string.Empty;
                BirthDate = physician.GetValue<DateTime>("birthdate");
                LicenseNumber = physician.LicenseNumber;
                GraduationDate = physician.GraduationDate;

                // Load specializations
                var selectedSpecs = physician.Specializations ?? new List<MedicalSpecialization>();
                foreach (var item in SpecializationItems)
                {
                    item.IsSelected = selectedSpecs.Contains(item.Specialization);
                }

                ClearValidation();
            }
        }

        private bool CanSave()
        {
            // Validate specialization count (must be between 1 and 5)
            var selectedCount = SelectedSpecializationCount;
            var hasValidSpecializationCount = selectedCount >= MIN_SPECIALIZATIONS && selectedCount <= MAX_SPECIALIZATIONS;

            if (IsEditMode)
            {
                // For edit mode, need valid data
                return !string.IsNullOrWhiteSpace(Name) &&
                       !string.IsNullOrWhiteSpace(Address) &&
                       !string.IsNullOrWhiteSpace(LicenseNumber) &&
                       hasValidSpecializationCount;
            }
            else
            {
                // For create mode, need username and password too
                return !string.IsNullOrWhiteSpace(Username) &&
                       !string.IsNullOrWhiteSpace(Password) &&
                       !string.IsNullOrWhiteSpace(Name) &&
                       !string.IsNullOrWhiteSpace(Address) &&
                       !string.IsNullOrWhiteSpace(LicenseNumber) &&
                       hasValidSpecializationCount;
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
            var createCoreCommand = _commandFactory.CreateCommand(CreatePhysicianCommand.Key);
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
            var updateCoreCommand = _commandFactory.CreateCommand(UpdatePhysicianProfileCommand.Key);
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
            var selectedSpecs = SpecializationItems.Where(s => s.IsSelected).Select(s => s.Specialization).ToList();

            return new CommandParameters()
                .SetParameter(CreatePhysicianCommand.Parameters.Username, Username)
                .SetParameter(CreatePhysicianCommand.Parameters.Password, Password)
                .SetParameter(CreatePhysicianCommand.Parameters.Name, Name)
                .SetParameter(CreatePhysicianCommand.Parameters.Address, Address)
                .SetParameter(CreatePhysicianCommand.Parameters.Birthdate, BirthDate)
                .SetParameter(CreatePhysicianCommand.Parameters.LicenseNumber, LicenseNumber)
                .SetParameter(CreatePhysicianCommand.Parameters.GraduationDate, GraduationDate)
                .SetParameter(CreatePhysicianCommand.Parameters.Specializations, selectedSpecs);
        }

        private CommandParameters BuildUpdateParameters()
        {
            var selectedSpecs = SpecializationItems.Where(s => s.IsSelected).Select(s => s.Specialization).ToList();

            return new CommandParameters()
                .SetParameter(UpdatePhysicianProfileCommand.Parameters.ProfileId, PhysicianId)
                .SetParameter(UpdatePhysicianProfileCommand.Parameters.Name, Name)
                .SetParameter(UpdatePhysicianProfileCommand.Parameters.Address, Address)
                .SetParameter(UpdatePhysicianProfileCommand.Parameters.BirthDate, BirthDate)
                .SetParameter(UpdatePhysicianProfileCommand.Parameters.LicenseNumber, LicenseNumber)
                .SetParameter(UpdatePhysicianProfileCommand.Parameters.GraduationDate, GraduationDate)
                .SetParameter(UpdatePhysicianProfileCommand.Parameters.Specializations, selectedSpecs);
        }

        private void HandleSaveResult(CommandResult result)
        {
            if (result.Success)
            {
                // Navigate to appropriate page after successful save (must be on UI thread)
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (IsEditMode && PhysicianId.HasValue)
                    {
                        // After editing, navigate back to detail page
                        await _navigationService.NavigateToAsync($"PhysicianDetailPage?physicianId={PhysicianId.Value}");
                    }
                    else
                    {
                        // After creating, navigate to list page
                        await _navigationService.NavigateToAsync("PhysicianListPage");
                    }
                });
            }
            // Errors are already populated by the adapter
        }

        private async Task CancelEditAsync()
        {
            if (IsEditMode && PhysicianId.HasValue)
            {
                // If editing, go back to detail page
                await _navigationService.NavigateToAsync($"PhysicianDetailPage?physicianId={PhysicianId.Value}");
            }
            else
            {
                // If creating, go back to list
                await _navigationService.NavigateToAsync("PhysicianListPage");
            }
        }
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
