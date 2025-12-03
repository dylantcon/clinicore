using System.Collections.ObjectModel;
using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Profile;
using Core.CliniCore.Domain;
using Core.CliniCore.Services;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels
{
    /// <summary>
    /// ViewModel for Patient Detail page
    /// Displays patient information and provides edit/delete actions
    /// </summary>
    [QueryProperty(nameof(PatientIdString), "patientId")]
    public partial class PatientDetailViewModel : BaseViewModel
    {
        private readonly CommandFactory _commandFactory;
        private readonly INavigationService _navigationService;
        private readonly SessionManager _sessionManager;
        private readonly ProfileService _profileRegistry;

        private Guid _patientId;
        public Guid PatientId
        {
            get => _patientId;
            private set
            {
                if (SetProperty(ref _patientId, value))
                {
                    LoadPatientCommand.Execute(null);
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
            }
        }

        private PatientProfile? _patient;
        public PatientProfile? Patient
        {
            get => _patient;
            set => SetProperty(ref _patient, value);
        }

        private string _patientName = string.Empty;
        public string PatientName
        {
            get => _patientName;
            set => SetProperty(ref _patientName, value);
        }

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _address = string.Empty;
        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        private string _birthDate = string.Empty;
        public string BirthDate
        {
            get => _birthDate;
            set => SetProperty(ref _birthDate, value);
        }

        private string _gender = string.Empty;
        public string Gender
        {
            get => _gender;
            set => SetProperty(ref _gender, value);
        }

        private string _race = string.Empty;
        public string Race
        {
            get => _race;
            set => SetProperty(ref _race, value);
        }

        private string _primaryPhysician = string.Empty;
        public string PrimaryPhysician
        {
            get => _primaryPhysician;
            set => SetProperty(ref _primaryPhysician, value);
        }

        private ObservableCollection<PhysicianProfile> _availablePhysicians = [];
        public ObservableCollection<PhysicianProfile> AvailablePhysicians
        {
            get => _availablePhysicians;
            set => SetProperty(ref _availablePhysicians, value);
        }

        private PhysicianProfile? _selectedPhysician;
        public PhysicianProfile? SelectedPhysician
        {
            get => _selectedPhysician;
            set => SetProperty(ref _selectedPhysician, value);
        }

        private int _appointmentCount;
        public int AppointmentCount
        {
            get => _appointmentCount;
            set => SetProperty(ref _appointmentCount, value);
        }

        private int _documentCount;
        public int DocumentCount
        {
            get => _documentCount;
            set => SetProperty(ref _documentCount, value);
        }

        public MauiCommand LoadPatientCommand { get; }
        public MauiCommand LoadPhysiciansCommand { get; }
        public MauiCommand AssignPhysicianCommand { get; }
        public MauiCommand EditPatientCommand { get; }
        public MauiCommand DeletePatientCommand { get; }
        public MauiCommand ViewClinicalDocumentsCommand { get; }
        public MauiCommand BackCommand { get; }

        public PatientDetailViewModel(
            CommandFactory commandFactory,
            INavigationService navigationService,
            SessionManager sessionManager,
            ProfileService profileService)
        {
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _profileRegistry = profileService ?? throw new ArgumentNullException(nameof(profileService));

            Title = "Patient Details";

            // Create view command
            var viewCoreCommand = _commandFactory.CreateCommand(ViewPatientProfileCommand.Key);
            LoadPatientCommand = new MauiCommandAdapter(
                viewCoreCommand!,
                parameterBuilder: BuildViewParameters,
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleViewResult,
                viewModel: this
            );

            // Load physicians command
            var listPhysiciansCoreCommand = _commandFactory.CreateCommand(ListPhysiciansCommand.Key);
            LoadPhysiciansCommand = new MauiCommandAdapter(
                listPhysiciansCoreCommand!,
                parameterBuilder: () => new CommandParameters(),
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleLoadPhysiciansResult,
                viewModel: this
            );

            // Assign physician command
            var assignCoreCommand = _commandFactory.CreateCommand(AssignPatientToPhysicianCommand.Key);
            AssignPhysicianCommand = new MauiCommandAdapter(
                assignCoreCommand!,
                parameterBuilder: BuildAssignParameters,
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleAssignResult,
                viewModel: this
            );

            // Edit command
            EditPatientCommand = new AsyncRelayCommand(NavigateToEditAsync);

            // Delete command - with confirmation dialog
            DeletePatientCommand = new AsyncRelayCommand(ExecuteDeleteAsync);

            // View clinical documents command
            ViewClinicalDocumentsCommand = new AsyncRelayCommand(async () =>
            {
                await _navigationService.NavigateToAsync($"ClinicalDocumentListPage?patientId={PatientId}");
            });

            // Back command - navigate explicitly to list
            BackCommand = new AsyncRelayCommand(async () => await _navigationService.NavigateToAsync("PatientListPage"));

            // Load physicians list on initialization
            LoadPhysiciansCommand.Execute(null);
        }

        private CommandParameters BuildViewParameters()
        {
            return new CommandParameters()
                .SetParameter(ViewPatientProfileCommand.Parameters.ProfileId, PatientId)
                .SetParameter(ViewPatientProfileCommand.Parameters.ShowDetails, true);
        }

        private void HandleViewResult(CommandResult result)
        {
            if (result.Success && result.Data is PatientProfile patient)
            {
                Patient = patient;
                PatientName = patient.Name;
                Username = patient.Username;
                Address = patient.Address;
                BirthDate = patient.BirthDate.ToString("yyyy-MM-dd");
                Gender = patient.Gender.ToString();
                Race = patient.Race;
                AppointmentCount = patient.AppointmentIds.Count;
                DocumentCount = patient.ClinicalDocumentIds.Count;

                // Load primary physician name
                if (patient.PrimaryPhysicianId.HasValue)
                {
                    PrimaryPhysician = _profileRegistry.GetProfileById(patient.PrimaryPhysicianId.Value) is PhysicianProfile physician ? $"Dr. {physician.Name}" : "Unknown";
                }
                else
                {
                    PrimaryPhysician = "None assigned";
                }

                Title = $"Patient: {patient.Name}";
                ClearValidation();
            }
        }

        private CommandParameters BuildDeleteParameters()
        {
            return new CommandParameters()
                .SetParameter(DeleteProfileCommand.Parameters.ProfileId, PatientId);
        }

        private void HandleDeleteResult(CommandResult result)
        {
            if (result.Success)
            {
                // Navigate to list page after successful delete (must be on UI thread)
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await _navigationService.NavigateToAsync("PatientListPage");
                });
            }
        }

        private void HandleLoadPhysiciansResult(CommandResult result)
        {
            if (result.Success && result.Data is IEnumerable<PhysicianProfile> physicians)
            {
                AvailablePhysicians.Clear();
                foreach (var physician in physicians)
                {
                    AvailablePhysicians.Add(physician);
                }
            }
        }

        private CommandParameters BuildAssignParameters()
        {
            return new CommandParameters()
                .SetParameter(AssignPatientToPhysicianCommand.Parameters.PatientId, PatientId)
                .SetParameter(AssignPatientToPhysicianCommand.Parameters.PhysicianId, SelectedPhysician?.Id)
                .SetParameter(AssignPatientToPhysicianCommand.Parameters.SetPrimary, true);
        }

        private void HandleAssignResult(CommandResult result)
        {
            if (result.Success)
            {
                // Reload patient to show updated physician assignment
                LoadPatientCommand.Execute(null);
            }
        }

        private async Task ExecuteDeleteAsync()
        {
            // Show confirmation dialog
            var confirmed = await Application.Current!.MainPage!.DisplayAlert(
                "Delete Patient",
                $"Are you sure you want to delete {PatientName}? This action cannot be undone.",
                "Yes",
                "No"
            );

            if (!confirmed)
            {
                return;
            }

            // Execute the actual delete command
            var deleteCoreCommand = _commandFactory.CreateCommand(DeleteProfileCommand.Key);
            var deleteAdapter = new MauiCommandAdapter(
                deleteCoreCommand!,
                parameterBuilder: BuildDeleteParameters,
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleDeleteResult,
                viewModel: this
            );

            deleteAdapter.Execute(null);
        }

        private async Task NavigateToEditAsync()
        {
            await _navigationService.NavigateToAsync($"PatientEditPage?patientId={PatientId}");
        }
    }
}
