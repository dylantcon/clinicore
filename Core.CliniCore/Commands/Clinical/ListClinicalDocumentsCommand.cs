// Core.CliniCore/Commands/Clinical/ListClinicalDocumentsCommand.cs
using System;
using System.Text;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Service;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.Domain.ClinicalDocumentation;

namespace Core.CliniCore.Commands.Clinical
{
    /// <summary>
    /// Command that lists clinical documents using optional filters such as patient, physician, and date range.
    /// </summary>
    public class ListClinicalDocumentsCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "listclinicaldocuments";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="ListClinicalDocumentsCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for filtering by patient identifier.
            /// </summary>
            public const string PatientId = "patient_id";

            /// <summary>
            /// Parameter key for filtering by physician identifier.
            /// </summary>
            public const string PhysicianId = "physician_id";

            /// <summary>
            /// Parameter key for the start of the creation date range filter.
            /// </summary>
            public const string StartDate = "start_date";

            /// <summary>
            /// Parameter key for the end of the creation date range filter.
            /// </summary>
            public const string EndDate = "end_date";

            /// <summary>
            /// Parameter key indicating that only incomplete (draft) documents should be returned.
            /// </summary>
            public const string IncompleteOnly = "incomplete_only";
        }

        private readonly ClinicalDocumentService _documentRegistry;
        private readonly ProfileService _profileRegistry;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListClinicalDocumentsCommand"/> class.
        /// </summary>
        /// <param name="profileService">The profile service used to resolve patient and physician details.</param>
        /// <param name="clinicalDocService">The clinical document service used to query documents.</param>
        /// <exception cref="ArgumentNullException">Thrown when any dependency is <c>null</c>.</exception>
        public ListClinicalDocumentsCommand(ProfileService profileService, ClinicalDocumentService clinicalDocService)
        {
            _profileRegistry = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _documentRegistry = clinicalDocService ?? throw new ArgumentNullException(nameof(clinicalDocService));
        }

        /// <inheritdoc />
        public override string Description => "Lists clinical documents with various filters";

        /// <inheritdoc />
        public override bool CanUndo => false;

        /// <inheritdoc />
        public override Permission? GetRequiredPermission()
            => Permission.CreateClinicalDocument;

        /// <inheritdoc />
        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Validate date range if provided
            var startDate = parameters.GetParameter<DateTime?>(Parameters.StartDate);
            var endDate = parameters.GetParameter<DateTime?>(Parameters.EndDate);

            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                result.AddError("Start date must be before end date");
            }

            return result;
        }

        /// <inheritdoc />
        protected override CommandValidationResult ValidateSpecific(CommandParameters parameters, SessionContext? session)
        {
            var result = CommandValidationResult.Success();

            // Patients can only view their own documents
            if (session != null && session.UserRole == UserRole.Patient)
            {
                var patientId = parameters.GetParameter<Guid?>(Parameters.PatientId);
                if (patientId.HasValue && patientId.Value != session.UserId)
                {
                    result.AddError("Patients can only view their own clinical documents");
                }
            }

            return result;
        }

        /// <inheritdoc />
        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                IEnumerable<ClinicalDocument> documents;

                // Get filter parameters
                var patientId = parameters.GetParameter<Guid?>(Parameters.PatientId);
                var physicianId = parameters.GetParameter<Guid?>(Parameters.PhysicianId);
                var startDate = parameters.GetParameter<DateTime?>(Parameters.StartDate) ?? DateTime.MinValue;
                var endDate = parameters.GetParameter<DateTime?>(Parameters.EndDate) ?? DateTime.MaxValue;
                var incompleteOnly = parameters.GetParameter<bool?>(Parameters.IncompleteOnly) ?? false;

                // Apply role-based filtering
                if (session?.UserRole == UserRole.Patient)
                {
                    // Patients see only their own documents
                    patientId = session.UserId;
                }

                // Get documents based on filters
                if (incompleteOnly)
                {
                    documents = _documentRegistry.GetIncompleteDocuments(physicianId);
                    if (patientId.HasValue)
                    {
                        documents = documents.Where(d => d.PatientId == patientId.Value);
                    }
                }
                else if (patientId.HasValue)
                {
                    documents = _documentRegistry.GetPatientDocuments(patientId.Value);
                }
                else if (physicianId.HasValue)
                {
                    documents = _documentRegistry.GetPhysicianDocuments(physicianId.Value);
                }
                else
                {
                    documents = _documentRegistry.GetDocumentsInDateRange(
                        startDate, endDate, null, null);
                }

                var documentList = documents.ToList();

                if (!documentList.Any())
                {
                    return CommandResult.Ok("No clinical documents found matching criteria.", documentList);
                }

                var sb = new StringBuilder();
                sb.AppendLine($"Found {documentList.Count} clinical document(s):");
                sb.AppendLine(new string('-', 80));

                foreach (var doc in documentList)
                {
                    sb.AppendLine(FormatDocumentSummary(doc));
                }

                return CommandResult.Ok(sb.ToString(), documentList);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to list clinical documents: {ex.Message}", ex);
            }
        }

        private string FormatDocumentSummary(ClinicalDocument doc)
        {
            var patient = _profileRegistry.GetProfileById(doc.PatientId) as PatientProfile;
            var physician = _profileRegistry.GetProfileById(doc.PhysicianId) as PhysicianProfile;

            var status = doc.IsCompleted ? "COMPLETED" : "DRAFT";
            var diagnosisCount = doc.GetDiagnoses().Count();
            var prescriptionCount = doc.GetPrescriptions().Count();

            return $"  Document ID: {doc.Id:N}\n" +
                   $"  Date: {doc.CreatedAt:yyyy-MM-dd HH:mm}\n" +
                   $"  Status: {status}\n" +
                   $"  Patient: {patient?.GetValue<string>(CommonEntryType.Name.GetKey()) ?? "Unknown"}\n" +
                   $"  Physician: Dr. {physician?.GetValue<string>(CommonEntryType.Name.GetKey()) ?? "Unknown"}\n" +
                   $"  Chief Complaint: {doc.ChiefComplaint}\n" +
                   $"  Diagnoses: {diagnosisCount}, Prescriptions: {prescriptionCount}\n" +
                   $"  ---";
        }
    }
}
