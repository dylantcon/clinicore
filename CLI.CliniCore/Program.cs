using CLI.CliniCore.Service;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Scheduling;

namespace CLI.CliniCore
{
    public class Program
    {
        static void Main(string[] args)
        {
            ServiceContainer? container = null;
            try
            {
                // Initialize profile registry (singleton)
                var profileRegistry = ProfileRegistry.Instance;
                
                // Create the service container which handles all dependency resolution
                container = ServiceContainer.Create();
                
                // Create default admin account if none exists
                CreateDefaultAdminIfNeeded(profileRegistry, container.GetAuthenticationService());
                
                // Start the application
                var consoleEngine = container.GetConsoleEngine();
                consoleEngine.Start();
            }
            catch (Exception ex)
            {
                var console = ThreadSafeConsoleManager.Instance;
                console.SetForegroundColor(ConsoleColor.Red);
                console.WriteLine($"Fatal error: {ex.Message}");
                console.WriteLine($"Stack trace: {ex.StackTrace}");
                console.ResetColor();
                console.WriteLine("Press any key to exit...");
                console.ReadKey();
                Environment.Exit(1);
            }
            finally
            {
                // Ensure proper cleanup of resources
                container?.Dispose();
            }
        }
        
        private static void CreateDefaultAdminIfNeeded(ProfileRegistry registry, IAuthenticationService authService)
        {
            try
            {
                // Check if any admin exists
                var adminProfiles = registry.GetAllAdministrators();
                bool hasAdmin = false;
                
                hasAdmin = adminProfiles.Any();
                
                if (!hasAdmin)
                {
                    var console = ThreadSafeConsoleManager.Instance;
                    console.SetForegroundColor(ConsoleColor.Yellow);
                    console.WriteLine("No administrator account found. Creating default admin account...");
                    console.ResetColor();
                    
                    // Create default admin
                    var adminProfile = new AdministratorProfile();
                    adminProfile.Username = "admin";  // Username is a direct property, not a template entry
                    adminProfile.SetValue("name", "System Administrator");
                    
                    // Basic validation - profile template will handle detailed validation
                    if (string.IsNullOrEmpty(adminProfile.Username))
                    {
                        console.SetForegroundColor(ConsoleColor.Red);
                        console.WriteLine("Failed to create default admin profile: Username is required");
                        console.ResetColor();
                        return;
                    }
                    
                    // Add the profile to registry
                    if (!registry.AddProfile(adminProfile))
                    {
                        console.SetForegroundColor(ConsoleColor.Red);
                        console.WriteLine("Failed to add default admin profile: Username may already exist");
                        console.ResetColor();
                        return;
                    }
                    
                    // Register with auth service (includes password)
                    authService.Register(adminProfile, "admin123");
                    
                    console.SetForegroundColor(ConsoleColor.Green);
                    console.WriteLine("Default administrator account created successfully!");
                    console.WriteLine("  Username: admin");
                    console.WriteLine("  Password: admin123");
                    console.WriteLine("  *** Please change the password after first login ***");
                    console.ResetColor();
                    console.WriteLine();
                    console.WriteLine("Press any key to continue...");
                    console.ReadKey();
                }
                
                // Also create some sample data for testing
                CreateSampleDataIfEmpty(registry, authService);
            }
            catch (Exception ex)
            {
                var console = ThreadSafeConsoleManager.Instance;
                console.SetForegroundColor(ConsoleColor.Red);
                console.WriteLine($"Error creating default admin: {ex.Message}");
                console.ResetColor();
            }
        }
        
        private static void CreateSampleDataIfEmpty(ProfileRegistry registry, IAuthenticationService authService)
        {
            var allProfiles = new List<IUserProfile>();
            allProfiles.AddRange(registry.GetAllAdministrators());
            allProfiles.AddRange(registry.GetAllPhysicians());
            allProfiles.AddRange(registry.GetAllPatients());
            int profileCount = allProfiles.Count;
            
            // Only create sample data if we have just the admin account
            if (profileCount == 1)
            {
                var console = ThreadSafeConsoleManager.Instance;
                console.SetForegroundColor(ConsoleColor.Yellow);
                console.WriteLine("Creating sample data for demonstration...");
                console.ResetColor();
                
                try
                {
                    // Create a sample physician with implicit validation through ProfileEntry validator
                    var physician = new PhysicianProfile();
                    physician.Username = "greeneggsnham";

                    physician.SetValue(CommonEntryTypeExtensions
                        .GetKey(CommonEntryType.Name), "Seuss");
                    physician.SetValue(PhysicianEntryTypeExtensions
                        .GetKey(PhysicianEntryType.LicenseNumber), "MD12345");
                    physician.SetValue(PhysicianEntryTypeExtensions
                        .GetKey(PhysicianEntryType.GraduationDate), new DateTime(2010, 5, 15));
                    physician.SetValue(PhysicianEntryTypeExtensions
                        .GetKey(PhysicianEntryType.Specializations), MedicalSpecialization.FamilyMedicine);
                    
                    // Basic validation and add to registry
                    if (!string.IsNullOrEmpty(physician.Username) && registry.AddProfile(physician))
                    {
                        authService.Register(physician, "password");
                        console.WriteLine($"  Created sample physician: Dr. Seuss (username: greeneggsnham, password: password)");
                    }
                    
                    // Create a sample patient
                    var patient = new PatientProfile();
                    patient.Username = "jdoe";

                    patient.SetValue(CommonEntryTypeExtensions
                        .GetKey(CommonEntryType.Name), "Jane Doe");
                    patient.SetValue(CommonEntryTypeExtensions
                        .GetKey(CommonEntryType.Address), "123 Main St, Anytown, USA");
                    patient.SetValue(CommonEntryTypeExtensions
                        .GetKey(CommonEntryType.BirthDate), new DateTime(1985, 3, 20));
                    patient.SetValue(PatientEntryTypeExtensions
                        .GetKey(PatientEntryType.Gender), Gender.Woman);
                    patient.SetValue(PatientEntryTypeExtensions
                        .GetKey(PatientEntryType.Race), "White");
                    
                    // Basic validation and add to registry
                    if (!string.IsNullOrEmpty(patient.Username) && registry.AddProfile(patient))
                    {
                        authService.Register(patient, "password");
                        console.WriteLine("  Created sample patient: Jane Doe (username: jdoe, password: patient123)");
                    }
                    
                    // Create a sample appointment for testing the editor
                    if (physician != null && patient != null)
                    {
                        CreateSampleAppointment(physician, patient, console);
                    }
                    
                    console.SetForegroundColor(ConsoleColor.Green);
                    console.WriteLine("Sample data created successfully!");
                    console.ResetColor();
                }
                catch (Exception ex)
                {
                    console.SetForegroundColor(ConsoleColor.Red);
                    console.WriteLine($"Warning: Could not create sample data: {ex.Message}");
                    console.ResetColor();
                }
                
                console.WriteLine();
                console.WriteLine("Press any key to continue...");
                console.ReadKey();
            }
        }
        
        private static void CreateSampleAppointment(PhysicianProfile physician, PatientProfile patient, ThreadSafeConsoleManager console)
        {
            try
            {
                var scheduleManager = Core.CliniCore.Scheduling.Management.ScheduleManager.Instance;
                var bookingStrategy = new Core.CliniCore.Scheduling.BookingStrategies.FirstAvailableBookingStrategy();
                
                // Get the next available weekday
                var now = DateTime.Now;
                var nextWeekday = GetNextWeekday(now);
                
                // Set appointment time to 9:00 AM on the next weekday
                var appointmentStart = new DateTime(nextWeekday.Year, nextWeekday.Month, nextWeekday.Day, 9, 0, 0);
                var appointmentEnd = appointmentStart.Add(Core.CliniCore.Scheduling.AppointmentTimeInterval.StandardDurations.StandardVisit);
                
                // Create the appointment
                var appointment = new Core.CliniCore.Scheduling.AppointmentTimeInterval(
                    appointmentStart,
                    appointmentEnd,
                    patient.Id,
                    physician.Id,
                    "Sample appointment for editor testing"
                );
                
                // Schedule the appointment
                var result = scheduleManager.ScheduleAppointment(appointment);
                
                if (result.Success)
                {
                    console.WriteLine($"  Created sample appointment: {appointmentStart:yyyy-MM-dd HH:mm} - {appointmentEnd:HH:mm}");
                    console.WriteLine($"    Patient: {patient.Name} | Physician: Dr. {physician.Name}");
                    console.WriteLine($"    Appointment ID: {appointment.Id}");
                }
                else
                {
                    console.SetForegroundColor(ConsoleColor.Yellow);
                    console.WriteLine($"  Warning: Could not create sample appointment: {result.Message}");
                    console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                console.SetForegroundColor(ConsoleColor.Yellow);
                console.WriteLine($"  Warning: Could not create sample appointment: {ex.Message}");
                console.ResetColor();
            }
        }
        
        private static DateTime GetNextWeekday(DateTime date)
        {
            // If it's already a weekday, return tomorrow (unless it's Friday, then return Monday)
            if (date.DayOfWeek >= DayOfWeek.Monday && date.DayOfWeek <= DayOfWeek.Thursday)
            {
                return date.AddDays(1);
            }
            
            // If it's Friday, Saturday, or Sunday, return the next Monday
            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)date.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0) daysUntilMonday = 7; // If today is Monday, get next Monday
            
            return date.AddDays(daysUntilMonday);
        }
    }
}