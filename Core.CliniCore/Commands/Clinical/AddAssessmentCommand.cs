// Core.CliniCore/Commands/Clinical/AddAssessmentCommand.cs
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Service;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.ClinicalDocumentation.ClinicalEntries;

namespace Core.CliniCore.Commands.Clinical
{
    /// <summary>
    /// Command that adds a new clinical assessment entry to an existing clinical document.
    /// </summary>
    public class AddAssessmentCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "addassessment";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="AddAssessmentCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the target clinical document identifier.
            /// </summary>
            public const string DocumentId = "document_id";

            /// <summary>
            /// Parameter key for the clinical impression text.
            /// </summary>
            public const string ClinicalImpression = "clinical_impression";

            /// <summary>
            /// Parameter key for the patient's overall condition assessment.
            /// </summary>
            public const string Condition = "condition";

            /// <summary>
            /// Parameter key for the prognosis assessment.
            /// </summary>
            public const string Prognosis = "prognosis";

            /// <summary>
            /// Parameter key indicating whether the assessment requires immediate action.
            /// </summary>
            public const string RequiresImmediateAction = "requires_immediate_action";

            /// <summary>
            /// Parameter key for the severity of the clinical situation.
            /// </summary>
            public const string Severity = "severity";

            /// <summary>
            /// Parameter key for the confidence level in the assessment.
            /// </summary>
            public const string Confidence = "confidence";

            /// <summary>
            /// Parameter key for a collection of differential diagnoses.
            /// </summary>
            public const string DifferentialDiagnoses = "differential_diagnoses";

            /// <summary>
            /// Parameter key for a collection of identified risk factors.
            /// </summary>
            public const string RiskFactors = "risk_factors";
        }

        private readonly ClinicalDocumentService _documentRegistry;
        private AssessmentEntry? _addedAssessment;
        private Guid? _targetDocumentId;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddAssessmentCommand"/> class.
        /// </summary>
        /// <param name="clinicalDocService">The clinical document service used to access and modify documents.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="clinicalDocService"/> is <c>null</c>.</exception>
        public AddAssessmentCommand(ClinicalDocumentService clinicalDocService)
        {
            _documentRegistry = clinicalDocService ?? throw new ArgumentNullException(nameof(clinicalDocService));
        }

        /// <inheritdoc />
        public override string Description => "Adds a clinical assessment to a document";

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

        /// <inheritdoc />
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

                // Persist the assessment via the service's entry-level method
                _documentRegistry.AddAssessment(documentId, _addedAssessment);

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

        /// <inheritdoc />
        protected override object? CaptureStateForUndo(CommandParameters parameters, SessionContext? session)
        {
            return new UndoState
            {
                DocumentId = _targetDocumentId ?? Guid.Empty,
                AssessmentId = _addedAssessment?.Id ?? Guid.Empty
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
            /// <summary>
            /// Gets or sets the identifier of the clinical document that owns the assessment.
            /// </summary>
            public Guid DocumentId { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the assessment entry that was added.
            /// </summary>
            public Guid AssessmentId { get; set; }
        }
    }
}