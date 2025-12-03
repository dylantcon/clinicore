using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Enumerations;

namespace Core.CliniCore.Repositories
{
    /// <summary>
    /// Repository interface for administrator profile operations.
    /// Extends generic repository with administrator-specific queries.
    /// </summary>
    public interface IAdministratorRepository : IRepository<AdministratorProfile>
    {
        /// <summary>
        /// Gets an administrator by their username
        /// </summary>
        AdministratorProfile? GetByUsername(string username);

        /// <summary>
        /// Gets all administrators in a specific department
        /// </summary>
        IEnumerable<AdministratorProfile> GetByDepartment(string department);

        /// <summary>
        /// Gets all administrators with a specific permission
        /// </summary>
        IEnumerable<AdministratorProfile> GetByPermission(Permission permission);
    }
}
