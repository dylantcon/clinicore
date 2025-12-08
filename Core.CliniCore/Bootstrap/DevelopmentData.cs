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

        public const string AdminUsername = "admin";
        public const string AdminPassword = "admin123";
        public const string AdminName = "System Administrator";
        public const string AdminAddress = "123 Admin St, Medical Center, MC 12345";
        public static readonly DateTime AdminBirthDate = new(1980, 1, 1);
        public const string AdminEmail = "admin@clinicore.local";
        public const string AdminDepartment = "Administration";

        #endregion

        #region Physician

        public const string PhysicianUsername = "greeneggsnham";
        public const string PhysicianPassword = "password";
        public const string PhysicianName = "Seuss";
        public const string PhysicianAddress = "456 Medical Plaza, Whoville, WH 12345";
        public static readonly DateTime PhysicianBirthDate = new(1975, 3, 2);
        public const string PhysicianLicenseNumber = "MD12345";
        public static readonly DateTime PhysicianGraduationDate = new(2010, 5, 15);
        public static readonly IReadOnlyList<MedicalSpecialization> PhysicianSpecializations =
        [
            MedicalSpecialization.FamilyMedicine,
            MedicalSpecialization.Pediatrics
        ];

        // Display name for UI (derived from Name)
        public const string PhysicianDisplayName = "Dr. Seuss";

        #endregion

        #region Patient 1 (Primary sample patient)

        public const string Patient1Username = "jdoe";
        public const string Patient1Password = "patient123";
        public const string Patient1Name = "Jane Doe";
        public const string Patient1Address = "123 Main St, Anytown, USA";
        public static readonly DateTime Patient1BirthDate = new(1985, 3, 20);
        public static readonly Gender Patient1Gender = Gender.Woman;
        public const string Patient1Race = "White";

        // Display name alias
        public const string PatientDisplayName = Patient1Name;

        #endregion

        #region Patient 2

        public const string Patient2Username = "johndoe";
        public const string Patient2Password = "password";
        public const string Patient2Name = "John Doe";
        public const string Patient2Address = "456 Oak Ave";
        public static readonly DateTime Patient2BirthDate = new(1980, 6, 15);
        public static readonly Gender Patient2Gender = Gender.Man;
        public const string Patient2Race = "Other";

        #endregion

        #region Patient 3

        public const string Patient3Username = "asmith";
        public const string Patient3Password = "password";
        public const string Patient3Name = "Alice Smith";
        public const string Patient3Address = "789 Pine Rd";
        public static readonly DateTime Patient3BirthDate = new(1992, 9, 3);
        public static readonly Gender Patient3Gender = Gender.Woman;
        public const string Patient3Race = "Other";

        #endregion

        #region Patient 4

        public const string Patient4Username = "bwilson";
        public const string Patient4Password = "password";
        public const string Patient4Name = "Bob Wilson";
        public const string Patient4Address = "321 Elm St";
        public static readonly DateTime Patient4BirthDate = new(1975, 12, 25);
        public static readonly Gender Patient4Gender = Gender.Man;
        public const string Patient4Race = "Other";

        #endregion
    }

    /// <summary>
    /// Specification for a development credential.
    /// </summary>
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

        public static class CommonKeys
        {
            public static string Name => CommonEntryType.Name.GetKey();
            public static string Address => CommonEntryType.Address.GetKey();
            public static string BirthDate => CommonEntryType.BirthDate.GetKey();
        }

        public static class PatientKeys
        {
            public static string Gender => PatientEntryType.Gender.GetKey();
            public static string Race => PatientEntryType.Race.GetKey();
        }

        public static class PhysicianKeys
        {
            public static string LicenseNumber => PhysicianEntryType.LicenseNumber.GetKey();
            public static string GraduationDate => PhysicianEntryType.GraduationDate.GetKey();
            public static string Specializations => PhysicianEntryType.Specializations.GetKey();
        }

        public static class AdministratorKeys
        {
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
        public static IEnumerable<DevCredentialSpec> GetMissingCredentials(ICredentialRepository repo)
            => ExpectedCredentials.Where(c => !repo.Exists(c.Username));

        /// <summary>
        /// Checks if all expected development credentials exist.
        /// </summary>
        public static bool IsFullySeeded(ICredentialRepository repo)
            => !GetMissingCredentials(repo).Any();

        /// <summary>
        /// Gets credential spec by username.
        /// </summary>
        public static DevCredentialSpec? GetByUsername(string username)
            => ExpectedCredentials.FirstOrDefault(c =>
                c.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Gets all credential specs for a specific role.
        /// </summary>
        public static IEnumerable<DevCredentialSpec> GetByRole(UserRole role)
            => ExpectedCredentials.Where(c => c.Role == role);

        #endregion
    }
}
