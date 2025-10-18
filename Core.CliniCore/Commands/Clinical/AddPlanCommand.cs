// Core.CliniCore/Commands/Clinical/AddPlanCommand.cs
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.ClinicalDoc;

namespace Core.CliniCore.Commands.Clinical
{
    public class AddPlanCommand : AbstractCommand
    {
        public const string Key = "addplan";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string DocumentId = "document_id";
            public const string PlanDescription = "plan_description";
            public const string PlanType = "plan_type";
            public const string Priority = "priority";
            public const string TargetDate = "target_date";
            public const string FollowUpInstructions = "follow_up_instructions";
            public const string Severity = "severity";
            public const string RelatedDiagnoses = "related_diagnoses";
        }

        private readonly ClinicalDocumentRegistry _documentRegistry = ClinicalDocumentRegistry.Instance;
        private PlanEntry? _addedPlan;
        private Guid? _targetDocumentId;

        public AddPlanCommand() {}

        public override string Description => "Adds a treatment plan entry to a document";

        public override bool CanUndo => true;

        public override Permission? GetRequiredPermission()
            => Permission.CreateClinicalDocument;

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

                // Add to document
                document.AddEntry(_addedPlan);

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

        protected override object? CaptureStateForUndo(CommandParameters parameters, SessionContext? session)
        {
            return new UndoState
            {
                DocumentId = _targetDocumentId ?? Guid.Empty,
                PlanId = _addedPlan?.Id ?? Guid.Empty
            };
        }

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
            public Guid DocumentId { get; set; }
            public Guid PlanId { get; set; }
        }
    }
}
