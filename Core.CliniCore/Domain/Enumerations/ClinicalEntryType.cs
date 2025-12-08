namespace Core.CliniCore.Domain.Enumerations
{
    /// <summary>
    /// Types of clinical entries in a clinical document
    /// </summary>
    public enum ClinicalEntryType
    {
        ChiefComplaint,
        Observation,
        Assessment,
        Diagnosis,
        Plan,
        Prescription,
        ProgressNote,
        Procedure,
        LabResult,
        VitalSigns
    }
}
