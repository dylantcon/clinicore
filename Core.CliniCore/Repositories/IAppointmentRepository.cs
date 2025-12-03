using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Scheduling;

namespace Core.CliniCore.Repositories
{
    /// <summary>
    /// Repository interface for appointment (time interval) operations.
    /// Extends generic repository with scheduling-specific queries.
    /// </summary>
    public interface IAppointmentRepository : IRepository<AppointmentTimeInterval>
    {
        /// <summary>
        /// Gets all appointments for a specific date
        /// </summary>
        IEnumerable<AppointmentTimeInterval> GetByDate(DateTime date);

        /// <summary>
        /// Gets all appointments for a specific physician
        /// </summary>
        IEnumerable<AppointmentTimeInterval> GetByPhysician(Guid physicianId);

        /// <summary>
        /// Gets all appointments for a specific patient
        /// </summary>
        IEnumerable<AppointmentTimeInterval> GetByPatient(Guid patientId);

        /// <summary>
        /// Gets appointments filtered by status
        /// </summary>
        IEnumerable<AppointmentTimeInterval> GetByStatus(AppointmentStatus status);

        /// <summary>
        /// Checks if a proposed appointment would conflict with existing ones
        /// </summary>
        bool HasConflict(Guid physicianId, DateTime start, TimeSpan duration, Guid? excludeAppointmentId = null);

        /// <summary>
        /// Gets available time slots for a physician on a given date
        /// </summary>
        IEnumerable<(DateTime Start, DateTime End)> GetAvailableSlots(Guid physicianId, DateTime date, TimeSpan duration);
    }
}
