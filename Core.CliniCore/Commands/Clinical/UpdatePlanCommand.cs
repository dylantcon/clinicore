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
    /// Command that updates an existing plan entry within a clinical document.
    /// </summary>
    public class UpdatePlanCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "updateplan";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="UpdatePlanCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the clinical document identifier that owns the plan.
            /// </summary>
            public const string DocumentId = "document_id";

            /// <summary>
            /// Parameter key for the plan entry identifier.
            /// </summary>
            public const string PlanId = "plan_id";

            /// <summary>
            /// Parameter key for the updated plan content.
            /// </summary>
            public const string Content = "content";

            /// <summary>
            /// Parameter key for the plan type.
            /// </summary>
            public const string Type = "type";

            /// <summary>
            /// Parameter key for the target completion date.
            /// </summary>
            public const string TargetDate = "target_date";

            /// <summary>
            /// Parameter key indicating whether the plan is completed.
            /// </summary>
            public const string IsCompleted = "is_completed";

            /// <summary>
            /// Parameter key for the plan priority.
            /// </summary>
            public const string Priority = "priority";

            /// <summary>
            /// Parameter key for identifiers of related diagnoses.
            /// </summary>
            public const string RelatedDiagnoses = "related_diagnoses";

            /// <summary>
            /// Parameter key for follow-up instructions.
            /// </summary>
            public const string FollowUpInstructions = "follow_up_instructions";

            /// <summary>
            /// Parameter key for a coding system identifier associated with the plan.
            /// </summary>
            public const string Code = "code";

            /// <summary>
            /// Parameter key for the severity associated with the plan.
            /// </summary>
            public const string Severity = "severity";
        }

        private readonly ClinicalDocumentService _documentRegistry;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatePlanCommand"/> class.
        /// </summary>
        /// <param name="clinicalDocService">The clinical document service used to access and update documents.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="clinicalDocService"/> is <c>null</c>.</exception>
        public UpdatePlanCommand(ClinicalDocumentService clinicalDocService)
        {
            _documentRegistry = clinicalDocService ?? throw new ArgumentNullException(nameof(clinicalDocService));
        }

        /// <inheritdoc />
        public override string Description => "Updates a plan entry within a clinical document";

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
            var missingParams = parameters.GetMissingRequired(Parameters.DocumentId, Parameters.PlanId);
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

            var planId = parameters.GetParameter<Guid?>(Parameters.PlanId);
            if (!planId.HasValue || planId.Value == Guid.Empty)
            {
                result.AddError("Invalid plan ID");
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

            // Check plan exists and is correct type
            var entry = document.Entries.FirstOrDefault(e => e.Id == planId.Value);
            if (entry == null)
            {
                result.AddError($"Plan with ID {planId.Value} not found in document");
                return result;
            }

            if (entry is not PlanEntry)
            {
                result.AddError($"Entry with ID {planId.Value} is not a plan entry");
                return result;
            }

            var planEntry = entry as PlanEntry;

            // Validate enums
            var typeStr = parameters.GetParameter<string>(Parameters.Type);
            if (!string.IsNullOrEmpty(typeStr) && !Enum.TryParse<PlanType>(typeStr, true, out _))
            {
                var validTypes = string.Join(", ", Enum.GetNames<PlanType>());
                result.AddError($"Invalid plan type. Valid values are: {validTypes}");
            }

            var priorityStr = parameters.GetParameter<string>(Parameters.Priority);
            if (!string.IsNullOrEmpty(priorityStr) && !Enum.TryParse<PlanPriority>(priorityStr, true, out _))
            {
                var validPriorities = string.Join(", ", Enum.GetNames<PlanPriority>());
                result.AddError($"Invalid plan priority. Valid values are: {validPriorities}");
            }

            var severityStr = parameters.GetParameter<string>(Parameters.Severity);
            if (!string.IsNullOrEmpty(severityStr) && !Enum.TryParse<EntrySeverity>(severityStr, true, out _))
            {
                var validSeverities = string.Join(", ", Enum.GetNames<EntrySeverity>());
                result.AddError($"Invalid severity. Valid values are: {validSeverities}");
            }

            // Validate target date
            var targetDate = parameters.GetParameter<DateTime?>(Parameters.TargetDate);
            if (targetDate.HasValue && planEntry != null && targetDate.Value < planEntry.CreatedAt)
            {
                result.AddError("Target date cannot be before plan creation date");
            }

            // Validate related diagnoses exist
            var relatedDiagnoses = parameters.GetParameter<List<Guid>>(Parameters.RelatedDiagnoses);
            if (relatedDiagnoses != null)
            {
                var diagnosisIds = document.GetDiagnoses().Select(d => d.Id).ToHashSet();
                var invalidIds = relatedDiagnoses.Where(id => !diagnosisIds.Contains(id)).ToList();
                if (invalidIds.Any())
                {
                    result.AddError($"Related diagnosis IDs not found in document: {string.Join(", ", invalidIds)}");
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
                var planId = parameters.GetRequiredParameter<Guid>(Parameters.PlanId);

                var document = _documentRegistry.GetDocumentById(documentId);
                if (document == null)
                {
                    return CommandResult.Fail("Clinical document not found");
                }

                var plan = document.Entries.FirstOrDefault(e => e.Id == planId) as PlanEntry;
                if (plan == null)
                {
                    return CommandResult.Fail("Plan entry not found in document");
                }

                var fieldsUpdated = new List<string>();

                // Update content (plan description)
                var content = parameters.GetParameter<string>(Parameters.Content);
                if (!string.IsNullOrEmpty(content) && content != plan.Content)
                {
                    plan.Update(content);
                    fieldsUpdated.Add("content");
                }

                // Update plan type
                var typeStr = parameters.GetParameter<string>(Parameters.Type);
                if (!string.IsNullOrEmpty(typeStr) && Enum.TryParse<PlanType>(typeStr, true, out var type))
                {
                    if (type != plan.Type)
                    {
                        plan.Type = type;
                        fieldsUpdated.Add("type");
                    }
                }

                // Update target date
                var targetDate = parameters.GetParameter<DateTime?>(Parameters.TargetDate);
                if (targetDate != plan.TargetDate)
                {
                    plan.TargetDate = targetDate;
                    fieldsUpdated.Add("target_date");
                }

                // Update completion status
                var isCompleted = parameters.GetParameter<bool?>(Parameters.IsCompleted);
                if (isCompleted.HasValue && isCompleted.Value != plan.IsCompleted)
                {
                    if (isCompleted.Value)
                    {
                        plan.MarkCompleted();
                        fieldsUpdated.Add("is_completed");
                        fieldsUpdated.Add("completed_date");
                    }
                    else
                    {
                        plan.IsCompleted = false;
                        plan.CompletedDate = null;
                        fieldsUpdated.Add("is_completed");
                    }
                }

                // Update priority
                var priorityStr = parameters.GetParameter<string>(Parameters.Priority);
                if (!string.IsNullOrEmpty(priorityStr) && Enum.TryParse<PlanPriority>(priorityStr, true, out var priority))
                {
                    if (priority != plan.Priority)
                    {
                        plan.Priority = priority;
                        fieldsUpdated.Add("priority");
                    }
                }

                // Update related diagnoses
                var relatedDiagnoses = parameters.GetParameter<List<Guid>>(Parameters.RelatedDiagnoses);
                if (relatedDiagnoses != null)
                {
                    plan.RelatedDiagnoses.Clear();
                    plan.RelatedDiagnoses.AddRange(relatedDiagnoses);
                    fieldsUpdated.Add("related_diagnoses");
                }

                // Update follow-up instructions
                var followUpInstructions = parameters.GetParameter<string>(Parameters.FollowUpInstructions);
                if (followUpInstructions != null && followUpInstructions != plan.FollowUpInstructions)
                {
                    plan.FollowUpInstructions = string.IsNullOrEmpty(followUpInstructions) ? null : followUpInstructions;
                    fieldsUpdated.Add("follow_up_instructions");
                }

                // Update code
                var code = parameters.GetParameter<string>(Parameters.Code);
                if (code != null && code != plan.Code)
                {
                    plan.Code = string.IsNullOrEmpty(code) ? null : code;
                    fieldsUpdated.Add("code");
                }

                // Update severity
                var severityStr = parameters.GetParameter<string>(Parameters.Severity);
                if (!string.IsNullOrEmpty(severityStr) && Enum.TryParse<EntrySeverity>(severityStr, true, out var severity))
                {
                    if (severity != plan.Severity)
                    {
                        plan.Severity = severity;
                        fieldsUpdated.Add("severity");
                    }
                }

                // Validate after updates
                var errors = plan.GetValidationErrors();
                if (errors.Any())
                {
                    return CommandResult.ValidationFailed(errors);
                }

                if (fieldsUpdated.Any())
                {
                    // Persist the changes to the repository
                    _documentRegistry.UpdateDocument(document);

                    return CommandResult.Ok(
                        $"Plan entry updated successfully. Fields changed: {string.Join(", ", fieldsUpdated)}",
                        new {
                            DocumentId = documentId,
                            PlanId = planId,
                            UpdatedFields = fieldsUpdated,
                            ModifiedAt = plan.ModifiedAt,
                            IsCompleted = plan.IsCompleted,
                            CompletedDate = plan.CompletedDate
                        });
                }
                else
                {
                    return CommandResult.Ok("No changes were made to the plan entry", plan);
                }
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to update plan: {ex.Message}", ex);
            }
        }
    }
}