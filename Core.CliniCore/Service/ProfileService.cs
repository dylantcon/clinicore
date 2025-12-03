using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Repositories;

namespace Core.CliniCore.Services
{
    /// <summary>
    /// Service for managing user profiles across all profile types.
    /// Provides cross-cutting operations that span multiple repositories.
    ///
    /// Design Notes:
    /// - This is a facade over the individual profile repositories
    /// - Use specific repositories (IPatientRepository, etc.) for type-specific operations
    /// - Use ProfileService for operations that need to work across profile types
    /// </summary>
    public class ProfileService
    {
        private readonly IPatientRepository _patientRepo;
        private readonly IPhysicianRepository _physicianRepo;
        private readonly IAdministratorRepository _adminRepo;

        public ProfileService(
            IPatientRepository patientRepo,
            IPhysicianRepository physicianRepo,
            IAdministratorRepository adminRepo)
        {
            _patientRepo = patientRepo ?? throw new ArgumentNullException(nameof(patientRepo));
            _physicianRepo = physicianRepo ?? throw new ArgumentNullException(nameof(physicianRepo));
            _adminRepo = adminRepo ?? throw new ArgumentNullException(nameof(adminRepo));
        }

        #region Cross-Profile Lookups

        /// <summary>
        /// Gets a profile by ID, checking all profile types
        /// </summary>
        public IUserProfile? GetProfileById(Guid profileId)
        {
            return (IUserProfile?)_patientRepo.GetById(profileId)
                ?? (IUserProfile?)_physicianRepo.GetById(profileId)
                ?? (IUserProfile?)_adminRepo.GetById(profileId);
        }

        /// <summary>
        /// Gets a profile by username, checking all profile types
        /// </summary>
        public IUserProfile? GetProfileByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;

            return (IUserProfile?)_patientRepo.GetByUsername(username)
                ?? (IUserProfile?)_physicianRepo.GetByUsername(username)
                ?? (IUserProfile?)_adminRepo.GetByUsername(username);
        }

        /// <summary>
        /// Checks if a username exists across all profile types
        /// </summary>
        public bool UsernameExists(string username)
        {
            return GetProfileByUsername(username) != null;
        }

        #endregion

        #region Profile Registration

        /// <summary>
        /// Adds a profile to the appropriate repository based on its type
        /// </summary>
        public bool AddProfile(IUserProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            // Check username uniqueness across all profile types
            if (UsernameExists(profile.Username))
                return false;

            switch (profile)
            {
                case PatientProfile patient:
                    _patientRepo.Add(patient);
                    return true;
                case PhysicianProfile physician:
                    _physicianRepo.Add(physician);
                    return true;
                case AdministratorProfile admin:
                    _adminRepo.Add(admin);
                    return true;
                default:
                    throw new ArgumentException($"Unknown profile type: {profile.GetType().Name}");
            }
        }

        /// <summary>
        /// Removes a profile from the appropriate repository
        /// </summary>
        public bool RemoveProfile(Guid profileId)
        {
            var profile = GetProfileById(profileId);
            if (profile == null) return false;

            switch (profile)
            {
                case PatientProfile:
                    _patientRepo.Delete(profileId);
                    return true;
                case PhysicianProfile:
                    _physicianRepo.Delete(profileId);
                    return true;
                case AdministratorProfile:
                    _adminRepo.Delete(profileId);
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        #region Type-Specific Accessors

        /// <summary>
        /// Gets all profiles of a specific type
        /// </summary>
        public IEnumerable<T> GetProfilesByType<T>() where T : class, IUserProfile
        {
            if (typeof(T) == typeof(PatientProfile))
                return _patientRepo.GetAll().Cast<T>();
            if (typeof(T) == typeof(PhysicianProfile))
                return _physicianRepo.GetAll().Cast<T>();
            if (typeof(T) == typeof(AdministratorProfile))
                return _adminRepo.GetAll().Cast<T>();

            return Enumerable.Empty<T>();
        }

        /// <summary>
        /// Gets all profiles with a specific role
        /// </summary>
        public IEnumerable<IUserProfile> GetProfilesByRole(UserRole role)
        {
            return role switch
            {
                UserRole.Patient => _patientRepo.GetAll().Cast<IUserProfile>(),
                UserRole.Physician => _physicianRepo.GetAll().Cast<IUserProfile>(),
                UserRole.Administrator => _adminRepo.GetAll().Cast<IUserProfile>(),
                _ => Enumerable.Empty<IUserProfile>()
            };
        }

        /// <summary>
        /// Gets all patient profiles
        /// </summary>
        public IEnumerable<PatientProfile> GetAllPatients() => _patientRepo.GetAll();

        /// <summary>
        /// Gets all physician profiles
        /// </summary>
        public IEnumerable<PhysicianProfile> GetAllPhysicians() => _physicianRepo.GetAll();

        /// <summary>
        /// Gets all administrator profiles
        /// </summary>
        public IEnumerable<AdministratorProfile> GetAllAdministrators() => _adminRepo.GetAll();

        /// <summary>
        /// Gets all profiles in the system
        /// </summary>
        public IEnumerable<IUserProfile> GetAllProfiles()
        {
            return _patientRepo.GetAll().Cast<IUserProfile>()
                .Concat(_physicianRepo.GetAll().Cast<IUserProfile>())
                .Concat(_adminRepo.GetAll().Cast<IUserProfile>());
        }

        #endregion

        #region Search

        /// <summary>
        /// Searches for profiles by name across all profile types
        /// </summary>
        public IEnumerable<IUserProfile> SearchByName(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<IUserProfile>();

            // Use the Search method on each repository
            return _patientRepo.Search(searchTerm).Cast<IUserProfile>()
                .Concat(_physicianRepo.Search(searchTerm).Cast<IUserProfile>())
                .Concat(_adminRepo.Search(searchTerm).Cast<IUserProfile>());
        }

        #endregion

        #region Physician-Patient Relationships

        /// <summary>
        /// Establishes a physician-patient relationship
        /// </summary>
        public bool AssignPatientToPhysician(Guid patientId, Guid physicianId, bool setPrimary = false)
        {
            var patient = _patientRepo.GetById(patientId);
            var physician = _physicianRepo.GetById(physicianId);

            if (patient == null || physician == null)
                return false;

            // Add patient to physician's list
            if (!physician.PatientIds.Contains(patientId))
            {
                physician.PatientIds.Add(patientId);
                _physicianRepo.Update(physician);
            }

            // Optionally set as primary physician
            if (setPrimary)
            {
                patient.PrimaryPhysicianId = physicianId;
                _patientRepo.Update(patient);
            }

            return true;
        }

        /// <summary>
        /// Gets all patients for a physician
        /// </summary>
        public IEnumerable<PatientProfile> GetPhysicianPatients(Guid physicianId)
        {
            return _patientRepo.GetByPhysician(physicianId);
        }

        /// <summary>
        /// Gets all physicians for a patient (those who have this patient in their list)
        /// </summary>
        public IEnumerable<PhysicianProfile> GetPatientPhysicians(Guid patientId)
        {
            return _physicianRepo.GetAll()
                .Where(physician => physician.PatientIds.Contains(patientId));
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Gets the total count of all profiles
        /// </summary>
        public int Count =>
            _patientRepo.GetAll().Count() +
            _physicianRepo.GetAll().Count() +
            _adminRepo.GetAll().Count();

        /// <summary>
        /// Gets statistics about profile counts
        /// </summary>
        public ProfileServiceStatistics GetStatistics()
        {
            return new ProfileServiceStatistics
            {
                TotalProfiles = Count,
                PatientCount = _patientRepo.GetAll().Count(),
                PhysicianCount = _physicianRepo.GetAll().Count(),
                AdministratorCount = _adminRepo.GetAll().Count()
            };
        }

        #endregion
    }

    /// <summary>
    /// Statistics about the profile service
    /// </summary>
    public class ProfileServiceStatistics
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
