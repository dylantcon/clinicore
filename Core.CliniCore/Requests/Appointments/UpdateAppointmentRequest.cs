using System;

namespace Core.CliniCore.Requests.Appointments
{
    /// <summary>
    /// Request DTO for updating an existing appointment
    /// </summary>
    public class UpdateAppointmentRequest
    {
        public DateTime? Start { get; set; }

        public DateTime? End { get; set; }

        public string? Status { get; set; }

        public string? ReasonForVisit { get; set; }

        public string? Notes { get; set; }

        public string? CancellationReason { get; set; }

        /// <summary>
        /// Room number (1-999) where the appointment will take place.
        /// If specified, validates no double-booking of rooms.
        /// </summary>
        public int? RoomNumber { get; set; }

        /// <summary>
        /// When this appointment was last modified.
        /// Used for tracking client-side modifications.
        /// </summary>
        public DateTime? ModifiedAt { get; set; }
    }
}
