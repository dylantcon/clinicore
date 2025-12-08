using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.CliniCore.Service;
using Core.CliniCore.Domain.Authentication.Representation;

namespace Core.CliniCore.Commands.Clinical
{
    public class DeleteClinicalDocumentCommand : AbstractCommand
    {
        public const string Key = "deleteclinicaldocument";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string DocumentId = "document_id";
            public const string Force = "force";
        }

        private readonly ClinicalDocumentService _documentRegistry;

        public DeleteClinicalDocumentCommand(ClinicalDocumentService clinicalDocService)
        {
            _documentRegistry = clinicalDocService ?? throw new ArgumentNullException(nameof(clinicalDocService));
        }

        public override string Description => "Permanently deletes a clinical document from the system";

        public override bool CanUndo => false; // Deletions cannot be undone

        public override Permission? GetRequiredPermission()
            => Permission.DeleteClinicalDocument;

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Check required DocumentId parameter
            var missingParams = parameters.GetMissingRequired(Parameters.DocumentId);
            if (missingParams.Any())
            {
                foreach (var error in missingParams)
                    result.AddError(error);
                return result;
            }

            // Validate document exists
            var documentId = parameters.GetParameter<Guid?>(Parameters.DocumentId);
            if (!documentId.HasValue || documentId.Value == Guid.Empty)
            {
                result.AddError("Invalid document ID");
                return result;
            }

            var document = _documentRegistry.GetDocumentById(documentId.Value);
            if (document == null)
            {
                result.AddError($"Clinical document with ID {documentId.Value} not found");
                return result;
            }

            // Check if document is completed
            var force = parameters.GetParameter<bool?>(Parameters.Force) ?? false;
            if (document.IsCompleted && !force)
            {
                result.AddWarning("Document is completed - consider using force=true to override");
                result.AddError("Cannot delete completed document without force parameter");
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var documentId = parameters.GetRequiredParameter<Guid>(Parameters.DocumentId);
                var force = parameters.GetParameter<bool?>(Parameters.Force) ?? false;

                var document = _documentRegistry.GetDocumentById(documentId);
                if (document == null)
                {
                    return CommandResult.Fail("Clinical document not found");
                }

                // Check completion status
                if (document.IsCompleted && !force)
                {
                    return CommandResult.Fail("Cannot delete completed document. Use force parameter to override.");
                }

                // Store document info for result message
                var patientId = document.PatientId;
                var physicianId = document.PhysicianId;
                var appointmentId = document.AppointmentId;
                var createdAt = document.CreatedAt;
                var entryCount = document.Entries.Count;
                var isCompleted = document.IsCompleted;

                // Remove the document from registry
                var success = _documentRegistry.RemoveDocument(documentId);
                if (!success)
                {
                    return CommandResult.Fail("Failed to remove clinical document from registry");
                }

                var statusText = isCompleted ? "completed" : "incomplete";
                return CommandResult.Ok(
                    $"Clinical document (ID: {documentId}) with {entryCount} entries has been permanently deleted. " +
                    $"Document was {statusText} and created on {createdAt:yyyy-MM-dd}.",
                    new {
                        DocumentId = documentId,
                        PatientId = patientId,
                        PhysicianId = physicianId,
                        AppointmentId = appointmentId,
                        EntryCount = entryCount,
                        WasCompleted = isCompleted,
                        CreatedAt = createdAt
                    });
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to delete clinical document: {ex.Message}", ex);
            }
        }
    }
}