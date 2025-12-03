using Core.CliniCore.ClinicalDoc;
using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Clinical;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Scheduling;
using Core.CliniCore.Services;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;
using System.Collections.ObjectModel;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels
{
    /// <summary>
    /// ViewModel for Clinical Document Edit/Create page
    /// Handles both creating new documents and editing existing ones
    /// Manages 5 entry types: Observation, Assessment, Diagnosis, Plan, Prescription
    /// </summary>
    [QueryProperty(nameof(DocumentIdString), "documentId")]
    [QueryProperty(nameof(PatientIdString), "patientId")]
    [QueryProperty(nameof(PhysicianIdString), "physicianId")]
    [QueryProperty(nameof(AppointmentIdString), "appointmentId")]
    public partial class ClinicalDocumentEditViewModel : BaseViewModel
    {
        private readonly CommandFactory _commandFactory;
        private readonly INavigationService _navigationService;
        private readonly SessionManager _sessionManager;
        private readonly ProfileService _profileRegistry;
        private readonly SchedulerService _scheduleManager;

        private Guid? _documentId;
        private Guid? _patientId;
        private Guid? _physicianId;
        private Guid? _appointmentId;
        private ClinicalDocument? _document;

        // Mode detection
        public bool IsCreateMode => !_documentId.HasValue || _documentId.Value == Guid.Empty;
        public bool IsEditMode => !IsCreateMode;
        public string SaveButtonText => IsCreateMode ? "Create Document" : "Save Changes";

        // Query properties for navigation
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
                    OnPropertyChanged(nameof(PatientInfo));
                    (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
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
                    OnPropertyChanged(nameof(PhysicianInfo));
                    (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
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
                }
            }
        }

        // Document properties
        private string _chiefComplaint = string.Empty;
        public string ChiefComplaint
        {
            get => _chiefComplaint;
            set
            {
                if (SetProperty(ref _chiefComplaint, value))
                {
                    (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        // Display info
        public string PatientInfo
        {
            get
            {
                if (_patientId.HasValue)
                {
                    var patient = _profileRegistry.GetProfileById(_patientId.Value) as PatientProfile;
                    return patient != null ? $"Patient: {patient.Name}" : "Patient: Unknown";
                }
                return "Patient: Not selected";
            }
        }

        public string PhysicianInfo
        {
            get
            {
                if (_physicianId.HasValue)
                {
                    var physician = _profileRegistry.GetProfileById(_physicianId.Value) as PhysicianProfile;
                    return physician != null ? $"Physician: Dr. {physician.Name}" : "Physician: Unknown";
                }
                return "Physician: Not selected";
            }
        }

        // Entry collections
        public ObservableCollection<ObservationDisplayModel> Observations { get; } = new();
        public ObservableCollection<AssessmentDisplayModel> Assessments { get; } = new();
        public ObservableCollection<DiagnosisDisplayModel> Diagnoses { get; } = new();
        public ObservableCollection<PlanDisplayModel> Plans { get; } = new();
        public ObservableCollection<PrescriptionDisplayModel> Prescriptions { get; } = new();

        // New entry forms
        private string _newObservation = string.Empty;
        public string NewObservation
        {
            get => _newObservation;
            set
            {
                if (SetProperty(ref _newObservation, value))
                {
                    (AddObservationCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _newObservationType = ObservationType.HistoryOfPresentIllness.ToString();
        public string NewObservationType
        {
            get => _newObservationType;
            set => SetProperty(ref _newObservationType, value);
        }

        // Objective observation form
        private string _newObjectiveObservation = string.Empty;
        public string NewObjectiveObservation
        {
            get => _newObjectiveObservation;
            set
            {
                if (SetProperty(ref _newObjectiveObservation, value))
                {
                    (AddObjectiveObservationCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _newObjectiveObservationType = ObservationType.PhysicalExam.ToString();
        public string NewObjectiveObservationType
        {
            get => _newObjectiveObservationType;
            set => SetProperty(ref _newObjectiveObservationType, value);
        }

        private string _newClinicalImpression = string.Empty;
        public string NewClinicalImpression
        {
            get => _newClinicalImpression;
            set
            {
                if (SetProperty(ref _newClinicalImpression, value))
                {
                    (AddAssessmentCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _newDiagnosisDescription = string.Empty;
        public string NewDiagnosisDescription
        {
            get => _newDiagnosisDescription;
            set
            {
                if (SetProperty(ref _newDiagnosisDescription, value))
                {
                    (AddDiagnosisCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _newDiagnosisICD10Code = string.Empty;
        public string NewDiagnosisICD10Code
        {
            get => _newDiagnosisICD10Code;
            set => SetProperty(ref _newDiagnosisICD10Code, value);
        }

        private string _newPlanDescription = string.Empty;
        public string NewPlanDescription
        {
            get => _newPlanDescription;
            set
            {
                if (SetProperty(ref _newPlanDescription, value))
                {
                    (AddPlanCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _newPrescriptionMedication = string.Empty;
        public string NewPrescriptionMedication
        {
            get => _newPrescriptionMedication;
            set
            {
                if (SetProperty(ref _newPrescriptionMedication, value))
                {
                    (AddPrescriptionCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _newPrescriptionDosage = string.Empty;
        public string NewPrescriptionDosage
        {
            get => _newPrescriptionDosage;
            set
            {
                if (SetProperty(ref _newPrescriptionDosage, value))
                {
                    (AddPrescriptionCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _newPrescriptionFrequency = string.Empty;
        public string NewPrescriptionFrequency
        {
            get => _newPrescriptionFrequency;
            set
            {
                if (SetProperty(ref _newPrescriptionFrequency, value))
                {
                    (AddPrescriptionCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _newPrescriptionDuration = string.Empty;
        public string NewPrescriptionDuration
        {
            get => _newPrescriptionDuration;
            set => SetProperty(ref _newPrescriptionDuration, value);
        }

        private string _newPrescriptionRoute = "Oral";
        public string NewPrescriptionRoute
        {
            get => _newPrescriptionRoute;
            set => SetProperty(ref _newPrescriptionRoute, value);
        }

        private Guid? _newPrescriptionDiagnosisId;
        public Guid? NewPrescriptionDiagnosisId
        {
            get => _newPrescriptionDiagnosisId;
            set
            {
                if (SetProperty(ref _newPrescriptionDiagnosisId, value))
                {
                    (AddPrescriptionCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private DiagnosisDisplayModel? _selectedDiagnosisForPrescription;
        public DiagnosisDisplayModel? SelectedDiagnosisForPrescription
        {
            get => _selectedDiagnosisForPrescription;
            set
            {
                if (SetProperty(ref _selectedDiagnosisForPrescription, value))
                {
                    NewPrescriptionDiagnosisId = value?.Id;
                }
            }
        }

        // Patient/Physician pickers for create mode
        private ObservableCollection<PatientProfile> _availablePatients = new();
        public ObservableCollection<PatientProfile> AvailablePatients
        {
            get => _availablePatients;
            set => SetProperty(ref _availablePatients, value);
        }

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
                    LoadAppointments(); // Reload appointments when patient changes
                    (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private ObservableCollection<PhysicianProfile> _availablePhysicians = new();
        public ObservableCollection<PhysicianProfile> AvailablePhysicians
        {
            get => _availablePhysicians;
            set => SetProperty(ref _availablePhysicians, value);
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
                    LoadAppointments(); // Reload appointments when physician changes
                    (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        // Appointment picker for create mode
        private ObservableCollection<AppointmentDisplayModel> _availableAppointments = new();
        public ObservableCollection<AppointmentDisplayModel> AvailableAppointments
        {
            get => _availableAppointments;
            set => SetProperty(ref _availableAppointments, value);
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
                    (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        // Enum options for pickers - split into Subjective and Objective
        public List<string> SubjectiveObservationTypes { get; } = new List<string>
        {
            ObservationType.ChiefComplaint.ToString(),
            ObservationType.HistoryOfPresentIllness.ToString(),
            ObservationType.SocialHistory.ToString(),
            ObservationType.FamilyHistory.ToString(),
            ObservationType.Allergy.ToString()
        };

        public List<string> ObjectiveObservationTypes { get; } = new List<string>
        {
            ObservationType.PhysicalExam.ToString(),
            ObservationType.VitalSigns.ToString(),
            ObservationType.LabResult.ToString(),
            ObservationType.ImagingResult.ToString(),
            ObservationType.ReviewOfSystems.ToString()
        };

        // Available diagnoses for prescription linking
        public ObservableCollection<DiagnosisDisplayModel> AvailableDiagnoses => Diagnoses;

        // Commands
        public MauiCommand LoadDocumentCommand { get; }
        public MauiCommand SaveCommand { get; }
        public MauiCommand FinalizeCommand { get; }
        public MauiCommand AddObservationCommand { get; }
        public MauiCommand AddObjectiveObservationCommand { get; }
        public MauiCommand AddAssessmentCommand { get; }
        public MauiCommand AddDiagnosisCommand { get; }
        public MauiCommand AddPlanCommand { get; }
        public MauiCommand AddPrescriptionCommand { get; }
        public MauiCommand BackCommand { get; }

        public ClinicalDocumentEditViewModel(
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

            Title = "Clinical Document";

            // Load command (for edit mode)
            var viewCoreCommand = _commandFactory.CreateCommand(ViewClinicalDocumentCommand.Key);
            LoadDocumentCommand = new MauiCommandAdapter(
                viewCoreCommand!,
                parameterBuilder: BuildLoadParameters,
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleLoadResult,
                viewModel: this
            );

            // Save command
            SaveCommand = new AsyncRelayCommand(
                execute: async () => await ExecuteSaveAsync(),
                canExecute: CanSave
            );

            // Finalize command
            FinalizeCommand = new AsyncRelayCommand(
                execute: async () => await ExecuteFinalizeAsync(),
                canExecute: CanFinalize
            );

            // Add entry commands
            AddObservationCommand = new RelayCommand(
                execute: ExecuteAddObservation,
                canExecute: CanAddObservation
            );

            AddObjectiveObservationCommand = new RelayCommand(
                execute: ExecuteAddObjectiveObservation,
                canExecute: CanAddObjectiveObservation
            );

            AddAssessmentCommand = new RelayCommand(
                execute: ExecuteAddAssessment,
                canExecute: CanAddAssessment
            );

            AddDiagnosisCommand = new RelayCommand(
                execute: ExecuteAddDiagnosis,
                canExecute: CanAddDiagnosis
            );

            AddPlanCommand = new RelayCommand(
                execute: ExecuteAddPlan,
                canExecute: CanAddPlan
            );

            AddPrescriptionCommand = new RelayCommand(
                execute: ExecuteAddPrescription,
                canExecute: CanAddPrescription
            );

            // Back command - always navigate to unfiltered list
            BackCommand = new AsyncRelayCommand(async () =>
            {
                if (_documentId.HasValue && _documentId.Value != Guid.Empty)
                {
                    await _navigationService.NavigateToAsync($"ClinicalDocumentDetailPage?documentId={_documentId.Value}");
                }
                else
                {
                    // Don't include patientId filter - let users see all documents
                    await _navigationService.NavigateToAsync("ClinicalDocumentListPage");
                }
            });

            // Load available patients and physicians for create mode
            LoadPatientsAndPhysicians();
        }

        private void LoadPatientsAndPhysicians()
        {
            // Load all patients
            var patients = _profileRegistry.GetAllProfiles().OfType<PatientProfile>();
            foreach (var patient in patients)
            {
                AvailablePatients.Add(patient);
            }

            // Load all physicians
            var physicians = _profileRegistry.GetAllProfiles().OfType<PhysicianProfile>();
            foreach (var physician in physicians)
            {
                AvailablePhysicians.Add(physician);
            }

            // Pre-select based on provided IDs
            if (_patientId.HasValue)
            {
                SelectedPatient = AvailablePatients.FirstOrDefault(p => p.Id == _patientId.Value);
            }
            if (_physicianId.HasValue)
            {
                SelectedPhysician = AvailablePhysicians.FirstOrDefault(p => p.Id == _physicianId.Value);
            }
        }

        private void LoadAppointments()
        {
            AvailableAppointments.Clear();

            // Only load if both patient and physician are selected
            if (!_patientId.HasValue || !_physicianId.HasValue)
                return;

            // Get appointments for this patient
            var patientAppointments = _scheduleManager.GetPatientAppointments(_patientId.Value);

            // Filter to appointments with the selected physician that don't have documents yet
            var availableAppts = patientAppointments
                .Where(a => a.PhysicianId == _physicianId.Value &&
                           a.Status == AppointmentStatus.Scheduled &&
                           !a.ClinicalDocumentId.HasValue) // Only show appointments without documents
                .OrderBy(a => a.Start);

            var patientProfile = _profileRegistry.GetProfileById(_patientId.Value) as PatientProfile;
            var physicianProfile = _profileRegistry.GetProfileById(_physicianId.Value) as PhysicianProfile;

            foreach (var appt in availableAppts)
            {
                AvailableAppointments.Add(new AppointmentDisplayModel
                {
                    Id = appt.Id,
                    Start = appt.Start,
                    End = appt.End,
                    PatientName = patientProfile?.Name ?? "Unknown",
                    PhysicianName = physicianProfile?.Name ?? "Unknown",
                    AppointmentType = appt.AppointmentType,
                    ReasonForVisit = appt.ReasonForVisit ?? "N/A"
                });
            }

            // Pre-select if appointmentId was provided
            if (_appointmentId.HasValue)
            {
                SelectedAppointment = AvailableAppointments.FirstOrDefault(a => a.Id == _appointmentId.Value);
            }
        }

        private CommandParameters BuildLoadParameters()
        {
            return new CommandParameters()
                .SetParameter(ViewClinicalDocumentCommand.Parameters.DocumentId, _documentId);
        }

        private void HandleLoadResult(CommandResult result)
        {
            if (result.Success && result.Data is ClinicalDocument document)
            {
                _document = document;
                _patientId = document.PatientId;
                _physicianId = document.PhysicianId;

                // Load document properties
                ChiefComplaint = document.ChiefComplaint ?? string.Empty;

                // Load entries
                LoadObservations(document);
                LoadAssessments(document);
                LoadDiagnoses(document);
                LoadPlans(document);
                LoadPrescriptions(document);

                // Update display
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

        private void LoadObservations(ClinicalDocument document)
        {
            Observations.Clear();
            var observations = document.GetObservations();
            foreach (var obs in observations)
            {
                Observations.Add(new ObservationDisplayModel
                {
                    Id = obs.Id,
                    Type = obs.Type.ToString(),
                    Description = obs.Content
                });
            }
        }

        private void LoadAssessments(ClinicalDocument document)
        {
            Assessments.Clear();
            var assessments = document.GetAssessments();
            foreach (var assessment in assessments)
            {
                Assessments.Add(new AssessmentDisplayModel
                {
                    Id = assessment.Id,
                    Notes = assessment.Content
                });
            }
        }

        private void LoadDiagnoses(ClinicalDocument document)
        {
            Diagnoses.Clear();
            var diagnoses = document.GetDiagnoses();
            foreach (var diagnosis in diagnoses)
            {
                Diagnoses.Add(new DiagnosisDisplayModel
                {
                    Id = diagnosis.Id,
                    Code = diagnosis.ICD10Code ?? string.Empty,
                    Description = diagnosis.Content
                });
            }
        }

        private void LoadPlans(ClinicalDocument document)
        {
            Plans.Clear();
            var plans = document.GetPlans();
            foreach (var plan in plans)
            {
                Plans.Add(new PlanDisplayModel
                {
                    Id = plan.Id,
                    Description = plan.Content
                });
            }
        }

        private void LoadPrescriptions(ClinicalDocument document)
        {
            Prescriptions.Clear();
            var prescriptions = document.GetPrescriptions();
            foreach (var prescription in prescriptions)
            {
                Prescriptions.Add(new PrescriptionDisplayModel
                {
                    Id = prescription.Id,
                    MedicationName = prescription.MedicationName,
                    Dosage = prescription.Dosage ?? string.Empty,
                    Frequency = prescription.Frequency ?? string.Empty,
                    Duration = prescription.Duration ?? string.Empty,
                    DiagnosisId = prescription.DiagnosisId
                });
            }
        }

        private async Task ExecuteSaveAsync()
        {
            try
            {
                if (IsCreateMode)
                {
                    await CreateDocumentAsync();
                }
                else
                {
                    await UpdateDocumentAsync();
                }
            }
            catch (Exception ex)
            {
                ValidationErrors.Clear();
                ValidationErrors.Add($"Error saving document: {ex.Message}");
            }
        }

        private async Task CreateDocumentAsync()
        {
            if (!_patientId.HasValue || !_physicianId.HasValue || !_appointmentId.HasValue)
            {
                ValidationErrors.Clear();
                ValidationErrors.Add("Patient, Physician, and Appointment are required");
                return;
            }

            var createCommand = _commandFactory.CreateCommand(CreateClinicalDocumentCommand.Key);
            var parameters = new CommandParameters()
                .SetParameter(CreateClinicalDocumentCommand.Parameters.PatientId, _patientId.Value)
                .SetParameter(CreateClinicalDocumentCommand.Parameters.AppointmentId, _appointmentId.Value)
                .SetParameter(CreateClinicalDocumentCommand.Parameters.ChiefComplaint, ChiefComplaint);

            var result = createCommand!.Execute(parameters, _sessionManager.CurrentSession!);

            if (result.Success && result.Data is ClinicalDocument newDocument)
            {
                _document = newDocument;
                _documentId = newDocument.Id;

                // Navigate to edit mode (not detail) so user can add entries
                await _navigationService.NavigateToAsync($"ClinicalDocumentEditPage?documentId={_documentId.Value}");
            }
            else
            {
                ValidationErrors.Clear();
                ValidationErrors.Add(result.Message ?? "Failed to create document");
            }
        }

        private async Task UpdateDocumentAsync()
        {
            if (_document == null || !_documentId.HasValue) return;

            // Update chief complaint if changed
            if (_document.ChiefComplaint != ChiefComplaint)
            {
                var updateCommand = _commandFactory.CreateCommand(UpdateClinicalDocumentCommand.Key);
                var parameters = new CommandParameters()
                    .SetParameter(UpdateClinicalDocumentCommand.Parameters.DocumentId, _documentId.Value)
                    .SetParameter(UpdateClinicalDocumentCommand.Parameters.ChiefComplaint, ChiefComplaint);

                var result = updateCommand!.Execute(parameters, _sessionManager.CurrentSession!);

                if (!result.Success)
                {
                    ValidationErrors.Clear();
                    ValidationErrors.Add(result.Message ?? "Failed to update document");
                    return;
                }
            }

            // Navigate to detail page
            await _navigationService.NavigateToAsync($"ClinicalDocumentDetailPage?documentId={_documentId.Value}");
        }

        private async Task ExecuteFinalizeAsync()
        {
            if (_document == null || !_documentId.HasValue) return;

            // Check if already completed
            if (_document.IsCompleted)
            {
                ValidationErrors.Clear();
                ValidationErrors.Add("Document is already finalized");
                return;
            }

            // Validate completeness
            var validationErrors = _document.GetValidationErrors();
            if (validationErrors.Any())
            {
                ValidationErrors.Clear();
                ValidationErrors.Add("Document cannot be finalized. Missing required entries:");
                foreach (var error in validationErrors)
                {
                    ValidationErrors.Add($"  - {error}");
                }
                return;
            }

            // Call UpdateClinicalDocumentCommand with Complete=true
            var finalizeCommand = _commandFactory.CreateCommand(UpdateClinicalDocumentCommand.Key);
            var parameters = new CommandParameters()
                .SetParameter(UpdateClinicalDocumentCommand.Parameters.DocumentId, _documentId.Value)
                .SetParameter(UpdateClinicalDocumentCommand.Parameters.Complete, true);

            var result = finalizeCommand!.Execute(parameters, _sessionManager.CurrentSession!);

            if (result.Success)
            {
                // Reload document to get updated state
                var loadCommand = _commandFactory.CreateCommand(ViewClinicalDocumentCommand.Key);
                var loadParams = new CommandParameters()
                    .SetParameter(ViewClinicalDocumentCommand.Parameters.DocumentId, _documentId.Value);
                var loadResult = loadCommand!.Execute(loadParams, _sessionManager.CurrentSession!);

                if (loadResult.Success && loadResult.Data is ClinicalDocument updatedDoc)
                {
                    _document = updatedDoc;
                    (FinalizeCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }

                // Navigate to detail page
                await _navigationService.NavigateToAsync($"ClinicalDocumentDetailPage?documentId={_documentId.Value}");
            }
            else
            {
                ValidationErrors.Clear();
                ValidationErrors.Add(result.Message ?? "Failed to finalize document");
            }
        }

        private void ExecuteAddObservation()
        {
            if (_document == null || !_documentId.HasValue) return;

            if (!Enum.TryParse<ObservationType>(NewObservationType, out var observationType))
            {
                ValidationErrors.Clear();
                ValidationErrors.Add("Invalid observation type");
                return;
            }

            var addCommand = _commandFactory.CreateCommand(Core.CliniCore.Commands.Clinical.AddObservationCommand.Key);
            var parameters = new CommandParameters()
                .SetParameter(Core.CliniCore.Commands.Clinical.AddObservationCommand.Parameters.DocumentId, _documentId.Value)
                .SetParameter(Core.CliniCore.Commands.Clinical.AddObservationCommand.Parameters.Observation, NewObservation)
                .SetParameter(Core.CliniCore.Commands.Clinical.AddObservationCommand.Parameters.ObservationType, observationType);

            var result = addCommand!.Execute(parameters, _sessionManager.CurrentSession!);

            if (result.Success && result.Data is ObservationEntry entry)
            {
                Observations.Add(new ObservationDisplayModel
                {
                    Id = entry.Id,
                    Type = entry.Type.ToString(),
                    Description = entry.Content
                });

                // Reset form
                NewObservation = string.Empty;
                ClearValidation();
            }
            else
            {
                ValidationErrors.Clear();
                ValidationErrors.Add(result.Message ?? "Failed to add observation");
            }
        }

        private void ExecuteAddObjectiveObservation()
        {
            if (_document == null || !_documentId.HasValue) return;

            if (!Enum.TryParse<ObservationType>(NewObjectiveObservationType, out var observationType))
            {
                ValidationErrors.Clear();
                ValidationErrors.Add("Invalid objective observation type");
                return;
            }

            var addCommand = _commandFactory.CreateCommand(Core.CliniCore.Commands.Clinical.AddObservationCommand.Key);
            var parameters = new CommandParameters()
                .SetParameter(Core.CliniCore.Commands.Clinical.AddObservationCommand.Parameters.DocumentId, _documentId.Value)
                .SetParameter(Core.CliniCore.Commands.Clinical.AddObservationCommand.Parameters.Observation, NewObjectiveObservation)
                .SetParameter(Core.CliniCore.Commands.Clinical.AddObservationCommand.Parameters.ObservationType, observationType);

            var result = addCommand!.Execute(parameters, _sessionManager.CurrentSession!);

            if (result.Success && result.Data is ObservationEntry entry)
            {
                Observations.Add(new ObservationDisplayModel
                {
                    Id = entry.Id,
                    Type = entry.Type.ToString(),
                    Description = entry.Content
                });

                // Reset form
                NewObjectiveObservation = string.Empty;
                ClearValidation();
            }
            else
            {
                ValidationErrors.Clear();
                ValidationErrors.Add(result.Message ?? "Failed to add objective observation");
            }
        }

        private void ExecuteAddAssessment()
        {
            if (_document == null || !_documentId.HasValue) return;

            var addCommand = _commandFactory.CreateCommand(Core.CliniCore.Commands.Clinical.AddAssessmentCommand.Key);
            var parameters = new CommandParameters()
                .SetParameter(Core.CliniCore.Commands.Clinical.AddAssessmentCommand.Parameters.DocumentId, _documentId.Value)
                .SetParameter(Core.CliniCore.Commands.Clinical.AddAssessmentCommand.Parameters.ClinicalImpression, NewClinicalImpression);

            var result = addCommand!.Execute(parameters, _sessionManager.CurrentSession!);

            if (result.Success && result.Data is AssessmentEntry entry)
            {
                Assessments.Add(new AssessmentDisplayModel
                {
                    Id = entry.Id,
                    Notes = entry.Content
                });

                NewClinicalImpression = string.Empty;
                ClearValidation();
            }
            else
            {
                ValidationErrors.Clear();
                ValidationErrors.Add(result.Message ?? "Failed to add assessment");
            }
        }

        private void ExecuteAddDiagnosis()
        {
            if (_document == null || !_documentId.HasValue) return;

            var addCommand = _commandFactory.CreateCommand(Core.CliniCore.Commands.Clinical.AddDiagnosisCommand.Key);
            var parameters = new CommandParameters()
                .SetParameter(Core.CliniCore.Commands.Clinical.AddDiagnosisCommand.Parameters.DocumentId, _documentId.Value)
                .SetParameter(Core.CliniCore.Commands.Clinical.AddDiagnosisCommand.Parameters.DiagnosisDescription, NewDiagnosisDescription);

            if (!string.IsNullOrWhiteSpace(NewDiagnosisICD10Code))
            {
                parameters.SetParameter(Core.CliniCore.Commands.Clinical.AddDiagnosisCommand.Parameters.ICD10Code, NewDiagnosisICD10Code);
            }

            var result = addCommand!.Execute(parameters, _sessionManager.CurrentSession!);

            if (result.Success && result.Data is DiagnosisEntry entry)
            {
                Diagnoses.Add(new DiagnosisDisplayModel
                {
                    Id = entry.Id,
                    Code = entry.ICD10Code ?? string.Empty,
                    Description = entry.Content
                });

                NewDiagnosisDescription = string.Empty;
                NewDiagnosisICD10Code = string.Empty;
                ClearValidation();

                // Notify that available diagnoses changed for prescription form
                OnPropertyChanged(nameof(AvailableDiagnoses));
            }
            else
            {
                ValidationErrors.Clear();
                ValidationErrors.Add(result.Message ?? "Failed to add diagnosis");
            }
        }

        private void ExecuteAddPlan()
        {
            if (_document == null || !_documentId.HasValue) return;

            var addCommand = _commandFactory.CreateCommand(Core.CliniCore.Commands.Clinical.AddPlanCommand.Key);
            var parameters = new CommandParameters()
                .SetParameter(Core.CliniCore.Commands.Clinical.AddPlanCommand.Parameters.DocumentId, _documentId.Value)
                .SetParameter(Core.CliniCore.Commands.Clinical.AddPlanCommand.Parameters.PlanDescription, NewPlanDescription);

            var result = addCommand!.Execute(parameters, _sessionManager.CurrentSession!);

            if (result.Success && result.Data is PlanEntry entry)
            {
                Plans.Add(new PlanDisplayModel
                {
                    Id = entry.Id,
                    Description = entry.Content
                });

                NewPlanDescription = string.Empty;
                ClearValidation();
            }
            else
            {
                ValidationErrors.Clear();
                ValidationErrors.Add(result.Message ?? "Failed to add plan");
            }
        }

        private void ExecuteAddPrescription()
        {
            if (_document == null || !_documentId.HasValue) return;

            if (!NewPrescriptionDiagnosisId.HasValue)
            {
                ValidationErrors.Clear();
                ValidationErrors.Add("Please select a diagnosis for this prescription");
                return;
            }

            var addCommand = _commandFactory.CreateCommand(Core.CliniCore.Commands.Clinical.AddPrescriptionCommand.Key);
            var parameters = new CommandParameters()
                .SetParameter(Core.CliniCore.Commands.Clinical.AddPrescriptionCommand.Parameters.DocumentId, _documentId.Value)
                .SetParameter(Core.CliniCore.Commands.Clinical.AddPrescriptionCommand.Parameters.MedicationName, NewPrescriptionMedication)
                .SetParameter(Core.CliniCore.Commands.Clinical.AddPrescriptionCommand.Parameters.Dosage, NewPrescriptionDosage)
                .SetParameter(Core.CliniCore.Commands.Clinical.AddPrescriptionCommand.Parameters.Frequency, NewPrescriptionFrequency)
                .SetParameter(Core.CliniCore.Commands.Clinical.AddPrescriptionCommand.Parameters.DiagnosisId, NewPrescriptionDiagnosisId.Value);

            if (!string.IsNullOrWhiteSpace(NewPrescriptionDuration))
            {
                parameters.SetParameter(Core.CliniCore.Commands.Clinical.AddPrescriptionCommand.Parameters.Duration, NewPrescriptionDuration);
            }

            if (!string.IsNullOrWhiteSpace(NewPrescriptionRoute))
            {
                parameters.SetParameter(Core.CliniCore.Commands.Clinical.AddPrescriptionCommand.Parameters.Route, NewPrescriptionRoute);
            }

            var result = addCommand!.Execute(parameters, _sessionManager.CurrentSession!);

            if (result.Success && result.Data is PrescriptionEntry entry)
            {
                Prescriptions.Add(new PrescriptionDisplayModel
                {
                    Id = entry.Id,
                    MedicationName = entry.MedicationName,
                    Dosage = entry.Dosage ?? string.Empty,
                    Frequency = entry.Frequency ?? string.Empty,
                    Duration = entry.Duration ?? string.Empty,
                    DiagnosisId = entry.DiagnosisId
                });

                NewPrescriptionMedication = string.Empty;
                NewPrescriptionDosage = string.Empty;
                NewPrescriptionFrequency = string.Empty;
                NewPrescriptionDuration = string.Empty;
                NewPrescriptionRoute = "Oral";
                NewPrescriptionDiagnosisId = null;
                SelectedDiagnosisForPrescription = null;
                ClearValidation();
            }
            else
            {
                ValidationErrors.Clear();
                ValidationErrors.Add(result.Message ?? "Failed to add prescription");
            }
        }

        // Can execute methods
        private bool CanSave()
        {
            if (IsCreateMode)
            {
                return !string.IsNullOrWhiteSpace(ChiefComplaint) &&
                       _patientId.HasValue &&
                       _physicianId.HasValue &&
                       _appointmentId.HasValue; // Appointment required in create mode
            }
            else
            {
                return !string.IsNullOrWhiteSpace(ChiefComplaint);
            }
        }

        private bool CanFinalize()
        {
            return _document != null &&
                   _documentId.HasValue &&
                   !_document.IsCompleted && // Can only finalize if not already completed
                   IsEditMode; // Only available in edit mode, not create mode
        }

        private bool CanAddObservation()
        {
            return _document != null &&
                   !string.IsNullOrWhiteSpace(NewObservation);
        }

        private bool CanAddObjectiveObservation()
        {
            return _document != null &&
                   !string.IsNullOrWhiteSpace(NewObjectiveObservation);
        }

        private bool CanAddAssessment()
        {
            return _document != null &&
                   !string.IsNullOrWhiteSpace(NewClinicalImpression);
        }

        private bool CanAddDiagnosis()
        {
            return _document != null &&
                   !string.IsNullOrWhiteSpace(NewDiagnosisDescription);
        }

        private bool CanAddPlan()
        {
            return _document != null &&
                   !string.IsNullOrWhiteSpace(NewPlanDescription);
        }

        private bool CanAddPrescription()
        {
            return _document != null &&
                   Diagnoses.Count > 0 &&
                   NewPrescriptionDiagnosisId.HasValue &&
                   !string.IsNullOrWhiteSpace(NewPrescriptionMedication) &&
                   !string.IsNullOrWhiteSpace(NewPrescriptionDosage) &&
                   !string.IsNullOrWhiteSpace(NewPrescriptionFrequency);
        }

    }

    // Display models for entry lists
    public class ObservationDisplayModel
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Display => $"{Type}: {Description}";
    }

    public class AssessmentDisplayModel
    {
        public Guid Id { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string ClinicalImpression { get; set; } = string.Empty;  // For detail page
        public string ConfidenceLevel { get; set; } = string.Empty;  // For detail page
        public string Display => Notes.Length > 60 ? Notes.Substring(0, 60) + "..." : Notes;
    }

    public class DiagnosisDisplayModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ICD10Code { get; set; } = string.Empty;  // For detail page
        public string Status { get; set; } = string.Empty;  // For detail page
        public string Display => string.IsNullOrWhiteSpace(Code) ? Description : $"[{Code}] {Description}";
    }

    public class PlanDisplayModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;  // For detail page
        public string Display => Description.Length > 60 ? Description.Substring(0, 60) + "..." : Description;
    }

    public class PrescriptionDisplayModel
    {
        public Guid Id { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;  // For detail page
        public string Duration { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;  // For detail page
        public Guid? DiagnosisId { get; set; }
        public string Display => $"{MedicationName} {Dosage} - {Frequency}" + (!string.IsNullOrWhiteSpace(Duration) ? $" for {Duration}" : "");
    }

    public class AppointmentDisplayModel
    {
        public Guid Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PhysicianName { get; set; } = string.Empty;
        public string AppointmentType { get; set; } = string.Empty;
        public string ReasonForVisit { get; set; } = string.Empty;
        public string Display => $"{Start:yyyy-MM-dd HH:mm} - {AppointmentType} ({ReasonForVisit})";
    }
}
