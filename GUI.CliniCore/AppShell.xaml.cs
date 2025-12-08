using GUI.CliniCore.Views.Authentication;
using GUI.CliniCore.Views.Home;
using GUI.CliniCore.Views.Patients;
using GUI.CliniCore.Views.Physicians;
using GUI.CliniCore.Views.Users;
using GUI.CliniCore.Views.ClinicalDocuments;
using GUI.CliniCore.Views.Appointments;
using GUI.CliniCore.Views.Stub;

namespace GUI.CliniCore
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for DI-supported navigation
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));

            // Patient Management routes
            Routing.RegisterRoute("PatientListPage", typeof(PatientListPage));
            Routing.RegisterRoute("PatientDetailPage", typeof(PatientDetailPage));
            Routing.RegisterRoute(nameof(CreatePatientPage), typeof(CreatePatientPage));
            Routing.RegisterRoute("PatientEditPage", typeof(PatientEditPage));

            // Physician Management routes
            Routing.RegisterRoute("PhysicianListPage", typeof(PhysicianListPage));
            Routing.RegisterRoute("PhysicianDetailPage", typeof(PhysicianDetailPage));
            Routing.RegisterRoute("CreatePhysicianPage", typeof(CreatePhysicianPage));
            Routing.RegisterRoute("PhysicianEditPage", typeof(PhysicianEditPage));

            // User Management routes
            Routing.RegisterRoute("UserListPage", typeof(UserListPage));
            Routing.RegisterRoute("AdministratorEditPage", typeof(AdministratorEditPage));

            // Clinical Document routes
            Routing.RegisterRoute("ClinicalDocumentListPage", typeof(ClinicalDocumentListPage));
            Routing.RegisterRoute("ClinicalDocumentDetailPage", typeof(ClinicalDocumentDetailPage));
            Routing.RegisterRoute("ClinicalDocumentEditPage", typeof(ClinicalDocumentEditPage));
            Routing.RegisterRoute("CreateClinicalDocumentPage", typeof(CreateClinicalDocumentPage));

            // Appointment routes
            Routing.RegisterRoute("AppointmentListPage", typeof(AppointmentListPage));
            Routing.RegisterRoute("AppointmentDetailPage", typeof(AppointmentDetailPage));
            Routing.RegisterRoute("CreateAppointmentPage", typeof(CreateAppointmentPage));
            Routing.RegisterRoute("AppointmentEditPage", typeof(AppointmentEditPage));

            // Stub route
            Routing.RegisterRoute("StubPage", typeof(StubPage));
        }

        protected override void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);
            // Shell navigation events for debugging
            System.Diagnostics.Debug.WriteLine($"Navigating to: {args.Target.Location}");
        }
    }
}
