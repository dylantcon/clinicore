using System.ComponentModel.DataAnnotations;

namespace Core.CliniCore.Requests.Appointments
{
    /// <summary>
    /// Request DTO for creating a new appointment
    /// </summary>
    public class CreateAppointmentRequest
    {
        [Required(ErrorMessage = "Start time is required")]
        public DateTime Start { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public DateTime End { get; set; }

        [Required(ErrorMessage = "Patient ID is required")]
        public Guid PatientId { get; set; }

        [Required(ErrorMessage = "Physician ID is required")]
        public Guid PhysicianId { get; set; }

        public string? ReasonForVisit { get; set; }

        public string? Notes { get; set; }

        /// <summary>
        /// Room number (1-999) where the appointment will take place.
        /// If specified, validates no double-booking of rooms.
        /// </summary>
        [Range(1, 999, ErrorMessage = "Room number must be between 1 and 999")]
        public int? RoomNumber { get; set; }
    }
}
