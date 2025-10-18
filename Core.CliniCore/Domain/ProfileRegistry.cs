using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.CliniCore.Domain.Enumerations;

namespace Core.CliniCore.Domain
{
    /// <summary>
    /// Registry for managing all user profiles in the system
    /// Implements Singleton pattern with thread-safe operations
    /// </summary>
    public class ProfileRegistry
    {
        private readonly Dictionary<Guid, IUserProfile> _profilesById;
        private readonly Dictionary<string, IUserProfile> _profilesByUsername;
        private readonly static object _lock = new object();
        private static ProfileRegistry? _instance;

        private ProfileRegistry()
        {
            _profilesById = new Dictionary<Guid, IUserProfile>();
            _profilesByUsername = new Dictionary<string, IUserProfile>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the singleton instance of the ProfileRegistry
        /// </summary>
        public static ProfileRegistry Instance
        {
            get
            {
                lock(_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new ProfileRegistry();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Adds a profile to the registry
        /// </summary>
        public bool AddProfile(IUserProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            lock (_lock)
            {
                if (_profilesById.ContainsKey(profile.Id))
                    return false;

                if (_profilesByUsername.ContainsKey(profile.Username))
                    return false;

                _profilesById[profile.Id] = profile;
                _profilesByUsername[profile.Username] = profile;
                return true;
            }
        }

        /// <summary>
        /// Removes a profile from the registry
        /// </summary>
        public bool RemoveProfile(Guid profileId)
        {
            lock (_lock)
            {
                if (_profilesById.TryGetValue(profileId, out var profile))
                {
                    _profilesById.Remove(profileId);
                    _profilesByUsername.Remove(profile.Username);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets a profile by ID
        /// </summary>
        public IUserProfile? GetProfileById(Guid profileId)
        {
            lock (_lock)
            {
                return _profilesById.TryGetValue(profileId, out var profile) ? profile : null;
            }
        }

        /// <summary>
        /// Gets a profile by username
        /// </summary>
        public IUserProfile? GetProfileByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;

            lock (_lock)
            {
                return _profilesByUsername.TryGetValue(username, out var profile) ? profile : null;
            }
        }

        /// <summary>
        /// Gets all profiles of a specific type
        /// </summary>
        public IEnumerable<T> GetProfilesByType<T>() where T : IUserProfile
        {
            lock (_lock)
            {
                return _profilesById.Values.OfType<T>().ToList();
            }
        }

        /// <summary>
        /// Gets all profiles with a specific role
        /// </summary>
        public IEnumerable<IUserProfile> GetProfilesByRole(UserRole role)
        {
            lock (_lock)
            {
                return _profilesById.Values.Where(p => p.Role == role).ToList();
            }
        }

        /// <summary>
        /// Gets all patient profiles
        /// </summary>
        public IEnumerable<PatientProfile> GetAllPatients()
        {
            lock (_lock)
            {
                return GetProfilesByType<PatientProfile>();
            }
        }

        /// <summary>
        /// Gets all physician profiles
        /// </summary>
        public IEnumerable<PhysicianProfile> GetAllPhysicians()
        {
            lock (_lock)
            {
                return GetProfilesByType<PhysicianProfile>();
            }
        }

        /// <summary>
        /// Gets all administrator profiles
        /// </summary>
        public IEnumerable<AdministratorProfile> GetAllAdministrators()
        {
            lock (_lock)
            {
                return GetProfilesByType<AdministratorProfile>();
            }
        }

        /// <summary>
        /// Searches for profiles by name
        /// </summary>
        public IEnumerable<IUserProfile> SearchByName(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return Enumerable.Empty<IUserProfile>();

            lock (_lock)
            {
                return _profilesById.Values
                    .Where(p =>
                    {
                        var name = p.GetValue<string>("name");
                        return !string.IsNullOrEmpty(name) &&
                               name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
                    })
                    .ToList();
            }
        }

        /// <summary>
        /// Checks if a username exists
        /// </summary>
        public bool UsernameExists(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;

            lock (_lock)
            {
                return _profilesByUsername.ContainsKey(username);
            }
        }

        /// <summary>
        /// Gets the total count of profiles
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _profilesById.Count;
                }
            }
        }

        /// <summary>
        /// Gets all profiles in the registry
        /// </summary>
        public IEnumerable<IUserProfile> GetAllProfiles()
        {
            lock (_lock)
            {
                return _profilesById.Values.ToList();
            }
        }

        /// <summary>
        /// Gets statistics about the registry
        /// </summary>
        public ProfileRegistryStatistics GetStatistics()
        {
            lock (_lock)
            {
                return new ProfileRegistryStatistics
                {
                    TotalProfiles = _profilesById.Count,
                    PatientCount = GetAllPatients().Count(),
                    PhysicianCount = GetAllPhysicians().Count(),
                    AdministratorCount = GetAllAdministrators().Count()
                };
            }
        }

        /// <summary>
        /// Clears all profiles (for testing purposes)
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _profilesById.Clear();
                _profilesByUsername.Clear();
            }
        }

        /// <summary>
        /// Establishes a physician-patient relationship
        /// </summary>
        public bool AssignPatientToPhysician(Guid patientId, Guid physicianId, bool setPrimary = false)
        {
            lock (_lock)
            {
                var patient = GetProfileById(patientId) as PatientProfile;
                var physician = GetProfileById(physicianId) as PhysicianProfile;

                if (patient == null || physician == null)
                    return false;

                // Add patient to physician's list
                if (!physician.PatientIds.Contains(patientId))
                {
                    physician.PatientIds.Add(patientId);
                }

                // Optionally set as primary physician
                if (setPrimary)
                {
                    patient.PrimaryPhysicianId = physicianId;
                }

                return true;
            }
        }

        /// <summary>
        /// Gets all patients for a physician
        /// </summary>
        public IEnumerable<PatientProfile> GetPhysicianPatients(Guid physicianId)
        {
            lock (_lock)
            {
                var physician = GetProfileById(physicianId) as PhysicianProfile;
                if (physician == null)
                    return Enumerable.Empty<PatientProfile>();

                return physician.PatientIds
                    .Select(id => GetProfileById(id) as PatientProfile)
                    .Where(p => p != null)
                    .Cast<PatientProfile>()
                    .ToList();
            }
        }

        /// <summary>
        /// Gets all physicians for a patient
        /// </summary>
        public IEnumerable<PhysicianProfile> GetPatientPhysicians(Guid patientId)
        {
            lock (_lock)
            {
                // Find all physicians who have this patient
                return _profilesById.Values
                    .OfType<PhysicianProfile>()
                    .Where(physician => physician.PatientIds.Contains(patientId))
                    .ToList();
            }
        }

    }

    /// <summary>
    /// Statistics about the profile registry
    /// </summary>
    public class ProfileRegistryStatistics
    {
        public int TotalProfiles { get; set; }
        public int PatientCount { get; set; }
        public int PhysicianCount { get; set; }
        public int AdministratorCount { get; set; }

        public override string ToString()
        {
            return $"Total: {TotalProfiles} (Patients: {PatientCount}, Physicians: {PhysicianCount}, Admins: {AdministratorCount})";
        }
    }
}
