using System.Collections.ObjectModel;
using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Profile;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users.Concrete;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;
using GUI.CliniCore.ViewModels.Base;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels.Physicians
{
    /// <summary>
    /// ViewModel for Physician Detail page
    /// Displays physician information and provides edit/delete actions
    /// </summary>
    [QueryProperty(nameof(PhysicianIdString), "physicianId")]
    public partial class PhysicianDetailViewModel : BaseViewModel
    {
        private readonly CommandFactory _commandFactory;
        private readonly CommandInvoker _commandInvoker;
        private readonly INavigationService _navigationService;
        private readonly SessionManager _sessionManager;

        private Guid _physicianId;
        public Guid PhysicianId
        {
            get => _physicianId;
            private set
            {
                if (SetProperty(ref _physicianId, value))
                {
                    LoadPhysicianCommand.Execute(null);
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
            }
        }

        private PhysicianProfile? _physician;
        public PhysicianProfile? Physician
        {
            get => _physician;
            set => SetProperty(ref _physician, value);
        }

        private string _physicianName = string.Empty;
        public string PhysicianName
        {
            get => _physicianName;
            set => SetProperty(ref _physicianName, value);
        }

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _licenseNumber = string.Empty;
        public string LicenseNumber
        {
            get => _licenseNumber;
            set => SetProperty(ref _licenseNumber, value);
        }

        private string _graduationDate = string.Empty;
        public string GraduationDate
        {
            get => _graduationDate;
            set => SetProperty(ref _graduationDate, value);
        }

        private string _specializations = string.Empty;
        public string Specializations
        {
            get => _specializations;
            set => SetProperty(ref _specializations, value);
        }

        private int _patientCount;
        public int PatientCount
        {
            get => _patientCount;
            set => SetProperty(ref _patientCount, value);
        }

        private int _appointmentCount;
        public int AppointmentCount
        {
            get => _appointmentCount;
            set => SetProperty(ref _appointmentCount, value);
        }

        // RBAC properties
        public bool CanEditPhysician => HasPermission(_sessionManager, Permission.UpdatePhysicianProfile);
        public bool CanDeletePhysician => HasPermission(_sessionManager, Permission.DeletePhysicianProfile);
        public bool CanViewPatients => HasPermission(_sessionManager, Permission.ViewAllPatients);

        public MauiCommand LoadPhysicianCommand { get; }
        public MauiCommand EditPhysicianCommand { get; }
        public MauiCommand DeletePhysicianCommand { get; }
        public MauiCommand ViewPatientsCommand { get; }
        public MauiCommand BackCommand { get; }

        public PhysicianDetailViewModel(
            CommandFactory commandFactory,
            CommandInvoker commandInvoker,
            INavigationService navigationService,
            SessionManager sessionManager)
        {
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));

            Title = "Physician Details";

            // Create view command
            var viewCoreCommand = _commandFactory.CreateCommand(ViewPhysicianProfileCommand.Key);
            LoadPhysicianCommand = new MauiCommandAdapter(
                _commandInvoker,
                viewCoreCommand!,
                parameterBuilder: BuildViewParameters,
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleViewResult
            );

            // Edit command
            EditPhysicianCommand = new AsyncRelayCommand(NavigateToEditAsync);

            // Delete command - with confirmation dialog
            DeletePhysicianCommand = new AsyncRelayCommand(ExecuteDeleteAsync);

            // View patients command
            ViewPatientsCommand = new AsyncRelayCommand(async () => await _navigationService.NavigateToAsync("PatientListPage"));

            // Back command - navigate explicitly to list
            BackCommand = new AsyncRelayCommand(async () => await _navigationService.NavigateToAsync("PhysicianListPage"));
        }

        private CommandParameters BuildViewParameters()
        {
            return new CommandParameters()
                .SetParameter(ViewPhysicianProfileCommand.Parameters.ProfileId, PhysicianId)
                .SetParameter(ViewPhysicianProfileCommand.Parameters.ShowDetails, true);
        }

        private void HandleViewResult(CommandResult result)
        {
            if (result.Success && result.Data is PhysicianProfile physician)
            {
                Physician = physician;
                PhysicianName = physician.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty;
                Username = physician.Username;
                LicenseNumber = physician.GetValue<string>(PhysicianEntryType.LicenseNumber.GetKey()) ?? string.Empty;
                GraduationDate = physician.GetValue<DateTime>(PhysicianEntryType.GraduationDate.GetKey()).ToString("yyyy-MM-dd");

                // Format specializations
                var specs = physician.GetValue<List<MedicalSpecialization>>(PhysicianEntryType.Specializations.GetKey()) ?? new List<MedicalSpecialization>();
                if (specs.Any())
                {
                    Specializations = string.Join(", ", specs);
                }
                else
                {
                    Specializations = "None";
                }

                // Get counts from command result
                PatientCount = result.GetData<int>(ViewPhysicianProfileCommand.Results.PatientCount);
                AppointmentCount = result.GetData<int>(ViewPhysicianProfileCommand.Results.AppointmentCount);

                Title = $"Dr. {physician.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty}";
                ClearValidation();
            }
        }

        private CommandParameters BuildDeleteParameters()
        {
            return new CommandParameters()
                .SetParameter(DeleteProfileCommand.Parameters.ProfileId, PhysicianId)
                .SetParameter(DeleteProfileCommand.Parameters.Force, true); // User already confirmed via dialog
        }

        private void HandleDeleteResult(CommandResult result)
        {
            if (result.Success)
            {
                // Navigate to list page after successful delete (must be on UI thread)
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await _navigationService.NavigateToAsync("PhysicianListPage");
                });
            }
        }

        private async Task ExecuteDeleteAsync()
        {
            // Show confirmation dialog
            var confirmed = await Application.Current!.MainPage!.DisplayAlert(
                "Delete Physician",
                $"Are you sure you want to delete Dr. {PhysicianName}? This action cannot be undone.",
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
                _commandInvoker,
                deleteCoreCommand!,
                parameterBuilder: BuildDeleteParameters,
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleDeleteResult
            );

            deleteAdapter.Execute(null);
        }

        private async Task NavigateToEditAsync()
        {
            await _navigationService.NavigateToAsync($"PhysicianEditPage?physicianId={PhysicianId}");
        }
    }
}
