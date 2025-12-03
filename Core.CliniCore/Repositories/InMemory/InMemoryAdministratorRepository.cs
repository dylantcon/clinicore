using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Enumerations;

namespace Core.CliniCore.Repositories.InMemory
{
    /// <summary>
    /// In-memory implementation of IAdministratorRepository.
    /// Provides administrator-specific query operations.
    /// </summary>
    public class InMemoryAdministratorRepository : InMemoryRepositoryBase<AdministratorProfile>, IAdministratorRepository
    {
        /// <summary>
        /// Gets all administrators in a specific department
        /// </summary>
        public IEnumerable<AdministratorProfile> GetByDepartment(string department)
        {
            if (string.IsNullOrWhiteSpace(department))
                return Enumerable.Empty<AdministratorProfile>();

            lock (_lock)
            {
                return _entities.Values
                    .Where(a => a.Department.Equals(department, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        /// <summary>
        /// Gets all administrators with a specific permission
        /// </summary>
        public IEnumerable<AdministratorProfile> GetByPermission(Permission permission)
        {
            lock (_lock)
            {
                return _entities.Values
                    .Where(a => a.GrantedPermissions.Contains(permission))
                    .ToList();
            }
        }

        /// <summary>
        /// Searches administrators by name, username, or department
        /// </summary>
        public override IEnumerable<AdministratorProfile> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return GetAll();

            var lowerQuery = query.ToLowerInvariant();

            lock (_lock)
            {
                return _entities.Values
                    .Where(a =>
                        a.Name.ToLowerInvariant().Contains(lowerQuery) ||
                        a.Username.ToLowerInvariant().Contains(lowerQuery) ||
                        a.Department.ToLowerInvariant().Contains(lowerQuery))
                    .ToList();
            }
        }

        /// <summary>
        /// Gets an administrator by their username
        /// </summary>
        public AdministratorProfile? GetByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            lock (_lock)
            {
                return _entities.Values
                    .FirstOrDefault(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
