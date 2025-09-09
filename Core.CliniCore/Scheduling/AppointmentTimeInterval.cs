using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.CliniCore.Domain.Enumerations;

namespace Core.CliniCore.Scheduling
{
    /// <summary>
    /// Represents a scheduled medical appointment time interval
    /// </summary>
    public class AppointmentTimeInterval : AbstractTimeInterval
    {
        /// <summary>
        /// Standard appointment durations
        /// </summary>
        public static class StandardDurations
        {
            public static readonly TimeSpan QuickCheckup = TimeSpan.FromMinutes(15);
            public static readonly TimeSpan StandardVisit = TimeSpan.FromMinutes(30);
            public static readonly TimeSpan ExtendedConsultation = TimeSpan.FromMinutes(45);
            public static readonly TimeSpan ComprehensiveExam = TimeSpan.FromMinutes(60);
            public static readonly TimeSpan Procedure = TimeSpan.FromMinutes(90);
        }

        public AppointmentTimeInterval(
            DateTime start,
            DateTime end,
            Guid patientId,
            Guid physicianId,
            string description = "",
            AppointmentStatus status = AppointmentStatus.Scheduled)
            : base(start, end, description)
        {
            PatientId = patientId;
            PhysicianId = physicianId;
            Status = status;
            CreatedAt = DateTime.Now;
            AppointmentType = DetermineAppointmentType(Duration);
        }

        /// <summary>
        /// The patient this appointment is for
        /// </summary>
        public Guid PatientId { get; set; }

        /// <summary>
        /// The physician conducting this appointment
        /// </summary>
        public Guid PhysicianId { get; set; }

        /// <summary>
        /// Current status of the appointment
        /// </summary>
        public AppointmentStatus Status { get; set; }

        /// <summary>
        /// When this appointment was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When this appointment was last modified
        /// </summary>
        public DateTime? ModifiedAt { get; set; }

        /// <summary>
        /// Type of appointment based on duration
        /// </summary>
        public string AppointmentType { get; set; }

        /// <summary>
        /// Reason for visit / chief complaint
        /// </summary>
        public string? ReasonForVisit { get; set; }

        /// <summary>
        /// Any notes about this appointment
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// ID of the clinical document created during this appointment
        /// </summary>
        public Guid? ClinicalDocumentId { get; set; }

        /// <summary>
        /// If this appointment was rescheduled from another
        /// </summary>
        public Guid? RescheduledFromId { get; set; }

        /// <summary>
        /// If this appointment was cancelled, the reason
        /// </summary>
        public string? CancellationReason { get; set; }

        /// <summary>
        /// Override to add appointment-specific description
        /// </summary>
        public override string Description
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(base.Description))
                    return base.Description;

                return $"{AppointmentType} - Patient: {PatientId:N}, Physician: {PhysicianId:N}";
            }
            protected set => base.Description = value;
        }

        /// <summary>
        /// Checks if this appointment can be rescheduled
        /// </summary>
        public bool CanReschedule()
        {
            return Status == AppointmentStatus.Scheduled &&
                   Start > DateTime.Now;
        }

        /// <summary>
        /// Checks if this appointment can be cancelled
        /// </summary>
        public bool CanCancel()
        {
            return Status == AppointmentStatus.Scheduled &&
                   Start > DateTime.Now;
        }

        /// <summary>
        /// Marks the appointment as completed
        /// </summary>
        public void MarkCompleted()
        {
            Status = AppointmentStatus.Completed;
            ModifiedAt = DateTime.Now;
        }

        /// <summary>
        /// Marks the appointment as cancelled
        /// </summary>
        public void Cancel(string reason = "")
        {
            Status = AppointmentStatus.Cancelled;
            CancellationReason = reason;
            ModifiedAt = DateTime.Now;
        }

        /// <summary>
        /// Marks the appointment as no-show
        /// </summary>
        public void MarkNoShow()
        {
            Status = AppointmentStatus.NoShow;
            ModifiedAt = DateTime.Now;
        }

        /// <summary>
        /// Creates a rescheduled copy of this appointment
        /// </summary>
        public AppointmentTimeInterval Reschedule(DateTime newStart, DateTime newEnd)
        {
            var rescheduled = new AppointmentTimeInterval(
                newStart,
                newEnd,
                PatientId,
                PhysicianId,
                Description,
                AppointmentStatus.Scheduled)
            {
                RescheduledFromId = this.Id,
                ReasonForVisit = this.ReasonForVisit,
                Notes = $"Rescheduled from {Start:yyyy-MM-dd HH:mm}. {Notes}"
            };

            // Cancel the original
            Cancel("Rescheduled");

            return rescheduled;
        }

        /// <summary>
        /// Checks if this appointment conflicts with another for the same physician
        /// </summary>
        public bool ConflictsWith(AppointmentTimeInterval other)
        {
            if (other == null || other.Id == this.Id)
                return false;

            // Only check conflicts for same physician
            if (PhysicianId != other.PhysicianId)
                return false;

            // Only active appointments can conflict
            if (Status != AppointmentStatus.Scheduled || other.Status != AppointmentStatus.Scheduled)
                return false;

            return Overlaps(other);
        }

        /// <summary>
        /// Override to add appointment-specific validation
        /// </summary>
        protected override List<string> GetSpecificValidationErrors()
        {
            var errors = new List<string>();

            if (PatientId == Guid.Empty)
            {
                errors.Add("Patient ID is required");
            }

            if (PhysicianId == Guid.Empty)
            {
                errors.Add("Physician ID is required");
            }

            // Must be within business hours for regular appointments
            if (!IsWithinBusinessHours())
            {
                errors.Add("Appointments must be scheduled during business hours (Monday-Friday, 8 AM - 5 PM)");
            }

            // Can't schedule appointments in the past
            if (Start < DateTime.Now && Status == AppointmentStatus.Scheduled)
            {
                errors.Add("Cannot schedule appointments in the past");
            }

            // Appointment duration validation
            if (Duration < StandardDurations.QuickCheckup)
            {
                errors.Add($"Appointment must be at least {StandardDurations.QuickCheckup.TotalMinutes} minutes");
            }

            if (Duration > TimeSpan.FromHours(3))
            {
                errors.Add("Appointment cannot exceed 3 hours");
            }

            return errors;
        }

        /// <summary>
        /// Creates a merged appointment interval (not typically used, but required by base)
        /// </summary>
        protected override ITimeInterval? CreateMergedInterval(
            DateTime start, DateTime end, string description, ITimeInterval other)
        {
            // Appointments typically don't merge, but if needed for scheduling blocks:
            if (other is AppointmentTimeInterval otherAppt &&
                otherAppt.PatientId == PatientId &&
                otherAppt.PhysicianId == PhysicianId)
            {
                return new AppointmentTimeInterval(start, end, PatientId, PhysicianId, description);
            }
            return null;
        }

        /// <summary>
        /// Determines appointment type based on duration
        /// </summary>
        private string DetermineAppointmentType(TimeSpan duration)
        {
            if (duration <= StandardDurations.QuickCheckup)
                return "Quick Checkup";
            else if (duration <= StandardDurations.StandardVisit)
                return "Standard Visit";
            else if (duration <= StandardDurations.ExtendedConsultation)
                return "Extended Consultation";
            else if (duration <= StandardDurations.ComprehensiveExam)
                return "Comprehensive Exam";
            else
                return "Extended Procedure";
        }

        public override string ToString()
        {
            return $"Appointment [{Status}]: {Start:yyyy-MM-dd HH:mm} - {End:HH:mm} " +
                   $"(Patient: {PatientId:N}, Physician: {PhysicianId:N})";
        }
    }
}