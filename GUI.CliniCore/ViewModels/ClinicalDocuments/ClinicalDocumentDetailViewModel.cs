using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Clinical;
using Core.CliniCore.Domain.ClinicalDocumentation;
using Core.CliniCore.Domain.ClinicalDocumentation.ClinicalEntries;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.Service;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;
using GUI.CliniCore.ViewModels.Base;
using System.Collections.ObjectModel;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels.ClinicalDocuments
{
    /// <summary>
    /// ViewModel for Clinical Document Detail page
    /// Displays SOAP note and provides document actions
    /// </summary>
    [QueryProperty(nameof(DocumentIdString), "documentId")]
    public partial class ClinicalDocumentDetailViewModel : BaseViewModel
    {
        private readonly CommandInvoker _commandInvoker;
        private readonly CommandFactory _commandFactory;
        private readonly INavigationService _navigationService;
        private readonly SessionManager _sessionManager;
        private readonly ProfileService _profileRegistry;

        private Guid? _documentId;
        private ClinicalDocument? _document;

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

        private string _chiefComplaint = string.Empty;
        public string ChiefComplaint
        {
            get => _chiefComplaint;
            set => SetProperty(ref _chiefComplaint, value);
        }

        private string _documentInfo = string.Empty;
        public string DocumentInfo
        {
            get => _documentInfo;
            set => SetProperty(ref _documentInfo, value);
        }

        // Categorized entry collections for card-based display
        public ObservableCollection<ObservationDisplayModel> SubjectiveObservations { get; } = new();
        public ObservableCollection<ObservationDisplayModel> ObjectiveObservations { get; } = new();
        public ObservableCollection<AssessmentDisplayModel> Assessments { get; } = new();
        public ObservableCollection<DiagnosisDisplayModel> Diagnoses { get; } = new();
        public ObservableCollection<PlanDisplayModel> Plans { get; } = new();
        public ObservableCollection<PrescriptionDisplayModel> Prescriptions { get; } = new();

        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (SetProperty(ref _isCompleted, value))
                {
                    OnPropertyChanged(nameof(CanEdit));
                    OnPropertyChanged(nameof(CanComplete));
                    OnPropertyChanged(nameof(CanDelete));
                }
            }
        }

        // Action availability
        public bool CanEdit => !IsCompleted;
        public bool CanComplete => !IsCompleted && _document != null && _document.IsComplete();
        public bool CanDelete => _document != null;  // Can delete any document (will prompt for completed)

        public MauiCommand LoadDocumentCommand { get; }
        public MauiCommand EditCommand { get; }
        public MauiCommand CompleteCommand { get; }
        public MauiCommand DeleteCommand { get; }
        public MauiCommand BackCommand { get; }

        public ClinicalDocumentDetailViewModel(
            CommandInvoker commandInvoker,
            CommandFactory commandFactory,
            INavigationService navigationService,
            SessionManager sessionManager,
            ProfileService profileService)
        {
            _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _profileRegistry = profileService ?? throw new ArgumentNullException(nameof(profileService));

            Title = "Clinical Document";

            // Load command
            var viewCoreCommand = _commandFactory.CreateCommand(ViewClinicalDocumentCommand.Key);
            LoadDocumentCommand = new MauiCommandAdapter(
                _commandInvoker,
                viewCoreCommand!,
                parameterBuilder: BuildLoadParameters,
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleLoadResult
            );

            // Edit command
            EditCommand = new AsyncRelayCommand(
                execute: async () => await NavigateToEditAsync(),
                canExecute: () => CanEdit
            );

            // Complete command
            CompleteCommand = new RelayCommand(
                execute: () => ExecuteComplete(),
                canExecute: () => CanComplete
            );

            // Delete command
            DeleteCommand = new AsyncRelayCommand(
                execute: async () => await ExecuteDeleteAsync(),
                canExecute: () => CanDelete
            );

            // Back command
            BackCommand = new AsyncRelayCommand(async () =>
            {
                // Navigate back to list with patient filter if available
                if (_document != null)
                {
                    await _navigationService.NavigateToAsync($"ClinicalDocumentListPage?patientId={_document.PatientId}");
                }
                else
                {
                    await _navigationService.NavigateToAsync("ClinicalDocumentListPage");
                }
            });
        }

        private CommandParameters BuildLoadParameters()
        {
            return new CommandParameters()
                .SetParameter(ViewClinicalDocumentCommand.Parameters.DocumentId, _documentId)
                .SetParameter(ViewClinicalDocumentCommand.Parameters.Format, "soap");
        }

        private void HandleLoadResult(CommandResult result)
        {
            if (result.Success && result.Data is ClinicalDocument document)
            {
                _document = document;
                IsCompleted = document.IsCompleted;

                // Set chief complaint
                ChiefComplaint = document.ChiefComplaint ?? "Not documented";

                // Generate document info header
                var patient = _profileRegistry.GetProfileById(document.PatientId) as PatientProfile;
                var physician = _profileRegistry.GetProfileById(document.PhysicianId) as PhysicianProfile;

                DocumentInfo = $"Patient: {patient?.GetValue<string>(CommonEntryType.Name.GetKey()) ?? "Unknown"}\n" +
                              $"Physician: Dr. {physician?.GetValue<string>(CommonEntryType.Name.GetKey()) ?? "Unknown"}\n" +
                              $"Date: {document.CreatedAt:yyyy-MM-dd HH:mm}\n" +
                              $"Status: {(document.IsCompleted ? "Completed" : "Draft")}";

                // Populate categorized entry collections
                PopulateEntryCollections(document);

                ClearValidation();

                // Update action button states
                (EditCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (CompleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DeleteCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private void PopulateEntryCollections(ClinicalDocument document)
        {
            // Clear all collections
            SubjectiveObservations.Clear();
            ObjectiveObservations.Clear();
            Assessments.Clear();
            Diagnoses.Clear();
            Plans.Clear();
            Prescriptions.Clear();

            // Categorize observations into Subjective and Objective
            var subjectiveTypes = new[]
            {
                ObservationType.ChiefComplaint,
                ObservationType.HistoryOfPresentIllness,
                ObservationType.SocialHistory,
                ObservationType.FamilyHistory,
                ObservationType.Allergy
            };

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

                if (subjectiveTypes.Contains(obs.Type))
                {
                    SubjectiveObservations.Add(displayModel);
                }
                else
                {
                    ObjectiveObservations.Add(displayModel);
                }
            }

            // Populate Assessments
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

            // Populate Diagnoses
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

            // Populate Plans
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

            // Populate Prescriptions
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

        private async Task NavigateToEditAsync()
        {
            if (_documentId.HasValue)
            {
                await _navigationService.NavigateToAsync($"ClinicalDocumentEditPage?documentId={_documentId.Value}");
            }
        }

        private void ExecuteComplete()
        {
            if (_document == null) return;

            try
            {
                _document.Complete();
                IsCompleted = true;

                // Reload to refresh display
                LoadDocumentCommand.Execute(null);

                ValidationErrors.Clear();
                ValidationErrors.Add("Document completed successfully!");
            }
            catch (InvalidOperationException ex)
            {
                ValidationErrors.Clear();
                ValidationErrors.Add($"Cannot complete document: {ex.Message}");
            }
        }

        private async Task ExecuteDeleteAsync()
        {
            if (!_documentId.HasValue || _document == null) return;

            // Confirm deletion, especially for completed documents
            var confirmMessage = _document.IsCompleted
                ? "This is a COMPLETED document. Are you sure you want to permanently delete it?"
                : "Are you sure you want to delete this document?";

            bool confirmed = await Application.Current!.MainPage!.DisplayAlert(
                "Confirm Deletion",
                confirmMessage,
                "Delete",
                "Cancel");

            if (!confirmed) return;

            var deleteCoreCommand = _commandFactory.CreateCommand(DeleteClinicalDocumentCommand.Key);
            var parameters = new CommandParameters()
                .SetParameter(DeleteClinicalDocumentCommand.Parameters.DocumentId, _documentId.Value);

            // Add Force parameter for completed documents
            if (_document.IsCompleted)
            {
                parameters.SetParameter(DeleteClinicalDocumentCommand.Parameters.Force, true);
            }

            var deleteCommand = new MauiCommandAdapter(
                _commandInvoker,
                deleteCoreCommand!,
                parameterBuilder: () => parameters,
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleDeleteResult
            );

            deleteCommand.Execute(null);
        }

        private void HandleDeleteResult(CommandResult result)
        {
            ClearValidation();

            if (result.Success)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (_document != null)
                    {
                        await _navigationService.NavigateToAsync($"ClinicalDocumentListPage?patientId={_document.PatientId}");
                    }
                    else
                    {
                        await _navigationService.NavigateToAsync("ClinicalDocumentListPage");
                    }
                });
            }
            else
            {
                SetValidationError(result.GetDisplayMessage());
            }
        }
    }
}
