// Core.CliniCore/Commands/Clinical/AddPlanCommand.cs
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Service;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.ClinicalDocumentation.ClinicalEntries;

namespace Core.CliniCore.Commands.Clinical
{
    /// <summary>
    /// Command that adds a treatment or care plan entry to an existing clinical document.
    /// </summary>
    public class AddPlanCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "addplan";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="AddPlanCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the target clinical document identifier.
            /// </summary>
            public const string DocumentId = "document_id";

            /// <summary>
            /// Parameter key for the plan description.
            /// </summary>
            public const string PlanDescription = "plan_description";

            /// <summary>
            /// Parameter key for the plan type.
            /// </summary>
            public const string PlanType = "plan_type";

            /// <summary>
            /// Parameter key for the plan priority.
            /// </summary>
            public const string Priority = "priority";

            /// <summary>
            /// Parameter key for the target date by which the plan should be completed.
            /// </summary>
            public const string TargetDate = "target_date";

            /// <summary>
            /// Parameter key for follow-up instructions.
            /// </summary>
            public const string FollowUpInstructions = "follow_up_instructions";

            /// <summary>
            /// Parameter key for the severity associated with the plan.
            /// </summary>
            public const string Severity = "severity";

            /// <summary>
            /// Parameter key for identifiers of related diagnoses.
            /// </summary>
            public const string RelatedDiagnoses = "related_diagnoses";
        }

        private readonly ClinicalDocumentService _documentRegistry;
        private PlanEntry? _addedPlan;
        private Guid? _targetDocumentId;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddPlanCommand"/> class.
        /// </summary>
        /// <param name="clinicalDocService">The clinical document service used to access and modify documents.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="clinicalDocService"/> is <c>null</c>.</exception>
        public AddPlanCommand(ClinicalDocumentService clinicalDocService)
        {
            _documentRegistry = clinicalDocService ?? throw new ArgumentNullException(nameof(clinicalDocService));
        }

        /// <inheritdoc />
        public override string Description => "Adds a treatment plan entry to a document";

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
                Parameters.DocumentId, Parameters.PlanDescription);

            if (missingParams.Any())
            {
                foreach (var error in missingParams)
                    result.AddError(error);
                return result;
            }

            // Validate document exists - improved null handling
            var documentId = parameters.GetParameter<Guid?>(Parameters.DocumentId);

            // Early return if null or empty
            if (!documentId.HasValue || documentId.Value == Guid.Empty)
            {
                result.AddError("Invalid document ID");
                return result;
            }

            // Now we know documentId has a value, extract it
            var docId = documentId.Value;

            if (!_documentRegistry.DocumentExists(docId))
            {
                result.AddError($"Clinical document {docId} not found");
                return result;
            }

            // Now we can safely get the document
            var document = _documentRegistry.GetDocumentById(docId);

            if (document != null && document.IsCompleted)
            {
                result.AddError("Cannot modify completed clinical document");
                return result; // Early return on error
            }

            // For certain plan types, ensure diagnosis exists
            var planType = parameters.GetParameter<PlanType?>(Parameters.PlanType) ?? PlanType.Treatment;
            if (planType == PlanType.Treatment || planType == PlanType.Procedure)
            {
                if (document != null && !document.GetDiagnoses().Any())
                {
                    result.AddWarning("Treatment/Procedure plans typically require a diagnosis");
                }
            }

            // Validate plan description
            var description = parameters.GetParameter<string>(Parameters.PlanDescription);
            if (string.IsNullOrWhiteSpace(description))
            {
                result.AddError("Plan description cannot be empty");
            }

            // Validate target date if provided
            var targetDate = parameters.GetParameter<DateTime?>(Parameters.TargetDate);
            if (targetDate.HasValue && targetDate.Value < DateTime.Now)
            {
                result.AddError("Target date cannot be in the past");
            }

            // Validate related diagnosis IDs if provided
            var relatedDiagnoses = parameters.GetParameter<List<Guid>>(Parameters.RelatedDiagnoses);
            if (document != null && relatedDiagnoses != null && relatedDiagnoses.Any())
            {
                var validDiagnoses = document.GetDiagnoses().Select(d => d.Id).ToHashSet();

                foreach (var diagId in relatedDiagnoses)
                {
                    if (!validDiagnoses.Contains(diagId))
                    {
                        result.AddWarning($"Diagnosis {diagId} not found in document");
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
                var planDescription = parameters.GetRequiredParameter<string>(Parameters.PlanDescription);
                var physicianId = session?.UserId ?? Guid.Empty;

                _targetDocumentId = documentId;
                var document = _documentRegistry.GetDocumentById(documentId);
                if (document == null)
                {
                    return CommandResult.Fail($"Clinical document {documentId} not found");
                }

                // Create the plan
                _addedPlan = new PlanEntry(physicianId, planDescription)
                {
                    Type = parameters.GetParameter<PlanType?>(Parameters.PlanType) ?? PlanType.Treatment,
                    Priority = parameters.GetParameter<PlanPriority?>(Parameters.Priority) ?? PlanPriority.Routine,
                    TargetDate = parameters.GetParameter<DateTime?>(Parameters.TargetDate),
                    FollowUpInstructions = parameters.GetParameter<string>(Parameters.FollowUpInstructions),
                    Severity = parameters.GetParameter<EntrySeverity?>(Parameters.Severity) ?? EntrySeverity.Routine
                };

                // Link to diagnoses if provided
                var relatedDiagnoses = parameters.GetParameter<List<Guid>>(Parameters.RelatedDiagnoses);
                if (relatedDiagnoses != null)
                {
                    var validDiagnoses = document.GetDiagnoses().Select(d => d.Id).ToHashSet();
                    foreach (var diagId in relatedDiagnoses.Where(id => validDiagnoses.Contains(id)))
                    {
                        _addedPlan.RelatedDiagnoses.Add(diagId);
                    }
                }

                // Persist the plan via the service's entry-level method
                _documentRegistry.AddPlan(documentId, _addedPlan);

                // Build confirmation message
                var priorityStr = _addedPlan.Priority != PlanPriority.Routine
                    ? $" [{_addedPlan.Priority} Priority]"
                    : "";
                var targetStr = _addedPlan.TargetDate.HasValue
                    ? $"\n  Target Date: {_addedPlan.TargetDate:yyyy-MM-dd}"
                    : "";
                var followUpStr = !string.IsNullOrWhiteSpace(_addedPlan.FollowUpInstructions)
                    ? $"\n  Follow-up: {_addedPlan.FollowUpInstructions}"
                    : "";
                var diagnosisStr = _addedPlan.RelatedDiagnoses.Any()
                    ? $"\n  Related to {_addedPlan.RelatedDiagnoses.Count} diagnosis(es)"
                    : "";

                return CommandResult.Ok(
                    $"Plan added successfully:{priorityStr}\n" +
                    $"  Type: {_addedPlan.Type}\n" +
                    $"  Description: {planDescription}{targetStr}{followUpStr}{diagnosisStr}\n" +
                    $"  Plan ID: {_addedPlan.Id}",
                    _addedPlan);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to add plan: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        protected override object? CaptureStateForUndo(CommandParameters parameters, SessionContext? session)
        {
            return new UndoState
            {
                DocumentId = _targetDocumentId ?? Guid.Empty,
                PlanId = _addedPlan?.Id ?? Guid.Empty
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
                    var plan = document.GetPlans().FirstOrDefault(p => p.Id == state.PlanId);

                    if (plan != null)
                    {
                        plan.IsActive = false;
                        return CommandResult.Ok($"Plan {state.PlanId} has been marked inactive");
                    }
                }
            }
            return CommandResult.Fail("Unable to undo plan addition");
        }

        private class UndoState
        {
            /// <summary>
            /// Gets or sets the identifier of the clinical document that owns the plan.
            /// </summary>
            public Guid DocumentId { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the plan entry that was added.
            /// </summary>
            public Guid PlanId { get; set; }
        }
    }
}
