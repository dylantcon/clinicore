using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users.Concrete;

namespace Core.CliniCore.Repositories.InMemory
{
    /// <summary>
    /// In-memory implementation of IPatientRepository.
    /// Provides patient-specific query operations.
    /// </summary>
    public class InMemoryPatientRepository : InMemoryRepositoryBase<PatientProfile>, IPatientRepository
    {
        /// <summary>
        /// Gets all patients assigned to a specific physician
        /// </summary>
        public IEnumerable<PatientProfile> GetByPhysician(Guid physicianId)
        {
            lock (_lock)
            {
                return _entities.Values
                    .Where(p => p.PrimaryPhysicianId == physicianId)
                    .ToList();
            }
        }

        /// <summary>
        /// Gets all patients not assigned to any physician
        /// </summary>
        public IEnumerable<PatientProfile> GetUnassigned()
        {
            lock (_lock)
            {
                return _entities.Values
                    .Where(p => !p.PrimaryPhysicianId.HasValue)
                    .ToList();
            }
        }

        /// <summary>
        /// Searches patients by name, username, or address
        /// </summary>
        public override IEnumerable<PatientProfile> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return GetAll();

            var lowerQuery = query.ToLowerInvariant();

            lock (_lock)
            {
                return _entities.Values
                    .Where(p =>
                        (p.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty).ToLowerInvariant().Contains(lowerQuery) ||
                        p.Username.ToLowerInvariant().Contains(lowerQuery) ||
                        (p.GetValue<string>(CommonEntryType.Address.GetKey()) ?? string.Empty).ToLowerInvariant().Contains(lowerQuery) ||
                        (p.GetValue<string>(PatientEntryType.Race.GetKey()) ?? string.Empty).ToLowerInvariant().Contains(lowerQuery))
                    .ToList();
            }
        }

        /// <summary>
        /// Gets a patient by their username
        /// </summary>
        public PatientProfile? GetByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            lock (_lock)
            {
                return _entities.Values
                    .FirstOrDefault(p => p.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
