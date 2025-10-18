// Core.CliniCore/Commands/Clinical/ViewClinicalDocumentCommand.cs
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.ClinicalDoc;

namespace Core.CliniCore.Commands.Clinical
{
    public class ViewClinicalDocumentCommand : AbstractCommand
    {
        public const string Key = "viewclinicaldocument";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string DocumentId = "document_id";
            public const string Format = "format";
        }

        private readonly ClinicalDocumentRegistry _documentRegistry = ClinicalDocumentRegistry.Instance;
        private readonly ProfileRegistry _profileRegistry = ProfileRegistry.Instance;

        public override string Description => "Views the full details of a clinical document";

        public override bool CanUndo => false;

        public override Permission? GetRequiredPermission()
            => Permission.ViewOwnClinicalDocuments;

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            var missingParams = parameters.GetMissingRequired(Parameters.DocumentId);
            if (missingParams.Any())
            {
                foreach (var error in missingParams)
                    result.AddError(error);
                return result;
            }

            var documentId = parameters.GetParameter<Guid?>(Parameters.DocumentId);
            if (!documentId.HasValue || documentId.Value == Guid.Empty)
            {
                result.AddError("Invalid document ID");
            }
            else if (!_documentRegistry.DocumentExists(documentId.Value))
            {
                result.AddError($"Clinical document {documentId} not found");
            }

            return result;
        }

        protected override CommandValidationResult ValidateSpecific(CommandParameters parameters, SessionContext? session)
        {
            var result = CommandValidationResult.Success();

            if (session == null)
            {
                result.AddError("Must be logged in to view clinical documents");
                return result;
            }

            var documentId = parameters.GetParameter<Guid?>(Parameters.DocumentId);
            if (documentId.HasValue)
            {
                var document = _documentRegistry.GetDocumentById(documentId.Value);
                if (document != null)
                {
                    // Check access permissions
                    if (session.UserRole == UserRole.Patient && document.PatientId != session.UserId)
                    {
                        result.AddError("Patients can only view their own clinical documents");
                    }
                    else if (session.UserRole == UserRole.Physician)
                    {
                        // Physicians can view documents they created or for their patients
                        var physician = _profileRegistry.GetProfileById(session.UserId) as PhysicianProfile;
                        if (document.PhysicianId != session.UserId &&
                            (physician == null || !physician.PatientIds.Contains(document.PatientId)))
                        {
                            result.AddWarning("Viewing document for patient not under your care");
                        }
                    }
                }
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var documentId = parameters.GetRequiredParameter<Guid>(Parameters.DocumentId);
                var outputFormat = parameters.GetParameter<string>(Parameters.Format) ?? "full";

                var document = _documentRegistry.GetDocumentById(documentId);
                if (document == null)
                {
                    return CommandResult.Fail("Document not found");
                }

                string output = outputFormat.ToLower() switch
                {
                    "soap" => document.GenerateSOAPNote(),
                    "summary" => GenerateSummary(document),
                    _ => GenerateFullView(document)
                };

                return CommandResult.Ok(output, document);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to view clinical document: {ex.Message}", ex);
            }
        }

        private string GenerateSummary(ClinicalDocument doc)
        {
            var patient = _profileRegistry.GetProfileById(doc.PatientId) as PatientProfile;
            var physician = _profileRegistry.GetProfileById(doc.PhysicianId) as PhysicianProfile;

            return $"=== CLINICAL DOCUMENT SUMMARY ===\n" +
                   $"Document ID: {doc.Id}\n" +
                   $"Created: {doc.CreatedAt:yyyy-MM-dd HH:mm}\n" +
                   $"Status: {(doc.IsCompleted ? "Completed" : "Draft")}\n" +
                   $"Patient: {patient?.Name ?? "Unknown"} (ID: {doc.PatientId:N})\n" +
                   $"Physician: Dr. {physician?.Name ?? "Unknown"}\n" +
                   $"Chief Complaint: {doc.ChiefComplaint}\n" +
                   $"Total Entries: {doc.Entries.Count}\n" +
                   $"Diagnoses: {doc.GetDiagnoses().Count()}\n" +
                   $"Prescriptions: {doc.GetPrescriptions().Count()}\n";
        }

        private string GenerateFullView(ClinicalDocument doc)
        {
            // Use the built-in SOAP note generator plus additional details
            var soap = doc.GenerateSOAPNote();

            // Add entry count statistics
            var stats = $"\n=== DOCUMENT STATISTICS ===\n" +
                       $"Total Entries: {doc.Entries.Count}\n" +
                       $"Observations: {doc.GetObservations().Count()}\n" +
                       $"Assessments: {doc.GetAssessments().Count()}\n" +
                       $"Diagnoses: {doc.GetDiagnoses().Count()}\n" +
                       $"Plans: {doc.GetPlans().Count()}\n" +
                       $"Prescriptions: {doc.GetPrescriptions().Count()}\n";

            return soap + stats;
        }
    }
}