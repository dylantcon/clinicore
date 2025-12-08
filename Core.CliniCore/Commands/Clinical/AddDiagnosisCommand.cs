// Core.CliniCore/Commands/Clinical/AddDiagnosisCommand.cs
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Service;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.ClinicalDocumentation.ClinicalEntries;

namespace Core.CliniCore.Commands.Clinical
{
    /// <summary>
    /// Command that adds a diagnosis entry to an existing clinical document.
    /// </summary>
    public class AddDiagnosisCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "adddiagnosis";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="AddDiagnosisCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the target clinical document identifier.
            /// </summary>
            public const string DocumentId = "document_id";

            /// <summary>
            /// Parameter key for the diagnosis description.
            /// </summary>
            public const string DiagnosisDescription = "diagnosis_description";

            /// <summary>
            /// Parameter key for the ICD-10 diagnosis code.
            /// </summary>
            public const string ICD10Code = "icd10_code";

            /// <summary>
            /// Parameter key for the diagnosis type.
            /// </summary>
            public const string DiagnosisType = "diagnosis_type";

            /// <summary>
            /// Parameter key for the diagnosis status.
            /// </summary>
            public const string DiagnosisStatus = "diagnosis_status";

            /// <summary>
            /// Parameter key indicating whether this is the primary diagnosis.
            /// </summary>
            public const string IsPrimary = "is_primary";

            /// <summary>
            /// Parameter key for the severity of the diagnosis.
            /// </summary>
            public const string Severity = "severity";

            /// <summary>
            /// Parameter key for the diagnosis onset date.
            /// </summary>
            public const string OnsetDate = "onset_date";
        }

        private readonly ClinicalDocumentService _documentRegistry;
        private DiagnosisEntry? _addedDiagnosis;
        private Guid? _targetDocumentId;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddDiagnosisCommand"/> class.
        /// </summary>
        /// <param name="clinicalDocService">The clinical document service used to access and modify documents.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="clinicalDocService"/> is <c>null</c>.</exception>
        public AddDiagnosisCommand(ClinicalDocumentService clinicalDocService)
        {
            _documentRegistry = clinicalDocService ?? throw new ArgumentNullException(nameof(clinicalDocService));
        }

        /// <inheritdoc />
        public override string Description => "Adds a diagnosis to a clinical document";

        /// <inheritdoc />
        public override bool CanUndo => true;

        /// <inheritdoc />
        public override Permission? GetRequiredPermission()
            => Permission.CreateClinicalDocument;

        /// <inheritdoc />
        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Check required parameters
            var missingParams = parameters.GetMissingRequired(
                Parameters.DocumentId, Parameters.DiagnosisDescription);

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
                result.AddError($"Clinical document {documentId} not found");
                return result;
            }

            var document = _documentRegistry.GetDocumentById(documentId.Value);
            if (document == null)
            {
                result.AddError($"Clinical document {documentId.Value} not found");
            }
            else if (document.IsCompleted)
            {
                result.AddError("Cannot modify completed clinical document");
            }

            // Validate diagnosis description
            var description = parameters.GetParameter<string>(Parameters.DiagnosisDescription);
            if (string.IsNullOrWhiteSpace(description))
            {
                result.AddError("Diagnosis description cannot be empty");
            }

            // Validate ICD-10 code if provided
            var icd10Code = parameters.GetParameter<string>(Parameters.ICD10Code);
            if (!string.IsNullOrWhiteSpace(icd10Code))
            {
                // Basic ICD-10 format validation (letter followed by numbers and optional decimal)
                if (!System.Text.RegularExpressions.Regex.IsMatch(icd10Code, @"^[A-Z]\d{2}(\.\d{1,4})?$"))
                {
                    result.AddWarning("ICD-10 code format may be invalid");
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
                var description = parameters.GetRequiredParameter<string>(Parameters.DiagnosisDescription);
                var physicianId = session?.UserId ?? Guid.Empty;

                _targetDocumentId = documentId;
                var document = _documentRegistry.GetDocumentById(documentId);
                if (document == null)
                {
                    return CommandResult.Fail($"Clinical document {documentId} not found");
                }

                // Create the diagnosis
                _addedDiagnosis = new DiagnosisEntry(physicianId, description)
                {
                    ICD10Code = parameters.GetParameter<string>(Parameters.ICD10Code),
                    Type = parameters.GetParameter<DiagnosisType?>(Parameters.DiagnosisType) ?? DiagnosisType.Working,
                    Status = parameters.GetParameter<DiagnosisStatus?>(Parameters.DiagnosisStatus) ?? DiagnosisStatus.Active,
                    IsPrimary = parameters.GetParameter<bool?>(Parameters.IsPrimary) ?? false,
                    Severity = parameters.GetParameter<EntrySeverity?>(Parameters.Severity) ?? EntrySeverity.Routine,
                    OnsetDate = parameters.GetParameter<DateTime?>(Parameters.OnsetDate)
                };

                // Persist the diagnosis via the service's entry-level method
                _documentRegistry.AddDiagnosis(documentId, _addedDiagnosis);

                var diagnosisTypeStr = _addedDiagnosis.Type != DiagnosisType.Final
                    ? $" ({_addedDiagnosis.Type})"
                    : "";
                var icd10Str = !string.IsNullOrEmpty(_addedDiagnosis.ICD10Code)
                    ? $" [ICD-10: {_addedDiagnosis.ICD10Code}]"
                    : "";

                return CommandResult.Ok(
                    $"Diagnosis added successfully:\n" +
                    $"  Description: {description}{diagnosisTypeStr}\n" +
                    $"  Primary: {(_addedDiagnosis.IsPrimary ? "Yes" : "No")}\n" +
                    $"  Severity: {_addedDiagnosis.Severity}{icd10Str}\n" +
                    $"  Diagnosis ID: {_addedDiagnosis.Id}",
                    _addedDiagnosis);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to add diagnosis: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        protected override object? CaptureStateForUndo(CommandParameters parameters, SessionContext? session)
        {
            return new UndoState
            {
                DocumentId = _targetDocumentId ?? Guid.Empty,
                DiagnosisId = _addedDiagnosis?.Id ?? Guid.Empty
            };
        }

        /// <inheritdoc />
        protected override CommandResult UndoCore(object previousState, SessionContext? session)
        {
            if (previousState is UndoState state)
            {
                var document = _documentRegistry.GetDocumentById(state.DocumentId);
                if (document != null)
                {
                    // Note: We'd need to add a RemoveEntry method to ClinicalDocument
                    // For now, we can mark it as inactive
                    var diagnosis = document.GetDiagnoses().FirstOrDefault(d => d.Id == state.DiagnosisId);
                    if (diagnosis != null)
                    {
                        diagnosis.IsActive = false;
                        return CommandResult.Ok($"Diagnosis {state.DiagnosisId} has been marked inactive");
                    }
                }
            }
            return CommandResult.Fail("Unable to undo diagnosis addition");
        }

        private class UndoState
        {
            /// <summary>
            /// Gets or sets the identifier of the clinical document that owns the diagnosis.
            /// </summary>
            public Guid DocumentId { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the diagnosis entry that was added.
            /// </summary>
            public Guid DiagnosisId { get; set; }
        }
    }
}
