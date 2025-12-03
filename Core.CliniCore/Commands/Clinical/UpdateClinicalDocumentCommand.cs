using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.CliniCore.Service;

namespace Core.CliniCore.Commands.Clinical
{
    public class UpdateClinicalDocumentCommand : AbstractCommand
    {
        public const string Key = "updateclinicaldocument";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string DocumentId = "document_id";
            public const string ChiefComplaint = "chief_complaint";
            public const string Complete = "complete";
        }

        private readonly ClinicalDocumentService _documentRegistry;

        public UpdateClinicalDocumentCommand(ClinicalDocumentService clinicalDocService)
        {
            _documentRegistry = clinicalDocService ?? throw new ArgumentNullException(nameof(clinicalDocService));
        }

        public override string Description => "Updates an existing clinical document";

        public override bool CanUndo => false; // Document updates create audit trail, don't undo

        public override Permission? GetRequiredPermission()
            => Permission.UpdateClinicalDocument;

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var documentId = parameters.GetRequiredParameter<Guid>(Parameters.DocumentId);
                var document = _documentRegistry.GetDocumentById(documentId);

                if (document == null)
                {
                    return CommandResult.Fail("Clinical document not found");
                }

                if (document.IsCompleted)
                {
                    return CommandResult.Fail("Cannot modify a completed clinical document");
                }

                var fieldsUpdated = new List<string>();

                // Update chief complaint if provided
                var chiefComplaint = parameters.GetParameter<string>(Parameters.ChiefComplaint);
                if (!string.IsNullOrEmpty(chiefComplaint) && chiefComplaint != document.ChiefComplaint)
                {
                    document.ChiefComplaint = chiefComplaint;
                    fieldsUpdated.Add("chief complaint");
                }

                // Complete document if requested
                var complete = parameters.GetParameter<bool?>(Parameters.Complete) ?? false;
                if (complete && !document.IsCompleted)
                {
                    try
                    {
                        document.Complete();
                        fieldsUpdated.Add("completed status");
                    }
                    catch (InvalidOperationException ex)
                    {
                        return CommandResult.Fail($"Failed to complete document: {ex.Message}");
                    }
                }

                if (fieldsUpdated.Any())
                {
                    return CommandResult.Ok(
                        $"Clinical document updated successfully. Fields changed: {string.Join(", ", fieldsUpdated)}",
                        new { DocumentId = documentId, UpdatedFields = fieldsUpdated, IsCompleted = document.IsCompleted });
                }
                else
                {
                    return CommandResult.Ok("No changes were made to the clinical document", document);
                }
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to update clinical document: {ex.Message}", ex);
            }
        }

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

            // Check if document is already completed
            if (document.IsCompleted)
            {
                result.AddError("Cannot modify a completed clinical document");
                return result;
            }

            // Validate completion request
            var complete = parameters.GetParameter<bool?>(Parameters.Complete) ?? false;
            if (complete && !document.IsComplete())
            {
                var errors = document.GetValidationErrors();
                if (errors.Any())
                {
                    result.AddError($"Cannot complete document - validation errors: {string.Join("; ", errors)}");
                }
            }

            return result;
        }
    }
}
