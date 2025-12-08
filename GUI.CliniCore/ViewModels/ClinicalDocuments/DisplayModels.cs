using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users.Concrete;

namespace GUI.CliniCore.ViewModels.ClinicalDocuments
{
    /// <summary>
    /// Display model for patients in Pickers
    /// </summary>
    public class PatientDisplayModel
    {
        public PatientDisplayModel() { }

        public PatientDisplayModel(PatientProfile patient)
        {
            ArgumentNullException.ThrowIfNull(patient);
            Id = patient.Id;
            Name = patient.GetValue<string>(CommonEntryType.Name.GetKey()) ?? "Unknown";

            var birthDate = patient.GetValue<DateTime?>(CommonEntryType.BirthDate.GetKey());
            Age = birthDate.HasValue ? CalculateAge(birthDate.Value) : null;
        }

        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? Age { get; set; }

        public string DisplayName => Age.HasValue ? $"{Name} (Age: {Age})" : Name;

        public override string ToString() => DisplayName;

        private static int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
        }
    }

    /// <summary>
    /// Display model for physicians in Pickers
    /// </summary>
    public class PhysicianDisplayModel
    {
        public PhysicianDisplayModel() { }

        public PhysicianDisplayModel(PhysicianProfile physician)
        {
            ArgumentNullException.ThrowIfNull(physician);
            Id = physician.Id;
            Name = physician.GetValue<string>(CommonEntryType.Name.GetKey()) ?? "Unknown";

            var specializations = physician.GetValue<List<string>>(PhysicianEntryType.Specializations.GetKey());
            Specializations = specializations ?? new List<string>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Specializations { get; set; } = new();

        public string DisplayName => Specializations.Count > 0
            ? $"Dr. {Name} ({string.Join(", ", Specializations.Take(2))})"
            : $"Dr. {Name}";

        public override string ToString() => DisplayName;
    }


    /// <summary>
    /// Enum-typed display model for observations with full domain field support
    /// </summary>
    public class ObservationDisplayModel
    {
        public Guid Id { get; set; }
        public ObservationType Type { get; set; }
        public string Content { get; set; } = string.Empty;
        public BodySystem? BodySystem { get; set; }
        public bool IsAbnormal { get; set; }
        public EntrySeverity Severity { get; set; } = EntrySeverity.Routine;
        public double? NumericValue { get; set; }
        public string? Unit { get; set; }
        public string? ReferenceRange { get; set; }

        // Computed display properties
        public string TypeDisplay => Type.GetDisplayName();
        public string BodySystemDisplay => BodySystem?.GetDisplayName() ?? string.Empty;
        public string SeverityDisplay => Severity.GetDisplayName();
        public string AbnormalDisplay => IsAbnormal ? "[ABNORMAL]" : string.Empty;
        public string Display => $"{TypeDisplay}: {Content}";
        public string DetailDisplay => IsAbnormal
            ? $"{AbnormalDisplay} {TypeDisplay}: {Content}"
            : $"{TypeDisplay}: {Content}";
    }

    /// <summary>
    /// Enum-typed display model for assessments with full domain field support
    /// </summary>
    public class AssessmentDisplayModel
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public PatientCondition Condition { get; set; } = PatientCondition.Stable;
        public Prognosis Prognosis { get; set; } = Prognosis.Good;
        public ConfidenceLevel Confidence { get; set; } = ConfidenceLevel.Moderate;
        public EntrySeverity Severity { get; set; } = EntrySeverity.Routine;
        public bool RequiresImmediateAction { get; set; }
        public List<string> DifferentialDiagnoses { get; set; } = new();
        public List<string> RiskFactors { get; set; } = new();

        // Computed display properties
        public string ConditionDisplay => Condition.GetDisplayName();
        public string PrognosisDisplay => Prognosis.GetDisplayName();
        public string ConfidenceDisplay => Confidence.GetDisplayName();
        public string SeverityDisplay => Severity.GetDisplayName();
        public string Display => Content.Length > 60 ? Content[..60] + "..." : Content;
        public string UrgentDisplay => RequiresImmediateAction ? "[URGENT]" : string.Empty;
    }

    /// <summary>
    /// Enum-typed display model for diagnoses with full domain field support
    /// </summary>
    public class DiagnosisDisplayModel
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? ICD10Code { get; set; }
        public DiagnosisType Type { get; set; } = DiagnosisType.Working;
        public DiagnosisStatus Status { get; set; } = DiagnosisStatus.Active;
        public EntrySeverity Severity { get; set; } = EntrySeverity.Routine;
        public bool IsPrimary { get; set; }
        public DateTime? OnsetDate { get; set; }

        // Computed display properties
        public string TypeDisplay => Type.GetDisplayName();
        public string StatusDisplay => Status.GetDisplayName();
        public string SeverityDisplay => Severity.GetDisplayName();
        public string CodeDisplay => ICD10Code ?? string.Empty;
        public string PrimaryDisplay => IsPrimary ? "[PRIMARY]" : string.Empty;
        public string Display => string.IsNullOrWhiteSpace(ICD10Code)
            ? Content
            : $"[{ICD10Code}] {Content}";
    }

    /// <summary>
    /// Enum-typed display model for plans with full domain field support
    /// </summary>
    public class PlanDisplayModel
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public PlanType Type { get; set; } = PlanType.Treatment;
        public PlanPriority Priority { get; set; } = PlanPriority.Routine;
        public EntrySeverity Severity { get; set; } = EntrySeverity.Routine;
        public DateTime? TargetDate { get; set; }
        public string? FollowUpInstructions { get; set; }
        public bool IsCompleted { get; set; }
        public List<Guid> RelatedDiagnosisIds { get; set; } = new();

        // Computed display properties
        public string TypeDisplay => Type.GetDisplayName();
        public string PriorityDisplay => Priority.GetDisplayName();
        public string SeverityDisplay => Severity.GetDisplayName();
        public string CompletedDisplay => IsCompleted ? "[DONE]" : string.Empty;
        public string Display => Content.Length > 60 ? Content[..60] + "..." : Content;
        public string TargetDateDisplay => TargetDate?.ToString("yyyy-MM-dd") ?? string.Empty;
    }

    /// <summary>
    /// Enum-typed display model for prescriptions with full domain field support
    /// </summary>
    public class PrescriptionDisplayModel
    {
        public Guid Id { get; set; }
        public Guid DiagnosisId { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string? Dosage { get; set; }
        public DosageFrequency? Frequency { get; set; }
        public MedicationRoute Route { get; set; } = MedicationRoute.Oral;
        public string? Duration { get; set; }
        public int Refills { get; set; }
        public bool GenericAllowed { get; set; } = true;
        public int? DEASchedule { get; set; }
        public string? Instructions { get; set; }
        public EntrySeverity Severity { get; set; } = EntrySeverity.Routine;

        // Computed display properties
        public string FrequencyDisplay => Frequency?.GetDisplayName() ?? string.Empty;
        public string FrequencyAbbrev => Frequency?.GetAbbreviation() ?? string.Empty;
        public string RouteDisplay => Route.GetDisplayName();
        public string RouteAbbrev => Route.GetAbbreviation();
        public string SeverityDisplay => Severity.GetDisplayName();
        public string ControlledDisplay => DEASchedule.HasValue ? $"[C-{DEASchedule}]" : string.Empty;
        public string GenericDisplay => GenericAllowed ? "Generic OK" : "Brand Only";

        public string Display =>
            $"{MedicationName} {Dosage ?? ""} - {FrequencyDisplay}" +
            (!string.IsNullOrWhiteSpace(Duration) ? $" for {Duration}" : "");

        public string SigDisplay =>
            $"Take {Dosage ?? "as directed"} {RouteAbbrev} {FrequencyAbbrev}" +
            (!string.IsNullOrWhiteSpace(Duration) ? $" x {Duration}" : "");
    }

    /// <summary>
    /// Display model for appointments (used in document creation)
    /// </summary>
    public class AppointmentDisplayModel
    {
        public AppointmentDisplayModel() { }

        public AppointmentDisplayModel(Core.CliniCore.Scheduling.AppointmentTimeInterval appointment)
        {
            ArgumentNullException.ThrowIfNull(appointment);
            Id = appointment.Id;
            Start = appointment.Start;
            End = appointment.End;
            AppointmentType = appointment.AppointmentType ?? string.Empty;
            ReasonForVisit = appointment.ReasonForVisit ?? string.Empty;
        }

        public Guid Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PhysicianName { get; set; } = string.Empty;
        public string AppointmentType { get; set; } = string.Empty;
        public string ReasonForVisit { get; set; } = string.Empty;

        public string Display => $"{Start:yyyy-MM-dd HH:mm} - {AppointmentType} ({ReasonForVisit})";
        public string TimeDisplay => $"{Start:HH:mm} - {End:HH:mm}";
        public string DateDisplay => Start.ToString("yyyy-MM-dd");

        public override string ToString() => Display;
    }
}
