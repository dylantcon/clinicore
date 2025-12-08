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
        /// <summary>
        /// Represents the base path segment for API endpoints.
        /// </summary>
        /// <remarks>This constant is typically used as the root path for constructing API
        /// routes.</remarks>
        public const string Base = "api";

        /// <summary>
        /// Routes for Appointments API (/api/appointments)
        /// </summary>
        public static class Appointments
        {
            /// <summary>
            /// Represents the controller name used for handling appointment-related operations.
            /// </summary>
            public const string Controller = "appointments";

            /// <summary>
            /// Represents the base path for the appointments API, combining the base URL and the controller name.
            /// </summary>
            /// <remarks>This constant is constructed by concatenating the base URL and the appointment controller
            /// name. Ensure that the <c>Base</c> and <c>Controller</c> constants are properly defined before using this
            /// value.</remarks>
            public const string BasePath = $"{Base}/{Controller}";

            /// <summary>
            /// Represents a placeholder string used to identify a resource by its unique identifier.
            /// </summary>
            /// <remarks>This constant is typically used in route templates or query strings where a
            /// resource ID is required.</remarks>
            public const string ById = "{id}";

            /// <summary>
            /// Represents the route template for accessing resources by patient ID.
            /// </summary>
            /// <remarks>This constant defines the route pattern "patient/{patientId}", where
            /// <c>{patientId}</c>  is a placeholder for the unique identifier of a patient.</remarks>
            public const string ByPatient = "patient/{patientId}";

            /// <summary>
            /// Represents the route template for accessing resources by a specific physician.
            /// </summary>
            /// <remarks>The route includes a placeholder for the physician's unique identifier, which
            /// should be replaced with the actual <c>physicianId</c> when constructing the route.</remarks>
            public const string ByPhysician = "physician/{physicianId}";

            /// <summary>
            /// Represents the endpoint path for canceling an operation with a specific identifier.
            /// </summary>
            /// <remarks>The placeholder <c>{id}</c> in the path should be replaced with the unique
            /// identifier of the operation to be canceled.</remarks>
            public const string Cancel = "{id}/cancel";

            /// <summary>
            /// Represents the endpoint template for retrieving available slots for a specific physician.
            /// </summary>
            /// <remarks>The placeholder <c>{physicianId}</c> in the template should be replaced with
            /// the unique identifier of the physician whose available slots are being queried.</remarks>
            public const string AvailableSlots = "physician/{physicianId}/available-slots";

            /// <summary>
            /// Represents the key used to identify statuses in a configuration or data context.
            /// </summary>
            public const string Statuses = "statuses";

            /// <summary>
            /// Gets the base path used by the application.
            /// </summary>
            /// <returns>The base path as a string.</returns>
            public static string GetAll() => BasePath;

            /// <summary>
            /// Constructs a URL path by appending the specified identifier to the base path.
            /// </summary>
            /// <param name="id">The unique identifier to append to the base path.</param>
            /// <returns>A string representing the full URL path, including the base path and the specified identifier.</returns>
            public static string GetById(Guid id) => $"{BasePath}/{id}";

            /// <summary>
            /// Constructs a URL path for retrieving resources associated with a specific patient.
            /// </summary>
            /// <param name="patientId">The unique identifier of the patient.</param>
            /// <returns>A string representing the URL path for the specified patient.</returns>
            public static string GetByPatient(Guid patientId) => $"{BasePath}/patient/{patientId}";

            /// <summary>
            /// Constructs a URL path for retrieving resources associated with a specific physician.
            /// </summary>
            /// <param name="physicianId">The unique identifier of the physician.</param>
            /// <returns>A string representing the URL path for the specified physician.</returns>
            public static string GetByPhysician(Guid physicianId) => $"{BasePath}/physician/{physicianId}";

            /// <summary>
            /// Constructs a URL path for retrieving data associated with a specific date.
            /// </summary>
            /// <param name="date">The date for which the URL path is generated. The date is formatted as "yyyy-MM-dd".</param>
            /// <returns>A string representing the URL path for the specified date.</returns>
            public static string GetByDate(DateTime date) => $"{BasePath}/by-date/{date:yyyy-MM-dd}";

            /// <summary>
            /// Constructs a URL for retrieving resources filtered by the specified status.
            /// </summary>
            /// <param name="status">The status used to filter the resources. This value is appended to the URL path.</param>
            /// <returns>A string representing the constructed URL for the specified status.</returns>
            public static string GetByStatus(string status) => $"{BasePath}/by-status/{status}";

            /// <summary>
            /// Constructs a URL to retrieve available appointment slots for a specified physician on a given date.
            /// </summary>
            /// <param name="physicianId">The unique identifier of the physician whose availability is being queried.</param>
            /// <param name="date">The date for which to retrieve available slots. Only the date component is used.</param>
            /// <param name="durationMinutes">The desired duration of the appointment slots, in minutes.</param>
            /// <returns>A string representing the URL to query available slots for the specified parameters.</returns>
            public static string GetAvailableSlots(Guid physicianId, DateTime date, int durationMinutes) =>
                $"{BasePath}/available-slots?physicianId={physicianId}&date={date:yyyy-MM-dd}&durationMinutes={durationMinutes}";

            /// <summary>
            /// Constructs a URL that checks for scheduling conflicts for a proposed appointment.
            /// </summary>
            /// <param name="physicianId">The unique identifier of the physician whose schedule is being queried</param>
            /// <param name="start">The date and time of the start of the proposed appointment</param>
            /// <param name="durationMinutes">The duration of the proposed appointment in minutes</param>
            /// <param name="excludeId">The unique identifier of the appointment to optionally exclude</param>
            /// <returns>A string representing the API path used to check for scheduling conflicts.</returns>

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
            /// <summary>
            /// The base controller name for patient-related API endpoints.
            /// </summary>
            public const string Controller = "patients";

            /// <summary>
            /// The base path for patient-related API endpoints, combining the base URL and controller name.
            /// </summary>
            public const string BasePath = $"{Base}/{Controller}";

            // Route segments for [HttpGet] attributes

            /// <summary>
            /// The route segment used to identify a patient by their unique identifier.
            /// </summary>
            public const string ById = "{id}";

            /// <summary>
            /// Search route segment for querying patients by a search term.
            /// </summary>
            public const string Search = "search";

            /// <summary>
            /// Route segment for retrieving all patients assigned to a physician
            /// </summary>
            public const string ByPhysician = "physician/{physicianId}";

            // Full paths for HttpClient calls

            /// <summary>
            /// Base route for accessing all patients.
            /// </summary>
            /// <returns>The base path for patients.</returns>
            public static string GetAll() => BasePath;

            /// <summary>
            /// Constructs a URL that fetches a patient by their unique identifier
            /// </summary>
            /// <param name="id">The unique identifier of the patient to be fetched</param>
            /// <returns>The full path to the patient resource.</returns>
            public static string GetById(Guid id) => $"{BasePath}/{id}";

            /// <summary>
            /// Constructs a search URL using the specified query string.
            /// </summary>
            /// <param name="query">The search query to include in the URL. Must not be <c>null</c>.</param>
            /// <returns>A string containing the full search URL with the query parameter properly escaped.</returns>
            public static string SearchByQuery(string query) => $"{BasePath}/search?q={Uri.EscapeDataString(query)}";

            /// <summary>
            /// Returns the API endpoint URL for retrieving patients assigned to the specified physician.
            /// </summary>
            /// <param name="physicianId">The unique identifier of the physician.</param>
            /// <returns>The full path for querying patients by physician.</returns>
            public static string GetByPhysician(Guid physicianId) => $"{BasePath}/physician/{physicianId}";

            /// <summary>
            /// Returns the API endpoint URL for retrieving unassigned patients.
            /// </summary>
            /// <returns>The full path for querying unassigned patients.</returns>
            public static string GetUnassigned() => $"{BasePath}/unassigned";
        }

        /// <summary>
        /// Routes for Physicians API (/api/physicians)
        /// </summary>
        public static class Physicians
        {
            /// <summary>
            /// Represents the route segment used to identify physician-related controllers in API endpoints.
            /// </summary>
            /// <remarks>This constant can be used to construct URLs or route templates for endpoints
            /// that manage physician resources.</remarks>
            public const string Controller = "physicians";

            /// <summary>
            /// Represents the base path for the controller, combining the base route and controller name.
            /// </summary>
            /// <remarks>Use this constant to construct API endpoints or route URLs that require the
            /// controller's base path.</remarks>
            public const string BasePath = $"{Base}/{Controller}";

            // Route segments for [HttpGet] attributes

            /// <summary>
            /// Represents the route segment used to identify a physician by its unique identifier in HTTP GET requests.
            /// </summary>
            /// <remarks>This constant is typically used in route templates to specify endpoints that
            /// operate on a single resource, such as <c>GET /items/{id}</c>.</remarks>
            public const string ById = "{id}";

            /// <summary>
            /// Represents the string value used to identify the "search" operation or resource.
            /// </summary>
            public const string Search = "search";

            /// <summary>
            /// Represents the route template for accessing resources by specialization.
            /// </summary>
            public const string BySpecialization = "specialization/{specialization}";

            /// <summary>
            /// Represents the key used to access the set of valid physician specializations from the API.
            /// </summary>
            public const string Specializations = "specializations";

            /// <summary>
            /// Represents the route template for assigning a patient to an entity by identifier.
            /// </summary>
            /// <remarks>The template includes placeholders for <c>id</c> and <c>patientId</c>, which
            /// should be replaced with the appropriate values when constructing the route.</remarks>
            public const string AssignPatient = "{id}/patients/{patientId}";

            // Full paths for HttpClient calls

            /// <summary>
            /// Gets the base route string used for retrieving all physicians.
            /// </summary>
            /// <returns>The base route string for retrieving all physicians</returns>
            public static string GetAll() => BasePath;

            /// <summary>
            /// Returns the resource path corresponding to the specified physician's unique identifier.
            /// </summary>
            /// <param name="id">The unique identifier of the physician to retrieve. Must not be <see cref="Guid.Empty"/>.</param>
            /// <returns>A string containing the resource path for the specified <paramref name="id"/>.</returns>
            public static string GetById(Guid id) => $"{BasePath}/{id}";

            /// <summary>
            /// Constructs a search URL using the specified query string.
            /// </summary>
            /// <param name="query">The search query to include in the URL. Must not be <see langword="null"/>.</param>
            /// <returns>A string containing the search URL with the query parameter encoded.</returns>
            public static string SearchByQuery(string query) => $"{BasePath}/search?q={Uri.EscapeDataString(query)}";

            /// <summary>
            /// Returns the API endpoint URL for retrieving physicians by specialization.
            /// </summary>
            /// <param name="specialization">The specialization identifier to include in the endpoint URL. Must not be <see langword="null"/> or
            /// empty.</param>
            /// <returns>A string containing the full API endpoint URL for the specified specialization.</returns>
            public static string GetBySpecialization(string specialization) => $"{BasePath}/specialization/{specialization}";

            /// <summary>
            /// Generates a URI string for querying available resources on the specified date.
            /// </summary>
            /// <param name="date">The date for which to retrieve available resources. The date is formatted as "yyyy-MM-dd" in the
            /// resulting URI.</param>
            /// <returns>A URI string that can be used to request available resources for the specified date.</returns>
            public static string GetAvailableOn(DateTime date) => $"{BasePath}/available?date={date:yyyy-MM-dd}";
        }

        /// <summary>
        /// Routes for Administrators API (/api/administrators)
        /// </summary>
        public static class Administrators
        {
            /// <summary>
            /// Represents the route segment used to identify administrator-related controllers in routing operations.
            /// </summary>
            /// <remarks>Use this constant when constructing routes or URLs that target administrator
            /// functionality to ensure consistency across the application.</remarks>
            public const string Controller = "administrators";
            /// <summary>
            /// Represents the base path for the administrators API, combining the base route and controller name.
            /// </summary>
            public const string BasePath = $"{Base}/{Controller}";

            // Route segments for [HttpGet] attributes

            /// <summary>
            /// Represents the route segment used to identify an administrator by its unique identifier in HTTP GET requests.
            /// </summary>
            /// <remarks>This constant is typically used in route templates for controller actions
            /// that retrieve a single administrator profile by its <c>id</c> parameter, such as <c>[HttpGet("{id}")]</c>.</remarks>
            public const string ById = "{id}";

            /// <summary>
            /// Represents the route template for accessing administrators by department.
            /// </summary>
            /// <remarks>The <c>ByDepartment</c> constant can be used to construct URLs or route
            /// patterns where the <c>{department}</c> placeholder is replaced with the specific department name or
            /// identifier.</remarks>
            public const string ByDepartment = "by-department/{department}";

            /// <summary>
            /// Represents the route template for accessing resources by a specific permission.
            /// </summary>
            /// <remarks>Use this constant to construct URLs or route patterns that require a
            /// permission parameter. Replace <c>{permission}</c> with the desired permission value.</remarks>
            public const string ByPermission = "by-permission/{permission}";

            // Full paths for HttpClient calls

            /// <summary>
            /// Gets the full API endpoint path for retrieving all administrators.
            /// </summary>
            /// <returns>A string containing the absolute path to the endpoint used for fetching all administrators.</returns>
            public static string GetAll() => BasePath;

            /// <summary>
            /// Returns the resource path corresponding to the specified unique identifier.
            /// </summary>
            /// <param name="id">The unique identifier of the administrator to retrieve. Must not be <see cref="Guid.Empty"/>.</param>
            /// <returns>A string containing the resource path for the specified <paramref name="id"/>.</returns>
            public static string GetById(Guid id) => $"{BasePath}/{id}";

            /// <summary>
            /// Returns the API endpoint URL for retrieving administrators associated with the specified department.
            /// </summary>
            /// <param name="department">The name of the department for which to generate the endpoint URL. Cannot be <c>null</c> or empty.</param>
            /// <returns>A string containing the relative URL for accessing resources by department. The department name is
            /// URL-encoded in the returned path.</returns>
            public static string GetByDepartment(string department) => $"{BasePath}/by-department/{Uri.EscapeDataString(department)}";

            /// <summary>
            /// Returns the API endpoint URL for accessing administrators associated with the specified permission.
            /// </summary>
            /// <param name="permission">The permission identifier used to filter resources. Cannot be <c>null</c> or empty.</param>
            /// <returns>A string containing the URL path for resources filtered by the given permission.</returns>
            public static string GetByPermission(string permission) => $"{BasePath}/by-permission/{permission}";
        }

        /// <summary>
        /// Routes for Auth API (/api/auth)
        /// </summary>
        public static class Auth
        {
            /// <summary>
            /// Specifies the route segment for authentication-related controllers.
            /// </summary>
            /// <remarks>Use this constant to reference the "auth" route segment when defining
            /// controller routes or endpoints related to authentication functionality. This helps ensure consistency
            /// across the application.</remarks>
            public const string Controller = "auth";

            /// <summary>
            /// Represents the base path for API endpoints, composed of the base URL and controller segment.
            /// </summary>
            /// <remarks>Use this constant to construct endpoint URLs consistently throughout the
            /// application. The value is formed by combining the <c>Base</c> and <c>Controller</c> segments.</remarks>
            public const string BasePath = $"{Base}/{Controller}";

            // Route segments for [HttpGet] attributes

            /// <summary>
            /// Represents the route segment used to identify a resource by its unique identifier in HTTP GET requests.
            /// </summary>
            /// <remarks>This constant is typically used in route templates to specify endpoints that
            /// operate on a single resource, such as retrieving an entity by its ID.</remarks>
            public const string ById = "{id}";

            /// <summary>
            /// Represents the route template for accessing a resource by username.
            /// </summary>
            /// <remarks>The template includes a <c>{username}</c> placeholder, which should be
            /// replaced with the actual username when constructing the route.</remarks>
            public const string ByUsername = "by-username/{username}";

            /// <summary>
            /// Represents the string value used to identify the "search" operation.
            /// </summary>
            public const string Search = "search";

            /// <summary>
            /// Represents the action name for validation operations.
            /// </summary>
            /// <remarks>Use this constant to specify the "validate" action when interacting with APIs
            /// or components that require an action identifier for validation.</remarks>
            public const string Validate = "validate";

            /// <summary>
            /// Specifies the action name for user registration requests.
            /// </summary>
            /// <remarks>Use this constant to identify or reference the "register" action in
            /// authentication workflows or API calls, ensuring consistency across the application.</remarks>
            public const string Register = "register";

            /// <summary>
            /// Represents the action name for changing a user's password.
            /// </summary>
            /// <remarks>This constant can be used to identify or reference the "change-password"
            /// operation in authentication workflows, routing, or authorization policies.</remarks>
            public const string ChangePassword = "change-password";

            // Full paths for HttpClient calls

            /// <summary>
            /// Gets the full API endpoint path for retrieving all resources.
            /// </summary>
            /// <returns>A string containing the full path to the API endpoint used to fetch all items.</returns>
            public static string GetAll() => BasePath;

            /// <summary>
            /// Returns the resource path corresponding to the specified unique identifier.
            /// </summary>
            /// <param name="id">The unique identifier of the resource to retrieve. Must not be <see cref="Guid.Empty"/>.</param>
            /// <returns>A string containing the resource path for the specified <paramref name="id"/>.</returns>
            public static string GetById(Guid id) => $"{BasePath}/{id}";

            /// <summary>
            /// Returns the API endpoint URL for retrieving a resource by the specified username.
            /// </summary>
            /// <param name="username">The username to include in the endpoint URL. Cannot be <c>null</c> or empty.</param>
            /// <returns>A string containing the full API endpoint URL with the username safely encoded.</returns>
            public static string GetByUsername(string username) => $"{BasePath}/by-username/{Uri.EscapeDataString(username)}";

            /// <summary>
            /// Constructs a search URL using the specified query string.
            /// </summary>
            /// <param name="query">The search query to include in the URL. If <paramref name="query"/> is <see langword="null"/>, an empty
            /// string is used.</param>
            /// <returns>A string containing the search URL with the query parameter properly URL-encoded.</returns>
            public static string SearchByQuery(string query) => $"{BasePath}/search?q={Uri.EscapeDataString(query ?? "")}";

            /// <summary>
            /// Returns the endpoint URL for validating user credentials.
            /// </summary>
            /// <returns>A string containing the full URL to the credentials validation endpoint.</returns>
            public static string ValidateCredentials() => $"{BasePath}/validate";

            /// <summary>
            /// Gets the relative URL path for the user registration endpoint.
            /// </summary>
            /// <returns>A string containing the relative path to the registration endpoint.</returns>
            public static string RegisterCredentials() => $"{BasePath}/register";
            
            /// <summary>
            /// Gets the relative URL path for the user password change endpoint.
            /// </summary>
            /// <returns>A string containing the relative path to the change password API endpoint.</returns>
            public static string ChangeUserPassword() => $"{BasePath}/change-password";

            /// <summary>
            /// Generates the API endpoint URL to lock the specified user account.
            /// </summary>
            /// <param name="username">The username of the account to be locked. Must not be null or empty.</param>
            /// <returns>A string containing the relative URL for the lock operation on the specified user account.</returns>
            public static string LockAccount(string username) => $"{BasePath}/{Uri.EscapeDataString(username)}/lock";

            /// <summary>
            /// Generates the API endpoint URL to unlock the account associated with the specified username.
            /// </summary>
            /// <param name="username">The username of the account to unlock. Must not be <c>null</c> or empty.</param>
            /// <returns>A string containing the relative URL for the unlock account API endpoint for the specified user.</returns>
            public static string UnlockAccount(string username) => $"{BasePath}/{Uri.EscapeDataString(username)}/unlock";
        }

        /// <summary>
        /// Routes for Clinical Documents API (/api/clinicaldocuments)
        /// </summary>
        public static class ClinicalDocuments
        {
            /// <summary>
            /// Represents the route segment used for clinical documents-related controllers.
            /// </summary>
            public const string Controller = "clinicaldocuments";

            /// <summary>
            /// Represents the base path for clinical documents endpoints, combining the base route and controller name.
            /// </summary>
            public const string BasePath = $"{Base}/{Controller}";

            // Route segments for [HttpGet] attributes
            /// <summary>
            /// Route segment used to identify a clinical document by its unique identifier.
            /// </summary>
            public const string ById = "{id}";
            /// <summary>
            /// Route segment for retrieving clinical documents by patient.
            /// </summary>
            public const string ByPatient = "patient/{patientId}";
            /// <summary>
            /// Route segment for retrieving clinical documents by physician.
            /// </summary>
            public const string ByPhysician = "physician/{physicianId}";
            /// <summary>
            /// Route segment for retrieving clinical documents by appointment.
            /// </summary>
            public const string ByAppointment = "appointment/{appointmentId}";
            /// <summary>
            /// Route segment for completing a clinical document.
            /// </summary>
            public const string Complete = "{id}/complete";
            /// <summary>
            /// Route segment for diagnosis search.
            /// </summary>
            public const string SearchDiagnosis = "search/diagnosis";
            /// <summary>
            /// Route segment for medication search.
            /// </summary>
            public const string SearchMedication = "search/medication";
            /// <summary>
            /// Route segment for clinical document statistics.
            /// </summary>
            public const string Statistics = "statistics";
            /// <summary>
            /// Route segment for retrieving a SOAP note for a clinical document.
            /// </summary>
            public const string SoapNote = "{id}/soap-note";

            // Entry route segments
            /// <summary>
            /// Route segment for observations within a clinical document.
            /// </summary>
            public const string Observations = "{docId}/observations";
            /// <summary>
            /// Route segment for a specific observation by entry ID.
            /// </summary>
            public const string ObservationById = "{docId}/observations/{entryId}";
            /// <summary>
            /// Route segment for assessments within a clinical document.
            /// </summary>
            public const string Assessments = "{docId}/assessments";
            /// <summary>
            /// Route segment for a specific assessment by entry ID.
            /// </summary>
            public const string AssessmentById = "{docId}/assessments/{entryId}";
            /// <summary>
            /// Route segment for diagnoses within a clinical document.
            /// </summary>
            public const string Diagnoses = "{docId}/diagnoses";
            /// <summary>
            /// Route segment for a specific diagnosis by entry ID.
            /// </summary>
            public const string DiagnosisById = "{docId}/diagnoses/{entryId}";
            /// <summary>
            /// Route segment for plans within a clinical document.
            /// </summary>
            public const string Plans = "{docId}/plans";
            /// <summary>
            /// Route segment for a specific plan by entry ID.
            /// </summary>
            public const string PlanById = "{docId}/plans/{entryId}";
            /// <summary>
            /// Route segment for prescriptions within a clinical document.
            /// </summary>
            public const string Prescriptions = "{docId}/prescriptions";
            /// <summary>
            /// Route segment for a specific prescription by entry ID.
            /// </summary>
            public const string PrescriptionById = "{docId}/prescriptions/{entryId}";

            // Full paths for HttpClient calls
            /// <summary>
            /// Gets the base route for accessing all clinical documents.
            /// </summary>
            /// <returns>The base path for clinical documents.</returns>
            public static string GetAll() => BasePath;
            /// <summary>
            /// Returns the path to retrieve a clinical document by its unique identifier.
            /// </summary>
            /// <param name="id">The clinical document identifier.</param>
            /// <returns>The full path for the specified clinical document.</returns>
            public static string GetById(Guid id) => $"{BasePath}/{id}";
            /// <summary>
            /// Returns the path to retrieve clinical documents by patient.
            /// </summary>
            /// <param name="patientId">The patient identifier.</param>
            /// <returns>The full path for documents by patient.</returns>
            public static string GetByPatient(Guid patientId) => $"{BasePath}/patient/{patientId}";
            /// <summary>
            /// Returns the path to retrieve clinical documents by physician.
            /// </summary>
            /// <param name="physicianId">The physician identifier.</param>
            /// <returns>The full path for documents by physician.</returns>
            public static string GetByPhysician(Guid physicianId) => $"{BasePath}/physician/{physicianId}";
            /// <summary>
            /// Returns the path to retrieve clinical documents by appointment.
            /// </summary>
            /// <param name="appointmentId">The appointment identifier.</param>
            /// <returns>The full path for documents by appointment.</returns>
            public static string GetByAppointment(Guid appointmentId) => $"{BasePath}/appointment/{appointmentId}";
            /// <summary>
            /// Returns the path to retrieve incomplete clinical documents.
            /// </summary>
            /// <returns>The full path filtered to incomplete documents.</returns>
            public static string GetIncomplete() => $"{BasePath}?completed=false";
            /// <summary>
            /// Returns the path to retrieve clinical documents in a date range.
            /// </summary>
            /// <param name="start">Start date (inclusive).</param>
            /// <param name="end">End date (inclusive).</param>
            /// <returns>The full path with date range query parameters.</returns>
            public static string GetByDateRange(DateTime start, DateTime end) =>
                $"{BasePath}?fromDate={start:yyyy-MM-dd}&toDate={end:yyyy-MM-dd}";
            /// <summary>
            /// Returns the path to search clinical documents by diagnosis code.
            /// </summary>
            /// <param name="code">The diagnosis code to search.</param>
            /// <returns>The full path including the encoded diagnosis code.</returns>
            public static string SearchByDiagnosis(string code) => $"{BasePath}/search-diagnosis?code={Uri.EscapeDataString(code)}";
            /// <summary>
            /// Returns the path to search clinical documents by medication name.
            /// </summary>
            /// <param name="name">The medication name to search.</param>
            /// <returns>The full path including the encoded medication name.</returns>
            public static string SearchByMedication(string name) => $"{BasePath}/search-medication?name={Uri.EscapeDataString(name)}";
            /// <summary>
            /// Returns the path to mark a clinical document as complete.
            /// </summary>
            /// <param name="id">The clinical document identifier.</param>
            /// <returns>The full path to complete the document.</returns>
            public static string CompleteDocument(Guid id) => $"{BasePath}/{id}/complete";
            /// <summary>
            /// Returns the path to retrieve the SOAP note for a clinical document.
            /// </summary>
            /// <param name="id">The clinical document identifier.</param>
            /// <returns>The full path to the SOAP note endpoint.</returns>
            public static string GetSoapNote(Guid id) => $"{BasePath}/{id}/soap-note";
            /// <summary>
            /// Returns the path to search clinical documents by a free-text query.
            /// </summary>
            /// <param name="query">The search query string.</param>
            /// <returns>The full path including the encoded query string.</returns>
            public static string SearchByQuery(string query) => $"{BasePath}/search?q={Uri.EscapeDataString(query)}";

            // Entry paths
            /// <summary>
            /// Returns the path to retrieve observations for a clinical document.
            /// </summary>
            /// <param name="docId">The clinical document identifier.</param>
            /// <returns>The full path for observations.</returns>
            public static string GetObservations(Guid docId) => $"{BasePath}/{docId}/observations";
            /// <summary>
            /// Returns the path to retrieve a specific observation by entry ID.
            /// </summary>
            /// <param name="docId">The clinical document identifier.</param>
            /// <param name="entryId">The observation entry identifier.</param>
            /// <returns>The full path for the specified observation.</returns>
            public static string GetObservation(Guid docId, Guid entryId) => $"{BasePath}/{docId}/observations/{entryId}";
            /// <summary>
            /// Returns the path to retrieve assessments for a clinical document.
            /// </summary>
            /// <param name="docId">The clinical document identifier.</param>
            /// <returns>The full path for assessments.</returns>
            public static string GetAssessments(Guid docId) => $"{BasePath}/{docId}/assessments";
            /// <summary>
            /// Returns the path to retrieve a specific assessment by entry ID.
            /// </summary>
            /// <param name="docId">The clinical document identifier.</param>
            /// <param name="entryId">The assessment entry identifier.</param>
            /// <returns>The full path for the specified assessment.</returns>
            public static string GetAssessment(Guid docId, Guid entryId) => $"{BasePath}/{docId}/assessments/{entryId}";
            /// <summary>
            /// Returns the path to retrieve diagnoses for a clinical document.
            /// </summary>
            /// <param name="docId">The clinical document identifier.</param>
            /// <returns>The full path for diagnoses.</returns>
            public static string GetDiagnoses(Guid docId) => $"{BasePath}/{docId}/diagnoses";
            /// <summary>
            /// Returns the path to retrieve a specific diagnosis by entry ID.
            /// </summary>
            /// <param name="docId">The clinical document identifier.</param>
            /// <param name="entryId">The diagnosis entry identifier.</param>
            /// <returns>The full path for the specified diagnosis.</returns>
            public static string GetDiagnosis(Guid docId, Guid entryId) => $"{BasePath}/{docId}/diagnoses/{entryId}";
            /// <summary>
            /// Returns the path to retrieve plans for a clinical document.
            /// </summary>
            /// <param name="docId">The clinical document identifier.</param>
            /// <returns>The full path for plans.</returns>
            public static string GetPlans(Guid docId) => $"{BasePath}/{docId}/plans";
            /// <summary>
            /// Returns the path to retrieve a specific plan by entry ID.
            /// </summary>
            /// <param name="docId">The clinical document identifier.</param>
            /// <param name="entryId">The plan entry identifier.</param>
            /// <returns>The full path for the specified plan.</returns>
            public static string GetPlan(Guid docId, Guid entryId) => $"{BasePath}/{docId}/plans/{entryId}";
            /// <summary>
            /// Returns the path to retrieve prescriptions for a clinical document.
            /// </summary>
            /// <param name="docId">The clinical document identifier.</param>
            /// <returns>The full path for prescriptions.</returns>
            public static string GetPrescriptions(Guid docId) => $"{BasePath}/{docId}/prescriptions";
            /// <summary>
            /// Returns the path to retrieve a specific prescription by entry ID.
            /// </summary>
            /// <param name="docId">The clinical document identifier.</param>
            /// <param name="entryId">The prescription entry identifier.</param>
            /// <returns>The full path for the specified prescription.</returns>
            public static string GetPrescription(Guid docId, Guid entryId) => $"{BasePath}/{docId}/prescriptions/{entryId}";
        }
    }
}
