using System;

namespace API.CliniCore.Data.Entities
{
    /// <summary>
    /// EF Core entity for appointment persistence.
    /// Maps closely to AppointmentTimeInterval.
    /// </summary>
    public class AppointmentEntity
    {
        public Guid Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Description { get; set; } = string.Empty;

        // Appointment specific
        public Guid PatientId { get; set; }
        public Guid PhysicianId { get; set; }
        public string Status { get; set; } = "Scheduled";
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string AppointmentType { get; set; } = "Standard Visit";
        public string? ReasonForVisit { get; set; }
        public string? Notes { get; set; }
        public Guid? ClinicalDocumentId { get; set; }
        public Guid? RescheduledFromId { get; set; }
        public string? CancellationReason { get; set; }
        public int? RoomNumber { get; set; }
    }
}
