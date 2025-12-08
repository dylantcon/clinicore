using System.Collections.ObjectModel;
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
        public MauiCommand LogoutCommand { get; }

        public AdministratorHomeViewModel(
            SessionManager sessionManager,
            INavigationService navigationService,
            SchedulerService schedulerService)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));

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
