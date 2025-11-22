using System.Collections.ObjectModel;
using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Scheduling;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Scheduling;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels
{
    /// <summary>
    /// ViewModel for Appointment List page
    /// Supports listing, filtering, and navigating to appointments
    /// </summary>
    [QueryProperty(nameof(PatientIdString), "patientId")]
    [QueryProperty(nameof(PhysicianIdString), "physicianId")]
    public partial class AppointmentListViewModel : BaseViewModel
    {
        private readonly CommandFactory _commandFactory;
        private readonly INavigationService _navigationService;
        private readonly SessionManager _sessionManager;
        private readonly ProfileRegistry _profileRegistry = ProfileRegistry.Instance;

        private Guid? _patientId;
        private Guid? _physicianId;

        public string PatientIdString
        {
            set
            {
                if (Guid.TryParse(value, out var guid))
                {
                    _patientId = guid;
                    LoadAppointmentsCommand.Execute(null);
                }
            }
        }

        public string PhysicianIdString
        {
            set
            {
                if (Guid.TryParse(value, out var guid))
                {
                    _physicianId = guid;
                    LoadAppointmentsCommand.Execute(null);
                }
            }
        }

        private ObservableCollection<AppointmentListDisplayModel> _appointments = [];
        public ObservableCollection<AppointmentListDisplayModel> Appointments
        {
            get => _appointments;
            set => SetProperty(ref _appointments, value);
        }

        private AppointmentListDisplayModel? _selectedAppointment;
        public AppointmentListDisplayModel? SelectedAppointment
        {
            get => _selectedAppointment;
            set
            {
                if (SetProperty(ref _selectedAppointment, value) && value != null)
                {
                    // Navigate to detail when appointment is selected
                    ViewAppointmentCommand.Execute(value.Id);
                }
            }
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    LoadAppointmentsCommand.Execute(null);
                }
            }
        }

        public MauiCommand LoadAppointmentsCommand { get; }
        public MauiCommand ViewAppointmentCommand { get; }
        public MauiCommand CreateAppointmentCommand { get; }
        public MauiCommand RefreshCommand { get; }
        public MauiCommand BackCommand { get; }

        public AppointmentListViewModel(
            CommandFactory commandFactory,
            INavigationService navigationService,
            SessionManager sessionManager)
        {
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));

            Title = "Appointments";

            // Create list command
            var listCoreCommand = _commandFactory.CreateCommand(ListAppointmentsCommand.Key);
            LoadAppointmentsCommand = new MauiCommandAdapter(
                listCoreCommand!,
                parameterBuilder: BuildListParameters,
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleListResult,
                viewModel: this
            );

            // Navigate to detail
            ViewAppointmentCommand = new RelayCommand<Guid>(
                execute: async (appointmentId) => await NavigateToDetailAsync(appointmentId)
            );

            // Navigate to create
            CreateAppointmentCommand = new AsyncRelayCommand(NavigateToCreateAsync);

            // Refresh command
            RefreshCommand = new RelayCommand(() =>
            {
                IsRefreshing = true;
                LoadAppointmentsCommand.Execute(null);
                IsRefreshing = false;
            });

            // Back command
            BackCommand = new AsyncRelayCommand(async () => await _navigationService.NavigateToHomeAsync());

            // Load appointments on initialization
            LoadAppointmentsCommand.Execute(null);
        }

        private CommandParameters BuildListParameters()
        {
            var parameters = new CommandParameters();

            // Add date filter
            parameters.SetParameter(ListAppointmentsCommand.Parameters.Date, SelectedDate);

            // Add patient filter if provided
            if (_patientId.HasValue && _patientId.Value != Guid.Empty)
            {
                parameters.SetParameter(ListAppointmentsCommand.Parameters.PatientId, _patientId.Value);
            }

            // Add physician filter if provided
            if (_physicianId.HasValue && _physicianId.Value != Guid.Empty)
            {
                parameters.SetParameter(ListAppointmentsCommand.Parameters.PhysicianId, _physicianId.Value);
            }

            return parameters;
        }

        private void HandleListResult(CommandResult result)
        {
            if (result.Success && result.Data is IEnumerable<AppointmentTimeInterval> appointments)
            {
                Appointments.Clear();
                foreach (var apt in appointments.OrderBy(a => a.Start))
                {
                    Appointments.Add(new AppointmentListDisplayModel(apt, _profileRegistry));
                }

                // Clear any previous errors
                ClearValidation();
            }
            else
            {
                // Errors are already populated by the adapter
                Appointments.Clear();
            }
        }

        private async Task NavigateToDetailAsync(Guid appointmentId)
        {
            await _navigationService.NavigateToAsync($"AppointmentDetailPage?appointmentId={appointmentId}");
        }

        private async Task NavigateToCreateAsync()
        {
            var route = "CreateAppointmentPage";
            if (_patientId.HasValue)
            {
                route += $"?patientId={_patientId.Value}";
            }
            if (_physicianId.HasValue)
            {
                route += _patientId.HasValue ? $"&physicianId={_physicianId.Value}" : $"?physicianId={_physicianId.Value}";
            }
            await _navigationService.NavigateToAsync(route);
        }
    }

    /// <summary>
    /// Display model wrapper for AppointmentTimeInterval to enable easier binding in XAML for list view
    /// </summary>
    public class AppointmentListDisplayModel
    {
        private readonly AppointmentTimeInterval _appointment;
        private readonly ProfileRegistry _profileRegistry;

        public AppointmentListDisplayModel(AppointmentTimeInterval appointment, ProfileRegistry profileRegistry)
        {
            _appointment = appointment ?? throw new ArgumentNullException(nameof(appointment));
            _profileRegistry = profileRegistry ?? throw new ArgumentNullException(nameof(profileRegistry));
        }

        public Guid Id => _appointment.Id;
        public DateTime Start => _appointment.Start;
        public DateTime End => _appointment.End;
        public string StartTimeDisplay => Start.ToString("yyyy-MM-dd HH:mm");
        public string EndTimeDisplay => End.ToString("HH:mm");
        public string TimeRangeDisplay => $"{StartTimeDisplay} - {EndTimeDisplay}";
        public int DurationMinutes => (int)(End - Start).TotalMinutes;
        public string DurationDisplay => $"{DurationMinutes} min";
        public AppointmentStatus Status => _appointment.Status;
        public string StatusDisplay => Status.ToString();
        public Color StatusColor => Status switch
        {
            AppointmentStatus.Scheduled => Color.FromArgb("#4CAF50"),   // Green
            AppointmentStatus.Tentative => Color.FromArgb("#2196F3"),   // Blue
            AppointmentStatus.InProgress => Color.FromArgb("#FF9800"),  // Orange
            AppointmentStatus.Completed => Color.FromArgb("#9E9E9E"),   // Gray
            AppointmentStatus.Cancelled => Color.FromArgb("#F44336"),   // Red
            AppointmentStatus.NoShow => Color.FromArgb("#F44336"),      // Red
            AppointmentStatus.Rescheduled => Color.FromArgb("#9C27B0"), // Purple
            _ => Color.FromArgb("#757575")                              // Default gray
        };

        public string PatientName
        {
            get
            {
                var patient = _profileRegistry.GetProfileById(_appointment.PatientId) as PatientProfile;
                return patient?.Name ?? "Unknown Patient";
            }
        }

        public string PhysicianName
        {
            get
            {
                var physician = _profileRegistry.GetProfileById(_appointment.PhysicianId) as PhysicianProfile;
                return physician != null ? $"Dr. {physician.Name}" : "Unknown Physician";
            }
        }

        public string Reason => _appointment.ReasonForVisit ?? "General Consultation";
        public string Summary => $"{PatientName} with {PhysicianName}";
    }
}
