using Core.CliniCore.Domain;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels
{
    /// <summary>
    /// ViewModel for Patient home page
    /// Provides access to view own appointments, clinical documents, and care team
    /// </summary>
    public class PatientHomeViewModel : BaseViewModel
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

        // Navigation Commands - matching CLI menu structure for patients
        public MauiCommand ViewMyAppointmentsCommand { get; }
        public MauiCommand ViewMyClinicalDocumentsCommand { get; }
        public MauiCommand ViewMyPhysiciansCommand { get; }
        public MauiCommand LogoutCommand { get; }

        public PatientHomeViewModel(
            SessionManager sessionManager,
            INavigationService navigationService)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            Title = "Patient Portal";

            // Get patient name from profile
            var patientProfile = _sessionManager.CurrentSession?.UserId != null
                ? _profileRegistry.GetProfileById(_sessionManager.CurrentSession.UserId) as PatientProfile
                : null;
            var displayName = patientProfile?.Name ?? _sessionManager.CurrentUsername;
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
