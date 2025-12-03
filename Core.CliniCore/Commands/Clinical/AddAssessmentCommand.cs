// Core.CliniCore/Commands/Clinical/AddAssessmentCommand.cs
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.ClinicalDoc;
using Core.CliniCore.Service;

namespace Core.CliniCore.Commands.Clinical
{
    public class AddAssessmentCommand : AbstractCommand
    {
        public const string Key = "addassessment";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string DocumentId = "document_id";
            public const string ClinicalImpression = "clinical_impression";
            public const string Condition = "condition";
            public const string Prognosis = "prognosis";
            public const string RequiresImmediateAction = "requires_immediate_action";
            public const string Severity = "severity";
            public const string Confidence = "confidence";
            public const string DifferentialDiagnoses = "differential_diagnoses";
            public const string RiskFactors = "risk_factors";
        }

        private readonly ClinicalDocumentService _documentRegistry;
        private AssessmentEntry? _addedAssessment;
        private Guid? _targetDocumentId;

        public AddAssessmentCommand(ClinicalDocumentService clinicalDocService)
        {
            _documentRegistry = clinicalDocService ?? throw new ArgumentNullException(nameof(clinicalDocService));
        }

        public override string Description => "Adds a clinical assessment to a document";

        public override bool CanUndo => true;

        public override Permission? GetRequiredPermission()
            => Permission.CreateClinicalDocument;

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Check required parameters
            var missingParams = parameters.GetMissingRequired(
                Parameters.DocumentId, Parameters.ClinicalImpression);

            if (missingParams.Any())
            {
                foreach (var error in missingParams)
                    result.AddError(error);
                return result;
            }

            // Validate document exists
            var documentId = parameters.GetParameter<Guid?>(Parameters.DocumentId);
            if (!documentId.HasValue || !_documentRegistry.DocumentExists(documentId.Value))
            {
                result.AddError($"Clinical document {documentId} not found");
            }
            else
            {
                var document = _documentRegistry.GetDocumentById(documentId.Value);
                if (document != null && document.IsCompleted)
                {
                    result.AddError("Cannot modify completed clinical document");
                }
            }

            // Validate clinical impression
            var impression = parameters.GetParameter<string>(Parameters.ClinicalImpression);
            if (string.IsNullOrWhiteSpace(impression))
            {
                result.AddError("Clinical impression cannot be empty");
            }

            // Validate condition and prognosis consistency
            var condition = parameters.GetParameter<PatientCondition?>(Parameters.Condition);
            var prognosis = parameters.GetParameter<Prognosis?>(Parameters.Prognosis);

            if (condition == PatientCondition.Critical && prognosis == Prognosis.Excellent)
            {
                result.AddWarning("Critical condition with excellent prognosis seems inconsistent");
            }

            // Validate immediate action flag with severity
            var immediateAction = parameters.GetParameter<bool?>(Parameters.RequiresImmediateAction) ?? false;
            var severity = parameters.GetParameter<EntrySeverity?>(Parameters.Severity) ?? EntrySeverity.Routine;

            if (immediateAction && severity < EntrySeverity.Urgent)
            {
                result.AddWarning("Immediate action usually requires Urgent or higher severity");
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var documentId = parameters.GetRequiredParameter<Guid>(Parameters.DocumentId);
                var clinicalImpression = parameters.GetRequiredParameter<string>(Parameters.ClinicalImpression);
                var physicianId = session?.UserId ?? Guid.Empty;

                _targetDocumentId = documentId;
                var document = _documentRegistry.GetDocumentById(documentId);
                if (document == null)
                {
                    return CommandResult.Fail($"Clinical document {documentId} not found");
                }

                // Create the assessment
                _addedAssessment = new AssessmentEntry(physicianId, clinicalImpression)
                {
                    Condition = parameters.GetParameter<PatientCondition?>(Parameters.Condition)
                        ?? PatientCondition.Stable,
                    Prognosis = parameters.GetParameter<Prognosis?>(Parameters.Prognosis)
                        ?? Prognosis.Good,
                    RequiresImmediateAction = parameters.GetParameter<bool?>(Parameters.RequiresImmediateAction)
                        ?? false,
                    Confidence = parameters.GetParameter<ConfidenceLevel?>(Parameters.Confidence)
                        ?? ConfidenceLevel.Moderate,
                    Severity = parameters.GetParameter<EntrySeverity?>(Parameters.Severity)
                        ?? EntrySeverity.Routine
                };

                // Add differential diagnoses if provided
                var differentials = parameters.GetParameter<List<string>>(Parameters.DifferentialDiagnoses);
                if (differentials != null)
                {
                    foreach (var diff in differentials.Where(d => !string.IsNullOrWhiteSpace(d)))
                    {
                        _addedAssessment.DifferentialDiagnoses.Add(diff);
                    }
                }

                // Add risk factors if provided
                var riskFactors = parameters.GetParameter<List<string>>(Parameters.RiskFactors);
                if (riskFactors != null)
                {
                    foreach (var risk in riskFactors.Where(r => !string.IsNullOrWhiteSpace(r)))
                    {
                        _addedAssessment.RiskFactors.Add(risk);
                    }
                }

                // Add to document
                document.AddEntry(_addedAssessment);

                // Build confirmation message
                var immediateStr = _addedAssessment.RequiresImmediateAction
                    ? " [IMMEDIATE ACTION REQUIRED]"
                    : "";
                var differentialStr = _addedAssessment.DifferentialDiagnoses.Any()
                    ? $"\n  Differential: {string.Join(", ", _addedAssessment.DifferentialDiagnoses)}"
                    : "";
                var riskStr = _addedAssessment.RiskFactors.Any()
                    ? $"\n  Risk Factors: {string.Join(", ", _addedAssessment.RiskFactors)}"
                    : "";

                return CommandResult.Ok(
                    $"Assessment added successfully:{immediateStr}\n" +
                    $"  Clinical Impression: {clinicalImpression}\n" +
                    $"  Condition: {_addedAssessment.Condition}\n" +
                    $"  Prognosis: {_addedAssessment.Prognosis}\n" +
                    $"  Confidence: {_addedAssessment.Confidence}{differentialStr}{riskStr}\n" +
                    $"  Assessment ID: {_addedAssessment.Id}",
                    _addedAssessment);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to add assessment: {ex.Message}", ex);
            }
        }

        protected override object? CaptureStateForUndo(CommandParameters parameters, SessionContext? session)
        {
            return new UndoState
            {
                DocumentId = _targetDocumentId ?? Guid.Empty,
                AssessmentId = _addedAssessment?.Id ?? Guid.Empty
            };
        }

        protected override CommandResult UndoCore(object previousState, SessionContext? session)
        {
            if (previousState is UndoState state)
            {
                var document = _documentRegistry.GetDocumentById(state.DocumentId);
                if (document != null)
                {
                    var assessment = document.GetAssessments()
                        .FirstOrDefault(a => a.Id == state.AssessmentId);

                    if (assessment != null)
                    {
                        assessment.IsActive = false;
                        return CommandResult.Ok($"Assessment {state.AssessmentId} has been marked inactive");
                    }
                }
            }
            return CommandResult.Fail("Unable to undo assessment addition");
        }

        private class UndoState
        {
            public Guid DocumentId { get; set; }
            public Guid AssessmentId { get; set; }
        }
    }
}