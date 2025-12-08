namespace Core.CliniCore.Api
{
    /// <summary>
    /// Single source of truth for all API route patterns.
    /// Used by both API controllers and Remote repositories.
    ///
    /// Structure:
    /// - Constants: Route segments used in [HttpGet] attributes
    /// - Static methods: Full paths used in HttpClient calls
    /// </summary>
    public static class ApiRoutes
    {
        public const string Base = "api";

        /// <summary>
        /// Routes for Appointments API (/api/appointments)
        /// </summary>
        public static class Appointments
        {
            public const string Controller = "appointments";
            public const string BasePath = $"{Base}/{Controller}";

            // Route segments for [HttpGet] attributes
            public const string ById = "{id}";
            public const string ByPatient = "patient/{patientId}";
            public const string ByPhysician = "physician/{physicianId}";
            public const string Cancel = "{id}/cancel";
            public const string AvailableSlots = "physician/{physicianId}/available-slots";
            public const string Statuses = "statuses";

            // Full paths for HttpClient calls
            public static string GetAll() => BasePath;
            public static string GetById(Guid id) => $"{BasePath}/{id}";
            public static string GetByPatient(Guid patientId) => $"{BasePath}/patient/{patientId}";
            public static string GetByPhysician(Guid physicianId) => $"{BasePath}/physician/{physicianId}";
            public static string GetByDate(DateTime date) => $"{BasePath}/by-date/{date:yyyy-MM-dd}";
            public static string GetByStatus(string status) => $"{BasePath}/by-status/{status}";
            public static string CancelAppointment(Guid id) => $"{BasePath}/{id}/cancel";
            public static string GetAvailableSlots(Guid physicianId, DateTime date, int durationMinutes) =>
                $"{BasePath}/available-slots?physicianId={physicianId}&date={date:yyyy-MM-dd}&durationMinutes={durationMinutes}";
            public static string CheckConflict(Guid physicianId, DateTime start, int durationMinutes, Guid? excludeId = null)
            {
                var url = $"{BasePath}/check-conflict?physicianId={physicianId}&start={start:O}&durationMinutes={durationMinutes}";
                if (excludeId.HasValue)
                    url += $"&excludeId={excludeId.Value}";
                return url;
            }
        }

        /// <summary>
        /// Routes for Patients API (/api/patients)
        /// </summary>
        public static class Patients
        {
            public const string Controller = "patients";
            public const string BasePath = $"{Base}/{Controller}";

            // Route segments for [HttpGet] attributes
            public const string ById = "{id}";
            public const string Search = "search";
            public const string ByPhysician = "physician/{physicianId}";

            // Full paths for HttpClient calls
            public static string GetAll() => BasePath;
            public static string GetById(Guid id) => $"{BasePath}/{id}";
            public static string SearchByQuery(string query) => $"{BasePath}/search?q={Uri.EscapeDataString(query)}";
            public static string GetByPhysician(Guid physicianId) => $"{BasePath}/physician/{physicianId}";
            public static string GetUnassigned() => $"{BasePath}/unassigned";
        }

        /// <summary>
        /// Routes for Physicians API (/api/physicians)
        /// </summary>
        public static class Physicians
        {
            public const string Controller = "physicians";
            public const string BasePath = $"{Base}/{Controller}";

            // Route segments for [HttpGet] attributes
            public const string ById = "{id}";
            public const string Search = "search";
            public const string BySpecialization = "specialization/{specialization}";
            public const string Specializations = "specializations";
            public const string AssignPatient = "{id}/patients/{patientId}";

            // Full paths for HttpClient calls
            public static string GetAll() => BasePath;
            public static string GetById(Guid id) => $"{BasePath}/{id}";
            public static string SearchByQuery(string query) => $"{BasePath}/search?q={Uri.EscapeDataString(query)}";
            public static string GetBySpecialization(string specialization) => $"{BasePath}/specialization/{specialization}";
            public static string GetAvailableOn(DateTime date) => $"{BasePath}/available?date={date:yyyy-MM-dd}";
            public static string AssignPatientTo(Guid physicianId, Guid patientId) => $"{BasePath}/{physicianId}/patients/{patientId}";
        }

        /// <summary>
        /// Routes for Administrators API (/api/administrators)
        /// </summary>
        public static class Administrators
        {
            public const string Controller = "administrators";
            public const string BasePath = $"{Base}/{Controller}";

            // Route segments for [HttpGet] attributes
            public const string ById = "{id}";
            public const string ByDepartment = "by-department/{department}";
            public const string ByPermission = "by-permission/{permission}";

            // Full paths for HttpClient calls
            public static string GetAll() => BasePath;
            public static string GetById(Guid id) => $"{BasePath}/{id}";
            public static string GetByDepartment(string department) => $"{BasePath}/by-department/{Uri.EscapeDataString(department)}";
            public static string GetByPermission(string permission) => $"{BasePath}/by-permission/{permission}";
        }

        /// <summary>
        /// Routes for Auth API (/api/auth)
        /// </summary>
        public static class Auth
        {
            public const string Controller = "auth";
            public const string BasePath = $"{Base}/{Controller}";

            // Route segments for [HttpGet] attributes
            public const string ById = "{id}";
            public const string ByUsername = "by-username/{username}";
            public const string Search = "search";
            public const string Validate = "validate";
            public const string Register = "register";
            public const string ChangePassword = "change-password";

            // Full paths for HttpClient calls
            public static string GetAll() => BasePath;
            public static string GetById(Guid id) => $"{BasePath}/{id}";
            public static string GetByUsername(string username) => $"{BasePath}/by-username/{Uri.EscapeDataString(username)}";
            public static string SearchByQuery(string query) => $"{BasePath}/search?q={Uri.EscapeDataString(query ?? "")}";
            public static string ValidateCredentials() => $"{BasePath}/validate";
            public static string RegisterCredentials() => $"{BasePath}/register";
            public static string ChangeUserPassword() => $"{BasePath}/change-password";
            public static string LockAccount(string username) => $"{BasePath}/{Uri.EscapeDataString(username)}/lock";
            public static string UnlockAccount(string username) => $"{BasePath}/{Uri.EscapeDataString(username)}/unlock";
        }

        /// <summary>
        /// Routes for Clinical Documents API (/api/clinicaldocuments)
        /// </summary>
        public static class ClinicalDocuments
        {
            public const string Controller = "clinicaldocuments";
            public const string BasePath = $"{Base}/{Controller}";

            // Route segments for [HttpGet] attributes
            public const string ById = "{id}";
            public const string ByPatient = "patient/{patientId}";
            public const string ByPhysician = "physician/{physicianId}";
            public const string ByAppointment = "appointment/{appointmentId}";
            public const string Complete = "{id}/complete";
            public const string SearchDiagnosis = "search/diagnosis";
            public const string SearchMedication = "search/medication";
            public const string Statistics = "statistics";
            public const string SoapNote = "{id}/soap-note";

            // Entry route segments
            public const string Observations = "{docId}/observations";
            public const string ObservationById = "{docId}/observations/{entryId}";
            public const string Assessments = "{docId}/assessments";
            public const string AssessmentById = "{docId}/assessments/{entryId}";
            public const string Diagnoses = "{docId}/diagnoses";
            public const string DiagnosisById = "{docId}/diagnoses/{entryId}";
            public const string Plans = "{docId}/plans";
            public const string PlanById = "{docId}/plans/{entryId}";
            public const string Prescriptions = "{docId}/prescriptions";
            public const string PrescriptionById = "{docId}/prescriptions/{entryId}";

            // Full paths for HttpClient calls
            public static string GetAll() => BasePath;
            public static string GetById(Guid id) => $"{BasePath}/{id}";
            public static string GetByPatient(Guid patientId) => $"{BasePath}/patient/{patientId}";
            public static string GetByPhysician(Guid physicianId) => $"{BasePath}/physician/{physicianId}";
            public static string GetByAppointment(Guid appointmentId) => $"{BasePath}/appointment/{appointmentId}";
            public static string GetIncomplete() => $"{BasePath}?completed=false";
            public static string GetByDateRange(DateTime start, DateTime end) =>
                $"{BasePath}?fromDate={start:yyyy-MM-dd}&toDate={end:yyyy-MM-dd}";
            public static string SearchByDiagnosis(string code) => $"{BasePath}/search-diagnosis?code={Uri.EscapeDataString(code)}";
            public static string SearchByMedication(string name) => $"{BasePath}/search-medication?name={Uri.EscapeDataString(name)}";
            public static string CompleteDocument(Guid id) => $"{BasePath}/{id}/complete";
            public static string GetSoapNote(Guid id) => $"{BasePath}/{id}/soap-note";
            public static string SearchByQuery(string query) => $"{BasePath}/search?q={Uri.EscapeDataString(query)}";

            // Entry paths
            public static string GetObservations(Guid docId) => $"{BasePath}/{docId}/observations";
            public static string GetObservation(Guid docId, Guid entryId) => $"{BasePath}/{docId}/observations/{entryId}";
            public static string GetAssessments(Guid docId) => $"{BasePath}/{docId}/assessments";
            public static string GetAssessment(Guid docId, Guid entryId) => $"{BasePath}/{docId}/assessments/{entryId}";
            public static string GetDiagnoses(Guid docId) => $"{BasePath}/{docId}/diagnoses";
            public static string GetDiagnosis(Guid docId, Guid entryId) => $"{BasePath}/{docId}/diagnoses/{entryId}";
            public static string GetPlans(Guid docId) => $"{BasePath}/{docId}/plans";
            public static string GetPlan(Guid docId, Guid entryId) => $"{BasePath}/{docId}/plans/{entryId}";
            public static string GetPrescriptions(Guid docId) => $"{BasePath}/{docId}/prescriptions";
            public static string GetPrescription(Guid docId, Guid entryId) => $"{BasePath}/{docId}/prescriptions/{entryId}";
        }
    }
}
