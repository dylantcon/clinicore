using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Users.Concrete;

namespace Core.CliniCore.Repositories
{
    /// <summary>
    /// Repository interface for physician profile operations.
    /// Extends generic repository with physician-specific queries.
    /// </summary>
    public interface IPhysicianRepository : IRepository<PhysicianProfile>
    {
        /// <summary>
        /// Gets a physician by their username
        /// </summary>
        PhysicianProfile? GetByUsername(string username);

        /// <summary>
        /// Finds physicians by medical specialization
        /// </summary>
        IEnumerable<PhysicianProfile> FindBySpecialization(MedicalSpecialization spec);

        /// <summary>
        /// Gets physicians available on a specific date
        /// </summary>
        IEnumerable<PhysicianProfile> GetAvailableOn(DateTime date);
    }
}
