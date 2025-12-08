using Core.CliniCore.Domain.Users.Concrete;

namespace Core.CliniCore.Repositories
{
    /// <summary>
    /// Repository interface for patient profile operations.
    /// Extends generic repository with patient-specific queries.
    /// </summary>
    public interface IPatientRepository : IRepository<PatientProfile>
    {
        /// <summary>
        /// Gets a patient by their username
        /// </summary>
        PatientProfile? GetByUsername(string username);

        /// <summary>
        /// Gets all patients assigned to a specific physician
        /// </summary>
        IEnumerable<PatientProfile> GetByPhysician(Guid physicianId);

        /// <summary>
        /// Gets all patients not assigned to any physician
        /// </summary>
        IEnumerable<PatientProfile> GetUnassigned();
    }
}
