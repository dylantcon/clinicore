using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Repositories;

namespace Core.CliniCore.Bootstrap
{
    /// <summary>
    /// Single source of truth for ALL development/demo data values.
    /// Grep for any dev credential string should ONLY find this file.
    ///
    /// Structure mirrors ProfileTemplates requirements:
    /// - CommonEntryType: Name, Address, BirthDate (all profiles)
    /// - PatientEntryType: Gender (required), Race (optional)
    /// - PhysicianEntryType: LicenseNumber, GraduationDate, Specializations (all required)
    /// - AdministratorEntryType: Email (required)
    /// </summary>
    public static class SampleCredentials
    {
        #region Administrator

        /// <summary>
        /// Development username for the administrator account.
        /// </summary>
        public const string AdminUsername = "admin";

        /// <summary>
        /// Development password for the administrator account.
        /// </summary>
        public const string AdminPassword = "admin123";

        /// <summary>
        /// Full name for the administrator profile.
        /// </summary>
        public const string AdminName = "System Administrator";

        /// <summary>
        /// Address for the administrator profile.
        /// </summary>
        public const string AdminAddress = "123 Admin St, Medical Center, MC 12345";

        /// <summary>
        /// Birth date for the administrator profile.
        /// </summary>
        public static readonly DateTime AdminBirthDate = new(1980, 1, 1);

        /// <summary>
        /// Email address for the administrator profile.
        /// </summary>
        public const string AdminEmail = "admin@clinicore.local";

        /// <summary>
        /// Department name for the administrator profile.
        /// </summary>
        public const string AdminDepartment = "Administration";

        #endregion

        #region Physician

        /// <summary>
        /// Development username for the physician account.
        /// </summary>
        public const string PhysicianUsername = "greeneggsnham";

        /// <summary>
        /// Development password for the physician account.
        /// </summary>
        public const string PhysicianPassword = "password";

        /// <summary>
        /// Full name for the physician profile.
        /// </summary>
        public const string PhysicianName = "Seuss";

        /// <summary>
        /// Address for the physician profile.
        /// </summary>
        public const string PhysicianAddress = "456 Medical Plaza, Whoville, WH 12345";

        /// <summary>
        /// Birth date for the physician profile.
        /// </summary>
        public static readonly DateTime PhysicianBirthDate = new(1975, 3, 2);

        /// <summary>
        /// Medical license number for the physician profile.
        /// </summary>
        public const string PhysicianLicenseNumber = "MD12345";

        /// <summary>
        /// Medical school graduation date for the physician profile.
        /// </summary>
        public static readonly DateTime PhysicianGraduationDate = new(2010, 5, 15);

        /// <summary>
        /// Medical specializations for the physician profile.
        /// </summary>
        public static readonly IReadOnlyList<MedicalSpecialization> PhysicianSpecializations =
        [
            MedicalSpecialization.FamilyMedicine,
            MedicalSpecialization.Pediatrics
        ];

        /// <summary>
        /// Display name for the physician (derived from Name), shown in UI.
        /// </summary>
        public const string PhysicianDisplayName = "Dr. Seuss";

        #endregion

        #region Patient 1 (Primary sample patient)

        /// <summary>
        /// Development username for the first patient account.
        /// </summary>
        public const string Patient1Username = "jdoe";

        /// <summary>
        /// Development password for the first patient account.
        /// </summary>
        public const string Patient1Password = "patient123";

        /// <summary>
        /// Full name for the first patient profile.
        /// </summary>
        public const string Patient1Name = "Jane Doe";

        /// <summary>
        /// Address for the first patient profile.
        /// </summary>
        public const string Patient1Address = "123 Main St, Anytown, USA";

        /// <summary>
        /// Birth date for the first patient profile.
        /// </summary>
        public static readonly DateTime Patient1BirthDate = new(1985, 3, 20);

        /// <summary>
        /// Gender for the first patient profile.
        /// </summary>
        public static readonly Gender Patient1Gender = Gender.Woman;

        /// <summary>
        /// Race for the first patient profile.
        /// </summary>
        public const string Patient1Race = "White";

        /// <summary>
        /// Display name alias for the primary patient (references Patient1Name).
        /// </summary>
        public const string PatientDisplayName = Patient1Name;

        #endregion

        #region Patient 2

        /// <summary>
        /// Development username for the second patient account.
        /// </summary>
        public const string Patient2Username = "johndoe";

        /// <summary>
        /// Development password for the second patient account.
        /// </summary>
        public const string Patient2Password = "password";

        /// <summary>
        /// Full name for the second patient profile.
        /// </summary>
        public const string Patient2Name = "John Doe";

        /// <summary>
        /// Address for the second patient profile.
        /// </summary>
        public const string Patient2Address = "456 Oak Ave";

        /// <summary>
        /// Birth date for the second patient profile.
        /// </summary>
        public static readonly DateTime Patient2BirthDate = new(1980, 6, 15);

        /// <summary>
        /// Gender for the second patient profile.
        /// </summary>
        public static readonly Gender Patient2Gender = Gender.Man;

        /// <summary>
        /// Race for the second patient profile.
        /// </summary>
        public const string Patient2Race = "Other";

        #endregion

        #region Patient 3

        /// <summary>
        /// Development username for the third patient account.
        /// </summary>
        public const string Patient3Username = "asmith";

        /// <summary>
        /// Development password for the third patient account.
        /// </summary>
        public const string Patient3Password = "password";

        /// <summary>
        /// Full name for the third patient profile.
        /// </summary>
        public const string Patient3Name = "Alice Smith";

        /// <summary>
        /// Address for the third patient profile.
        /// </summary>
        public const string Patient3Address = "789 Pine Rd";

        /// <summary>
        /// Birth date for the third patient profile.
        /// </summary>
        public static readonly DateTime Patient3BirthDate = new(1992, 9, 3);

        /// <summary>
        /// Gender for the third patient profile.
        /// </summary>
        public static readonly Gender Patient3Gender = Gender.Woman;

        /// <summary>
        /// Race for the third patient profile.
        /// </summary>
        public const string Patient3Race = "Other";

        #endregion

        #region Patient 4

        /// <summary>
        /// Development username for the fourth patient account.
        /// </summary>
        public const string Patient4Username = "bwilson";

        /// <summary>
        /// Development password for the fourth patient account.
        /// </summary>
        public const string Patient4Password = "password";

        /// <summary>
        /// Full name for the fourth patient profile.
        /// </summary>
        public const string Patient4Name = "Bob Wilson";

        /// <summary>
        /// Address for the fourth patient profile.
        /// </summary>
        public const string Patient4Address = "321 Elm St";

        /// <summary>
        /// Birth date for the fourth patient profile.
        /// </summary>
        public static readonly DateTime Patient4BirthDate = new(1975, 12, 25);

        /// <summary>
        /// Gender for the fourth patient profile.
        /// </summary>
        public static readonly Gender Patient4Gender = Gender.Man;

        /// <summary>
        /// Race for the fourth patient profile.
        /// </summary>
        public const string Patient4Race = "Other";

        #endregion
    }

    /// <summary>
    /// Specification for a development credential.
    /// </summary>
    /// <param name="Username">The username for login.</param>
    /// <param name="Password">The plaintext password (for development only).</param>
    /// <param name="Role">The user role (Patient, Physician, or Administrator).</param>
    public record DevCredentialSpec(
        string Username,
        string Password,
        UserRole Role);

    /// <summary>
    /// Canonical development data specifications and validation helpers.
    /// </summary>
    public static class DevelopmentData
    {
        /// <summary>
        /// Indicates whether the application is running in debug/development mode.
        /// Use this to conditionally display development credentials in UI.
        /// </summary>
        public static bool IsDebugMode =>
#if DEBUG
            true;
#else
            false;
#endif

        #region Entry Key Accessors (from EntryType extensions)

        /// <summary>
        /// Entry keys common to all profile types.
        /// </summary>
        public static class CommonKeys
        {
            /// <summary>
            /// Gets the entry key for the Name field.
            /// </summary>
            public static string Name => CommonEntryType.Name.GetKey();

            /// <summary>
            /// Gets the entry key for the Address field.
            /// </summary>
            public static string Address => CommonEntryType.Address.GetKey();

            /// <summary>
            /// Gets the entry key for the BirthDate field.
            /// </summary>
            public static string BirthDate => CommonEntryType.BirthDate.GetKey();
        }

        /// <summary>
        /// Entry keys specific to patient profiles.
        /// </summary>
        public static class PatientKeys
        {
            /// <summary>
            /// Gets the entry key for the Gender field.
            /// </summary>
            public static string Gender => PatientEntryType.Gender.GetKey();

            /// <summary>
            /// Gets the entry key for the Race field.
            /// </summary>
            public static string Race => PatientEntryType.Race.GetKey();
        }

        /// <summary>
        /// Entry keys specific to physician profiles.
        /// </summary>
        public static class PhysicianKeys
        {
            /// <summary>
            /// Gets the entry key for the LicenseNumber field.
            /// </summary>
            public static string LicenseNumber => PhysicianEntryType.LicenseNumber.GetKey();

            /// <summary>
            /// Gets the entry key for the GraduationDate field.
            /// </summary>
            public static string GraduationDate => PhysicianEntryType.GraduationDate.GetKey();

            /// <summary>
            /// Gets the entry key for the Specializations field.
            /// </summary>
            public static string Specializations => PhysicianEntryType.Specializations.GetKey();
        }

        /// <summary>
        /// Entry keys specific to administrator profiles.
        /// </summary>
        public static class AdministratorKeys
        {
            /// <summary>
            /// Gets the entry key for the Email field.
            /// </summary>
            public static string Email => AdministratorEntryType.Email.GetKey();
        }

        #endregion

        #region Credential Specifications

        /// <summary>
        /// All expected development credentials (built from SampleCredentials).
        /// </summary>
        public static IReadOnlyList<DevCredentialSpec> ExpectedCredentials => new[]
        {
            new DevCredentialSpec(SampleCredentials.AdminUsername, SampleCredentials.AdminPassword, UserRole.Administrator),
            new DevCredentialSpec(SampleCredentials.PhysicianUsername, SampleCredentials.PhysicianPassword, UserRole.Physician),
            new DevCredentialSpec(SampleCredentials.Patient1Username, SampleCredentials.Patient1Password, UserRole.Patient),
            new DevCredentialSpec(SampleCredentials.Patient2Username, SampleCredentials.Patient2Password, UserRole.Patient),
            new DevCredentialSpec(SampleCredentials.Patient3Username, SampleCredentials.Patient3Password, UserRole.Patient),
            new DevCredentialSpec(SampleCredentials.Patient4Username, SampleCredentials.Patient4Password, UserRole.Patient)
        };

        #endregion

        #region Validation Helpers

        /// <summary>
        /// Returns credentials that don't exist in the repository.
        /// </summary>
        /// <param name="repo">The credential repository to check against.</param>
        /// <returns>A collection of missing credential specifications.</returns>
        public static IEnumerable<DevCredentialSpec> GetMissingCredentials(ICredentialRepository repo)
            => ExpectedCredentials.Where(c => !repo.Exists(c.Username));

        /// <summary>
        /// Checks if all expected development credentials exist.
        /// </summary>
        /// <param name="repo">The credential repository to validate.</param>
        /// <returns>True if all credentials exist; otherwise, false.</returns>
        public static bool IsFullySeeded(ICredentialRepository repo)
            => !GetMissingCredentials(repo).Any();

        /// <summary>
        /// Gets credential spec by username.
        /// </summary>
        /// <param name="username">The username to search for (case-insensitive).</param>
        /// <returns>The matching credential specification, or null if not found.</returns>
        public static DevCredentialSpec? GetByUsername(string username)
            => ExpectedCredentials.FirstOrDefault(c =>
                c.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Gets all credential specs for a specific role.
        /// </summary>
        /// <param name="role">The user role to filter by.</param>
        /// <returns>A collection of credential specifications matching the specified role.</returns>
        public static IEnumerable<DevCredentialSpec> GetByRole(UserRole role)
            => ExpectedCredentials.Where(c => c.Role == role);

        #endregion
    }
}
