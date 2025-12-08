using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.Extensions;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GUI.CliniCore.ViewModels.ClinicalDocuments
{
    /// <summary>
    /// Base class for form models with property change notification
    /// </summary>
    public abstract class FormModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public abstract void Clear();
        public abstract bool IsValid { get; }
    }

    /// <summary>
    /// Form model for creating new observations
    /// </summary>
    public class NewObservationForm : FormModelBase
    {
        private string _content = string.Empty;
        private ObservationType _type = ObservationType.HistoryOfPresentIllness;
        private BodySystem? _bodySystem;
        private bool _isAbnormal;
        private EntrySeverity _severity = EntrySeverity.Routine;
        private double? _numericValue;
        private string? _unit;

        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public ObservationType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public BodySystem? BodySystem
        {
            get => _bodySystem;
            set => SetProperty(ref _bodySystem, value);
        }

        public bool IsAbnormal
        {
            get => _isAbnormal;
            set => SetProperty(ref _isAbnormal, value);
        }

        public EntrySeverity Severity
        {
            get => _severity;
            set => SetProperty(ref _severity, value);
        }

        public double? NumericValue
        {
            get => _numericValue;
            set => SetProperty(ref _numericValue, value);
        }

        public string? Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }

        // Picker options from extension classes
        public static ObservationType[] TypeOptions => ObservationTypeExtensions.All;
        public static BodySystem[] BodySystemOptions => BodySystemExtensions.All;
        public static EntrySeverity[] SeverityOptions => EntrySeverityExtensions.All;

        // Categorized type options for subjective vs objective forms
        public static ObservationType[] SubjectiveTypeOptions =>
        [
            ObservationType.ChiefComplaint,
            ObservationType.HistoryOfPresentIllness,
            ObservationType.SocialHistory,
            ObservationType.FamilyHistory,
            ObservationType.Allergy
        ];

        public static ObservationType[] ObjectiveTypeOptions =>
        [
            ObservationType.VitalSigns,
            ObservationType.PhysicalExam,
            ObservationType.LabResult,
            ObservationType.ImagingResult,
            ObservationType.ReviewOfSystems
        ];

        public override bool IsValid => !string.IsNullOrWhiteSpace(Content);

        public override void Clear()
        {
            Content = string.Empty;
            Type = ObservationType.HistoryOfPresentIllness;
            BodySystem = null;
            IsAbnormal = false;
            Severity = EntrySeverity.Routine;
            NumericValue = null;
            Unit = null;
        }
    }

    /// <summary>
    /// Form model for creating new assessments
    /// </summary>
    public class NewAssessmentForm : FormModelBase
    {
        private string _content = string.Empty;
        private PatientCondition _condition = PatientCondition.Stable;
        private Prognosis _prognosis = Prognosis.Good;
        private ConfidenceLevel _confidence = ConfidenceLevel.Moderate;
        private EntrySeverity _severity = EntrySeverity.Routine;
        private bool _requiresImmediateAction;

        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public PatientCondition Condition
        {
            get => _condition;
            set => SetProperty(ref _condition, value);
        }

        public Prognosis Prognosis
        {
            get => _prognosis;
            set => SetProperty(ref _prognosis, value);
        }

        public ConfidenceLevel Confidence
        {
            get => _confidence;
            set => SetProperty(ref _confidence, value);
        }

        public EntrySeverity Severity
        {
            get => _severity;
            set => SetProperty(ref _severity, value);
        }

        public bool RequiresImmediateAction
        {
            get => _requiresImmediateAction;
            set => SetProperty(ref _requiresImmediateAction, value);
        }

        // Picker options from extension classes
        public static PatientCondition[] ConditionOptions => PatientConditionExtensions.All;
        public static Prognosis[] PrognosisOptions => PrognosisExtensions.All;
        public static ConfidenceLevel[] ConfidenceOptions => ConfidenceLevelExtensions.All;
        public static EntrySeverity[] SeverityOptions => EntrySeverityExtensions.All;

        public override bool IsValid => !string.IsNullOrWhiteSpace(Content);

        public override void Clear()
        {
            Content = string.Empty;
            Condition = PatientCondition.Stable;
            Prognosis = Prognosis.Good;
            Confidence = ConfidenceLevel.Moderate;
            Severity = EntrySeverity.Routine;
            RequiresImmediateAction = false;
        }
    }

    /// <summary>
    /// Form model for creating new diagnoses
    /// </summary>
    public class NewDiagnosisForm : FormModelBase
    {
        private string _content = string.Empty;
        private string? _icd10Code;
        private DiagnosisType _type = DiagnosisType.Working;
        private DiagnosisStatus _status = DiagnosisStatus.Active;
        private EntrySeverity _severity = EntrySeverity.Routine;
        private bool _isPrimary;
        private DateTime? _onsetDate;

        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public string? ICD10Code
        {
            get => _icd10Code;
            set => SetProperty(ref _icd10Code, value);
        }

        public DiagnosisType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public DiagnosisStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public EntrySeverity Severity
        {
            get => _severity;
            set => SetProperty(ref _severity, value);
        }

        public bool IsPrimary
        {
            get => _isPrimary;
            set => SetProperty(ref _isPrimary, value);
        }

        public DateTime? OnsetDate
        {
            get => _onsetDate;
            set => SetProperty(ref _onsetDate, value);
        }

        /// <summary>
        /// Non-nullable proxy for DatePicker binding. Defaults to today if null.
        /// </summary>
        public DateTime OnsetDateValue
        {
            get => _onsetDate ?? DateTime.Today;
            set => OnsetDate = value;
        }

        // Picker options from extension classes
        public static DiagnosisType[] TypeOptions => DiagnosisTypeExtensions.All;
        public static DiagnosisStatus[] StatusOptions => DiagnosisStatusExtensions.All;
        public static EntrySeverity[] SeverityOptions => EntrySeverityExtensions.All;

        public override bool IsValid => !string.IsNullOrWhiteSpace(Content);

        public override void Clear()
        {
            Content = string.Empty;
            ICD10Code = null;
            Type = DiagnosisType.Working;
            Status = DiagnosisStatus.Active;
            Severity = EntrySeverity.Routine;
            IsPrimary = false;
            OnsetDate = null;
        }
    }

    /// <summary>
    /// Form model for creating new plans
    /// </summary>
    public class NewPlanForm : FormModelBase
    {
        private string _content = string.Empty;
        private PlanType _type = PlanType.Treatment;
        private PlanPriority _priority = PlanPriority.Routine;
        private EntrySeverity _severity = EntrySeverity.Routine;
        private DateTime? _targetDate;
        private string? _followUpInstructions;
        private List<Guid> _relatedDiagnosisIds = new();

        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public PlanType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public PlanPriority Priority
        {
            get => _priority;
            set => SetProperty(ref _priority, value);
        }

        public EntrySeverity Severity
        {
            get => _severity;
            set => SetProperty(ref _severity, value);
        }

        public DateTime? TargetDate
        {
            get => _targetDate;
            set => SetProperty(ref _targetDate, value);
        }

        /// <summary>
        /// Non-nullable proxy for DatePicker binding. Defaults to today if null.
        /// </summary>
        public DateTime TargetDateValue
        {
            get => _targetDate ?? DateTime.Today;
            set => TargetDate = value;
        }

        public string? FollowUpInstructions
        {
            get => _followUpInstructions;
            set => SetProperty(ref _followUpInstructions, value);
        }

        public List<Guid> RelatedDiagnosisIds
        {
            get => _relatedDiagnosisIds;
            set => SetProperty(ref _relatedDiagnosisIds, value);
        }

        // Picker options from extension classes
        public static PlanType[] TypeOptions => PlanTypeExtensions.All;
        public static PlanPriority[] PriorityOptions => PlanPriorityExtensions.All;
        public static EntrySeverity[] SeverityOptions => EntrySeverityExtensions.All;

        public override bool IsValid => !string.IsNullOrWhiteSpace(Content);

        public override void Clear()
        {
            Content = string.Empty;
            Type = PlanType.Treatment;
            Priority = PlanPriority.Routine;
            Severity = EntrySeverity.Routine;
            TargetDate = null;
            FollowUpInstructions = null;
            RelatedDiagnosisIds = new();
        }
    }

    /// <summary>
    /// Form model for creating new prescriptions
    /// </summary>
    public class NewPrescriptionForm : FormModelBase
    {
        private string _medicationName = string.Empty;
        private string? _dosage;
        private DosageFrequency? _frequency;
        private MedicationRoute _route = MedicationRoute.Oral;
        private string? _duration;
        private int _refills;
        private bool _genericAllowed = true;
        private int? _deaSchedule;
        private string? _instructions;
        private EntrySeverity _severity = EntrySeverity.Routine;
        private Guid? _diagnosisId;
        private DiagnosisDisplayModel? _selectedDiagnosis;

        public string MedicationName
        {
            get => _medicationName;
            set => SetProperty(ref _medicationName, value);
        }

        public string? Dosage
        {
            get => _dosage;
            set => SetProperty(ref _dosage, value);
        }

        public DosageFrequency? Frequency
        {
            get => _frequency;
            set => SetProperty(ref _frequency, value);
        }

        public MedicationRoute Route
        {
            get => _route;
            set => SetProperty(ref _route, value);
        }

        public string? Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        public int Refills
        {
            get => _refills;
            set => SetProperty(ref _refills, value);
        }

        public bool GenericAllowed
        {
            get => _genericAllowed;
            set => SetProperty(ref _genericAllowed, value);
        }

        public int? DEASchedule
        {
            get => _deaSchedule;
            set => SetProperty(ref _deaSchedule, value);
        }

        public string? Instructions
        {
            get => _instructions;
            set => SetProperty(ref _instructions, value);
        }

        public EntrySeverity Severity
        {
            get => _severity;
            set => SetProperty(ref _severity, value);
        }

        public Guid? DiagnosisId
        {
            get => _diagnosisId;
            set => SetProperty(ref _diagnosisId, value);
        }

        public DiagnosisDisplayModel? SelectedDiagnosis
        {
            get => _selectedDiagnosis;
            set
            {
                if (SetProperty(ref _selectedDiagnosis, value))
                {
                    DiagnosisId = value?.Id;
                }
            }
        }

        // Picker options from extension classes
        public static DosageFrequency[] FrequencyOptions => DosageFrequencyExtensions.All;
        public static MedicationRoute[] RouteOptions => MedicationRouteExtensions.All;
        public static EntrySeverity[] SeverityOptions => EntrySeverityExtensions.All;

        public override bool IsValid =>
            !string.IsNullOrWhiteSpace(MedicationName) &&
            !string.IsNullOrWhiteSpace(Dosage) &&
            Frequency.HasValue &&
            DiagnosisId.HasValue;

        public override void Clear()
        {
            MedicationName = string.Empty;
            Dosage = null;
            Frequency = null;
            Route = MedicationRoute.Oral;
            Duration = null;
            Refills = 0;
            GenericAllowed = true;
            DEASchedule = null;
            Instructions = null;
            Severity = EntrySeverity.Routine;
            DiagnosisId = null;
            SelectedDiagnosis = null;
        }
    }
}
