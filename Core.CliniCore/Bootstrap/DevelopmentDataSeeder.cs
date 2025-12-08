using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.Repositories;
using Core.CliniCore.Scheduling;
using Core.CliniCore.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Core.CliniCore.Bootstrap
{
    /// <summary>
    /// Seeds repositories with development/demo data through the service/repository abstraction layer.
    /// Storage-agnostic: works with any repository implementation (InMemory, EF Core, Remote).
    /// All credential values are sourced from SampleCredentials (single source of truth).
    /// </summary>
    public static class DevelopmentDataSeeder
    {
        // Configuration for random data generation
        private const int AdditionalPatientsToGenerate = 25;
        private const int AdditionalPhysiciansToGenerate = 8;

        /// <summary>
        /// Seeds repositories with sample data for development and demonstration.
        /// Works through the service layer, making it storage-agnostic.
        /// </summary>
        /// <param name="serviceProvider">The built service provider</param>
        /// <param name="createSampleData">Whether to create sample patient/physician data in addition to admin</param>
        public static void SeedDevelopmentData(IServiceProvider serviceProvider, bool createSampleData = true)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var authService = serviceProvider.GetRequiredService<IAuthenticationService>();
            var profileService = serviceProvider.GetRequiredService<ProfileService>();
            var schedulerService = serviceProvider.GetRequiredService<SchedulerService>();
            var credentialRepo = serviceProvider.GetRequiredService<ICredentialRepository>();

            try
            {
                if (!createSampleData)
                    return;

                var missingCredentials = DevelopmentData.GetMissingCredentials(credentialRepo).ToList();

                if (missingCredentials.Count == 0)
                {
                    Console.WriteLine("Development data already seeded.");
                    return;
                }

                Console.WriteLine("Seeding development data...");

                // Initialize random generator with consistent seed for reproducibility
                var generator = new RandomDataGenerator(seed: 42);

                // Reserve hardcoded usernames so they don't get regenerated
                generator.ReserveUsernames(DevelopmentData.ExpectedCredentials.Select(c => c.Username));

                // Track created profiles for appointment scheduling
                var physicians = new List<PhysicianProfile>();
                var patients = new List<PatientProfile>();

                // === Phase 1: Create hardcoded entries ===
                foreach (var credSpec in missingCredentials)
                {
                    switch (credSpec.Role)
                    {
                        case UserRole.Administrator:
                            CreateAdministrator(authService, profileService, credSpec);
                            break;

                        case UserRole.Physician:
                            var physician = CreatePhysician(authService, profileService, credSpec);
                            if (physician != null) physicians.Add(physician);
                            break;

                        case UserRole.Patient:
                            var patient = CreatePatient(authService, profileService, credSpec);
                            if (patient != null) patients.Add(patient);
                            break;
                    }
                }

                // === Phase 2: Generate additional random physicians ===
                Console.WriteLine($"Generating {AdditionalPhysiciansToGenerate} additional physicians...");
                for (int i = 0; i < AdditionalPhysiciansToGenerate; i++)
                {
                    var physician = CreateRandomPhysician(authService, profileService, generator);
                    if (physician != null) physicians.Add(physician);
                }

                // === Phase 3: Generate additional random patients ===
                Console.WriteLine($"Generating {AdditionalPatientsToGenerate} additional patients...");
                for (int i = 0; i < AdditionalPatientsToGenerate; i++)
                {
                    var patient = CreateRandomPatient(authService, profileService, generator);
                    if (patient != null) patients.Add(patient);
                }

                // === Phase 4: Schedule appointments with unique room numbers ===
                if (physicians.Count != 0 && patients.Count != 0)
                {
                    ScheduleSampleAppointments(patients, physicians, schedulerService, generator);
                }

                Console.WriteLine($"Development data seeding completed!");
                Console.WriteLine($"  Total physicians: {physicians.Count}");
                Console.WriteLine($"  Total patients: {patients.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not seed development data: {ex.Message}");
            }
        }

        #region Profile Creation (Hardcoded)

        private static void CreateAdministrator(
            IAuthenticationService authService,
            ProfileService profileService,
            DevCredentialSpec credSpec)
        {
            var admin = new AdministratorProfile { Username = credSpec.Username };
            admin.SetValue(DevelopmentData.CommonKeys.Name, SampleCredentials.AdminName);
            admin.SetValue(DevelopmentData.CommonKeys.Address, SampleCredentials.AdminAddress);
            admin.SetValue(DevelopmentData.CommonKeys.BirthDate, SampleCredentials.AdminBirthDate);
            admin.SetValue(DevelopmentData.AdministratorKeys.Email, SampleCredentials.AdminEmail);

            if (profileService.AddProfile(admin))
            {
                authService.Register(admin, credSpec.Password);
                Console.WriteLine($"  Created administrator: {SampleCredentials.AdminUsername}");
            }
        }

        private static PhysicianProfile? CreatePhysician(
            IAuthenticationService authService,
            ProfileService profileService,
            DevCredentialSpec credSpec)
        {
            var physician = new PhysicianProfile { Username = credSpec.Username };
            physician.SetValue(DevelopmentData.CommonKeys.Name, SampleCredentials.PhysicianName);
            physician.SetValue(DevelopmentData.CommonKeys.Address, SampleCredentials.PhysicianAddress);
            physician.SetValue(DevelopmentData.CommonKeys.BirthDate, SampleCredentials.PhysicianBirthDate);
            physician.SetValue(DevelopmentData.PhysicianKeys.LicenseNumber, SampleCredentials.PhysicianLicenseNumber);
            physician.SetValue(DevelopmentData.PhysicianKeys.GraduationDate, SampleCredentials.PhysicianGraduationDate);
            physician.SetValue(DevelopmentData.PhysicianKeys.Specializations, SampleCredentials.PhysicianSpecializations.ToList());

            if (profileService.AddProfile(physician))
            {
                authService.Register(physician, credSpec.Password);
                Console.WriteLine($"  Created physician: {SampleCredentials.PhysicianDisplayName}");
                return physician;
            }
            return null;
        }

        private static PatientProfile? CreatePatient(
            IAuthenticationService authService,
            ProfileService profileService,
            DevCredentialSpec credSpec)
        {
            // Map username to patient data from SampleCredentials
            var (name, address, birthDate, gender, race, password) = credSpec.Username switch
            {
                SampleCredentials.Patient1Username => (
                    SampleCredentials.Patient1Name,
                    SampleCredentials.Patient1Address,
                    SampleCredentials.Patient1BirthDate,
                    SampleCredentials.Patient1Gender,
                    SampleCredentials.Patient1Race,
                    SampleCredentials.Patient1Password),
                SampleCredentials.Patient2Username => (
                    SampleCredentials.Patient2Name,
                    SampleCredentials.Patient2Address,
                    SampleCredentials.Patient2BirthDate,
                    SampleCredentials.Patient2Gender,
                    SampleCredentials.Patient2Race,
                    SampleCredentials.Patient2Password),
                SampleCredentials.Patient3Username => (
                    SampleCredentials.Patient3Name,
                    SampleCredentials.Patient3Address,
                    SampleCredentials.Patient3BirthDate,
                    SampleCredentials.Patient3Gender,
                    SampleCredentials.Patient3Race,
                    SampleCredentials.Patient3Password),
                SampleCredentials.Patient4Username => (
                    SampleCredentials.Patient4Name,
                    SampleCredentials.Patient4Address,
                    SampleCredentials.Patient4BirthDate,
                    SampleCredentials.Patient4Gender,
                    SampleCredentials.Patient4Race,
                    SampleCredentials.Patient4Password),
                _ => throw new InvalidOperationException($"Unknown patient username: {credSpec.Username}")
            };

            var patient = new PatientProfile { Username = credSpec.Username };
            patient.SetValue(DevelopmentData.CommonKeys.Name, name);
            patient.SetValue(DevelopmentData.CommonKeys.Address, address);
            patient.SetValue(DevelopmentData.CommonKeys.BirthDate, birthDate);
            patient.SetValue(DevelopmentData.PatientKeys.Gender, gender);
            patient.SetValue(DevelopmentData.PatientKeys.Race, race);

            if (profileService.AddProfile(patient))
            {
                authService.Register(patient, password);
                Console.WriteLine($"  Created patient: {name}");
                return patient;
            }
            return null;
        }

        #endregion

        #region Random Profile Creation

        private static PhysicianProfile? CreateRandomPhysician(
            IAuthenticationService authService,
            ProfileService profileService,
            RandomDataGenerator generator)
        {
            var (name, lastName) = generator.GeneratePhysicianName();
            var username = generator.GenerateUsername("dr", lastName);
            var password = generator.GeneratePassword();
            var birthDate = generator.GenerateBirthDate(minAge: 30, maxAge: 65);
            var graduationDate = generator.GenerateGraduationDate(birthDate);
            var specializations = generator.GenerateSpecializations(min: 1, max: 5);
            var licenseNumber = generator.GenerateLicenseNumber();
            var address = generator.GenerateAddress();

            var physician = new PhysicianProfile { Username = username };
            physician.SetValue(DevelopmentData.CommonKeys.Name, name);
            physician.SetValue(DevelopmentData.CommonKeys.Address, address);
            physician.SetValue(DevelopmentData.CommonKeys.BirthDate, birthDate);
            physician.SetValue(DevelopmentData.PhysicianKeys.LicenseNumber, licenseNumber);
            physician.SetValue(DevelopmentData.PhysicianKeys.GraduationDate, graduationDate);
            physician.SetValue(DevelopmentData.PhysicianKeys.Specializations, specializations);

            if (profileService.AddProfile(physician))
            {
                authService.Register(physician, password);
                var specCount = specializations.Count;
                Console.WriteLine($"  Generated physician: Dr. {name} ({specCount} specialization{(specCount != 1 ? "s" : "")})");
                return physician;
            }
            return null;
        }

        private static PatientProfile? CreateRandomPatient(
            IAuthenticationService authService,
            ProfileService profileService,
            RandomDataGenerator generator)
        {
            var (fullName, firstName, lastName, gender) = generator.GeneratePatientName();
            var username = generator.GenerateUsername(firstName, lastName);
            var password = generator.GeneratePassword();
            var birthDate = generator.GenerateBirthDate(minAge: 18, maxAge: 85);
            var address = generator.GenerateAddress();
            var race = generator.RandomRace();

            var patient = new PatientProfile { Username = username };
            patient.SetValue(DevelopmentData.CommonKeys.Name, fullName);
            patient.SetValue(DevelopmentData.CommonKeys.Address, address);
            patient.SetValue(DevelopmentData.CommonKeys.BirthDate, birthDate);
            patient.SetValue(DevelopmentData.PatientKeys.Gender, gender);
            patient.SetValue(DevelopmentData.PatientKeys.Race, race);

            if (profileService.AddProfile(patient))
            {
                authService.Register(patient, password);
                Console.WriteLine($"  Generated patient: {fullName}");
                return patient;
            }
            return null;
        }

        #endregion

        #region Appointment Scheduling

        /// <summary>
        /// Schedules sample appointments with guaranteed unique room numbers.
        /// </summary>
        private static void ScheduleSampleAppointments(
            List<PatientProfile> patients,
            List<PhysicianProfile> physicians,
            SchedulerService schedulerService,
            RandomDataGenerator generator)
        {
            Console.WriteLine("Scheduling sample appointments...");

            // Start appointments on next Monday at 8 AM
            var baseAppointmentTime = GetNextWeekday(DateTime.Now.AddDays(7).Date).AddHours(8);
            var appointmentsScheduled = 0;

            // Distribute patients among physicians
            for (int i = 0; i < patients.Count; i++)
            {
                var patient = patients[i];
                var physician = physicians[i % physicians.Count]; // Round-robin distribution
                var reason = generator.RandomVisitReason();

                if (ScheduleSampleAppointment(patient, physician, schedulerService, generator, ref baseAppointmentTime, reason))
                {
                    appointmentsScheduled++;
                }
            }

            Console.WriteLine($"  Scheduled {appointmentsScheduled} appointments");
        }

        /// <summary>
        /// Schedules a sample appointment for a patient with conflict-aware time slot allocation
        /// and guaranteed unique room numbers.
        /// </summary>
        private static bool ScheduleSampleAppointment(
            PatientProfile patient,
            PhysicianProfile physician,
            SchedulerService schedulerService,
            RandomDataGenerator generator,
            ref DateTime baseTime,
            string reason)
        {
            const int SLOT_DURATION_MINUTES = 30;
            const int MAX_ATTEMPTS = 50;

            // Ensure we start on a weekday during business hours
            DateTime attemptTime = GetNextWeekday(baseTime.Date).AddHours(Math.Max(8, Math.Min(16, baseTime.Hour)));

            for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
            {
                // Generate unique room number
                int roomNumber;
                try
                {
                    roomNumber = generator.GenerateUniqueRoomNumber(100, 999);
                }
                catch (InvalidOperationException)
                {
                    // All room numbers used - this shouldn't happen with reasonable data sizes
                    Console.WriteLine("  Warning: Room number pool exhausted");
                    return false;
                }

                var appointment = new AppointmentTimeInterval(
                    attemptTime,
                    attemptTime.AddMinutes(SLOT_DURATION_MINUTES),
                    patient.Id,
                    physician.Id,
                    reason,
                    AppointmentStatus.Scheduled
                );

                appointment.ReasonForVisit = reason;
                appointment.RoomNumber = roomNumber;
                appointment.Notes = $"Auto-generated appointment for {patient.Name}";

                var result = schedulerService.ScheduleAppointment(appointment);
                if (result.Success)
                {
                    baseTime = GetNextBusinessSlot(attemptTime, SLOT_DURATION_MINUTES);
                    return true;
                }

                // Conflict detected, try next business slot
                attemptTime = GetNextBusinessSlot(attemptTime, SLOT_DURATION_MINUTES);
            }

            Console.WriteLine($"  Warning: Could not schedule appointment for {patient.Name} (no available slots)");
            return false;
        }

        #endregion

        #region Date/Time Helpers

        /// <summary>
        /// Gets the next weekday (Monday-Friday) from the given date.
        /// </summary>
        private static DateTime GetNextWeekday(DateTime date)
        {
            while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
            }
            return date;
        }

        /// <summary>
        /// Advances time to next business day slot, respecting weekends and business hours (8 AM - 5 PM).
        /// </summary>
        private static DateTime GetNextBusinessSlot(DateTime time, int slotMinutes)
        {
            var nextSlot = time.AddMinutes(slotMinutes);

            // If we go past 5 PM, move to next day at 8 AM
            if (nextSlot.Hour >= 17 || nextSlot.TimeOfDay > new TimeSpan(17, 0, 0))
            {
                nextSlot = nextSlot.Date.AddDays(1).AddHours(8);
            }

            // Skip weekends
            nextSlot = GetNextWeekday(nextSlot);

            return nextSlot;
        }

        #endregion
    }
}
