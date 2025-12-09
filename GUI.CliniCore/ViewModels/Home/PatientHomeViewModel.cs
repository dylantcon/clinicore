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
    /// ViewModel for Patient home page
    /// Provides access to view own appointments, clinical documents, and care team
    /// </summary>
    public class PatientHomeViewModel : BaseViewModel
    {
        private readonly SessionManager _sessionManager;
        private readonly INavigationService _navigationService;
        private readonly ProfileService _profileRegistry;
        private readonly SchedulerService _schedulerService;
        private readonly Dictionary<Guid, (string Name, List<MedicalSpecialization> Specializations)> _physicianCache = new();

        private string _welcomeMessage = string.Empty;
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        /// <summary>
        /// Appointments for the calendar view (patient's own appointments).
        /// </summary>
        public ObservableCollection<AppointmentTimeInterval> Appointments { get; } = new();

        private DateTime _selectedCalendarDate = DateTime.Today;
        public DateTime SelectedCalendarDate
        {
            get => _selectedCalendarDate;
            set => SetProperty(ref _selectedCalendarDate, value);
        }

        // Navigation Commands - matching CLI menu structure for patients
        public MauiCommand ViewMyAppointmentsCommand { get; }
        public MauiCommand ViewMyClinicalDocumentsCommand { get; }
        public MauiCommand ViewMyPhysiciansCommand { get; }
        public MauiCommand AppointmentTappedCommand { get; }
        public MauiCommand DayTappedCommand { get; }
        public MauiCommand LogoutCommand { get; }

        /// <summary>
        /// Lookup function for physician info (name and specializations) by ID.
        /// Used by CalendarView for appointment colors and display.
        /// </summary>
        public Func<Guid, (string Name, List<MedicalSpecialization> Specializations)?> PhysicianLookup => LookupPhysician;

        public PatientHomeViewModel(
            SessionManager sessionManager,
            INavigationService navigationService,
            ProfileService profileService,
            SchedulerService schedulerService)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _profileRegistry = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));

            Title = "Patient Portal";

            // Get patient name from profile
            var patientProfile = _sessionManager.CurrentSession?.UserId != null
                ? _profileRegistry.GetProfileById(_sessionManager.CurrentSession.UserId) as PatientProfile
                : null;
            var displayName = patientProfile?.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty;
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = _sessionManager.CurrentUsername;
            }
            WelcomeMessage = $"Welcome, {displayName}!";

            // Initialize commands
            ViewMyAppointmentsCommand = new AsyncRelayCommand(async () =>
            {
                var patientId = _sessionManager.CurrentSession?.UserId;
                if (patientId.HasValue)
                {
                    await _navigationService.NavigateToAsync($"AppointmentListPage?patientId={patientId.Value}");
                }
            });

            ViewMyClinicalDocumentsCommand = new AsyncRelayCommand(async () =>
            {
                var patientId = _sessionManager.CurrentSession?.UserId;
                if (patientId.HasValue)
                {
                    await _navigationService.NavigateToAsync($"ClinicalDocumentListPage?patientId={patientId.Value}");
                }
            });

            ViewMyPhysiciansCommand = new AsyncRelayCommand(async () =>
            {
                // Navigate to full physician list - patient can view all physicians
                await _navigationService.NavigateToAsync("PhysicianListPage");
            });

            AppointmentTappedCommand = new AsyncRelayCommand<AppointmentTimeInterval>(async appointment =>
            {
                if (appointment != null)
                {
                    await _navigationService.NavigateToAsync($"AppointmentDetailPage?appointmentId={appointment.Id}");
                }
            });

            DayTappedCommand = new AsyncRelayCommand<(DateTime, List<AppointmentTimeInterval>)>(async tuple =>
            {
                // Navigate to appointment list filtered by date for this patient
                var patientId = _sessionManager.CurrentSession?.UserId;
                if (patientId.HasValue)
                {
                    await _navigationService.NavigateToAsync($"AppointmentListPage?patientId={patientId.Value}&date={tuple.Item1:yyyy-MM-dd}");
                }
            });

            LogoutCommand = new AsyncRelayCommand(LogoutAsync);

            // Load appointments for calendar
            LoadAppointments();
        }

        private void LoadAppointments()
        {
            var patientId = _sessionManager.CurrentSession?.UserId;
            if (!patientId.HasValue) return;

            Appointments.Clear();
            var appointments = _schedulerService.GetPatientAppointments(patientId.Value);
            foreach (var appointment in appointments)
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

        private (string Name, List<MedicalSpecialization> Specializations)? LookupPhysician(Guid physicianId)
        {
            // Check cache first
            if (_physicianCache.TryGetValue(physicianId, out var cached))
                return cached;

            // Lookup from profile service
            var profile = _profileRegistry.GetProfileById(physicianId) as PhysicianProfile;
            if (profile == null)
                return null;

            var name = profile.Name ?? "Unknown";
            var specs = profile.Specializations.ToList();

            // Cache the result
            var result = (name, specs);
            _physicianCache[physicianId] = result;
            return result;
        }
    }
}
