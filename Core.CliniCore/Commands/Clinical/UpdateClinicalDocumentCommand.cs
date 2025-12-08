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
    /// <summary>
    /// Command that updates high-level properties of an existing clinical document.
    /// </summary>
    public class UpdateClinicalDocumentCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "updateclinicaldocument";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="UpdateClinicalDocumentCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the clinical document identifier to update.
            /// </summary>
            public const string DocumentId = "document_id";

            /// <summary>
            /// Parameter key for the updated chief complaint text.
            /// </summary>
            public const string ChiefComplaint = "chief_complaint";

            /// <summary>
            /// Parameter key indicating whether the document should be marked as completed.
            /// </summary>
            public const string Complete = "complete";
        }

        private readonly ClinicalDocumentService _documentRegistry;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateClinicalDocumentCommand"/> class.
        /// </summary>
        /// <param name="clinicalDocService">The clinical document service used to access and update documents.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="clinicalDocService"/> is <c>null</c>.</exception>
        public UpdateClinicalDocumentCommand(ClinicalDocumentService clinicalDocService)
        {
            _documentRegistry = clinicalDocService ?? throw new ArgumentNullException(nameof(clinicalDocService));
        }

        /// <inheritdoc />
        public override string Description => "Updates an existing clinical document";

        /// <inheritdoc />
        public override bool CanUndo => false; // Document updates create audit trail, don't undo

        /// <inheritdoc />
        public override Permission? GetRequiredPermission()
            => Permission.UpdateClinicalDocument;

        /// <inheritdoc />
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
                    // Persist the changes to the repository
                    _documentRegistry.UpdateDocument(document);

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

        /// <inheritdoc />
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
