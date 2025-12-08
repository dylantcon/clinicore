using System.Text.Json;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace API.CliniCore.Data.Repositories
{
    /// <summary>
    /// Entity Framework Core implementation of IPhysicianRepository.
    /// Provides SQLite-backed persistence for physician profiles.
    /// </summary>
    public class EfPhysicianRepository : IPhysicianRepository
    {
        private readonly CliniCoreDbContext _context;

        public EfPhysicianRepository(CliniCoreDbContext context)
        {
            _context = context;
        }

        public PhysicianProfile? GetById(Guid id)
        {
            var entity = _context.Physicians.Find(id);
            return entity?.ToDomain();
        }

        public IEnumerable<PhysicianProfile> GetAll()
        {
            return _context.Physicians
                .AsNoTracking()
                .ToList()
                .Select(e => e.ToDomain());
        }

        public void Add(PhysicianProfile physician)
        {
            var entity = physician.ToEntity();
            _context.Physicians.Add(entity);
            _context.SaveChanges();
        }

        public void Update(PhysicianProfile physician)
        {
            var entity = physician.ToEntity();
            var existing = _context.Physicians.Find(entity.Id);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                _context.SaveChanges();
            }
        }

        public void Delete(Guid id)
        {
            var entity = _context.Physicians.Find(id);
            if (entity != null)
            {
                _context.Physicians.Remove(entity);
                _context.SaveChanges();
            }
        }

        public IEnumerable<PhysicianProfile> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return GetAll();

            var lowerQuery = query.ToLowerInvariant();

            return _context.Physicians
                .AsNoTracking()
                .Where(p =>
                    p.Name.ToLower().Contains(lowerQuery) ||
                    p.Username.ToLower().Contains(lowerQuery) ||
                    p.LicenseNumber.ToLower().Contains(lowerQuery))
                .ToList()
                .Select(e => e.ToDomain());
        }

        public PhysicianProfile? GetByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            var entity = _context.Physicians
                .AsNoTracking()
                .FirstOrDefault(p => p.Username.ToLower() == username.ToLower());

            return entity?.ToDomain();
        }

        public IEnumerable<PhysicianProfile> FindBySpecialization(MedicalSpecialization spec)
        {
            var specName = spec.ToString();

            // Since specializations are stored as JSON, we need to filter in memory
            return _context.Physicians
                .AsNoTracking()
                .ToList()
                .Where(p =>
                {
                    var specs = JsonSerializer.Deserialize<List<string>>(p.SpecializationsJson) ?? new();
                    return specs.Contains(specName, StringComparer.OrdinalIgnoreCase);
                })
                .Select(e => e.ToDomain());
        }

        public IEnumerable<PhysicianProfile> GetAvailableOn(DateTime date)
        {
            // For simplicity, return all physicians
            // A full implementation would check availability schedules
            return GetAll();
        }
    }
}
