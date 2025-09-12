namespace Core.CliniCore.Commands
{
    /// <summary>
    /// Centralized command names to eliminate hardcoded strings in menu definitions.
    /// Each constant follows the naming convention: ClassName without "Command" suffix, converted to lowercase.
    /// This ensures consistency with the AbstractCommand.CommandKey property implementation.
    /// </summary>
    public static class CommandNames
    {
        // Authentication Commands
        public const string Login = "login";
        public const string Logout = "logout";
        public const string ChangePassword = "changepassword";

        // Profile Commands
        public const string CreatePatient = "createpatient";
        public const string CreatePhysician = "createphysician";
        public const string ListPatients = "listpatients";
        public const string ListPhysicians = "listphysicians";
        public const string ViewProfile = "viewprofile";
        public const string UpdateProfile = "updateprofile";
        public const string DeleteProfile = "deleteprofile";
        public const string AssignPatientToPhysician = "assignpatienttophysician";

        // Scheduling Commands
        public const string ScheduleAppointment = "scheduleappointment";
        public const string ListAppointments = "listappointments";
        public const string ViewAppointment = "viewappointment";
        public const string RescheduleAppointment = "rescheduleappointment";
        public const string CancelAppointment = "cancelappointment";
        public const string GetAvailableTimeSlots = "getavailabletimeslots";
        public const string CheckConflicts = "checkconflicts";
        public const string SetPhysicianAvailability = "setphysicianavailability";
        public const string GetSchedule = "getschedule";

        // Clinical Document Commands
        public const string CreateClinicalDocument = "createclinicaldocument";
        public const string ListClinicalDocuments = "listclinicaldocuments";
        public const string ViewClinicalDocument = "viewclinicaldocument";
        public const string UpdateClinicalDocument = "updateclinicaldocument";
        public const string AddDiagnosis = "adddiagnosis";
        public const string AddPrescription = "addprescription";
        public const string AddObservation = "addobservation";
        public const string AddAssessment = "addassessment";
        public const string AddPlan = "addplan";

        // Query Commands
        public const string SearchPatients = "searchpatients";
        public const string SearchClinicalNotes = "searchclinicalnotes";
        public const string FindPhysiciansBySpecialization = "findphysiciansbyspecialization";
        public const string FindPhysiciansByAvailability = "findphysiciansbyavailability";

        // Report Commands
        public const string GeneratePatientReport = "generatepatientreport";
        public const string GeneratePhysicianReport = "generatephysicianreport";
        public const string GenerateAppointmentReport = "generateappointmentreport";
        public const string GenerateFacilityReport = "generatefacilityreport";

        // Admin Commands
        public const string CreateFacility = "createfacility";
        public const string UpdateFacilitySettings = "updatefacilitysettings";
        public const string ManageUserRoles = "manageuserroles";
        public const string ViewAuditLog = "viewauditlog";
        public const string SystemMaintenance = "systemmaintenance";

        // Special command names that may not have direct command classes
        public const string CreateAdministrator = "createadministrator";
    }
}