using System.Text.Json;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace API.CliniCore.Data.Repositories
{
    /// <summary>
    /// Entity Framework Core implementation of IAdministratorRepository.
    /// Provides SQLite-backed persistence for administrator profiles.
    /// </summary>
    public class EfAdministratorRepository : IAdministratorRepository
    {
        private readonly CliniCoreDbContext _context;

        public EfAdministratorRepository(CliniCoreDbContext context)
        {
            _context = context;
        }

        public AdministratorProfile? GetById(Guid id)
        {
            var entity = _context.Administrators.Find(id);
            return entity?.ToDomain();
        }

        public IEnumerable<AdministratorProfile> GetAll()
        {
            return _context.Administrators
                .AsNoTracking()
                .ToList()
                .Select(e => e.ToDomain());
        }

        public void Add(AdministratorProfile admin)
        {
            var entity = admin.ToEntity();
            _context.Administrators.Add(entity);
            _context.SaveChanges();
        }

        public void Update(AdministratorProfile admin)
        {
            var entity = admin.ToEntity();
            var existing = _context.Administrators.Find(entity.Id);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                _context.SaveChanges();
            }
        }

        public void Delete(Guid id)
        {
            var entity = _context.Administrators.Find(id);
            if (entity != null)
            {
                _context.Administrators.Remove(entity);
                _context.SaveChanges();
            }
        }

        public IEnumerable<AdministratorProfile> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return GetAll();

            var lowerQuery = query.ToLowerInvariant();

            return _context.Administrators
                .AsNoTracking()
                .Where(a =>
                    a.Name.ToLower().Contains(lowerQuery) ||
                    a.Username.ToLower().Contains(lowerQuery) ||
                    a.Department.ToLower().Contains(lowerQuery))
                .ToList()
                .Select(e => e.ToDomain());
        }

        public AdministratorProfile? GetByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            var entity = _context.Administrators
                .AsNoTracking()
                .FirstOrDefault(a => a.Username.ToLower() == username.ToLower());

            return entity?.ToDomain();
        }

        public IEnumerable<AdministratorProfile> GetByDepartment(string department)
        {
            if (string.IsNullOrWhiteSpace(department))
                return Enumerable.Empty<AdministratorProfile>();

            return _context.Administrators
                .AsNoTracking()
                .Where(a => a.Department.ToLower() == department.ToLower())
                .ToList()
                .Select(e => e.ToDomain());
        }

        public IEnumerable<AdministratorProfile> GetByPermission(Permission permission)
        {
            var permName = permission.ToString();

            // Since permissions are stored as JSON, we need to filter in memory
            return _context.Administrators
                .AsNoTracking()
                .ToList()
                .Where(a =>
                {
                    var perms = JsonSerializer.Deserialize<List<string>>(a.PermissionsJson) ?? new();
                    return perms.Contains(permName, StringComparer.OrdinalIgnoreCase);
                })
                .Select(e => e.ToDomain());
        }
    }
}
