using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users.Concrete;

namespace Core.CliniCore.Repositories.InMemory
{
    /// <summary>
    /// In-memory implementation of IPhysicianRepository.
    /// Provides physician-specific query operations.
    /// </summary>
    public class InMemoryPhysicianRepository : InMemoryRepositoryBase<PhysicianProfile>, IPhysicianRepository
    {
        /// <summary>
        /// Finds physicians by medical specialization
        /// </summary>
        public IEnumerable<PhysicianProfile> FindBySpecialization(MedicalSpecialization spec)
        {
            lock (_lock)
            {
                return _entities.Values
                    .Where(p =>
                    {
                        var specializations = p.GetValue<List<MedicalSpecialization>>(PhysicianEntryType.Specializations.GetKey());
                        return specializations != null && specializations.Contains(spec);
                    })
                    .ToList();
            }
        }

        /// <summary>
        /// Gets physicians available on a specific date.
        /// Note: Full availability check requires integration with ScheduleService.
        /// This method returns all physicians as a base - actual availability
        /// should be verified against their appointment schedules.
        /// </summary>
        public IEnumerable<PhysicianProfile> GetAvailableOn(DateTime date)
        {
            // For now, return all physicians on weekdays
            // Full availability check will be handled by ScheduleService
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                return Enumerable.Empty<PhysicianProfile>();

            lock (_lock)
            {
                return _entities.Values.ToList();
            }
        }

        /// <summary>
        /// Searches physicians by name, username, license number, or specialization
        /// </summary>
        public override IEnumerable<PhysicianProfile> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return GetAll();

            var lowerQuery = query.ToLowerInvariant();

            lock (_lock)
            {
                return _entities.Values
                    .Where(p =>
                    {
                        var name = p.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty;
                        var licenseNumber = p.GetValue<string>(PhysicianEntryType.LicenseNumber.GetKey()) ?? string.Empty;
                        var specializations = p.GetValue<List<MedicalSpecialization>>(PhysicianEntryType.Specializations.GetKey());

                        return name.ToLowerInvariant().Contains(lowerQuery) ||
                               p.Username.ToLowerInvariant().Contains(lowerQuery) ||
                               licenseNumber.ToLowerInvariant().Contains(lowerQuery) ||
                               (specializations != null && specializations.Any(s =>
                                   s.ToString().ToLowerInvariant().Contains(lowerQuery)));
                    })
                    .ToList();
            }
        }

        /// <summary>
        /// Gets a physician by their username
        /// </summary>
        public PhysicianProfile? GetByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            lock (_lock)
            {
                return _entities.Values
                    .FirstOrDefault(p => p.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Gets physicians with their patient counts
        /// </summary>
        public IEnumerable<(PhysicianProfile Physician, int PatientCount)> GetWithPatientCounts()
        {
            lock (_lock)
            {
                return _entities.Values
                    .Select(p => (p, p.PatientIds.Count))
                    .ToList();
            }
        }
    }
}
