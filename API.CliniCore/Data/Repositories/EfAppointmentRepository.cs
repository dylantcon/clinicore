using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Repositories;
using Core.CliniCore.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace API.CliniCore.Data.Repositories
{
    /// <summary>
    /// Entity Framework Core implementation of IAppointmentRepository.
    /// Provides SQLite-backed persistence for appointments.
    /// </summary>
    public class EfAppointmentRepository : IAppointmentRepository
    {
        private readonly CliniCoreDbContext _context;

        public EfAppointmentRepository(CliniCoreDbContext context)
        {
            _context = context;
        }

        public AppointmentTimeInterval? GetById(Guid id)
        {
            var entity = _context.Appointments.Find(id);
            return entity?.ToDomain();
        }

        public IEnumerable<AppointmentTimeInterval> GetAll()
        {
            return _context.Appointments
                .AsNoTracking()
                .ToList()
                .Select(e => e.ToDomain());
        }

        public void Add(AppointmentTimeInterval appointment)
        {
            var entity = appointment.ToEntity();
            _context.Appointments.Add(entity);
            _context.SaveChanges();
        }

        public void Update(AppointmentTimeInterval appointment)
        {
            var entity = appointment.ToEntity();
            var existing = _context.Appointments.Find(entity.Id);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                _context.SaveChanges();
            }
        }

        public void Delete(Guid id)
        {
            var entity = _context.Appointments.Find(id);
            if (entity != null)
            {
                _context.Appointments.Remove(entity);
                _context.SaveChanges();
            }
        }

        public IEnumerable<AppointmentTimeInterval> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return GetAll();

            var lowerQuery = query.ToLowerInvariant();

            return _context.Appointments
                .AsNoTracking()
                .Where(a =>
                    (a.ReasonForVisit != null && a.ReasonForVisit.ToLower().Contains(lowerQuery)) ||
                    (a.Notes != null && a.Notes.ToLower().Contains(lowerQuery)) ||
                    a.AppointmentType.ToLower().Contains(lowerQuery))
                .ToList()
                .Select(e => e.ToDomain());
        }

        public IEnumerable<AppointmentTimeInterval> GetByDate(DateTime date)
        {
            var dateOnly = date.Date;
            var nextDay = dateOnly.AddDays(1);

            return _context.Appointments
                .AsNoTracking()
                .Where(a => a.Start >= dateOnly && a.Start < nextDay)
                .OrderBy(a => a.Start)
                .ToList()
                .Select(e => e.ToDomain());
        }

        public IEnumerable<AppointmentTimeInterval> GetByPhysician(Guid physicianId)
        {
            return _context.Appointments
                .AsNoTracking()
                .Where(a => a.PhysicianId == physicianId)
                .OrderBy(a => a.Start)
                .ToList()
                .Select(e => e.ToDomain());
        }

        public IEnumerable<AppointmentTimeInterval> GetByPatient(Guid patientId)
        {
            return _context.Appointments
                .AsNoTracking()
                .Where(a => a.PatientId == patientId)
                .OrderBy(a => a.Start)
                .ToList()
                .Select(e => e.ToDomain());
        }

        public IEnumerable<AppointmentTimeInterval> GetByStatus(AppointmentStatus status)
        {
            var statusName = status.ToString();

            return _context.Appointments
                .AsNoTracking()
                .Where(a => a.Status == statusName)
                .OrderBy(a => a.Start)
                .ToList()
                .Select(e => e.ToDomain());
        }

        public bool HasConflict(Guid physicianId, DateTime start, TimeSpan duration, Guid? excludeAppointmentId = null)
        {
            var end = start.Add(duration);

            var query = _context.Appointments
                .AsNoTracking()
                .Where(a =>
                    a.PhysicianId == physicianId &&
                    a.Status == AppointmentStatus.Scheduled.ToString() &&
                    a.Start < end &&
                    a.End > start);

            if (excludeAppointmentId.HasValue)
            {
                query = query.Where(a => a.Id != excludeAppointmentId.Value);
            }

            return query.Any();
        }

        public IEnumerable<(DateTime Start, DateTime End)> GetAvailableSlots(Guid physicianId, DateTime date, TimeSpan duration)
        {
            var slots = new List<(DateTime, DateTime)>();
            var dateOnly = date.Date;

            // Business hours: 8 AM to 5 PM
            var businessStart = dateOnly.AddHours(8);
            var businessEnd = dateOnly.AddHours(17);

            // Get all scheduled appointments for the physician on this date
            var appointments = _context.Appointments
                .AsNoTracking()
                .Where(a =>
                    a.PhysicianId == physicianId &&
                    a.Status == AppointmentStatus.Scheduled.ToString() &&
                    a.Start >= dateOnly &&
                    a.Start < dateOnly.AddDays(1))
                .OrderBy(a => a.Start)
                .ToList();

            var currentTime = businessStart;

            foreach (var apt in appointments)
            {
                // Add slot before this appointment if there's enough time
                if (apt.Start > currentTime && (apt.Start - currentTime) >= duration)
                {
                    slots.Add((currentTime, apt.Start));
                }
                currentTime = apt.End > currentTime ? apt.End : currentTime;
            }

            // Add slot after last appointment if there's enough time
            if (businessEnd > currentTime && (businessEnd - currentTime) >= duration)
            {
                slots.Add((currentTime, businessEnd));
            }

            return slots;
        }
    }
}
