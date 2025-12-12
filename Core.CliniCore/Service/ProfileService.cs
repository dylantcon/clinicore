using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Users;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.Repositories;

namespace Core.CliniCore.Service
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
    public class ProfileService(
        IPatientRepository patientRepo,
        IPhysicianRepository physicianRepo,
        IAdministratorRepository adminRepo,
        ICredentialRepository? credentialRepo = null,
        IAppointmentRepository? appointmentRepo = null,
        IClinicalDocumentRepository? clinicalDocRepo = null)
    {
        private readonly IPatientRepository _patientRepo = patientRepo ?? throw new ArgumentNullException(nameof(patientRepo));
        private readonly IPhysicianRepository _physicianRepo = physicianRepo ?? throw new ArgumentNullException(nameof(physicianRepo));
        private readonly IAdministratorRepository _adminRepo = adminRepo ?? throw new ArgumentNullException(nameof(adminRepo));
        private readonly ICredentialRepository? _credentialRepo = credentialRepo;
        private readonly IAppointmentRepository? _appointmentRepo = appointmentRepo;
        private readonly IClinicalDocumentRepository? _clinicalDocRepo = clinicalDocRepo;

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
                ?? _adminRepo.GetByUsername(username);
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
            ArgumentNullException.ThrowIfNull(profile);

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
        /// Removes a profile from the appropriate repository.
        /// Returns null on success, or an error message explaining why deletion failed.
        /// Also removes associated credentials when successful.
        /// </summary>
        public string? RemoveProfile(Guid profileId)
        {
            var profile = GetProfileById(profileId);
            if (profile == null)
                return "Profile not found";

            // Check for related data that would be orphaned
            var blockReason = GetDeletionBlockers(profileId, profile);
            if (blockReason != null)
                return blockReason;

            // Delete the profile from appropriate repository
            switch (profile)
            {
                case PatientProfile:
                    _patientRepo.Delete(profileId);
                    break;
                case PhysicianProfile:
                    _physicianRepo.Delete(profileId);
                    break;
                case AdministratorProfile:
                    _adminRepo.Delete(profileId);
                    break;
                default:
                    return "Unknown profile type";
            }

            // Delete associated credentials
            _credentialRepo?.Delete(profileId);

            return null; // Success
        }

        /// <summary>
        /// Checks if a profile can be safely deleted. Returns error message or null if deletable.
        /// Only blocks on scheduled/active appointments (cancelled/completed are historical).
        /// </summary>
        private string? GetDeletionBlockers(Guid profileId, IUserProfile profile)
        {
            var blockers = new List<string>();

            switch (profile)
            {
                case PatientProfile:
                    if (_appointmentRepo != null)
                    {
                        // Only block on scheduled appointments (not cancelled/completed)
                        var activeAppointments = _appointmentRepo.GetByPatient(profileId)
                            .Count(a => a.Status == Domain.Enumerations.AppointmentStatus.Scheduled);
                        if (activeAppointments > 0)
                            blockers.Add($"{activeAppointments} scheduled appointment(s)");
                    }
                    if (_clinicalDocRepo != null)
                    {
                        var docCount = _clinicalDocRepo.GetByPatient(profileId).Count();
                        if (docCount > 0)
                            blockers.Add($"{docCount} clinical document(s)");
                    }
                    break;

                case PhysicianProfile physician:
                    if (_appointmentRepo != null)
                    {
                        var activeAppointments = _appointmentRepo.GetByPhysician(profileId)
                            .Count(a => a.Status == Domain.Enumerations.AppointmentStatus.Scheduled);
                        if (activeAppointments > 0)
                            blockers.Add($"{activeAppointments} scheduled appointment(s)");
                    }
                    if (_clinicalDocRepo != null)
                    {
                        var docCount = _clinicalDocRepo.GetByPhysician(profileId).Count();
                        if (docCount > 0)
                            blockers.Add($"{docCount} clinical document(s)");
                    }
                    if (physician.PatientIds.Count > 0)
                        blockers.Add($"{physician.PatientIds.Count} assigned patient(s)");
                    break;

                case AdministratorProfile:
                    var adminCount = _adminRepo.GetAll().Count();
                    if (adminCount <= 1)
                        return "Cannot delete the last administrator";
                    break;
            }

            return blockers.Count > 0
                ? $"Has {string.Join(", ", blockers)}"
                : null;
        }

        /// <summary>
        /// Updates a profile in the appropriate repository based on its type
        /// </summary>
        public bool UpdateProfile(IUserProfile profile)
        {
            ArgumentNullException.ThrowIfNull(profile);

            switch (profile)
            {
                case PatientProfile patient:
                    _patientRepo.Update(patient);
                    return true;
                case PhysicianProfile physician:
                    _physicianRepo.Update(physician);
                    return true;
                case AdministratorProfile admin:
                    _adminRepo.Update(admin);
                    return true;
                default:
                    throw new ArgumentException($"Unknown profile type: {profile.GetType().Name}");
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

            return [];
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
                _ => []
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
                return [];

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
