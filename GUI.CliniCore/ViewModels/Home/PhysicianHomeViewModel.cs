using System.Collections.ObjectModel;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
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
    /// ViewModel for Physician home page
    /// Provides access to patient management, scheduling, and clinical documentation features
    /// </summary>
    public class PhysicianHomeViewModel : BaseViewModel
    {
        private readonly SessionManager _sessionManager;
        private readonly INavigationService _navigationService;
        private readonly ProfileService _profileRegistry;
        private readonly SchedulerService _schedulerService;
        private readonly Dictionary<Guid, string> _patientNameCache = new();

        private string _welcomeMessage = string.Empty;
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        /// <summary>
        /// Appointments for the calendar view (physician's scheduled appointments).
        /// </summary>
        public ObservableCollection<AppointmentTimeInterval> Appointments { get; } = new();

        private DateTime _selectedCalendarDate = DateTime.Today;
        public DateTime SelectedCalendarDate
        {
            get => _selectedCalendarDate;
            set => SetProperty(ref _selectedCalendarDate, value);
        }

        // Navigation Commands - matching CLI menu structure for physicians
        public MauiCommand ViewMyPatientsCommand { get; }
        public MauiCommand ViewMyScheduleCommand { get; }
        public MauiCommand CreateClinicalDocumentCommand { get; }
        public MauiCommand ManageAvailabilityCommand { get; }
        public MauiCommand AppointmentTappedCommand { get; }
        public MauiCommand DayTappedCommand { get; }
        public MauiCommand LogoutCommand { get; }

        /// <summary>
        /// Lookup function for patient name by ID.
        /// Used by CalendarView to display patient names on appointments.
        /// </summary>
        public Func<Guid, string?> PatientNameLookup => LookupPatientName;

        public PhysicianHomeViewModel(
            SessionManager sessionManager,
            INavigationService navigationService,
            ProfileService profileService,
            SchedulerService schedulerService)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _profileRegistry = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));

            Title = "Physician Dashboard";

            // Get physician name from profile
            var physicianProfile = _sessionManager.CurrentSession?.UserId != null
                ? _profileRegistry.GetProfileById(_sessionManager.CurrentSession.UserId) as PhysicianProfile
                : null;
            var displayName = physicianProfile?.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty;
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = _sessionManager.CurrentUsername;
            }
            WelcomeMessage = $"Welcome, Dr. {displayName}!";

            // Initialize navigation commands
            ViewMyPatientsCommand = new AsyncRelayCommand(async () => await _navigationService.NavigateToAsync("PatientListPage"));

            ViewMyScheduleCommand = new AsyncRelayCommand(async () =>
            {
                // Navigate to appointment list filtered by this physician
                var physicianId = _sessionManager.CurrentSession?.UserId;
                if (physicianId.HasValue)
                {
                    await _navigationService.NavigateToAsync($"AppointmentListPage?physicianId={physicianId.Value}");
                }
            });

            CreateClinicalDocumentCommand = new AsyncRelayCommand(async () =>
            {
                // Navigate to clinical documents list filtered by this physician
                var physicianId = _sessionManager.CurrentSession?.UserId;
                if (physicianId.HasValue)
                {
                    await _navigationService.NavigateToAsync($"ClinicalDocumentListPage?physicianId={physicianId.Value}");
                }
            });

            ManageAvailabilityCommand = new AsyncRelayCommand(async () =>
                await _navigationService.NavigateToAsync("StubPage?type=availability"));

            AppointmentTappedCommand = new AsyncRelayCommand<AppointmentTimeInterval>(async appointment =>
            {
                if (appointment != null)
                {
                    await _navigationService.NavigateToAsync($"AppointmentDetailPage?appointmentId={appointment.Id}");
                }
            });

            DayTappedCommand = new AsyncRelayCommand<(DateTime, List<AppointmentTimeInterval>)>(async tuple =>
            {
                // Navigate to appointment list filtered by date for this physician
                var physicianId = _sessionManager.CurrentSession?.UserId;
                if (physicianId.HasValue)
                {
                    await _navigationService.NavigateToAsync($"AppointmentListPage?physicianId={physicianId.Value}&date={tuple.Item1:yyyy-MM-dd}");
                }
            });

            LogoutCommand = new AsyncRelayCommand(LogoutAsync);

            // Load appointments for calendar
            LoadAppointments();
        }

        private void LoadAppointments()
        {
            var physicianId = _sessionManager.CurrentSession?.UserId;
            if (!physicianId.HasValue) return;

            Appointments.Clear();
            var schedule = _schedulerService.GetPhysicianSchedule(physicianId.Value);
            foreach (var appointment in schedule.GetAppointmentsInRange(DateTime.Today.AddMonths(-1), DateTime.Today.AddMonths(3)))
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

        private string? LookupPatientName(Guid patientId)
        {
            // Check cache first
            if (_patientNameCache.TryGetValue(patientId, out var cached))
                return cached;

            // Lookup from profile service
            var profile = _profileRegistry.GetProfileById(patientId) as PatientProfile;
            if (profile == null)
                return null;

            var name = profile.Name ?? "Unknown";

            // Cache the result
            _patientNameCache[patientId] = name;
            return name;
        }
    }
}
