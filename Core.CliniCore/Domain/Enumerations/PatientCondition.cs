namespace Core.CliniCore.Domain.Enumerations
{
    /// <summary>
    /// Patient's overall clinical condition
    /// </summary>
    public enum PatientCondition
    {
        Stable,
        Improving,
        Unchanged,
        Worsening,
        Critical
    }
}
