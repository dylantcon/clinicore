using System.Collections.ObjectModel;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Service;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;
using GUI.CliniCore.ViewModels.Base;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels.Appointments
{
    /// <summary>
    /// Base class for appointment form ViewModels.
    /// Contains shared properties, picker data, and navigation commands.
    /// Derived classes implement the specific save command logic.
    /// </summary>
    public abstract class AppointmentFormViewModelBase : BaseViewModel
    {
        protected readonly CommandFactory _commandFactory;
        protected readonly INavigationService _navigationService;
        protected readonly SessionManager _sessionManager;
        protected readonly ProfileService _profileRegistry;
        protected readonly SchedulerService _scheduleManager;

        #region Collections

        public ObservableCollection<PatientPickerModel> AvailablePatients { get; } = new();
        public ObservableCollection<PhysicianPickerModel> AvailablePhysicians { get; } = new();
        public List<int> AvailableDurations { get; } = new List<int> { 15, 30, 60, 90, 120 };

        #endregion

        #region Form Properties

        private PatientPickerModel? _selectedPatient;
        public PatientPickerModel? SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                if (SetProperty(ref _selectedPatient, value))
                {
                    (SaveCommand as MauiCommandAdapter)?.RaiseCanExecuteChanged();
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
                    (SaveCommand as MauiCommandAdapter)?.RaiseCanExecuteChanged();
                }
            }
        }

        private DateTime _selectedDate = DateTime.Today.AddDays(1);
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    (SaveCommand as MauiCommandAdapter)?.RaiseCanExecuteChanged();
                }
            }
        }

        private TimeSpan _selectedTime = new TimeSpan(9, 0, 0);
        public TimeSpan SelectedTime
        {
            get => _selectedTime;
            set
            {
                if (SetProperty(ref _selectedTime, value))
                {
                    (SaveCommand as MauiCommandAdapter)?.RaiseCanExecuteChanged();
                }
            }
        }

        private int _durationMinutes = 30;
        public int DurationMinutes
        {
            get => _durationMinutes;
            set
            {
                if (SetProperty(ref _durationMinutes, value))
                {
                    (SaveCommand as MauiCommandAdapter)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _reason = "General Consultation";
        public string Reason
        {
            get => _reason;
            set
            {
                if (SetProperty(ref _reason, value))
                {
                    (SaveCommand as MauiCommandAdapter)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        #endregion

        #region Commands

        private MauiCommandAdapter? _saveCommand;

        /// <summary>
        /// Lazy-initialized save command. Created on first access to ensure
        /// derived class is fully constructed before command creation.
        /// </summary>
        public MauiCommand SaveCommand => _saveCommand ??= CreateSaveCommand();

        /// <summary>
        /// Factory method for derived classes to create their specific save command.
        /// </summary>
        protected abstract MauiCommandAdapter CreateSaveCommand();

        public MauiCommand CancelCommand { get; }
        public MauiCommand BackCommand { get; }

        #endregion

        protected AppointmentFormViewModelBase(
            CommandFactory commandFactory,
            INavigationService navigationService,
            SessionManager sessionManager,
            ProfileService profileService,
            SchedulerService schedulerService)
        {
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _profileRegistry = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _scheduleManager = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));

            // Populate pickers
            LoadAvailablePatients();
            LoadAvailablePhysicians();

            // Navigation commands shared by all derived classes
            CancelCommand = new AsyncRelayCommand(NavigateBackAsync);
            BackCommand = new AsyncRelayCommand(NavigateBackAsync);
        }

        #region Data Loading

        protected void LoadAvailablePatients()
        {
            AvailablePatients.Clear();
            var patients = _profileRegistry.GetAllPatients();
            foreach (var patient in patients.OrderBy(p => p.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty))
            {
                var patientName = patient.GetValue<string>(CommonEntryType.Name.GetKey()) ?? "Unknown";
                AvailablePatients.Add(new PatientPickerModel
                {
                    Id = patient.Id,
                    Name = patientName,
                    Display = patientName
                });
            }
        }

        protected void LoadAvailablePhysicians()
        {
            AvailablePhysicians.Clear();
            var physicians = _profileRegistry.GetAllPhysicians();
            foreach (var physician in physicians.OrderBy(p => p.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty))
            {
                var physicianName = physician.GetValue<string>(CommonEntryType.Name.GetKey()) ?? "Unknown";
                AvailablePhysicians.Add(new PhysicianPickerModel
                {
                    Id = physician.Id,
                    Name = physicianName,
                    Display = $"Dr. {physicianName}"
                });
            }
        }

        #endregion

        #region Navigation

        private async Task NavigateBackAsync()
        {
            await _navigationService.NavigateToAsync("AppointmentListPage");
        }

        #endregion

        #region Command Handling

        /// <summary>
        /// Handles the result of the save command execution.
        /// Navigates back to list on success.
        /// </summary>
        protected void HandleSaveResult(CommandResult result)
        {
            if (result.Success)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await _navigationService.NavigateToAsync("AppointmentListPage");
                });
            }
        }

        /// <summary>
        /// Helper to get the combined start DateTime from selected date and time.
        /// </summary>
        protected DateTime GetStartDateTime() => SelectedDate.Date.Add(SelectedTime);

        #endregion
    }

    #region Picker Models

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

    #endregion
}
