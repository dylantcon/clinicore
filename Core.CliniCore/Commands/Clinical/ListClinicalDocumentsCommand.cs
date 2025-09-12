// Core.CliniCore/Commands/Clinical/ListClinicalDocumentsCommand.cs
using System;
using System.Text;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.ClinicalDoc;

namespace Core.CliniCore.Commands.Clinical
{
    public class ListClinicalDocumentsCommand : AbstractCommand
    {
        public static class Parameters
        {
            public const string PatientId = "patient_id";
            public const string PhysicianId = "physician_id";
            public const string StartDate = "start_date";
            public const string EndDate = "end_date";
            public const string IncompleteOnly = "incomplete_only";
        }

        private readonly ClinicalDocumentRegistry _documentRegistry = ClinicalDocumentRegistry.Instance;
        private readonly ProfileRegistry _profileRegistry = ProfileRegistry.Instance;

        public override string Description => "Lists clinical documents with various filters";

        public override bool CanUndo => false;

        public override Permission? GetRequiredPermission()
            => Permission.ViewOwnClinicalDocuments;

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
                   $"  Patient: {patient?.Name ?? "Unknown"}\n" +
                   $"  Physician: Dr. {physician?.Name ?? "Unknown"}\n" +
                   $"  Chief Complaint: {doc.ChiefComplaint}\n" +
                   $"  Diagnoses: {diagnosisCount}, Prescriptions: {prescriptionCount}\n" +
                   $"  ---";
        }
    }
}
