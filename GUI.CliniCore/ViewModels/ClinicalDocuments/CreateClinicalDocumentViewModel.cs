using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Clinical;
using Core.CliniCore.Domain.ClinicalDocumentation;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Service;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;
using GUI.CliniCore.ViewModels.Base;
using System.Collections.ObjectModel;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels.ClinicalDocuments;

/// <summary>
/// ViewModel for creating a new Clinical Document.
/// Separated from Edit to avoid race conditions when navigating after creation.
/// </summary>
[QueryProperty(nameof(PatientIdString), "patientId")]
[QueryProperty(nameof(PhysicianIdString), "physicianId")]
[QueryProperty(nameof(AppointmentIdString), "appointmentId")]
public partial class CreateClinicalDocumentViewModel : BaseViewModel
{
    private readonly CommandFactory _commandFactory;
    private readonly CommandInvoker _commandInvoker;
    private readonly INavigationService _navigationService;
    private readonly SessionManager _sessionManager;
    private readonly ProfileService _profileService;
    private readonly SchedulerService _schedulerService;

    private Guid? _patientId;
    private Guid? _physicianId;
    private Guid? _appointmentId;

    #region Properties

    private string _chiefComplaint = string.Empty;
    public string ChiefComplaint
    {
        get => _chiefComplaint;
        set
        {
            if (SetProperty(ref _chiefComplaint, value))
                (CreateCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    #endregion

    #region Patient/Physician/Appointment Selection

    public ObservableCollection<PatientDisplayModel> AvailablePatients { get; } = new();
    public ObservableCollection<PhysicianDisplayModel> AvailablePhysicians { get; } = new();
    public ObservableCollection<AppointmentDisplayModel> AvailableAppointments { get; } = new();

    private PatientDisplayModel? _selectedPatient;
    public PatientDisplayModel? SelectedPatient
    {
        get => _selectedPatient;
        set
        {
            if (SetProperty(ref _selectedPatient, value))
            {
                _patientId = value?.Id;
                OnPropertyChanged(nameof(PatientInfo));
                LoadAppointments();
                (CreateCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    private PhysicianDisplayModel? _selectedPhysician;
    public PhysicianDisplayModel? SelectedPhysician
    {
        get => _selectedPhysician;
        set
        {
            if (SetProperty(ref _selectedPhysician, value))
            {
                _physicianId = value?.Id;
                OnPropertyChanged(nameof(PhysicianInfo));
                LoadAppointments();
                (CreateCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    private AppointmentDisplayModel? _selectedAppointment;
    public AppointmentDisplayModel? SelectedAppointment
    {
        get => _selectedAppointment;
        set
        {
            if (SetProperty(ref _selectedAppointment, value))
            {
                _appointmentId = value?.Id;

                // Auto-populate ChiefComplaint from appointment's ReasonForVisit
                if (value != null && !string.IsNullOrWhiteSpace(value.ReasonForVisit) &&
                    string.IsNullOrWhiteSpace(ChiefComplaint))
                {
                    ChiefComplaint = value.ReasonForVisit;
                }

                (CreateCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string PatientInfo => SelectedPatient?.DisplayName ?? "Select a patient";

    public string PhysicianInfo => SelectedPhysician?.DisplayName ?? "Select a physician";

    #endregion

    #region Query Properties (Navigation Parameters)

    public string PatientIdString
    {
        set
        {
            if (Guid.TryParse(value, out var guid))
            {
                _patientId = guid;
                if (AvailablePatients.Count > 0)
                    SelectedPatient = AvailablePatients.FirstOrDefault(p => p.Id == guid);
                else
                    LoadSelectionLists();
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
                if (AvailablePhysicians.Count > 0)
                    SelectedPhysician = AvailablePhysicians.FirstOrDefault(p => p.Id == guid);
                else
                    LoadSelectionLists();
            }
        }
    }

    public string AppointmentIdString
    {
        set
        {
            if (Guid.TryParse(value, out var guid))
            {
                _appointmentId = guid;
                if (AvailableAppointments.Count > 0)
                    SelectedAppointment = AvailableAppointments.FirstOrDefault(a => a.Id == guid);
            }
        }
    }

    #endregion

    #region Commands

    public MauiCommand CreateCommand { get; }
    public MauiCommand BackCommand { get; }

    #endregion

    public CreateClinicalDocumentViewModel(
        CommandFactory commandFactory,
        CommandInvoker commandInvoker,
        INavigationService navigationService,
        SessionManager sessionManager,
        ProfileService profileService,
        SchedulerService schedulerService)
    {
        _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
        _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
        _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));

        Title = "New Clinical Document";

        CreateCommand = new AsyncRelayCommand(
            execute: ExecuteCreateAsync,
            canExecute: CanCreate
        );

        BackCommand = new AsyncRelayCommand(async () => await _navigationService.GoBackAsync());

        // Load selection lists
        LoadSelectionLists();
    }

    private bool CanCreate() =>
        !string.IsNullOrWhiteSpace(ChiefComplaint) &&
        _patientId.HasValue &&
        _physicianId.HasValue &&
        _appointmentId.HasValue;

    private async Task ExecuteCreateAsync()
    {
        if (!_patientId.HasValue || !_physicianId.HasValue || !_appointmentId.HasValue)
        {
            SetValidationError("Patient, Physician, and Appointment are required");
            return;
        }

        try
        {
            var command = _commandFactory.CreateCommand(CreateClinicalDocumentCommand.Key);
            var parameters = new CommandParameters()
                .SetParameter(CreateClinicalDocumentCommand.Parameters.PatientId, _patientId.Value)
                .SetParameter(CreateClinicalDocumentCommand.Parameters.PhysicianId, _physicianId.Value)
                .SetParameter(CreateClinicalDocumentCommand.Parameters.AppointmentId, _appointmentId.Value)
                .SetParameter(CreateClinicalDocumentCommand.Parameters.ChiefComplaint, ChiefComplaint);

            var result = _commandInvoker.Execute(command!, parameters, _sessionManager.CurrentSession);

            if (result.Success && result.Data is ClinicalDocument newDocument)
            {
                ClearValidation();
                // Navigate to DETAIL page (not edit) - let them view the created document
                // then choose to edit from there
                await _navigationService.NavigateToAsync($"ClinicalDocumentDetailPage?documentId={newDocument.Id}");
            }
            else
            {
                SetValidationError(result.Message ?? "Failed to create document");
            }
        }
        catch (Exception ex)
        {
            SetValidationError("Error creating document", ex);
        }
    }

    private void LoadSelectionLists()
    {
        // Load patients as display models
        AvailablePatients.Clear();
        foreach (var patient in _profileService.GetAllPatients())
        {
            AvailablePatients.Add(new PatientDisplayModel(patient));
        }

        // Load physicians as display models
        AvailablePhysicians.Clear();
        foreach (var physician in _profileService.GetAllPhysicians())
        {
            AvailablePhysicians.Add(new PhysicianDisplayModel(physician));
        }

        // Set pre-selected values if provided
        if (_patientId.HasValue)
            SelectedPatient = AvailablePatients.FirstOrDefault(p => p.Id == _patientId.Value);
        if (_physicianId.HasValue)
            SelectedPhysician = AvailablePhysicians.FirstOrDefault(p => p.Id == _physicianId.Value);

        // Load appointments if patient and physician selected
        LoadAppointments();
    }

    private void LoadAppointments()
    {
        AvailableAppointments.Clear();

        if (!_patientId.HasValue || !_physicianId.HasValue) return;

        // Get appointments for this patient-physician pair that don't have a document yet
        var allAppointments = _schedulerService.GetPatientAppointments(_patientId.Value);
        var eligibleAppointments = allAppointments
            .Where(a => a.PhysicianId == _physicianId.Value && !a.ClinicalDocumentId.HasValue)
            .OrderByDescending(a => a.Start);

        foreach (var appointment in eligibleAppointments)
        {
            AvailableAppointments.Add(new AppointmentDisplayModel(appointment));
        }

        // Auto-select if only one option
        if (AvailableAppointments.Count == 1)
        {
            SelectedAppointment = AvailableAppointments[0];
        }
        else if (_appointmentId.HasValue)
        {
            SelectedAppointment = AvailableAppointments.FirstOrDefault(a => a.Id == _appointmentId.Value);
        }
    }
}
