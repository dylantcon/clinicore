using Core.CliniCore.Domain;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels
{
    /// <summary>
    /// ViewModel for Physician home page
    /// Provides access to patient management, scheduling, and clinical documentation features
    /// </summary>
    public class PhysicianHomeViewModel : BaseViewModel
    {
        private readonly SessionManager _sessionManager;
        private readonly INavigationService _navigationService;
        private readonly ProfileRegistry _profileRegistry = ProfileRegistry.Instance;

        private string _welcomeMessage = string.Empty;
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        // Navigation Commands - matching CLI menu structure for physicians
        public MauiCommand ViewMyPatientsCommand { get; }
        public MauiCommand ViewMyScheduleCommand { get; }
        public MauiCommand CreateClinicalDocumentCommand { get; }
        public MauiCommand ManageAvailabilityCommand { get; }
        public MauiCommand LogoutCommand { get; }

        public PhysicianHomeViewModel(
            SessionManager sessionManager,
            INavigationService navigationService)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            Title = "Physician Dashboard";

            // Get physician name from profile
            var physicianProfile = _sessionManager.CurrentSession?.UserId != null
                ? _profileRegistry.GetProfileById(_sessionManager.CurrentSession.UserId) as PhysicianProfile
                : null;
            var displayName = physicianProfile?.Name ?? _sessionManager.CurrentUsername;
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
            LogoutCommand = new AsyncRelayCommand(LogoutAsync);
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
