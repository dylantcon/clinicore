using System.Collections.ObjectModel;
using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Scheduling;
using Core.CliniCore.Domain;
using Core.CliniCore.Scheduling;
using Core.CliniCore.Scheduling.Management;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels
{
    /// <summary>
    /// ViewModel for Appointment Edit page
    /// Supports scheduling and rescheduling appointments with validation
    /// </summary>
    [QueryProperty(nameof(AppointmentIdString), "appointmentId")]
    [QueryProperty(nameof(PatientIdString), "patientId")]
    [QueryProperty(nameof(PhysicianIdString), "physicianId")]
    public partial class AppointmentEditViewModel : BaseViewModel
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
                    LoadAppointment();
                }
            }
        }

        public string PatientIdString
        {
            set
            {
                if (Guid.TryParse(value, out var guid) && guid != Guid.Empty)
                {
                    SelectedPatient = AvailablePatients.FirstOrDefault(p => p.Id == guid);
                }
            }
        }

        public string PhysicianIdString
        {
            set
            {
                if (Guid.TryParse(value, out var guid) && guid != Guid.Empty)
                {
                    SelectedPhysician = AvailablePhysicians.FirstOrDefault(p => p.Id == guid);
                }
            }
        }

        public ObservableCollection<PatientPickerModel> AvailablePatients { get; } = new();
        public ObservableCollection<PhysicianPickerModel> AvailablePhysicians { get; } = new();
        public List<int> AvailableDurations { get; } = new List<int> { 15, 30, 60, 90, 120 };

        private PatientPickerModel? _selectedPatient;
        public PatientPickerModel? SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                if (SetProperty(ref _selectedPatient, value))
                {
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private PhysicianPickerModel? _selectedPhysician;
        public PhysicianPickerModel? SelectedPhysician
        {
            get => _selectedPhysician;
            set
            {
                if (SetProperty(ref _selectedPhysician, value))
                {
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private DateTime _selectedDate = DateTime.Today.AddDays(1);
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set => SetProperty(ref _selectedDate, value);
        }

        private TimeSpan _selectedTime = new TimeSpan(9, 0, 0);
        public TimeSpan SelectedTime
        {
            get => _selectedTime;
            set => SetProperty(ref _selectedTime, value);
        }

        private int _durationMinutes = 30;
        public int DurationMinutes
        {
            get => _durationMinutes;
            set => SetProperty(ref _durationMinutes, value);
        }

        private string _reason = "General Consultation";
        public string Reason
        {
            get => _reason;
            set
            {
                if (SetProperty(ref _reason, value))
                {
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public MauiCommand SaveCommand { get; }
        public MauiCommand CancelCommand { get; }
        public MauiCommand BackCommand { get; }

        public AppointmentEditViewModel(
            CommandFactory commandFactory,
            INavigationService navigationService,
            SessionManager sessionManager)
        {
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));

            Title = "Schedule Appointment";

            // Populate pickers
            LoadAvailablePatients();
            LoadAvailablePhysicians();

            // Save command
            SaveCommand = new RelayCommand(
                execute: ExecuteSave,
                canExecute: CanSave
            );

            // Cancel command
            CancelCommand = new AsyncRelayCommand(async () =>
            {
                await _navigationService.NavigateToAsync("AppointmentListPage");
            });

            // Back command - navigate to list
            BackCommand = new AsyncRelayCommand(async () =>
            {
                await _navigationService.NavigateToAsync("AppointmentListPage");
            });
        }

        private void LoadAvailablePatients()
        {
            AvailablePatients.Clear();
            var patients = _profileRegistry.GetAllPatients();
            foreach (var patient in patients.OrderBy(p => p.Name))
            {
                AvailablePatients.Add(new PatientPickerModel
                {
                    Id = patient.Id,
                    Name = patient.Name ?? "Unknown",
                    Display = patient.Name ?? "Unknown"
                });
            }
        }

        private void LoadAvailablePhysicians()
        {
            AvailablePhysicians.Clear();
            var physicians = _profileRegistry.GetAllPhysicians();
            foreach (var physician in physicians.OrderBy(p => p.Name))
            {
                AvailablePhysicians.Add(new PhysicianPickerModel
                {
                    Id = physician.Id,
                    Name = physician.Name ?? "Unknown",
                    Display = $"Dr. {physician.Name ?? "Unknown"}"
                });
            }
        }

        private void LoadAppointment()
        {
            if (!_appointmentId.HasValue) return;

            try
            {
                var allAppointments = _scheduleManager.GetAllAppointments();
                _appointment = allAppointments.FirstOrDefault(a => a.Id == _appointmentId.Value);

                if (_appointment == null)
                {
                    ValidationErrors.Add("Appointment not found");
                    return;
                }

                Title = "Reschedule Appointment";

                // Load existing data
                SelectedPatient = AvailablePatients.FirstOrDefault(p => p.Id == _appointment.PatientId);
                SelectedPhysician = AvailablePhysicians.FirstOrDefault(p => p.Id == _appointment.PhysicianId);
                SelectedDate = _appointment.Start.Date;
                SelectedTime = _appointment.Start.TimeOfDay;
                DurationMinutes = (int)(_appointment.End - _appointment.Start).TotalMinutes;
                Reason = _appointment.ReasonForVisit ?? "General Consultation";
                Notes = _appointment.Notes ?? string.Empty;
            }
            catch (Exception ex)
            {
                ValidationErrors.Add($"Error loading appointment: {ex.Message}");
            }
        }

        private bool CanSave()
        {
            return SelectedPatient != null &&
                   SelectedPhysician != null &&
                   !string.IsNullOrWhiteSpace(Reason);
        }

        private void ExecuteSave()
        {
            if (SelectedPatient == null || SelectedPhysician == null)
            {
                ValidationErrors.Clear();
                ValidationErrors.Add("Please select both patient and physician");
                return;
            }

            try
            {
                // Combine date and time
                var startDateTime = SelectedDate.Date.Add(SelectedTime);

                // Validate business hours (8am-5pm Monday-Friday)
                if (startDateTime.DayOfWeek == DayOfWeek.Saturday ||
                    startDateTime.DayOfWeek == DayOfWeek.Sunday)
                {
                    ValidationErrors.Clear();
                    ValidationErrors.Add("Appointments can only be scheduled Monday through Friday");
                    return;
                }

                var endDateTime = startDateTime.AddMinutes(DurationMinutes);
                if (startDateTime.Hour < 8 || endDateTime.Hour > 17)
                {
                    ValidationErrors.Clear();
                    ValidationErrors.Add("Appointments must be scheduled between 8:00 AM and 5:00 PM");
                    return;
                }

                // Use ScheduleAppointmentCommand or RescheduleAppointmentCommand
                var commandKey = _appointmentId.HasValue
                    ? RescheduleAppointmentCommand.Key
                    : ScheduleAppointmentCommand.Key;

                var coreCommand = _commandFactory.CreateCommand(commandKey);

                var saveCommand = new MauiCommandAdapter(
                    coreCommand!,
                    parameterBuilder: () =>
                    {
                        var parameters = new CommandParameters();

                        if (_appointmentId.HasValue)
                        {
                            // Reschedule - note: RescheduleAppointmentCommand only changes start time, not duration
                            parameters.SetParameter(RescheduleAppointmentCommand.Parameters.AppointmentId, _appointmentId.Value);
                            parameters.SetParameter(RescheduleAppointmentCommand.Parameters.NewDateTime, startDateTime);
                        }
                        else
                        {
                            // New appointment
                            parameters.SetParameter(ScheduleAppointmentCommand.Parameters.PatientId, SelectedPatient.Id);
                            parameters.SetParameter(ScheduleAppointmentCommand.Parameters.PhysicianId, SelectedPhysician.Id);
                            parameters.SetParameter(ScheduleAppointmentCommand.Parameters.StartTime, startDateTime);
                            parameters.SetParameter(ScheduleAppointmentCommand.Parameters.DurationMinutes, DurationMinutes);
                            parameters.SetParameter(ScheduleAppointmentCommand.Parameters.Reason, Reason);
                            if (!string.IsNullOrWhiteSpace(Notes))
                            {
                                parameters.SetParameter(ScheduleAppointmentCommand.Parameters.Notes, Notes);
                            }
                        }

                        return parameters;
                    },
                    sessionProvider: () => _sessionManager.CurrentSession,
                    resultHandler: HandleSaveResult,
                    viewModel: this
                );

                saveCommand.Execute(null);
            }
            catch (Exception ex)
            {
                ValidationErrors.Clear();
                ValidationErrors.Add($"Error saving appointment: {ex.Message}");
            }
        }

        private void HandleSaveResult(CommandResult result)
        {
            if (result.Success)
            {
                // Navigate back to list on success
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await _navigationService.NavigateToAsync("AppointmentListPage");
                });
            }
        }
    }

    /// <summary>
    /// Display model for patient picker
    /// </summary>
    public class PatientPickerModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
    }

    /// <summary>
    /// Display model for physician picker
    /// </summary>
    public class PhysicianPickerModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
    }
}
