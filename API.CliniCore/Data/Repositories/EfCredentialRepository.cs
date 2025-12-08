using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Repositories;
using Core.CliniCore.Service;
using Microsoft.EntityFrameworkCore;

namespace API.CliniCore.Data.Repositories
{
    /// <summary>
    /// Entity Framework Core implementation of ICredentialRepository.
    /// Provides SQLite-backed persistence for user credentials.
    /// </summary>
    public class EfCredentialRepository : ICredentialRepository
    {
        private readonly CliniCoreDbContext _context;

        public EfCredentialRepository(CliniCoreDbContext context)
        {
            _context = context;
        }

        public UserCredential? GetById(Guid id)
        {
            var entity = _context.UserCredentials.Find(id);
            return entity?.ToDomain();
        }

        public UserCredential? GetByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            var entity = _context.UserCredentials
                .AsNoTracking()
                .FirstOrDefault(c => c.Username.ToLower() == username.ToLower());

            return entity?.ToDomain();
        }

        public bool Exists(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            return _context.UserCredentials
                .Any(c => c.Username.ToLower() == username.ToLower());
        }

        public IEnumerable<UserCredential> GetAll()
        {
            return _context.UserCredentials
                .AsNoTracking()
                .ToList()
                .Select(e => e.ToDomain());
        }

        public void Add(UserCredential credential)
        {
            var entity = credential.ToEntity();
            _context.UserCredentials.Add(entity);
            _context.SaveChanges();
        }

        public void Update(UserCredential credential)
        {
            var entity = credential.ToEntity();
            var existing = _context.UserCredentials.Find(entity.Id);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                _context.SaveChanges();
            }
        }

        public void Delete(Guid id)
        {
            var entity = _context.UserCredentials.Find(id);
            if (entity != null)
            {
                _context.UserCredentials.Remove(entity);
                _context.SaveChanges();
            }
        }

        public IEnumerable<UserCredential> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return GetAll();

            var lowerQuery = query.ToLowerInvariant();

            return _context.UserCredentials
                .AsNoTracking()
                .Where(c => c.Username.ToLower().Contains(lowerQuery))
                .ToList()
                .Select(e => e.ToDomain());
        }

        public UserCredential? Register(Guid id, string username, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            if (Exists(username))
                return null;

            var credential = new UserCredential
            {
                Id = id,
                Username = username,
                PasswordHash = BasicAuthenticationService.HashPassword(password),
                Role = role,
                CreatedAt = DateTime.UtcNow
            };

            Add(credential);
            return credential;
        }
    }
}
