using Core.CliniCore.Domain.Enumerations;
using System.Text;
using Core.CliniCore.Service;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.Domain.ClinicalDocumentation;

namespace Core.CliniCore.Commands.Query
{
    /// <summary>
    /// Command that searches clinical documents by diagnosis, medication, or general text content.
    /// Supports role-based access control where patients can only search their own documents
    /// and physicians can search documents for patients under their care.
    /// </summary>
    public class SearchClinicalNotesCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "searchclinicalnotes";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="SearchClinicalNotesCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the text to search for in clinical documents.
            /// </summary>
            public const string SearchTerm = "searchTerm";

            /// <summary>
            /// Parameter key for the search category (general, diagnosis, medication, prescription).
            /// </summary>
            public const string SearchType = "searchType";

            /// <summary>
            /// Parameter key for filtering results to a specific patient.
            /// </summary>
            public const string PatientId = "patientId";

            /// <summary>
            /// Parameter key for filtering results to documents authored by a specific physician.
            /// </summary>
            public const string PhysicianId = "physicianId";
        }

        private readonly ClinicalDocumentService _documentRegistry;
        private readonly ProfileService _profileRegistry;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchClinicalNotesCommand"/> class.
        /// </summary>
        /// <param name="profileService">The profile service for accessing user profiles.</param>
        /// <param name="clinicalDocService">The clinical document service for searching documents.</param>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is <see langword="null"/>.</exception>
        public SearchClinicalNotesCommand(ProfileService profileService, ClinicalDocumentService clinicalDocService)
        {
            _profileRegistry = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _documentRegistry = clinicalDocService ?? throw new ArgumentNullException(nameof(clinicalDocService));
        }

        /// <inheritdoc />
        public override string Description => "Search clinical documents by diagnosis, medication, or general text";

        /// <inheritdoc />
        public override Permission? GetRequiredPermission()
            => Permission.ViewOwnClinicalDocuments;

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            var missingParams = parameters.GetMissingRequired(Parameters.SearchTerm);
            if (missingParams.Count != 0)
            {
                foreach (var error in missingParams)
                    result.AddError(error);
                return result;
            }

            var searchTerm = parameters.GetParameter<string>(Parameters.SearchTerm);
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                result.AddError("Search term cannot be empty");
            }

            var searchType = parameters.GetParameter<string>(Parameters.SearchType) ?? "general";
            var validSearchTypes = new[] { "general", "diagnosis", "medication", "prescription" };
            if (!validSearchTypes.Contains(searchType.ToLowerInvariant()))
            {
                result.AddError($"Invalid search type. Valid types are: {string.Join(", ", validSearchTypes)}");
            }

            // Validate optional filters
            var patientId = parameters.GetParameter<Guid?>(Parameters.PatientId);
            if (patientId.HasValue && patientId.Value != Guid.Empty)
            {
                if (_profileRegistry.GetProfileById(patientId.Value) is not PatientProfile)
                {
                    result.AddError($"Patient with ID {patientId} not found");
                }
            }

            var physicianId = parameters.GetParameter<Guid?>(Parameters.PhysicianId);
            if (physicianId.HasValue && physicianId.Value != Guid.Empty)
            {
                if (_profileRegistry.GetProfileById(physicianId.Value) is not PhysicianProfile)
                {
                    result.AddError($"Physician with ID {physicianId} not found");
                }
            }

            return result;
        }

        protected override CommandValidationResult ValidateSpecific(CommandParameters parameters, SessionContext? session)
        {
            var result = CommandValidationResult.Success();

            if (session == null)
            {
                result.AddError("Must be logged in to search clinical documents");
                return result;
            }

            // Additional permission checks based on role
            if (session.UserRole == UserRole.Patient)
            {
                // Patients can only search their own documents
                var patientId = parameters.GetParameter<Guid?>(Parameters.PatientId);
                if (patientId.HasValue && patientId.Value != session.UserId)
                {
                    result.AddError("Patients can only search their own clinical documents");
                }
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var searchTerm = parameters.GetRequiredParameter<string>(Parameters.SearchTerm);
                var searchType = parameters.GetParameter<string>(Parameters.SearchType)?.ToLowerInvariant() ?? "general";
                var patientId = parameters.GetParameter<Guid?>(Parameters.PatientId);
                var physicianId = parameters.GetParameter<Guid?>(Parameters.PhysicianId);

                IEnumerable<ClinicalDocument> documents;

                // Perform the search based on type
                documents = searchType switch
                {
                    "diagnosis" => _documentRegistry.SearchByDiagnosis(searchTerm),
                    "medication" or "prescription" => _documentRegistry.SearchByMedication(searchTerm),
                    "general" or _ => SearchByGeneralText(searchTerm)
                };

                // Apply filters
                if (patientId.HasValue)
                {
                    documents = documents.Where(d => d.PatientId == patientId.Value);
                }

                if (physicianId.HasValue)
                {
                    documents = documents.Where(d => d.PhysicianId == physicianId.Value);
                }

                // Apply role-based filtering
                if (session?.UserRole == UserRole.Patient)
                {
                    documents = documents.Where(d => d.PatientId == session.UserId);
                }
                else if (session?.UserRole == UserRole.Physician)
                {
                    var physician = _profileRegistry.GetProfileById(session.UserId) as PhysicianProfile;
                    if (physician != null)
                    {
                        documents = documents.Where(d => d.PhysicianId == session.UserId ||
                                                       physician.PatientIds.Contains(d.PatientId));
                    }
                }

                var results = documents.ToList();

                if (!results.Any())
                {
                    return CommandResult.Ok($"No clinical documents found matching search term '{searchTerm}' in category '{searchType}'");
                }

                var output = FormatSearchResults(results, searchTerm, searchType);
                return CommandResult.Ok(output, results);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to search clinical documents: {ex.Message}", ex);
            }
        }

        private IEnumerable<ClinicalDocument> SearchByGeneralText(string searchTerm)
        {
            var allDocuments = _documentRegistry.GetAllDocuments();

            return allDocuments.Where(doc =>
                // Search in chief complaint
                (!string.IsNullOrEmpty(doc.ChiefComplaint) &&
                 doc.ChiefComplaint.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||

                // Search in entries
                doc.Entries.Any(entry =>
                    entry.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||

                // Search in diagnoses
                doc.GetDiagnoses().Any(d =>
                    d.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (d.ICD10Code != null && d.ICD10Code.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))) ||

                // Search in prescriptions
                doc.GetPrescriptions().Any(p =>
                    p.MedicationName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(p.Instructions) &&
                     p.Instructions.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
            );
        }

        private string FormatSearchResults(List<ClinicalDocument> results, string searchTerm, string searchType)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== CLINICAL DOCUMENT SEARCH RESULTS ===");
            sb.AppendLine($"Search Term: '{searchTerm}' (Type: {searchType})");
            sb.AppendLine($"Found {results.Count} document(s)");
            sb.AppendLine();

            foreach (var doc in results.OrderByDescending(d => d.CreatedAt))
            {
                var patient = _profileRegistry.GetProfileById(doc.PatientId) as PatientProfile;
                var physician = _profileRegistry.GetProfileById(doc.PhysicianId) as PhysicianProfile;

                sb.AppendLine($"Document ID: {doc.Id}");
                sb.AppendLine($"Created: {doc.CreatedAt:yyyy-MM-dd HH:mm}");
                sb.AppendLine($"Patient: {patient?.GetValue<string>(CommonEntryType.Name.GetKey()) ?? "Unknown"} (ID: {doc.PatientId:N})");
                sb.AppendLine($"Physician: Dr. {physician?.GetValue<string>(CommonEntryType.Name.GetKey()) ?? "Unknown"}");
                sb.AppendLine($"Chief Complaint: {doc.ChiefComplaint}");
                sb.AppendLine($"Status: {(doc.IsCompleted ? "Completed" : "Draft")}");

                // Show relevant snippets based on search type
                if (searchType == "diagnosis")
                {
                    var diagnoses = doc.GetDiagnoses().Where(d =>
                        d.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (d.ICD10Code != null && d.ICD10Code.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    );

                    if (diagnoses.Any())
                    {
                        sb.AppendLine("Matching Diagnoses:");
                        foreach (var diagnosis in diagnoses)
                        {
                            sb.AppendLine($"  • {diagnosis.Content}" + (diagnosis.ICD10Code != null ? $" ({diagnosis.ICD10Code})" : ""));
                        }
                    }
                }
                else if (searchType == "medication" || searchType == "prescription")
                {
                    var prescriptions = doc.GetPrescriptions().Where(p =>
                        p.MedicationName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrEmpty(p.Instructions) &&
                         p.Instructions.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    );

                    if (prescriptions.Any())
                    {
                        sb.AppendLine("Matching Prescriptions:");
                        foreach (var prescription in prescriptions)
                        {
                            sb.AppendLine($"  • {prescription.MedicationName} - {prescription.Dosage}");
                        }
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
