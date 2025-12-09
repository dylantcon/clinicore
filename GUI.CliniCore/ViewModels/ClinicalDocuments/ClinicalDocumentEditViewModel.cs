using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Clinical;
using Core.CliniCore.Domain.ClinicalDocumentation;
using Core.CliniCore.Domain.ClinicalDocumentation.ClinicalEntries;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.Scheduling;
using Core.CliniCore.Service;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;
using GUI.CliniCore.ViewModels.Base;
using System.Collections.ObjectModel;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels.ClinicalDocuments;

/// <summary>
/// ViewModel for Clinical Document Edit/Create page.
/// Uses FormModels for form state - ViewModel is a thin binding/coordination layer.
/// </summary>
[QueryProperty(nameof(DocumentIdString), "documentId")]
[QueryProperty(nameof(PatientIdString), "patientId")]
[QueryProperty(nameof(PhysicianIdString), "physicianId")]
[QueryProperty(nameof(AppointmentIdString), "appointmentId")]
public partial class ClinicalDocumentEditViewModel : BaseViewModel
{
    private readonly CommandFactory _commandFactory;
    private readonly CommandInvoker _commandInvoker;
    private readonly INavigationService _navigationService;
    private readonly SessionManager _sessionManager;
    private readonly ProfileService _profileService;
    private readonly SchedulerService _schedulerService;

    private Guid? _documentId;
    private Guid? _patientId;
    private Guid? _physicianId;
    private Guid? _appointmentId;
    private ClinicalDocument? _document;

    #region Form Models - Single source of truth for form state

    public NewObservationForm SubjectiveObservationForm { get; } = new();
    public NewObservationForm ObjectiveObservationForm { get; } = new() { Type = ObservationType.PhysicalExam };
    public NewAssessmentForm AssessmentForm { get; } = new();
    public NewDiagnosisForm DiagnosisForm { get; } = new();
    public NewPlanForm PlanForm { get; } = new();
    public NewPrescriptionForm PrescriptionForm { get; } = new();

    #endregion

    #region Entry Collections

    public ObservableCollection<ObservationDisplayModel> SubjectiveObservations { get; } = new();
    public ObservableCollection<ObservationDisplayModel> ObjectiveObservations { get; } = new();
    public ObservableCollection<AssessmentDisplayModel> Assessments { get; } = new();
    public ObservableCollection<DiagnosisDisplayModel> Diagnoses { get; } = new();
    public ObservableCollection<PlanDisplayModel> Plans { get; } = new();
    public ObservableCollection<PrescriptionDisplayModel> Prescriptions { get; } = new();

    // Types categorized as subjective (patient-reported)
    private static readonly ObservationType[] SubjectiveTypes =
    [
        ObservationType.ChiefComplaint,
        ObservationType.HistoryOfPresentIllness,
        ObservationType.SocialHistory,
        ObservationType.FamilyHistory,
        ObservationType.Allergy
    ];

    #endregion

    #region Mode & Display Properties

    public bool IsCreateMode => !_documentId.HasValue || _documentId.Value == Guid.Empty;
    public bool IsEditMode => !IsCreateMode;
    public string SaveButtonText => IsCreateMode ? "Create Document" : "Save Changes";

    private string _chiefComplaint = string.Empty;
    public string ChiefComplaint
    {
        get => _chiefComplaint;
        set
        {
            if (SetProperty(ref _chiefComplaint, value))
                (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    public string PatientInfo => GetPatientDisplayInfo();
    public string PhysicianInfo => GetPhysicianDisplayInfo();

    // Available diagnoses for prescription linking
    public ObservableCollection<DiagnosisDisplayModel> AvailableDiagnoses => Diagnoses;

    #endregion

    #region Patient/Physician/Appointment Selection (Create Mode)

    public ObservableCollection<PatientProfile> AvailablePatients { get; } = new();
    public ObservableCollection<PhysicianProfile> AvailablePhysicians { get; } = new();
    public ObservableCollection<AppointmentDisplayModel> AvailableAppointments { get; } = new();

    private PatientProfile? _selectedPatient;
    public PatientProfile? SelectedPatient
    {
        get => _selectedPatient;
        set
        {
            if (SetProperty(ref _selectedPatient, value))
            {
                _patientId = value?.Id;
                OnPropertyChanged(nameof(PatientInfo));
                LoadAppointments();
                (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    private PhysicianProfile? _selectedPhysician;
    public PhysicianProfile? SelectedPhysician
    {
        get => _selectedPhysician;
        set
        {
            if (SetProperty(ref _selectedPhysician, value))
            {
                _physicianId = value?.Id;
                OnPropertyChanged(nameof(PhysicianInfo));
                LoadAppointments();
                (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
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

                (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    #endregion

    #region Query Properties (Navigation Parameters)

    public string DocumentIdString
    {
        set
        {
            if (Guid.TryParse(value, out var guid))
            {
                _documentId = guid;
                LoadDocumentCommand.Execute(null);
            }
        }
    }

    public string PatientIdString
    {
        set
        {
            if (Guid.TryParse(value, out var guid))
            {
                _patientId = guid;
                if (AvailablePatients.Count > 0)
                    SelectedPatient = AvailablePatients.FirstOrDefault(p => p.Id == guid);
                OnPropertyChanged(nameof(PatientInfo));
                (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                LoadAppointments();
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
                OnPropertyChanged(nameof(PhysicianInfo));
                (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                LoadAppointments();
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

    public MauiCommand LoadDocumentCommand { get; }
    public MauiCommand SaveCommand { get; }
    public MauiCommand FinalizeCommand { get; }
    public MauiCommand AddSubjectiveObservationCmd { get; }
    public MauiCommand AddObjectiveObservationCmd { get; }
    public MauiCommand AddAssessmentCmd { get; }
    public MauiCommand AddDiagnosisCmd { get; }
    public MauiCommand AddPlanCmd { get; }
    public MauiCommand AddPrescriptionCmd { get; }
    public MauiCommand BackCommand { get; }

    #endregion

    public ClinicalDocumentEditViewModel(
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

        Title = "Clinical Document";

        // Initialize commands
        LoadDocumentCommand = new MauiCommandAdapter(
            _commandInvoker,
            _commandFactory.CreateCommand(ViewClinicalDocumentCommand.Key)!,
            parameterBuilder: () => new CommandParameters()
                .SetParameter(ViewClinicalDocumentCommand.Parameters.DocumentId, _documentId),
            sessionProvider: () => _sessionManager.CurrentSession,
            resultHandler: HandleLoadResult);

        SaveCommand = new AsyncRelayCommand(ExecuteSaveAsync, CanSave);
        FinalizeCommand = new AsyncRelayCommand(ExecuteFinalizeAsync, () => _document != null && !_document.IsCompleted && IsEditMode);

        AddSubjectiveObservationCmd = new RelayCommand(
            () => ExecuteAddObservation(SubjectiveObservationForm),
            () => _document != null && SubjectiveObservationForm.IsValid);

        AddObjectiveObservationCmd = new RelayCommand(
            () => ExecuteAddObservation(ObjectiveObservationForm),
            () => _document != null && ObjectiveObservationForm.IsValid);

        AddAssessmentCmd = new RelayCommand(ExecuteAddAssessment, () => _document != null && AssessmentForm.IsValid);
        AddDiagnosisCmd = new RelayCommand(ExecuteAddDiagnosis, () => _document != null && DiagnosisForm.IsValid);
        AddPlanCmd = new RelayCommand(ExecuteAddPlan, () => _document != null && PlanForm.IsValid);
        AddPrescriptionCmd = new RelayCommand(ExecuteAddPrescription, () => _document != null && PrescriptionForm.IsValid);

        BackCommand = new AsyncRelayCommand(async () =>
        {
            if (_documentId.HasValue && _documentId.Value != Guid.Empty)
                await _navigationService.NavigateToAsync($"ClinicalDocumentDetailPage?documentId={_documentId.Value}");
            else
                await _navigationService.NavigateToAsync("ClinicalDocumentListPage");
        });

        // Wire up form model property changes to refresh CanExecute
        SubjectiveObservationForm.PropertyChanged += (_, _) => (AddSubjectiveObservationCmd as RelayCommand)?.RaiseCanExecuteChanged();
        ObjectiveObservationForm.PropertyChanged += (_, _) => (AddObjectiveObservationCmd as RelayCommand)?.RaiseCanExecuteChanged();
        AssessmentForm.PropertyChanged += (_, _) => (AddAssessmentCmd as RelayCommand)?.RaiseCanExecuteChanged();
        DiagnosisForm.PropertyChanged += (_, _) => (AddDiagnosisCmd as RelayCommand)?.RaiseCanExecuteChanged();
        PlanForm.PropertyChanged += (_, _) => (AddPlanCmd as RelayCommand)?.RaiseCanExecuteChanged();
        PrescriptionForm.PropertyChanged += (_, _) => (AddPrescriptionCmd as RelayCommand)?.RaiseCanExecuteChanged();

        // Load available patients/physicians for create mode
        LoadPatientsAndPhysicians();
    }

    #region Data Loading

    private void LoadPatientsAndPhysicians()
    {
        foreach (var patient in _profileService.GetAllProfiles().OfType<PatientProfile>())
            AvailablePatients.Add(patient);

        foreach (var physician in _profileService.GetAllProfiles().OfType<PhysicianProfile>())
            AvailablePhysicians.Add(physician);

        if (_patientId.HasValue)
            SelectedPatient = AvailablePatients.FirstOrDefault(p => p.Id == _patientId.Value);
        if (_physicianId.HasValue)
            SelectedPhysician = AvailablePhysicians.FirstOrDefault(p => p.Id == _physicianId.Value);
    }

    private void LoadAppointments()
    {
        AvailableAppointments.Clear();

        if (!_patientId.HasValue || !_physicianId.HasValue)
            return;

        var patientAppointments = _schedulerService.GetPatientAppointments(_patientId.Value);
        var availableAppts = patientAppointments
            .Where(a => a.PhysicianId == _physicianId.Value &&
                       a.Status == AppointmentStatus.Scheduled &&
                       !a.ClinicalDocumentId.HasValue)
            .OrderBy(a => a.Start);

        var patient = _profileService.GetProfileById(_patientId.Value) as PatientProfile;
        var physician = _profileService.GetProfileById(_physicianId.Value) as PhysicianProfile;

        foreach (var appt in availableAppts)
        {
            AvailableAppointments.Add(new AppointmentDisplayModel
            {
                Id = appt.Id,
                Start = appt.Start,
                End = appt.End,
                PatientName = patient?.GetValue<string>(CommonEntryType.Name.GetKey()) ?? "Unknown",
                PhysicianName = physician?.GetValue<string>(CommonEntryType.Name.GetKey()) ?? "Unknown",
                AppointmentType = appt.AppointmentType,
                ReasonForVisit = appt.ReasonForVisit ?? "N/A"
            });
        }

        if (_appointmentId.HasValue)
            SelectedAppointment = AvailableAppointments.FirstOrDefault(a => a.Id == _appointmentId.Value);
    }

    private void HandleLoadResult(CommandResult result)
    {
        if (result.Success && result.Data is ClinicalDocument document)
        {
            _document = document;
            _patientId = document.PatientId;
            _physicianId = document.PhysicianId;
            ChiefComplaint = document.ChiefComplaint ?? string.Empty;

            LoadEntriesFromDocument(document);

            OnPropertyChanged(nameof(PatientInfo));
            OnPropertyChanged(nameof(PhysicianInfo));
            OnPropertyChanged(nameof(IsCreateMode));
            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(SaveButtonText));

            Title = "Edit Clinical Document";
            ClearValidation();
        }
        else
        {
            Title = "Create Clinical Document";
        }
    }

    private void LoadEntriesFromDocument(ClinicalDocument document)
    {
        SubjectiveObservations.Clear();
        ObjectiveObservations.Clear();
        foreach (var obs in document.GetObservations())
        {
            var displayModel = new ObservationDisplayModel
            {
                Id = obs.Id,
                Type = obs.Type,
                Content = obs.Content,
                BodySystem = obs.BodySystem,
                IsAbnormal = obs.IsAbnormal,
                Severity = obs.Severity,
                NumericValue = obs.NumericValue,
                Unit = obs.Unit
            };

            if (SubjectiveTypes.Contains(obs.Type))
                SubjectiveObservations.Add(displayModel);
            else
                ObjectiveObservations.Add(displayModel);
        }

        Assessments.Clear();
        foreach (var assessment in document.GetAssessments())
        {
            Assessments.Add(new AssessmentDisplayModel
            {
                Id = assessment.Id,
                Content = assessment.Content,
                Condition = assessment.Condition,
                Prognosis = assessment.Prognosis,
                Confidence = assessment.Confidence,
                Severity = assessment.Severity,
                RequiresImmediateAction = assessment.RequiresImmediateAction
            });
        }

        Diagnoses.Clear();
        foreach (var diagnosis in document.GetDiagnoses())
        {
            Diagnoses.Add(new DiagnosisDisplayModel
            {
                Id = diagnosis.Id,
                Content = diagnosis.Content,
                ICD10Code = diagnosis.ICD10Code,
                Type = diagnosis.Type,
                Status = diagnosis.Status,
                Severity = diagnosis.Severity,
                IsPrimary = diagnosis.IsPrimary,
                OnsetDate = diagnosis.OnsetDate
            });
        }

        Plans.Clear();
        foreach (var plan in document.GetPlans())
        {
            Plans.Add(new PlanDisplayModel
            {
                Id = plan.Id,
                Content = plan.Content,
                Type = plan.Type,
                Priority = plan.Priority,
                Severity = plan.Severity,
                TargetDate = plan.TargetDate,
                FollowUpInstructions = plan.FollowUpInstructions,
                IsCompleted = plan.IsCompleted
            });
        }

        Prescriptions.Clear();
        foreach (var rx in document.GetPrescriptions())
        {
            Prescriptions.Add(new PrescriptionDisplayModel
            {
                Id = rx.Id,
                MedicationName = rx.MedicationName,
                Dosage = rx.Dosage,
                Frequency = rx.Frequency,
                Route = rx.Route,
                Duration = rx.Duration,
                Refills = rx.Refills,
                GenericAllowed = rx.GenericAllowed,
                DEASchedule = rx.DEASchedule,
                Instructions = rx.Instructions,
                Severity = rx.Severity,
                DiagnosisId = rx.DiagnosisId
            });
        }
    }

    #endregion

    #region Command Execution

    private bool CanSave() => IsCreateMode
        ? !string.IsNullOrWhiteSpace(ChiefComplaint) && _patientId.HasValue && _physicianId.HasValue && _appointmentId.HasValue
        : !string.IsNullOrWhiteSpace(ChiefComplaint);

    private async Task ExecuteSaveAsync()
    {
        try
        {
            if (IsCreateMode)
                await CreateDocumentAsync();
            else
                await UpdateDocumentAsync();
        }
        catch (Exception ex)
        {
            SetValidationError("Error saving document", ex);
        }
    }

    private async Task CreateDocumentAsync()
    {
        if (!_patientId.HasValue || !_physicianId.HasValue || !_appointmentId.HasValue)
        {
            SetValidationError("Patient, Physician, and Appointment are required");
            return;
        }

        var command = _commandFactory.CreateCommand(CreateClinicalDocumentCommand.Key);
        var parameters = new CommandParameters()
            .SetParameter(CreateClinicalDocumentCommand.Parameters.PatientId, _patientId.Value)
            .SetParameter(CreateClinicalDocumentCommand.Parameters.PhysicianId, _physicianId.Value)
            .SetParameter(CreateClinicalDocumentCommand.Parameters.AppointmentId, _appointmentId.Value)
            .SetParameter(CreateClinicalDocumentCommand.Parameters.ChiefComplaint, ChiefComplaint);

        var result = _commandInvoker.Execute(command!, parameters, _sessionManager.CurrentSession);

        if (result.Success && result.Data is ClinicalDocument newDocument)
        {
            _document = newDocument;
            _documentId = newDocument.Id;
            await _navigationService.NavigateToAsync($"ClinicalDocumentEditPage?documentId={_documentId.Value}");
        }
        else
        {
            SetValidationError(result.Message ?? "Failed to create document");
        }
    }

    private async Task UpdateDocumentAsync()
    {
        if (_document == null || !_documentId.HasValue) return;

        if (_document.ChiefComplaint != ChiefComplaint)
        {
            var command = _commandFactory.CreateCommand(UpdateClinicalDocumentCommand.Key);
            var parameters = new CommandParameters()
                .SetParameter(UpdateClinicalDocumentCommand.Parameters.DocumentId, _documentId.Value)
                .SetParameter(UpdateClinicalDocumentCommand.Parameters.ChiefComplaint, ChiefComplaint);

            var result = _commandInvoker.Execute(command!, parameters, _sessionManager.CurrentSession);
            if (!result.Success)
            {
                SetValidationError(result.Message ?? "Failed to update document");
                return;
            }
        }

        await _navigationService.NavigateToAsync($"ClinicalDocumentDetailPage?documentId={_documentId.Value}");
    }

    private async Task ExecuteFinalizeAsync()
    {
        if (_document == null || !_documentId.HasValue) return;

        if (_document.IsCompleted)
        {
            SetValidationError("Document is already finalized");
            return;
        }

        var validationErrors = _document.GetValidationErrors();
        if (validationErrors.Any())
        {
            ValidationErrors.Clear();
            ValidationErrors.Add("Document cannot be finalized. Missing required entries:");
            foreach (var error in validationErrors)
                ValidationErrors.Add($"  - {error}");
            return;
        }

        var command = _commandFactory.CreateCommand(UpdateClinicalDocumentCommand.Key);
        var parameters = new CommandParameters()
            .SetParameter(UpdateClinicalDocumentCommand.Parameters.DocumentId, _documentId.Value)
            .SetParameter(UpdateClinicalDocumentCommand.Parameters.Complete, true);

        var result = _commandInvoker.Execute(command!, parameters, _sessionManager.CurrentSession);

        if (result.Success)
        {
            await _navigationService.NavigateToAsync($"ClinicalDocumentDetailPage?documentId={_documentId.Value}");
        }
        else
        {
            SetValidationError(result.Message ?? "Failed to finalize document");
        }
    }

    private void ExecuteAddObservation(NewObservationForm form)
    {
        if (_document == null || !_documentId.HasValue) return;

        var command = _commandFactory.CreateCommand(AddObservationCommand.Key);
        var parameters = new CommandParameters()
            .SetParameter(AddObservationCommand.Parameters.DocumentId, _documentId.Value)
            .SetParameter(AddObservationCommand.Parameters.Observation, form.Content)
            .SetParameter(AddObservationCommand.Parameters.ObservationType, form.Type)
            .SetParameter(AddObservationCommand.Parameters.Severity, form.Severity);

        if (form.BodySystem.HasValue)
            parameters.SetParameter(AddObservationCommand.Parameters.BodySystem, form.BodySystem.Value);
        if (form.IsAbnormal)
            parameters.SetParameter(AddObservationCommand.Parameters.IsAbnormal, form.IsAbnormal);
        if (form.NumericValue.HasValue)
            parameters.SetParameter(AddObservationCommand.Parameters.NumericValue, form.NumericValue.Value);
        if (!string.IsNullOrWhiteSpace(form.Unit))
            parameters.SetParameter(AddObservationCommand.Parameters.Unit, form.Unit);

        var result = _commandInvoker.Execute(command!, parameters, _sessionManager.CurrentSession);

        if (result.Success && result.Data is ObservationEntry entry)
        {
            var displayModel = new ObservationDisplayModel
            {
                Id = entry.Id,
                Type = entry.Type,
                Content = entry.Content,
                BodySystem = entry.BodySystem,
                IsAbnormal = entry.IsAbnormal,
                Severity = entry.Severity,
                NumericValue = entry.NumericValue,
                Unit = entry.Unit
            };

            // Add to the appropriate collection based on observation type
            if (SubjectiveTypes.Contains(entry.Type))
                SubjectiveObservations.Add(displayModel);
            else
                ObjectiveObservations.Add(displayModel);

            form.Clear();
            ClearValidation();
        }
        else
        {
            SetValidationError(result.Message ?? "Failed to add observation");
        }
    }

    private void ExecuteAddAssessment()
    {
        if (_document == null || !_documentId.HasValue) return;

        var command = _commandFactory.CreateCommand(AddAssessmentCommand.Key);
        var parameters = new CommandParameters()
            .SetParameter(AddAssessmentCommand.Parameters.DocumentId, _documentId.Value)
            .SetParameter(AddAssessmentCommand.Parameters.ClinicalImpression, AssessmentForm.Content)
            .SetParameter(AddAssessmentCommand.Parameters.Condition, AssessmentForm.Condition)
            .SetParameter(AddAssessmentCommand.Parameters.Prognosis, AssessmentForm.Prognosis)
            .SetParameter(AddAssessmentCommand.Parameters.Confidence, AssessmentForm.Confidence)
            .SetParameter(AddAssessmentCommand.Parameters.Severity, AssessmentForm.Severity)
            .SetParameter(AddAssessmentCommand.Parameters.RequiresImmediateAction, AssessmentForm.RequiresImmediateAction);

        var result = _commandInvoker.Execute(command!, parameters, _sessionManager.CurrentSession);

        if (result.Success && result.Data is AssessmentEntry entry)
        {
            Assessments.Add(new AssessmentDisplayModel
            {
                Id = entry.Id,
                Content = entry.Content,
                Condition = entry.Condition,
                Prognosis = entry.Prognosis,
                Confidence = entry.Confidence,
                Severity = entry.Severity,
                RequiresImmediateAction = entry.RequiresImmediateAction
            });
            AssessmentForm.Clear();
            ClearValidation();
        }
        else
        {
            SetValidationError(result.Message ?? "Failed to add assessment");
        }
    }

    private void ExecuteAddDiagnosis()
    {
        if (_document == null || !_documentId.HasValue) return;

        var command = _commandFactory.CreateCommand(AddDiagnosisCommand.Key);
        var parameters = new CommandParameters()
            .SetParameter(AddDiagnosisCommand.Parameters.DocumentId, _documentId.Value)
            .SetParameter(AddDiagnosisCommand.Parameters.DiagnosisDescription, DiagnosisForm.Content)
            .SetParameter(AddDiagnosisCommand.Parameters.DiagnosisType, DiagnosisForm.Type)
            .SetParameter(AddDiagnosisCommand.Parameters.DiagnosisStatus, DiagnosisForm.Status)
            .SetParameter(AddDiagnosisCommand.Parameters.Severity, DiagnosisForm.Severity)
            .SetParameter(AddDiagnosisCommand.Parameters.IsPrimary, DiagnosisForm.IsPrimary);

        if (!string.IsNullOrWhiteSpace(DiagnosisForm.ICD10Code))
            parameters.SetParameter(AddDiagnosisCommand.Parameters.ICD10Code, DiagnosisForm.ICD10Code);
        if (DiagnosisForm.OnsetDate.HasValue)
            parameters.SetParameter(AddDiagnosisCommand.Parameters.OnsetDate, DiagnosisForm.OnsetDate.Value);

        var result = _commandInvoker.Execute(command!, parameters, _sessionManager.CurrentSession);

        if (result.Success && result.Data is DiagnosisEntry entry)
        {
            Diagnoses.Add(new DiagnosisDisplayModel
            {
                Id = entry.Id,
                Content = entry.Content,
                ICD10Code = entry.ICD10Code,
                Type = entry.Type,
                Status = entry.Status,
                Severity = entry.Severity,
                IsPrimary = entry.IsPrimary,
                OnsetDate = entry.OnsetDate
            });
            DiagnosisForm.Clear();
            ClearValidation();
            OnPropertyChanged(nameof(AvailableDiagnoses));
        }
        else
        {
            SetValidationError(result.Message ?? "Failed to add diagnosis");
        }
    }

    private void ExecuteAddPlan()
    {
        if (_document == null || !_documentId.HasValue) return;

        var command = _commandFactory.CreateCommand(AddPlanCommand.Key);
        var parameters = new CommandParameters()
            .SetParameter(AddPlanCommand.Parameters.DocumentId, _documentId.Value)
            .SetParameter(AddPlanCommand.Parameters.PlanDescription, PlanForm.Content)
            .SetParameter(AddPlanCommand.Parameters.PlanType, PlanForm.Type)
            .SetParameter(AddPlanCommand.Parameters.Priority, PlanForm.Priority)
            .SetParameter(AddPlanCommand.Parameters.Severity, PlanForm.Severity);

        if (PlanForm.TargetDate.HasValue)
            parameters.SetParameter(AddPlanCommand.Parameters.TargetDate, PlanForm.TargetDate.Value);
        if (!string.IsNullOrWhiteSpace(PlanForm.FollowUpInstructions))
            parameters.SetParameter(AddPlanCommand.Parameters.FollowUpInstructions, PlanForm.FollowUpInstructions);

        var result = _commandInvoker.Execute(command!, parameters, _sessionManager.CurrentSession);

        if (result.Success && result.Data is PlanEntry entry)
        {
            Plans.Add(new PlanDisplayModel
            {
                Id = entry.Id,
                Content = entry.Content,
                Type = entry.Type,
                Priority = entry.Priority,
                Severity = entry.Severity,
                TargetDate = entry.TargetDate,
                FollowUpInstructions = entry.FollowUpInstructions,
                IsCompleted = entry.IsCompleted
            });
            PlanForm.Clear();
            ClearValidation();
        }
        else
        {
            SetValidationError(result.Message ?? "Failed to add plan");
        }
    }

    private void ExecuteAddPrescription()
    {
        if (_document == null || !_documentId.HasValue) return;

        if (!PrescriptionForm.DiagnosisId.HasValue)
        {
            SetValidationError("Please select a diagnosis for this prescription");
            return;
        }

        var command = _commandFactory.CreateCommand(AddPrescriptionCommand.Key);
        var parameters = new CommandParameters()
            .SetParameter(AddPrescriptionCommand.Parameters.DocumentId, _documentId.Value)
            .SetParameter(AddPrescriptionCommand.Parameters.MedicationName, PrescriptionForm.MedicationName)
            .SetParameter(AddPrescriptionCommand.Parameters.Dosage, PrescriptionForm.Dosage)
            .SetParameter(AddPrescriptionCommand.Parameters.Frequency, PrescriptionForm.Frequency)
            .SetParameter(AddPrescriptionCommand.Parameters.DiagnosisId, PrescriptionForm.DiagnosisId.Value)
            .SetParameter(AddPrescriptionCommand.Parameters.Route, PrescriptionForm.Route)
            .SetParameter(AddPrescriptionCommand.Parameters.Refills, PrescriptionForm.Refills)
            .SetParameter(AddPrescriptionCommand.Parameters.GenericAllowed, PrescriptionForm.GenericAllowed);

        if (!string.IsNullOrWhiteSpace(PrescriptionForm.Duration))
            parameters.SetParameter(AddPrescriptionCommand.Parameters.Duration, PrescriptionForm.Duration);
        if (PrescriptionForm.DEASchedule.HasValue)
            parameters.SetParameter(AddPrescriptionCommand.Parameters.DeaSchedule, PrescriptionForm.DEASchedule.Value);
        if (!string.IsNullOrWhiteSpace(PrescriptionForm.Instructions))
            parameters.SetParameter(AddPrescriptionCommand.Parameters.Instructions, PrescriptionForm.Instructions);

        var result = _commandInvoker.Execute(command!, parameters, _sessionManager.CurrentSession);

        if (result.Success && result.Data is PrescriptionEntry entry)
        {
            Prescriptions.Add(new PrescriptionDisplayModel
            {
                Id = entry.Id,
                MedicationName = entry.MedicationName,
                Dosage = entry.Dosage,
                Frequency = entry.Frequency,
                Route = entry.Route,
                Duration = entry.Duration,
                Refills = entry.Refills,
                GenericAllowed = entry.GenericAllowed,
                DEASchedule = entry.DEASchedule,
                Instructions = entry.Instructions,
                Severity = entry.Severity,
                DiagnosisId = entry.DiagnosisId
            });
            PrescriptionForm.Clear();
            ClearValidation();
        }
        else
        {
            SetValidationError(result.Message ?? "Failed to add prescription");
        }
    }

    #endregion

    #region Helpers

    private string GetPatientDisplayInfo()
    {
        if (_patientId.HasValue)
        {
            var patient = _profileService.GetProfileById(_patientId.Value) as PatientProfile;
            return patient != null
                ? $"Patient: {patient.GetValue<string>(CommonEntryType.Name.GetKey()) ?? "Unknown"}"
                : "Patient: Unknown";
        }
        return "Patient: Not selected";
    }

    private string GetPhysicianDisplayInfo()
    {
        if (_physicianId.HasValue)
        {
            var physician = _profileService.GetProfileById(_physicianId.Value) as PhysicianProfile;
            return physician != null
                ? $"Physician: Dr. {physician.GetValue<string>(CommonEntryType.Name.GetKey()) ?? "Unknown"}"
                : "Physician: Unknown";
        }
        return "Physician: Not selected";
    }

    #endregion
}
