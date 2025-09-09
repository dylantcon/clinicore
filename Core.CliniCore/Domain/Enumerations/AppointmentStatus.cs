namespace Core.CliniCore.Domain.Enumerations
{
    public enum AppointmentStatus
    {
        /// <summary>
        /// Appointment is scheduled and confirmed
        /// </summary>
        Scheduled,

        /// <summary>
        /// Appointment was completed successfully
        /// </summary>
        Completed,

        /// <summary>
        /// Appointment was cancelled
        /// </summary>
        Cancelled,

        /// <summary>
        /// Patient did not show up for appointment
        /// </summary>
        NoShow,

        /// <summary>
        /// Appointment is in progress
        /// </summary>
        InProgress,

        /// <summary>
        /// Appointment is tentatively scheduled, awaiting confirmation
        /// </summary>
        Tentative,

        /// <summary>
        /// Appointment was rescheduled
        /// </summary>
        Rescheduled
    }
}