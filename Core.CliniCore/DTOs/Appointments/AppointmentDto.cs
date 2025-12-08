using System;

namespace Core.CliniCore.DTOs.Appointments
{
    /// <summary>
    /// Response DTO representing an appointment
    /// </summary>
    public class AppointmentDto
    {
        public Guid Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int DurationMinutes { get; set; }
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public Guid PhysicianId { get; set; }
        public string? PhysicianName { get; set; }
        public string Status { get; set; } = string.Empty;
        public string AppointmentType { get; set; } = string.Empty;
        public string? ReasonForVisit { get; set; }
        public string? Notes { get; set; }
        public Guid? ClinicalDocumentId { get; set; }
        public string? CancellationReason { get; set; }
        public int? RoomNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
