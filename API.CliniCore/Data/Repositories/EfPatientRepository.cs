using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace API.CliniCore.Data.Repositories
{
    /// <summary>
    /// Entity Framework Core implementation of IPatientRepository.
    /// Provides SQLite-backed persistence for patient profiles.
    /// </summary>
    public class EfPatientRepository : IPatientRepository
    {
        private readonly CliniCoreDbContext _context;

        public EfPatientRepository(CliniCoreDbContext context)
        {
            _context = context;
        }

        public PatientProfile? GetById(Guid id)
        {
            var entity = _context.Patients.Find(id);
            return entity?.ToDomain();
        }

        public IEnumerable<PatientProfile> GetAll()
        {
            return _context.Patients
                .AsNoTracking()
                .ToList()
                .Select(e => e.ToDomain());
        }

        public void Add(PatientProfile patient)
        {
            var entity = patient.ToEntity();
            _context.Patients.Add(entity);
            _context.SaveChanges();
        }

        public void Update(PatientProfile patient)
        {
            var entity = patient.ToEntity();
            var existing = _context.Patients.Find(entity.Id);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                _context.SaveChanges();
            }
        }

        public void Delete(Guid id)
        {
            var entity = _context.Patients.Find(id);
            if (entity != null)
            {
                _context.Patients.Remove(entity);
                _context.SaveChanges();
            }
        }

        public IEnumerable<PatientProfile> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return GetAll();

            var lowerQuery = query.ToLowerInvariant();

            return _context.Patients
                .AsNoTracking()
                .Where(p =>
                    p.Name.ToLower().Contains(lowerQuery) ||
                    p.Username.ToLower().Contains(lowerQuery) ||
                    (p.Address != null && p.Address.ToLower().Contains(lowerQuery)) ||
                    (p.Race != null && p.Race.ToLower().Contains(lowerQuery)))
                .ToList()
                .Select(e => e.ToDomain());
        }

        public PatientProfile? GetByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            var entity = _context.Patients
                .AsNoTracking()
                .FirstOrDefault(p => p.Username.ToLower() == username.ToLower());

            return entity?.ToDomain();
        }

        public IEnumerable<PatientProfile> GetByPhysician(Guid physicianId)
        {
            return _context.Patients
                .AsNoTracking()
                .Where(p => p.PrimaryPhysicianId == physicianId)
                .ToList()
                .Select(e => e.ToDomain());
        }

        public IEnumerable<PatientProfile> GetUnassigned()
        {
            return _context.Patients
                .AsNoTracking()
                .Where(p => p.PrimaryPhysicianId == null)
                .ToList()
                .Select(e => e.ToDomain());
        }
    }
}
