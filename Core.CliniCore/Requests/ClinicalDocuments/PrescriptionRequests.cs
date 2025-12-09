using Core.CliniCore.Domain.Enumerations;

namespace Core.CliniCore.Requests.ClinicalDocuments
{
    /// <summary>
    /// Request to create a new prescription entry
    /// </summary>
    public class CreatePrescriptionRequest
    {
        public Guid? Id { get; set; }
        public Guid? AuthorId { get; set; }
        public Guid DiagnosisId { get; set; }  // Required: link to supporting diagnosis
        public string MedicationName { get; set; } = string.Empty;
        public string? Dosage { get; set; }
        public DosageFrequency? Frequency { get; set; }
        public MedicationRoute Route { get; set; } = MedicationRoute.Oral;
        public string? Duration { get; set; }
        public int Refills { get; set; }
        public bool GenericAllowed { get; set; } = true;
        public int? DEASchedule { get; set; }
        public string? NDCCode { get; set; }
        public string? Instructions { get; set; }
        public EntrySeverity Severity { get; set; } = EntrySeverity.Routine;
    }

    /// <summary>
    /// Request to update an existing prescription entry
    /// </summary>
    public class UpdatePrescriptionRequest
    {
        public string? MedicationName { get; set; }
        public string? Dosage { get; set; }
        public DosageFrequency? Frequency { get; set; }
        public MedicationRoute? Route { get; set; }
        public string? Duration { get; set; }
        public int? Refills { get; set; }
        public bool? GenericAllowed { get; set; }
        public int? DEASchedule { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string? NDCCode { get; set; }
        public string? Instructions { get; set; }
        public EntrySeverity? Severity { get; set; }
        public bool? IsActive { get; set; }
    }
}
