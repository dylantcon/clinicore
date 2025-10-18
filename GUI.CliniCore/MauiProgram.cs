using Microsoft.Extensions.Logging;
using Core.CliniCore.Bootstrap;
using GUI.CliniCore.Services;
using GUI.CliniCore.ViewModels;
using GUI.CliniCore.Views;

namespace GUI.CliniCore
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
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

            // Register Core.CliniCore services
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
            builder.Services.AddTransient<PatientEditViewModel>();

            // Physician Management ViewModels
            builder.Services.AddTransient<PhysicianListViewModel>();
            builder.Services.AddTransient<PhysicianDetailViewModel>();
            builder.Services.AddTransient<PhysicianEditViewModel>();

            // User Management ViewModels
            builder.Services.AddTransient<UserListViewModel>();
            builder.Services.AddTransient<AdministratorEditViewModel>();

            // Clinical Document ViewModels
            builder.Services.AddTransient<ClinicalDocumentListViewModel>();
            builder.Services.AddTransient<ClinicalDocumentDetailViewModel>();
            builder.Services.AddTransient<ClinicalDocumentEditViewModel>();

            // Appointment ViewModels
            builder.Services.AddTransient<AppointmentListViewModel>();
            builder.Services.AddTransient<AppointmentDetailViewModel>();
            builder.Services.AddTransient<AppointmentEditViewModel>();

            // Stub ViewModel
            builder.Services.AddTransient<StubViewModel>();

            // Register Pages
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<HomePage>();

            // Patient Management Pages
            builder.Services.AddTransient<PatientListPage>();
            builder.Services.AddTransient<PatientDetailPage>();
            builder.Services.AddTransient<PatientEditPage>();

            // Physician Management Pages
            builder.Services.AddTransient<PhysicianListPage>();
            builder.Services.AddTransient<PhysicianDetailPage>();
            builder.Services.AddTransient<PhysicianEditPage>();

            // User Management Pages
            builder.Services.AddTransient<UserListPage>();
            builder.Services.AddTransient<AdministratorEditPage>();

            // Clinical Document Pages
            builder.Services.AddTransient<ClinicalDocumentListPage>();
            builder.Services.AddTransient<ClinicalDocumentDetailPage>();
            builder.Services.AddTransient<ClinicalDocumentEditPage>();

            // Appointment Pages
            builder.Services.AddTransient<AppointmentListPage>();
            builder.Services.AddTransient<AppointmentDetailPage>();
            builder.Services.AddTransient<AppointmentEditPage>();

            // Stub Page
            builder.Services.AddTransient<StubPage>();

            // Register Shell
            builder.Services.AddSingleton<AppShell>();

            var app = builder.Build();

#if DEBUG
            // Initialize development data after building the service provider
            CoreServiceBootstrapper.InitializeDevelopmentData(app.Services, createSampleData: true);
#endif

            return app;
        }
    }
}
