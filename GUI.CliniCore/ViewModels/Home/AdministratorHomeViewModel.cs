using System.Collections.ObjectModel;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.Scheduling;
using Core.CliniCore.Service;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;
using GUI.CliniCore.ViewModels.Base;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels.Home
{
    /// <summary>
    /// ViewModel for Administrator home page
    /// Provides full system access including user management, system administration, and all clinical features
    /// </summary>
    public class AdministratorHomeViewModel : BaseViewModel
    {
        private readonly SessionManager _sessionManager;
        private readonly INavigationService _navigationService;
        private readonly SchedulerService _schedulerService;
        private readonly ProfileService _profileService;
        private readonly Dictionary<Guid, (string Name, List<MedicalSpecialization> Specializations)> _physicianCache = new();

        private string _welcomeMessage = string.Empty;
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        /// <summary>
        /// Appointments for the calendar view (all facility appointments).
        /// </summary>
        public ObservableCollection<AppointmentTimeInterval> Appointments { get; } = new();

        private DateTime _selectedCalendarDate = DateTime.Today;
        public DateTime SelectedCalendarDate
        {
            get => _selectedCalendarDate;
            set => SetProperty(ref _selectedCalendarDate, value);
        }

        // Navigation Commands - matching CLI menu structure
        public MauiCommand ManageUsersCommand { get; }
        public MauiCommand ManagePatientsCommand { get; }
        public MauiCommand ManagePhysiciansCommand { get; }
        public MauiCommand ViewSchedulingCommand { get; }
        public MauiCommand ViewClinicalDocumentsCommand { get; }
        public MauiCommand ViewReportsCommand { get; }
        public MauiCommand SystemAdminCommand { get; }
        public MauiCommand AppointmentTappedCommand { get; }
        public MauiCommand DayTappedCommand { get; }
        public MauiCommand LogoutCommand { get; }

        /// <summary>
        /// Lookup function for physician info (name and specializations) by ID.
        /// Used by CalendarView for appointment colors.
        /// </summary>
        public Func<Guid, (string Name, List<MedicalSpecialization> Specializations)?> PhysicianLookup => LookupPhysician;

        public AdministratorHomeViewModel(
            SessionManager sessionManager,
            INavigationService navigationService,
            SchedulerService schedulerService,
            ProfileService profileService)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));

            Title = "Administrator Dashboard";
            WelcomeMessage = $"Welcome, {_sessionManager.CurrentUsername}!";

            // Initialize navigation commands
            ManageUsersCommand = new AsyncRelayCommand(async () => await _navigationService.NavigateToAsync("UserListPage"));
            ManagePatientsCommand = new AsyncRelayCommand(async () => await _navigationService.NavigateToAsync("PatientListPage"));
            ManagePhysiciansCommand = new AsyncRelayCommand(async () => await _navigationService.NavigateToAsync("PhysicianListPage"));
            ViewSchedulingCommand = new AsyncRelayCommand(async () => await _navigationService.NavigateToAsync("AppointmentListPage"));
            ViewClinicalDocumentsCommand = new AsyncRelayCommand(async () => await _navigationService.NavigateToAsync("ClinicalDocumentListPage"));
            ViewReportsCommand = new AsyncRelayCommand(async () => await _navigationService.NavigateToAsync("StubPage?type=reports"));
            SystemAdminCommand = new AsyncRelayCommand(async () => await _navigationService.NavigateToAsync("StubPage?type=admin"));

            AppointmentTappedCommand = new AsyncRelayCommand<AppointmentTimeInterval>(async appointment =>
            {
                if (appointment != null)
                {
                    await _navigationService.NavigateToAsync($"AppointmentDetailPage?appointmentId={appointment.Id}");
                }
            });

            DayTappedCommand = new AsyncRelayCommand<(DateTime, List<AppointmentTimeInterval>)>(async tuple =>
            {
                // Navigate to appointment list filtered by date
                await _navigationService.NavigateToAsync($"AppointmentListPage?date={tuple.Item1:yyyy-MM-dd}");
            });

            LogoutCommand = new AsyncRelayCommand(LogoutAsync);

            // Load all facility appointments for calendar
            LoadAppointments();
        }

        private void LoadAppointments()
        {
            Appointments.Clear();
            foreach (var appointment in _schedulerService.GetAllAppointments())
            {
                Appointments.Add(appointment);
            }
        }

        private (string Name, List<MedicalSpecialization> Specializations)? LookupPhysician(Guid physicianId)
        {
            // Check cache first
            if (_physicianCache.TryGetValue(physicianId, out var cached))
                return cached;

            // Lookup from profile service
            var profile = _profileService.GetProfileById(physicianId) as PhysicianProfile;
            if (profile == null)
                return null;

            var name = profile.Name ?? "Unknown";
            var specs = profile.Specializations.ToList();

            // Cache the result
            var result = (name, specs);
            _physicianCache[physicianId] = result;
            return result;
        }

        private void ShowPlaceholder(string featureName)
        {
            ValidationErrors.Clear();
            ValidationErrors.Add($"{featureName} feature coming soon!");
        }

        private async Task LogoutAsync()
        {
            _sessionManager.ClearSession();
            await _navigationService.NavigateToLoginAsync();
        }
    }
}
