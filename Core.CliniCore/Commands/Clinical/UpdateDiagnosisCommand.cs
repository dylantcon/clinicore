using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.CliniCore.Service;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.ClinicalDocumentation.ClinicalEntries;

namespace Core.CliniCore.Commands.Clinical
{
    /// <summary>
    /// Command that updates an existing diagnosis entry within a clinical document.
    /// </summary>
    public class UpdateDiagnosisCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "updatediagnosis";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="UpdateDiagnosisCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the clinical document identifier that owns the diagnosis.
            /// </summary>
            public const string DocumentId = "document_id";

            /// <summary>
            /// Parameter key for the diagnosis entry identifier.
            /// </summary>
            public const string DiagnosisId = "diagnosis_id";

            /// <summary>
            /// Parameter key for the updated diagnosis description.
            /// </summary>
            public const string Content = "content";

            /// <summary>
            /// Parameter key for the diagnosis type.
            /// </summary>
            public const string Type = "type";

            /// <summary>
            /// Parameter key for the ICD-10 diagnosis code.
            /// </summary>
            public const string ICD10Code = "icd10_code";

            /// <summary>
            /// Parameter key indicating whether this is the primary diagnosis.
            /// </summary>
            public const string IsPrimary = "is_primary";

            /// <summary>
            /// Parameter key for the diagnosis onset date.
            /// </summary>
            public const string OnsetDate = "onset_date";

            /// <summary>
            /// Parameter key for the diagnosis status.
            /// </summary>
            public const string Status = "status";

            /// <summary>
            /// Parameter key for the severity associated with the diagnosis.
            /// </summary>
            public const string Severity = "severity";
        }

        private readonly ClinicalDocumentService _documentRegistry;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateDiagnosisCommand"/> class.
        /// </summary>
        /// <param name="clinicalDocService">The clinical document service used to access and update documents.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="clinicalDocService"/> is <c>null</c>.</exception>
        public UpdateDiagnosisCommand(ClinicalDocumentService clinicalDocService)
        {
            _documentRegistry = clinicalDocService ?? throw new ArgumentNullException(nameof(clinicalDocService));
        }

        /// <inheritdoc />
        public override string Description => "Updates a diagnosis entry within a clinical document";

        /// <inheritdoc />
        public override bool CanUndo => false;

        /// <inheritdoc />
        public override Permission? GetRequiredPermission()
            => Permission.UpdateClinicalDocument;

        /// <inheritdoc />
        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Check required parameters
            var missingParams = parameters.GetMissingRequired(Parameters.DocumentId, Parameters.DiagnosisId);
            if (missingParams.Any())
            {
                foreach (var error in missingParams)
                    result.AddError(error);
                return result;
            }

            // Validate IDs
            var documentId = parameters.GetParameter<Guid?>(Parameters.DocumentId);
            if (!documentId.HasValue || documentId.Value == Guid.Empty)
            {
                result.AddError("Invalid document ID");
                return result;
            }

            var diagnosisId = parameters.GetParameter<Guid?>(Parameters.DiagnosisId);
            if (!diagnosisId.HasValue || diagnosisId.Value == Guid.Empty)
            {
                result.AddError("Invalid diagnosis ID");
                return result;
            }

            // Check document exists
            var document = _documentRegistry.GetDocumentById(documentId.Value);
            if (document == null)
            {
                result.AddError($"Clinical document with ID {documentId.Value} not found");
                return result;
            }

            if (document.IsCompleted)
            {
                result.AddError("Cannot modify entries in a completed clinical document");
                return result;
            }

            // Check diagnosis exists and is correct type
            var entry = document.Entries.FirstOrDefault(e => e.Id == diagnosisId.Value);
            if (entry == null)
            {
                result.AddError($"Diagnosis with ID {diagnosisId.Value} not found in document");
                return result;
            }

            if (entry is not DiagnosisEntry)
            {
                result.AddError($"Entry with ID {diagnosisId.Value} is not a diagnosis entry");
                return result;
            }

            // Validate DiagnosisType if provided
            var typeStr = parameters.GetParameter<string>(Parameters.Type);
            if (!string.IsNullOrEmpty(typeStr) && !Enum.TryParse<DiagnosisType>(typeStr, true, out _))
            {
                var validTypes = string.Join(", ", Enum.GetNames<DiagnosisType>());
                result.AddError($"Invalid diagnosis type. Valid values are: {validTypes}");
            }

            // Validate DiagnosisStatus if provided
            var statusStr = parameters.GetParameter<string>(Parameters.Status);
            if (!string.IsNullOrEmpty(statusStr) && !Enum.TryParse<DiagnosisStatus>(statusStr, true, out _))
            {
                var validStatuses = string.Join(", ", Enum.GetNames<DiagnosisStatus>());
                result.AddError($"Invalid diagnosis status. Valid values are: {validStatuses}");
            }

            // Validate EntrySeverity if provided
            var severityStr = parameters.GetParameter<string>(Parameters.Severity);
            if (!string.IsNullOrEmpty(severityStr) && !Enum.TryParse<EntrySeverity>(severityStr, true, out _))
            {
                var validSeverities = string.Join(", ", Enum.GetNames<EntrySeverity>());
                result.AddError($"Invalid severity. Valid values are: {validSeverities}");
            }

            // Business rule: Final diagnosis requires ICD-10 code
            if (typeStr != null && Enum.TryParse<DiagnosisType>(typeStr, true, out var diagType))
            {
                if (diagType == DiagnosisType.Final)
                {
                    var icd10Code = parameters.GetParameter<string>(Parameters.ICD10Code);
                    var existingDiagnosis = entry as DiagnosisEntry;
                    if (string.IsNullOrEmpty(icd10Code) && string.IsNullOrEmpty(existingDiagnosis?.ICD10Code))
                    {
                        result.AddError("Final diagnosis requires an ICD-10 code");
                    }
                }
            }

            return result;
        }

        /// <inheritdoc />
        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var documentId = parameters.GetRequiredParameter<Guid>(Parameters.DocumentId);
                var diagnosisId = parameters.GetRequiredParameter<Guid>(Parameters.DiagnosisId);

                var document = _documentRegistry.GetDocumentById(documentId);
                if (document == null)
                {
                    return CommandResult.Fail("Clinical document not found");
                }

                var diagnosis = document.Entries.FirstOrDefault(e => e.Id == diagnosisId) as DiagnosisEntry;
                if (diagnosis == null)
                {
                    return CommandResult.Fail("Diagnosis entry not found in document");
                }

                var fieldsUpdated = new List<string>();

                // Update content (diagnosis description)
                var content = parameters.GetParameter<string>(Parameters.Content);
                if (!string.IsNullOrEmpty(content) && content != diagnosis.Content)
                {
                    diagnosis.Update(content);
                    fieldsUpdated.Add("content");
                }

                // Update diagnosis type
                var typeStr = parameters.GetParameter<string>(Parameters.Type);
                if (!string.IsNullOrEmpty(typeStr) && Enum.TryParse<DiagnosisType>(typeStr, true, out var type))
                {
                    if (type != diagnosis.Type)
                    {
                        diagnosis.Type = type;
                        fieldsUpdated.Add("type");
                    }
                }

                // Update ICD-10 code
                var icd10Code = parameters.GetParameter<string>(Parameters.ICD10Code);
                if (icd10Code != null && icd10Code != diagnosis.ICD10Code)
                {
                    diagnosis.ICD10Code = string.IsNullOrEmpty(icd10Code) ? null : icd10Code;
                    fieldsUpdated.Add("icd10_code");
                }

                // Update primary flag
                var isPrimary = parameters.GetParameter<bool?>(Parameters.IsPrimary);
                if (isPrimary.HasValue && isPrimary.Value != diagnosis.IsPrimary)
                {
                    // If setting as primary, unset other primary diagnoses
                    if (isPrimary.Value)
                    {
                        foreach (var otherDiagnosis in document.GetDiagnoses().Where(d => d.Id != diagnosisId))
                        {
                            otherDiagnosis.IsPrimary = false;
                        }
                    }
                    diagnosis.IsPrimary = isPrimary.Value;
                    fieldsUpdated.Add("is_primary");
                }

                // Update onset date
                var onsetDate = parameters.GetParameter<DateTime?>(Parameters.OnsetDate);
                if (onsetDate.HasValue && onsetDate != diagnosis.OnsetDate)
                {
                    diagnosis.OnsetDate = onsetDate.Value;
                    fieldsUpdated.Add("onset_date");
                }

                // Update status
                var statusStr = parameters.GetParameter<string>(Parameters.Status);
                if (!string.IsNullOrEmpty(statusStr) && Enum.TryParse<DiagnosisStatus>(statusStr, true, out var status))
                {
                    if (status != diagnosis.Status)
                    {
                        diagnosis.Status = status;
                        fieldsUpdated.Add("status");
                    }
                }

                // Update severity
                var severityStr = parameters.GetParameter<string>(Parameters.Severity);
                if (!string.IsNullOrEmpty(severityStr) && Enum.TryParse<EntrySeverity>(severityStr, true, out var severity))
                {
                    if (severity != diagnosis.Severity)
                    {
                        diagnosis.Severity = severity;
                        fieldsUpdated.Add("severity");
                    }
                }

                // Validate after updates
                var errors = diagnosis.GetValidationErrors();
                if (errors.Any())
                {
                    return CommandResult.ValidationFailed(errors);
                }

                if (fieldsUpdated.Any())
                {
                    // Persist the changes to the repository
                    _documentRegistry.UpdateDocument(document);

                    return CommandResult.Ok(
                        $"Diagnosis entry updated successfully. Fields changed: {string.Join(", ", fieldsUpdated)}",
                        new {
                            DocumentId = documentId,
                            DiagnosisId = diagnosisId,
                            UpdatedFields = fieldsUpdated,
                            ModifiedAt = diagnosis.ModifiedAt
                        });
                }
                else
                {
                    return CommandResult.Ok("No changes were made to the diagnosis entry", diagnosis);
                }
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to update diagnosis: {ex.Message}", ex);
            }
        }
    }
}