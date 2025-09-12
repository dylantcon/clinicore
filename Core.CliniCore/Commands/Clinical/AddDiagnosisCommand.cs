// Core.CliniCore/Commands/Clinical/AddDiagnosisCommand.cs
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.ClinicalDoc;

namespace Core.CliniCore.Commands.Clinical
{
    public class AddDiagnosisCommand : AbstractCommand
    {
        public static class Parameters
        {
            public const string DocumentId = "document_id";
            public const string DiagnosisDescription = "diagnosis_description";
            public const string ICD10Code = "icd10_code";
            public const string DiagnosisType = "diagnosis_type";
            public const string IsPrimary = "is_primary";
            public const string Severity = "severity";
            public const string OnsetDate = "onset_date";
        }

        private readonly ClinicalDocumentRegistry _documentRegistry = ClinicalDocumentRegistry.Instance;
        private DiagnosisEntry? _addedDiagnosis;
        private Guid? _targetDocumentId;

        public AddDiagnosisCommand() {}

        public override string Description => "Adds a diagnosis to a clinical document";

        public override bool CanUndo => true;

        public override Permission? GetRequiredPermission()
            => Permission.CreateClinicalDocument;

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
                    IsPrimary = parameters.GetParameter<bool?>(Parameters.IsPrimary) ?? false,
                    Severity = parameters.GetParameter<EntrySeverity?>(Parameters.Severity) ?? EntrySeverity.Routine,
                    OnsetDate = parameters.GetParameter<DateTime?>(Parameters.OnsetDate)
                };

                // Add to document
                document.AddEntry(_addedDiagnosis);

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

        protected override object? CaptureStateForUndo(CommandParameters parameters, SessionContext? session)
        {
            return new UndoState
            {
                DocumentId = _targetDocumentId ?? Guid.Empty,
                DiagnosisId = _addedDiagnosis?.Id ?? Guid.Empty
            };
        }

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
            public Guid DocumentId { get; set; }
            public Guid DiagnosisId { get; set; }
        }
    }
}
