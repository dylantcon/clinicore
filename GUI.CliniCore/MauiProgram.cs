using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Core.CliniCore.Bootstrap;
using Core.CliniCore.Repositories.InMemory;
using Core.CliniCore.Repositories.Remote;
using GUI.CliniCore.Services;
using GUI.CliniCore.ViewModels.Authentication;
using GUI.CliniCore.ViewModels.Home;
using GUI.CliniCore.ViewModels.Patients;
using GUI.CliniCore.ViewModels.Physicians;
using GUI.CliniCore.ViewModels.Appointments;
using GUI.CliniCore.ViewModels.ClinicalDocuments;
using GUI.CliniCore.ViewModels.Users;
using GUI.CliniCore.ViewModels.Stub;
using GUI.CliniCore.Views.Authentication;
using GUI.CliniCore.Views.Home;
using GUI.CliniCore.Views.Patients;
using GUI.CliniCore.Views.Physicians;
using GUI.CliniCore.Views.Appointments;
using GUI.CliniCore.Views.ClinicalDocuments;
using GUI.CliniCore.Views.Users;
using GUI.CliniCore.Views.Stub;
using System.Reflection;

namespace GUI.CliniCore
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            MauiAppBuilder builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIconsRegular");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // Load configuration from embedded appsettings.json
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("GUI.CliniCore.appsettings.json");
            var config = new ConfigurationBuilder()
                .AddJsonStream(stream ?? throw new InvalidOperationException("Could not load appsettings.json"))
                .Build();

            // Read API settings from configuration
            var apiBaseUrl = config["CliniCore:Api:BaseUrl"] ?? "http://localhost:5000";
            var localProvider = config.GetValue<bool>("CliniCore:Api:LocalProvider", true);
            var healthCheckTimeout = config.GetValue<int>("CliniCore:Api:HealthCheckTimeoutSeconds", 30);

            // Register repositories based on build configuration
#if USE_REMOTE
            // Ensure API is available (starts locally if configured and not running)
            var apiAvailable = CoreServiceBootstrapper.EnsureApiAvailableAsync(apiBaseUrl, localProvider, healthCheckTimeout)
                .GetAwaiter().GetResult();

            if (apiAvailable)
            {
                builder.Services.AddRemoteRepositories(apiBaseUrl);
            }
            else
            {
                Console.WriteLine("Warning: API unavailable, falling back to in-memory storage");
                builder.Services.AddInMemoryRepositories();
            }
#elif USE_INMEMORY
            // Use in-memory persistence (standalone, no API required)
            builder.Services.AddInMemoryRepositories();
#else
            // Default fallback to in-memory for safety
            builder.Services.AddInMemoryRepositories();
#endif

            // Register Core.CliniCore services (uses repositories registered above)
            builder.Services.AddCliniCoreServices();

            // Register GUI services
            builder.Services.AddSingleton<INavigationService, NavigationService>();
            builder.Services.AddSingleton<SessionManager>();
            builder.Services.AddSingleton<IHomeViewModelFactory, HomeViewModelFactory>();

            // Register ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<AdministratorHomeViewModel>();
            builder.Services.AddTransient<PhysicianHomeViewModel>();
            builder.Services.AddTransient<PatientHomeViewModel>();

            // Patient Management ViewModels
            builder.Services.AddTransient<PatientListViewModel>();
            builder.Services.AddTransient<PatientDetailViewModel>();
            builder.Services.AddTransient<CreatePatientViewModel>();
            builder.Services.AddTransient<PatientEditViewModel>();

            // Physician Management ViewModels
            builder.Services.AddTransient<PhysicianListViewModel>();
            builder.Services.AddTransient<PhysicianDetailViewModel>();
            builder.Services.AddTransient<CreatePhysicianViewModel>();
            builder.Services.AddTransient<PhysicianEditViewModel>();

            // User Management ViewModels
            builder.Services.AddTransient<UserListViewModel>();
            builder.Services.AddTransient<AdministratorEditViewModel>();

            // Clinical Document ViewModels
            builder.Services.AddTransient<ClinicalDocumentListViewModel>();
            builder.Services.AddTransient<ClinicalDocumentDetailViewModel>();
            builder.Services.AddTransient<ClinicalDocumentEditViewModel>();
            builder.Services.AddTransient<CreateClinicalDocumentViewModel>();

            // Appointment ViewModels
            builder.Services.AddTransient<AppointmentListViewModel>();
            builder.Services.AddTransient<AppointmentDetailViewModel>();
            builder.Services.AddTransient<CreateAppointmentViewModel>();
            builder.Services.AddTransient<AppointmentEditViewModel>();

            // Stub ViewModel
            builder.Services.AddTransient<StubViewModel>();

            // Register Pages
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<HomePage>();

            // Patient Management Pages
            builder.Services.AddTransient<PatientListPage>();
            builder.Services.AddTransient<PatientDetailPage>();
            builder.Services.AddTransient<CreatePatientPage>();
            builder.Services.AddTransient<PatientEditPage>();

            // Physician Management Pages
            builder.Services.AddTransient<PhysicianListPage>();
            builder.Services.AddTransient<PhysicianDetailPage>();
            builder.Services.AddTransient<CreatePhysicianPage>();
            builder.Services.AddTransient<PhysicianEditPage>();

            // User Management Pages
            builder.Services.AddTransient<UserListPage>();
            builder.Services.AddTransient<AdministratorEditPage>();

            // Clinical Document Pages
            builder.Services.AddTransient<ClinicalDocumentListPage>();
            builder.Services.AddTransient<ClinicalDocumentDetailPage>();
            builder.Services.AddTransient<ClinicalDocumentEditPage>();
            builder.Services.AddTransient<CreateClinicalDocumentPage>();

            // Appointment Pages
            builder.Services.AddTransient<AppointmentListPage>();
            builder.Services.AddTransient<AppointmentDetailPage>();
            builder.Services.AddTransient<CreateAppointmentPage>();
            builder.Services.AddTransient<AppointmentEditPage>();

            // Stub Page
            builder.Services.AddTransient<StubPage>();

            // Register Shell
            builder.Services.AddSingleton<AppShell>();

            var app = builder.Build();

#if DEBUG
            // Seed development data after building the service provider
            DevelopmentDataSeeder.SeedDevelopmentData(app.Services, createSampleData: true);
#else
            DevelopmentDataSeeder.SeedDevelopmentData(app.Services, createSampleData: false);
#endif

            return app;
        }
    }
}
