namespace Core.CliniCore.Domain.Enumerations
{
    /// <summary>
    /// Types of clinical diagnoses
    /// </summary>
    public enum DiagnosisType
    {
        Differential,  // Possible diagnosis
        Working,       // Probable diagnosis
        Final,         // Confirmed diagnosis
        RuledOut       // Excluded diagnosis
    }
}
