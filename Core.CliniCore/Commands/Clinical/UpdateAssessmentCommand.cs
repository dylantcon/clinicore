using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.ClinicalDoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Commands.Clinical
{
    public class UpdateAssessmentCommand : AbstractCommand
    {
        public const string Key = "updateassessment";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string DocumentId = "document_id";
            public const string AssessmentId = "assessment_id";
            public const string ClinicalImpression = "clinical_impression";
            public const string Condition = "condition";
            public const string Prognosis = "prognosis";
            public const string DifferentialDiagnoses = "differential_diagnoses";
            public const string RiskFactors = "risk_factors";
            public const string RequiresImmediateAction = "requires_immediate_action";
            public const string Confidence = "confidence";
            public const string Code = "code";
            public const string Severity = "severity";
        }

        private readonly ClinicalDocumentRegistry _documentRegistry = ClinicalDocumentRegistry.Instance;

        public UpdateAssessmentCommand()
        {
        }

        public override string Description => "Updates an assessment entry within a clinical document";

        public override bool CanUndo => false;

        public override Permission? GetRequiredPermission()
            => Permission.UpdateClinicalDocument;

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Check required parameters
            var missingParams = parameters.GetMissingRequired(Parameters.DocumentId, Parameters.AssessmentId);
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

            var assessmentId = parameters.GetParameter<Guid?>(Parameters.AssessmentId);
            if (!assessmentId.HasValue || assessmentId.Value == Guid.Empty)
            {
                result.AddError("Invalid assessment ID");
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

            // Check assessment exists and is correct type
            var entry = document.Entries.FirstOrDefault(e => e.Id == assessmentId.Value);
            if (entry == null)
            {
                result.AddError($"Assessment with ID {assessmentId.Value} not found in document");
                return result;
            }

            if (entry is not AssessmentEntry)
            {
                result.AddError($"Entry with ID {assessmentId.Value} is not an assessment entry");
                return result;
            }

            // Validate enums
            var conditionStr = parameters.GetParameter<string>(Parameters.Condition);
            if (!string.IsNullOrEmpty(conditionStr) && !Enum.TryParse<PatientCondition>(conditionStr, true, out _))
            {
                var validConditions = string.Join(", ", Enum.GetNames<PatientCondition>());
                result.AddError($"Invalid patient condition. Valid values are: {validConditions}");
            }

            var prognosisStr = parameters.GetParameter<string>(Parameters.Prognosis);
            if (!string.IsNullOrEmpty(prognosisStr) && !Enum.TryParse<Prognosis>(prognosisStr, true, out _))
            {
                var validPrognoses = string.Join(", ", Enum.GetNames<Prognosis>());
                result.AddError($"Invalid prognosis. Valid values are: {validPrognoses}");
            }

            var confidenceStr = parameters.GetParameter<string>(Parameters.Confidence);
            if (!string.IsNullOrEmpty(confidenceStr) && !Enum.TryParse<ConfidenceLevel>(confidenceStr, true, out _))
            {
                var validConfidences = string.Join(", ", Enum.GetNames<ConfidenceLevel>());
                result.AddError($"Invalid confidence level. Valid values are: {validConfidences}");
            }

            var severityStr = parameters.GetParameter<string>(Parameters.Severity);
            if (!string.IsNullOrEmpty(severityStr) && !Enum.TryParse<EntrySeverity>(severityStr, true, out _))
            {
                var validSeverities = string.Join(", ", Enum.GetNames<EntrySeverity>());
                result.AddError($"Invalid severity. Valid values are: {validSeverities}");
            }

            // Business rule validation
            var requiresImmediate = parameters.GetParameter<bool?>(Parameters.RequiresImmediateAction);
            if (requiresImmediate == true)
            {
                var assessment = entry as AssessmentEntry;
                if (severityStr != null && Enum.TryParse<EntrySeverity>(severityStr, true, out var severity))
                {
                    if (severity < EntrySeverity.Urgent)
                    {
                        result.AddWarning("Assessments requiring immediate action should have Urgent or higher severity");
                    }
                }
                else if (assessment != null && assessment.Severity < EntrySeverity.Urgent)
                {
                    result.AddWarning("Consider updating severity to Urgent or higher for immediate action items");
                }
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var documentId = parameters.GetRequiredParameter<Guid>(Parameters.DocumentId);
                var assessmentId = parameters.GetRequiredParameter<Guid>(Parameters.AssessmentId);

                var document = _documentRegistry.GetDocumentById(documentId);
                if (document == null)
                {
                    return CommandResult.Fail("Clinical document not found");
                }

                var assessment = document.Entries.FirstOrDefault(e => e.Id == assessmentId) as AssessmentEntry;
                if (assessment == null)
                {
                    return CommandResult.Fail("Assessment entry not found in document");
                }

                var fieldsUpdated = new List<string>();

                // Update clinical impression
                var clinicalImpression = parameters.GetParameter<string>(Parameters.ClinicalImpression);
                if (!string.IsNullOrEmpty(clinicalImpression) && clinicalImpression != assessment.ClinicalImpression)
                {
                    assessment.ClinicalImpression = clinicalImpression;
                    fieldsUpdated.Add("clinical_impression");
                }

                // Update patient condition
                var conditionStr = parameters.GetParameter<string>(Parameters.Condition);
                if (!string.IsNullOrEmpty(conditionStr) && Enum.TryParse<PatientCondition>(conditionStr, true, out var condition))
                {
                    if (condition != assessment.Condition)
                    {
                        assessment.Condition = condition;
                        fieldsUpdated.Add("condition");
                    }
                }

                // Update prognosis
                var prognosisStr = parameters.GetParameter<string>(Parameters.Prognosis);
                if (!string.IsNullOrEmpty(prognosisStr) && Enum.TryParse<Prognosis>(prognosisStr, true, out var prognosis))
                {
                    if (prognosis != assessment.Prognosis)
                    {
                        assessment.Prognosis = prognosis;
                        fieldsUpdated.Add("prognosis");
                    }
                }

                // Update differential diagnoses
                var differentialDiagnoses = parameters.GetParameter<List<string>>(Parameters.DifferentialDiagnoses);
                if (differentialDiagnoses != null)
                {
                    assessment.DifferentialDiagnoses.Clear();
                    assessment.DifferentialDiagnoses.AddRange(differentialDiagnoses);
                    fieldsUpdated.Add("differential_diagnoses");
                }

                // Update risk factors
                var riskFactors = parameters.GetParameter<List<string>>(Parameters.RiskFactors);
                if (riskFactors != null)
                {
                    assessment.RiskFactors.Clear();
                    assessment.RiskFactors.AddRange(riskFactors);
                    fieldsUpdated.Add("risk_factors");
                }

                // Update immediate action flag
                var requiresImmediateAction = parameters.GetParameter<bool?>(Parameters.RequiresImmediateAction);
                if (requiresImmediateAction.HasValue && requiresImmediateAction.Value != assessment.RequiresImmediateAction)
                {
                    assessment.RequiresImmediateAction = requiresImmediateAction.Value;
                    fieldsUpdated.Add("requires_immediate_action");
                }

                // Update confidence level
                var confidenceStr = parameters.GetParameter<string>(Parameters.Confidence);
                if (!string.IsNullOrEmpty(confidenceStr) && Enum.TryParse<ConfidenceLevel>(confidenceStr, true, out var confidence))
                {
                    if (confidence != assessment.Confidence)
                    {
                        assessment.Confidence = confidence;
                        fieldsUpdated.Add("confidence");
                    }
                }

                // Update code
                var code = parameters.GetParameter<string>(Parameters.Code);
                if (code != null && code != assessment.Code)
                {
                    assessment.Code = string.IsNullOrEmpty(code) ? null : code;
                    fieldsUpdated.Add("code");
                }

                // Update severity
                var severityStr = parameters.GetParameter<string>(Parameters.Severity);
                if (!string.IsNullOrEmpty(severityStr) && Enum.TryParse<EntrySeverity>(severityStr, true, out var severity))
                {
                    if (severity != assessment.Severity)
                    {
                        assessment.Severity = severity;
                        fieldsUpdated.Add("severity");
                    }
                }

                // Validate after updates
                var errors = assessment.GetValidationErrors();
                if (errors.Any())
                {
                    return CommandResult.ValidationFailed(errors);
                }

                if (fieldsUpdated.Any())
                {
                    // ModifiedAt is automatically updated by the Update() method called above

                    return CommandResult.Ok(
                        $"Assessment entry updated successfully. Fields changed: {string.Join(", ", fieldsUpdated)}",
                        new {
                            DocumentId = documentId,
                            AssessmentId = assessmentId,
                            UpdatedFields = fieldsUpdated,
                            ModifiedAt = assessment.ModifiedAt,
                            RequiresImmediateAction = assessment.RequiresImmediateAction
                        });
                }
                else
                {
                    return CommandResult.Ok("No changes were made to the assessment entry", assessment);
                }
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to update assessment: {ex.Message}", ex);
            }
        }
    }
}