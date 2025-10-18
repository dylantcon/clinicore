using Core.CliniCore.ClinicalDoc;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Scheduling;
using Core.CliniCore.Scheduling.Management;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Core.CliniCore.Bootstrap
{
    /// <summary>
    /// Sample development credentials for testing and demonstration purposes.
    /// These credentials match the users created in InitializeDevelopmentData.
    /// </summary>
    public static class SampleCredentials
    {
        // Administrator
        public static string AdminUsername => "admin";
        public static string AdminPassword => "admin123";
        public static string AdminDisplayName => "System Administrator";

        // Physician
        public static string PhysicianUsername => "greeneggsnham";
        public static string PhysicianPassword => "password";
        public static string PhysicianDisplayName => "Dr. Seuss";

        // Patient
        public static string PatientUsername => "jdoe";
        public static string PatientPassword => "patient123";
        public static string PatientDisplayName => "Jane Doe";
    }

    /// <summary>
    /// Provides dependency injection registration for all core CliniCore services.
    /// This bootstrapper ensures consistent service configuration across different client applications (CLI, GUI, etc.)
    /// </summary>
    public static class CoreServiceBootstrapper
    {
        /// <summary>
        /// Registers all core CliniCore services with the dependency injection container.
        /// Call this method from your client application's startup/configuration.
        /// </summary>
        /// <param name="services">The service collection to register services with</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddCliniCoreServices(this IServiceCollection services)
        {
            // Core Authentication Services
            services.AddSingleton<IAuthenticationService, BasicAuthenticationService>();
            services.AddSingleton<RoleBasedAuthorizationService>();

            // Domain Registries (Repositories)
            // Note: These use Singleton pattern internally, so we register the existing instances
            services.AddSingleton<ProfileRegistry>(sp => ProfileRegistry.Instance);
            services.AddSingleton<ClinicalDocumentRegistry>(sp => ClinicalDocumentRegistry.Instance);

            // Scheduling Management
            // Note: ScheduleManager uses Singleton pattern internally, but we register it for DI
            services.AddSingleton<ScheduleManager>(sp => ScheduleManager.Instance);
            services.AddSingleton<ScheduleConflictResolver>();

            // Command Infrastructure
            services.AddSingleton<CommandInvoker>();

            // CommandFactory needs special handling due to its dependencies
            services.AddSingleton<CommandFactory>(serviceProvider =>
                new CommandFactory(
                    serviceProvider.GetRequiredService<IAuthenticationService>(),
                    serviceProvider.GetRequiredService<ScheduleManager>()
                ));

            return services;
        }

        /// <summary>
        /// Registers core services with custom implementations.
        /// Useful for testing or when you need to override default implementations.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="customAuthService">Custom authentication service implementation</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddCliniCoreServicesWithCustomAuth(
            this IServiceCollection services,
            IAuthenticationService customAuthService)
        {
            // Register the custom auth service
            services.AddSingleton<IAuthenticationService>(customAuthService);
            services.AddSingleton<RoleBasedAuthorizationService>();

            // Register other services as normal
            services.AddSingleton<ProfileRegistry>(sp => ProfileRegistry.Instance);
            services.AddSingleton<ClinicalDocumentRegistry>(sp => ClinicalDocumentRegistry.Instance);
            services.AddSingleton<ScheduleManager>(sp => ScheduleManager.Instance);
            services.AddSingleton<ScheduleConflictResolver>();
            services.AddSingleton<CommandInvoker>();

            services.AddSingleton<CommandFactory>(serviceProvider =>
                new CommandFactory(
                    serviceProvider.GetRequiredService<IAuthenticationService>(),
                    serviceProvider.GetRequiredService<ScheduleManager>()
                ));

            return services;
        }

        /// <summary>
        /// Initializes development/demo data in the system for testing purposes.
        /// Call this AFTER building the service provider, not during service registration.
        /// This should only be called in development or demo environments.
        /// </summary>
        /// <param name="serviceProvider">The built service provider</param>
        /// <param name="createSampleData">Whether to create sample patient/physician data in addition to admin</param>
        public static void InitializeDevelopmentData(IServiceProvider serviceProvider, bool createSampleData = true)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            var authService = serviceProvider.GetRequiredService<IAuthenticationService>();
            var profileRegistry = serviceProvider.GetRequiredService<ProfileRegistry>();
            var scheduleManager = serviceProvider.GetRequiredService<ScheduleManager>();

            try
            {
                // Note: Admin is already created by BasicAuthenticationService constructor
                // We only need to create additional development data here

                // Create sample data if requested and database is nearly empty
                if (createSampleData)
                {
                    var allProfiles = new List<IUserProfile>();
                    allProfiles.AddRange(profileRegistry.GetAllAdministrators());
                    allProfiles.AddRange(profileRegistry.GetAllPhysicians());
                    allProfiles.AddRange(profileRegistry.GetAllPatients());

                    // Only create sample data if we have just the admin account
                    if (allProfiles.Count <= 1)
                    {
                        Console.WriteLine("Creating sample data for demonstration...");

                        // Create sample physician - Dr. Seuss
                        PhysicianProfile physician = new()
                        {
                            Username = SampleCredentials.PhysicianUsername
                        };
                        physician.SetValue(CommonEntryTypeExtensions.GetKey(CommonEntryType.Name), "Seuss");
                        physician.SetValue(CommonEntryTypeExtensions.GetKey(CommonEntryType.Address), "456 Medical Plaza, Whoville, WH 12345");
                        physician.SetValue(CommonEntryTypeExtensions.GetKey(CommonEntryType.BirthDate), new DateTime(1975, 3, 2));
                        physician.SetValue(PhysicianEntryTypeExtensions.GetKey(PhysicianEntryType.LicenseNumber), "MD12345");
                        physician.SetValue(PhysicianEntryTypeExtensions.GetKey(PhysicianEntryType.GraduationDate), new DateTime(2010, 5, 15));
                        physician.SetValue(PhysicianEntryTypeExtensions.GetKey(PhysicianEntryType.Specializations),
                            new List<MedicalSpecialization> { MedicalSpecialization.FamilyMedicine, MedicalSpecialization.Pediatrics });

                        if (!string.IsNullOrEmpty(physician.Username) && profileRegistry.AddProfile(physician))
                        {
                            authService.Register(physician, SampleCredentials.PhysicianPassword);
                            Console.WriteLine($"  Created sample physician: {SampleCredentials.PhysicianDisplayName}");
                        }

                        // Create sample patient - Jane Doe
                        var patient = new PatientProfile
                        {
                            Username = SampleCredentials.PatientUsername
                        };
                        patient.SetValue(CommonEntryTypeExtensions.GetKey(CommonEntryType.Name), SampleCredentials.PatientDisplayName);
                        patient.SetValue(CommonEntryTypeExtensions.GetKey(CommonEntryType.Address), "123 Main St, Anytown, USA");
                        patient.SetValue(CommonEntryTypeExtensions.GetKey(CommonEntryType.BirthDate), new DateTime(1985, 3, 20));
                        patient.SetValue(PatientEntryTypeExtensions.GetKey(PatientEntryType.Gender), Gender.Woman);
                        patient.SetValue(PatientEntryTypeExtensions.GetKey(PatientEntryType.Race), "White");

                        if (!string.IsNullOrEmpty(patient.Username) && profileRegistry.AddProfile(patient))
                        {
                            authService.Register(patient, SampleCredentials.PatientPassword);
                            Console.WriteLine($"  Created sample patient: {SampleCredentials.PatientDisplayName}");
                        }

                        // Create sample appointments
                        var baseAppointmentTime = DateTime.Now.AddDays(7).Date.AddHours(10); // Next week at 10 AM

                        // Create appointment for Jane Doe
                        if (physician != null && patient != null)
                        {
                            ScheduleSampleAppointment(patient, physician, scheduleManager, ref baseAppointmentTime, "Annual checkup");
                        }

                        // Create additional sample patients for list views
                        var additionalPatients = new[]
                        {
                            ("johndoe", "John Doe", "456 Oak Ave", new DateTime(1980, 6, 15), Gender.Man),
                            ("asmith", "Alice Smith", "789 Pine Rd", new DateTime(1992, 9, 3), Gender.Woman),
                            ("bwilson", "Bob Wilson", "321 Elm St", new DateTime(1975, 12, 25), Gender.Man)
                        };

                        foreach (var (username, name, address, birthDate, gender) in additionalPatients)
                        {
                            var p = new PatientProfile
                            {
                                Username = username
                            };
                            p.SetValue(CommonEntryTypeExtensions.GetKey(CommonEntryType.Name), name);
                            p.SetValue(CommonEntryTypeExtensions.GetKey(CommonEntryType.Address), address);
                            p.SetValue(CommonEntryTypeExtensions.GetKey(CommonEntryType.BirthDate), birthDate);
                            p.SetValue(PatientEntryTypeExtensions.GetKey(PatientEntryType.Gender), gender);
                            p.SetValue(PatientEntryTypeExtensions.GetKey(PatientEntryType.Race), "Other");

                            if (profileRegistry.AddProfile(p))
                            {
                                authService.Register(p, "password");
                                Console.WriteLine($"  Created sample patient: {name}");

                                // Schedule appointment for this patient
                                if (physician != null)
                                {
                                    ScheduleSampleAppointment(p, physician, scheduleManager, ref baseAppointmentTime, "Follow-up visit");
                                }
                            }
                        }

                        Console.WriteLine("Sample data created successfully!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not create development data: {ex.Message}");
            }
        }

        /// <summary>
        /// Schedules a sample appointment for a patient with conflict-aware time slot allocation
        /// </summary>
        private static void ScheduleSampleAppointment(
            PatientProfile patient,
            PhysicianProfile physician,
            ScheduleManager scheduleManager,
            ref DateTime baseTime,
            string reason)
        {
            const int SLOT_DURATION_MINUTES = 30;
            const int MAX_ATTEMPTS = 20; // Try up to 20 slots (10 hours)

            var physicianSchedule = scheduleManager.GetPhysicianSchedule(physician.Id);
            var attemptTime = baseTime;
            var scheduled = false;

            for (int attempt = 0; attempt < MAX_ATTEMPTS && !scheduled; attempt++)
            {
                var appointment = new AppointmentTimeInterval(
                    attemptTime,
                    attemptTime.AddMinutes(SLOT_DURATION_MINUTES),
                    patient.Id,
                    physician.Id,
                    reason,
                    AppointmentStatus.Scheduled
                );

                if (physicianSchedule.TryAddAppointment(appointment))
                {
                    Console.WriteLine($"  Scheduled appointment: {patient.GetValue<string>(CommonEntryTypeExtensions.GetKey(CommonEntryType.Name))} on {attemptTime:yyyy-MM-dd HH:mm}");
                    baseTime = attemptTime.AddMinutes(SLOT_DURATION_MINUTES); // Next slot starts after this one
                    scheduled = true;
                }
                else
                {
                    // Conflict detected, try next 30-minute slot
                    attemptTime = attemptTime.AddMinutes(SLOT_DURATION_MINUTES);
                }
            }

            if (!scheduled)
            {
                Console.WriteLine($"  Warning: Could not schedule appointment for {patient.GetValue<string>(CommonEntryTypeExtensions.GetKey(CommonEntryType.Name))} (no available slots)");
            }
        }
    }
}
