using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Scheduling;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Scheduling;
using Core.CliniCore.Scheduling.Management;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels
{
    /// <summary>
    /// ViewModel for Appointment Detail page
    /// Displays appointment details and provides actions (reschedule, cancel)
    /// </summary>
    [QueryProperty(nameof(AppointmentIdString), "appointmentId")]
    public partial class AppointmentDetailViewModel : BaseViewModel
    {
        private readonly CommandFactory _commandFactory;
        private readonly INavigationService _navigationService;
        private readonly SessionManager _sessionManager;
        private readonly ProfileRegistry _profileRegistry = ProfileRegistry.Instance;
        private readonly ScheduleManager _scheduleManager = ScheduleManager.Instance;

        private Guid? _appointmentId;
        private AppointmentTimeInterval? _appointment;

        public string AppointmentIdString
        {
            set
            {
                if (Guid.TryParse(value, out var guid))
                {
                    _appointmentId = guid;
                    LoadAppointmentCommand.Execute(null);
                }
            }
        }

        private string _appointmentInfo = string.Empty;
        public string AppointmentInfo
        {
            get => _appointmentInfo;
            set => SetProperty(ref _appointmentInfo, value);
        }

        private string _patientName = string.Empty;
        public string PatientName
        {
            get => _patientName;
            set => SetProperty(ref _patientName, value);
        }

        private string _physicianName = string.Empty;
        public string PhysicianName
        {
            get => _physicianName;
            set => SetProperty(ref _physicianName, value);
        }

        private string _startTime = string.Empty;
        public string StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        private string _duration = string.Empty;
        public string Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        private string _reason = string.Empty;
        public string Reason
        {
            get => _reason;
            set => SetProperty(ref _reason, value);
        }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        private string _status = string.Empty;
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private string _statusColor = string.Empty;
        public string StatusColor
        {
            get => _statusColor;
            set => SetProperty(ref _statusColor, value);
        }

        // Action availability - can only reschedule/cancel if not already cancelled or completed
        public bool CanReschedule => _appointment != null &&
                                     _appointment.Status != AppointmentStatus.Cancelled &&
                                     _appointment.Status != AppointmentStatus.Completed;

        public bool CanCancel => _appointment != null &&
                                _appointment.Status != AppointmentStatus.Cancelled;

        public bool CanDelete => _appointment != null; // Can delete any appointment

        public MauiCommand LoadAppointmentCommand { get; }
        public MauiCommand RescheduleCommand { get; }
        public MauiCommand CancelCommand { get; }
        public MauiCommand DeleteCommand { get; }
        public MauiCommand BackCommand { get; }

        public AppointmentDetailViewModel(
            CommandFactory commandFactory,
            INavigationService navigationService,
            SessionManager sessionManager)
        {
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));

            Title = "Appointment Details";

            // Load command - direct load from ScheduleManager for viewing
            LoadAppointmentCommand = new RelayCommand(LoadAppointment);

            // Reschedule command
            RescheduleCommand = new AsyncRelayCommand(
                execute: async () => await NavigateToRescheduleAsync(),
                canExecute: () => CanReschedule
            );

            // Cancel command
            CancelCommand = new AsyncRelayCommand(
                execute: async () => await ExecuteCancelAsync(),
                canExecute: () => CanCancel
            );

            // Delete command
            DeleteCommand = new AsyncRelayCommand(
                execute: async () => await ExecuteDeleteAsync(),
                canExecute: () => CanDelete
            );

            // Back command
            BackCommand = new AsyncRelayCommand(async () =>
            {
                // Navigate back to list
                await _navigationService.NavigateToAsync("AppointmentListPage");
            });
        }

        private void LoadAppointment()
        {
            if (!_appointmentId.HasValue) return;

            try
            {
                // Find appointment in schedule manager
                var allAppointments = _scheduleManager.GetAllAppointments();
                _appointment = allAppointments.FirstOrDefault(a => a.Id == _appointmentId.Value);

                if (_appointment == null)
                {
                    ValidationErrors.Clear();
                    ValidationErrors.Add("Appointment not found");
                    return;
                }

                // Get patient and physician info
                var patient = _profileRegistry.GetProfileById(_appointment.PatientId) as PatientProfile;
                var physician = _profileRegistry.GetProfileById(_appointment.PhysicianId) as PhysicianProfile;

                // Populate properties
                PatientName = patient?.Name ?? "Unknown Patient";
                PhysicianName = physician != null ? $"Dr. {physician.Name}" : "Unknown Physician";
                StartTime = _appointment.Start.ToString("yyyy-MM-dd HH:mm");
                Duration = $"{(int)(_appointment.End - _appointment.Start).TotalMinutes} minutes";
                Reason = _appointment.ReasonForVisit ?? "General Consultation";
                Notes = _appointment.Notes ?? "No notes";
                Status = _appointment.Status.ToString();
                StatusColor = _appointment.Status switch
                {
                    AppointmentStatus.Scheduled => "#4CAF50",   // Green
                    AppointmentStatus.Tentative => "#2196F3",   // Blue
                    AppointmentStatus.InProgress => "#FF9800",  // Orange
                    AppointmentStatus.Completed => "#9E9E9E",   // Gray
                    AppointmentStatus.Cancelled => "#F44336",   // Red
                    AppointmentStatus.NoShow => "#F44336",      // Red
                    AppointmentStatus.Rescheduled => "#9C27B0", // Purple
                    _ => "#757575"                              // Default gray
                };

                AppointmentInfo = $"Appointment ID: {_appointment.Id:N}";

                ClearValidation();

                // Update action button states and visibility
                (RescheduleCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (CancelCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (DeleteCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();

                // Notify UI about button visibility changes
                OnPropertyChanged(nameof(CanReschedule));
                OnPropertyChanged(nameof(CanCancel));
                OnPropertyChanged(nameof(CanDelete));
            }
            catch (Exception ex)
            {
                ValidationErrors.Clear();
                ValidationErrors.Add($"Error loading appointment: {ex.Message}");
            }
        }

        private async Task NavigateToRescheduleAsync()
        {
            if (_appointmentId.HasValue)
            {
                await _navigationService.NavigateToAsync($"AppointmentEditPage?appointmentId={_appointmentId.Value}");
            }
        }

        private async Task ExecuteCancelAsync()
        {
            if (_appointment == null || !_appointmentId.HasValue) return;

            try
            {
                // Confirm cancellation
                bool confirmed = await Application.Current!.MainPage!.DisplayAlert(
                    "Confirm Cancellation",
                    $"Are you sure you want to cancel this appointment with {PatientName}?",
                    "Cancel Appointment",
                    "Keep Appointment");

                if (!confirmed) return;

                var cancelCoreCommand = _commandFactory.CreateCommand(CancelAppointmentCommand.Key);
                var parameters = new CommandParameters()
                    .SetParameter(CancelAppointmentCommand.Parameters.AppointmentId, _appointmentId.Value)
                    .SetParameter(CancelAppointmentCommand.Parameters.Reason, "Cancelled by user");

                var cancelCommand = new MauiCommandAdapter(
                    cancelCoreCommand!,
                    parameterBuilder: () => parameters,
                    sessionProvider: () => _sessionManager.CurrentSession,
                    resultHandler: HandleCancelResult,
                    viewModel: this
                );

                cancelCommand.Execute(null);
            }
            catch (Exception ex)
            {
                ValidationErrors.Clear();
                ValidationErrors.Add($"Error cancelling appointment: {ex.Message}");
            }
        }

        private void HandleCancelResult(CommandResult result)
        {
            if (result.Success)
            {
                // Reload appointment to show updated status
                LoadAppointment();

                ValidationErrors.Clear();
                ValidationErrors.Add("Appointment cancelled successfully!");

                // Update button visibility since status changed
                OnPropertyChanged(nameof(CanReschedule));
                OnPropertyChanged(nameof(CanCancel));
                OnPropertyChanged(nameof(CanDelete));
            }
        }

        private async Task ExecuteDeleteAsync()
        {
            if (_appointment == null || !_appointmentId.HasValue) return;

            try
            {
                // Confirm deletion
                bool confirmed = await Application.Current!.MainPage!.DisplayAlert(
                    "Confirm Deletion",
                    $"Are you sure you want to permanently delete this appointment with {PatientName}? This action cannot be undone.",
                    "Delete",
                    "Cancel");

                if (!confirmed) return;

                var deleteCoreCommand = _commandFactory.CreateCommand(DeleteAppointmentCommand.Key);
                var parameters = new CommandParameters()
                    .SetParameter(DeleteAppointmentCommand.Parameters.AppointmentId, _appointmentId.Value);

                var deleteCommand = new MauiCommandAdapter(
                    deleteCoreCommand!,
                    parameterBuilder: () => parameters,
                    sessionProvider: () => _sessionManager.CurrentSession,
                    resultHandler: HandleDeleteResult,
                    viewModel: this
                );

                deleteCommand.Execute(null);
            }
            catch (Exception ex)
            {
                ValidationErrors.Clear();
                ValidationErrors.Add($"Error deleting appointment: {ex.Message}");
            }
        }

        private void HandleDeleteResult(CommandResult result)
        {
            if (result.Success)
            {
                // Navigate back to list after successful deletion
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await _navigationService.NavigateToAsync("AppointmentListPage");
                });
            }
        }
    }
}
