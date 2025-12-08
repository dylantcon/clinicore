namespace Core.CliniCore.Domain.Enumerations
{
    /// <summary>
    /// Types of treatment plan items
    /// </summary>
    public enum PlanType
    {
        Treatment,          // Medical treatment plan
        Diagnostic,         // Diagnostic tests to order
        Referral,           // Referral to specialist
        FollowUp,           // Follow-up appointment
        PatientEducation,   // Education provided/needed
        Procedure,          // Procedure to perform
        Monitoring,         // Ongoing monitoring
        Prevention          // Preventive care
    }
}
