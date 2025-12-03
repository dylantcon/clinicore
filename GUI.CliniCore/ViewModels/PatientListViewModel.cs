using System.Collections.ObjectModel;
using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Profile;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Services;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels
{
    /// <summary>
    /// ViewModel for Patient List page with enhanced filtering and assignment capabilities
    /// </summary>
    public partial class PatientListViewModel : BaseViewModel
    {
        private readonly CommandFactory _commandFactory;
        private readonly INavigationService _navigationService;
        private readonly SessionManager _sessionManager;
        private readonly ProfileService _profileRegistry;

        private ObservableCollection<PatientProfile> _patients = [];
        public ObservableCollection<PatientProfile> Patients
        {
            get => _patients;
            set => SetProperty(ref _patients, value);
        }

        private ObservableCollection<PhysicianProfile> _availablePhysicians = [];
        public ObservableCollection<PhysicianProfile> AvailablePhysicians
        {
            get => _availablePhysicians;
            set => SetProperty(ref _availablePhysicians, value);
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    SearchCommand.Execute(null);
                }
            }
        }

        private PatientProfile? _selectedPatient;
        public PatientProfile? SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                if (SetProperty(ref _selectedPatient, value) && value != null)
                {
                    ViewPatientCommand.Execute(value.Id);
                }
            }
        }

        // Filtering properties
        private bool _showMyPatientsOnly = true; // Default for physicians
        public bool ShowMyPatientsOnly
        {
            get => _showMyPatientsOnly;
            set
            {
                if (SetProperty(ref _showMyPatientsOnly, value))
                {
                    LoadPatientsCommand.Execute(null);
                }
            }
        }

        private PhysicianProfile? _selectedPhysicianFilter;
        public PhysicianProfile? SelectedPhysicianFilter
        {
            get => _selectedPhysicianFilter;
            set
            {
                if (SetProperty(ref _selectedPhysicianFilter, value))
                {
                    LoadPatientsCommand.Execute(null);
                }
            }
        }

        private int _assignmentFilterIndex = 0; // 0=All, 1=Assigned Only, 2=Unassigned Only
        public int AssignmentFilterIndex
        {
            get => _assignmentFilterIndex;
            set
            {
                if (SetProperty(ref _assignmentFilterIndex, value))
                {
                    // Semantic fix: If "Unassigned only" is selected, clear physician filter
                    // since filtering by assigned physician AND unassigned patients makes no sense
                    if (value == 2 && SelectedPhysicianFilter != null)
                    {
                        SelectedPhysicianFilter = null;
                    }
                    LoadPatientsCommand.Execute(null);
                }
            }
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        // RBAC properties
        public bool CanCreatePatient => HasPermission(_sessionManager, Permission.CreatePatientProfile);
        public bool CanAssignPatients => HasPermission(_sessionManager, Permission.CreatePatientProfile);
        public bool IsPhysician => GetCurrentRole(_sessionManager) == UserRole.Physician;
        public bool IsAdministrator => GetCurrentRole(_sessionManager) == UserRole.Administrator;

        // UI visibility helpers
        public bool ShowPhysicianFilter => IsAdministrator; // Only admins can select which physician
        public bool ShowMyPatientsToggle => IsPhysician; // Only physicians see this toggle

        public MauiCommand LoadPatientsCommand { get; }
        public MauiCommand SearchCommand { get; }
        public MauiCommand ViewPatientCommand { get; }
        public MauiCommand CreatePatientCommand { get; }
        public MauiCommand AssignPatientCommand { get; }
        public MauiCommand ClearPhysicianFilterCommand { get; }
        public MauiCommand RefreshCommand { get; }
        public MauiCommand BackCommand { get; }

        public PatientListViewModel(
            CommandFactory commandFactory,
            INavigationService navigationService,
            SessionManager sessionManager,
            ProfileService profileService)
        {
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _profileRegistry = profileService ?? throw new ArgumentNullException(nameof(profileService));

            Title = "Patients";

            // Initialize physician list for admin filter
            if (IsAdministrator)
            {
                var physicians = _profileRegistry.GetAllPhysicians();
                foreach (var physician in physicians)
                {
                    AvailablePhysicians.Add(physician);
                }
            }

            // Create list command
            var listCoreCommand = _commandFactory.CreateCommand(ListPatientsCommand.Key);
            LoadPatientsCommand = new MauiCommandAdapter(
                listCoreCommand!,
                parameterBuilder: BuildListParameters,
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleListResult,
                viewModel: this
            );

            SearchCommand = new RelayCommand(() => LoadPatientsCommand.Execute(null));

            ViewPatientCommand = new RelayCommand<Guid>(
                execute: async (patientId) => await NavigateToDetailAsync(patientId)
            );

            CreatePatientCommand = new AsyncRelayCommand(NavigateToCreateAsync);

            AssignPatientCommand = new RelayCommand<Guid>(
                execute: (patientId) => AssignPatient(patientId)
            );

            ClearPhysicianFilterCommand = new RelayCommand(() =>
            {
                SelectedPhysicianFilter = null;
            });

            RefreshCommand = new RelayCommand(() =>
            {
                IsRefreshing = true;
                LoadPatientsCommand.Execute(null);
                IsRefreshing = false;
            });

            BackCommand = new AsyncRelayCommand(async () => await _navigationService.NavigateToHomeAsync());

            // Load patients on initialization
            LoadPatientsCommand.Execute(null);
        }

        private CommandParameters BuildListParameters()
        {
            var parameters = new CommandParameters();

            // Add search filter if provided
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                parameters.SetParameter(ListPatientsCommand.Parameters.Search, SearchText);
            }

            // Apply physician filter based on role
            if (IsPhysician && ShowMyPatientsOnly)
            {
                // Physician viewing their own patients
                parameters.SetParameter(ListPatientsCommand.Parameters.PhysicianId, _sessionManager.CurrentSession?.UserId);
            }
            // NOTE: Administrators see ALL patients regardless of selected physician filter
            // The selected physician is only used for assignment purposes, not filtering

            return parameters;
        }

        /// <summary>
        /// Assigns a patient to a physician (self for physicians, selected for admins)
        /// </summary>
        public void AssignPatient(Guid patientId)
        {
            try
            {
                Guid physicianId;
                if (IsPhysician)
                {
                    physicianId = _sessionManager.CurrentSession?.UserId ?? Guid.Empty;
                }
                else if (IsAdministrator && SelectedPhysicianFilter != null)
                {
                    physicianId = SelectedPhysicianFilter.Id;
                }
                else
                {
                    ValidationErrors.Add("Please select a physician first");
                    return;
                }

                // Create and execute assign command
                var assignCommand = _commandFactory.CreateCommand(AssignPatientToPhysicianCommand.Key);
                var parameters = new CommandParameters();
                parameters.SetParameter(AssignPatientToPhysicianCommand.Parameters.PatientId, patientId);
                parameters.SetParameter(AssignPatientToPhysicianCommand.Parameters.PhysicianId, physicianId);
                parameters.SetParameter(AssignPatientToPhysicianCommand.Parameters.SetPrimary, true);

                var adapter = new MauiCommandAdapter(
                    assignCommand!,
                    () => parameters,
                    () => _sessionManager.CurrentSession,
                    HandleAssignResult,
                    this
                );

                adapter.Execute(null);
            }
            catch (Exception ex)
            {
                ValidationErrors.Add($"Error assigning patient: {ex.Message}");
            }
        }

        private void HandleListResult(CommandResult result)
        {
            if (result.Success && result.Data is IEnumerable<PatientProfile> allPatients)
            {
                Patients.Clear();

                // Apply client-side filtering for assignment status
                var filteredPatients = allPatients;

                if (AssignmentFilterIndex == 1) // Assigned Only
                {
                    filteredPatients = allPatients.Where(p => p.PrimaryPhysicianId != null);
                }
                else if (AssignmentFilterIndex == 2) // Unassigned Only
                {
                    filteredPatients = allPatients.Where(p => p.PrimaryPhysicianId == null);
                }

                foreach (var patient in filteredPatients)
                {
                    Patients.Add(patient);
                }

                ClearValidation();
            }
            else
            {
                Patients.Clear();
            }
        }

        private void HandleAssignResult(CommandResult result)
        {
            if (result.Success)
            {
                // Reload patient list to reflect assignment
                LoadPatientsCommand.Execute(null);
                ClearValidation();
            }
            // Errors are handled by MauiCommandAdapter
        }

        private async Task NavigateToDetailAsync(Guid patientId)
        {
            await _navigationService.NavigateToAsync($"PatientDetailPage?patientId={patientId}");
        }

        private async Task NavigateToCreateAsync()
        {
            await _navigationService.NavigateToAsync("PatientEditPage");
        }

        /// <summary>
        /// Helper method to get assignment status for a patient
        /// </summary>
        public string GetAssignmentStatus(PatientProfile patient)
        {
            if (patient.PrimaryPhysicianId == null)
                return "Unassigned";

            var physician = _profileRegistry.GetProfileById(patient.PrimaryPhysicianId.Value) as PhysicianProfile;
            return physician != null ? $"Assigned to Dr. {physician.Name}" : "Assigned";
        }

        /// <summary>
        /// Helper to determine if assign button should be visible for a specific patient
        /// </summary>
        public bool CanAssignPatient(PatientProfile patient)
        {
            if (!CanAssignPatients) return false;

            if (IsPhysician)
            {
                // Physicians can only assign if patient is not already assigned to them
                return patient.PrimaryPhysicianId != _sessionManager.CurrentSession?.UserId;
            }
            else if (IsAdministrator)
            {
                // Admins need a physician selected to assign
                return SelectedPhysicianFilter != null;
            }

            return false;
        }
    }
}
