namespace Core.CliniCore.Domain.Enumerations
{
    /// <summary>
    /// Prognosis assessment levels
    /// </summary>
    public enum Prognosis
    {
        /// <summary>
        /// The expected outcome is excellent.
        /// </summary>
        Excellent,
        /// <summary>
        /// The expected outcome is good.
        /// </summary>
        Good,
        /// <summary>
        /// The expected outcome is fair.
        /// </summary>
        Fair,
        /// <summary>
        /// The expected outcome is uncertain or guarded.
        /// </summary>
        Guarded,
        /// <summary>
        /// The expected outcome is poor.
        /// </summary>
        Poor
    }
}
