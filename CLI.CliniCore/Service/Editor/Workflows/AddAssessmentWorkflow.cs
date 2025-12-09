using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Clinical;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;

namespace CLI.CliniCore.Service.Editor.Workflows
{
    /// <summary>
    /// Workflow for creating assessment entries.
    /// Steps: Condition => Prognosis => Confidence => Severity => RequiresImmediateAction => Content
    /// </summary>
    public class AddAssessmentWorkflow : EntryWorkflowBase
    {
        private enum Step { Condition, Prognosis, Confidence, Severity, RequiresImmediateAction, Content }

        private Step CurrentStepEnum => (Step)_currentStep;

        public override string CommandKey => AddAssessmentCommand.Key;

        public override string CurrentPrompt => CurrentStepEnum switch
        {
            Step.Condition => "Condition: [S]table, [I]mproving, [U]nchanged, [W]orsening, [C]ritical: ",
            Step.Prognosis => "Prognosis: [P]oor, G[u]arded, [F]air, [G]ood, [E]xcellent: ",
            Step.Confidence => "Confidence: [L]ow, [M]oderate, [H]igh, [C]ertain [default: M]: ",
            Step.Severity => "Severity: [R]outine, [M]oderate, [U]rgent, Cri[T]ical, [E]mergency [default: R]: ",
            Step.RequiresImmediateAction => "Requires immediate action? [Y]es/[N]o [default: N]: ",
            Step.Content => "Enter clinical impression: ",
            _ => ""
        };

        public override string DefaultValue => CurrentStepEnum switch
        {
            Step.Confidence => "M",
            Step.Severity => "R",
            Step.RequiresImmediateAction => "N",
            _ => ""
        };

        public override void ProcessInput(string input)
        {
            switch (CurrentStepEnum)
            {
                case Step.Condition:
                    ProcessConditionSelection(input);
                    break;
                case Step.Prognosis:
                    ProcessPrognosisSelection(input);
                    break;
                case Step.Confidence:
                    ProcessConfidenceSelection(input);
                    break;
                case Step.Severity:
                    ProcessSeveritySelection(input);
                    break;
                case Step.RequiresImmediateAction:
                    ProcessRequiresImmediateAction(input);
                    break;
                case Step.Content:
                    ProcessContent(input);
                    break;
            }
        }

        private void ProcessConditionSelection(string input)
        {
            var selection = GetLastChar(input);
            PatientCondition? condition = selection switch
            {
                'S' => PatientCondition.Stable,
                'I' => PatientCondition.Improving,
                'U' => PatientCondition.Unchanged,
                'W' => PatientCondition.Worsening,
                'C' => PatientCondition.Critical,
                _ => null
            };

            if (condition == null)
            {
                CancelWithError("Invalid condition selection");
                return;
            }

            SetData("condition", condition.Value.ToString());
            NextStep();
        }

        private void ProcessPrognosisSelection(string input)
        {
            var selection = GetLastChar(input);
            Prognosis prognosis = selection switch
            {
                'P' => Prognosis.Poor,
                'U' => Prognosis.Guarded,
                'F' => Prognosis.Fair,
                'G' => Prognosis.Good,
                'E' => Prognosis.Excellent,
                _ => Prognosis.Good // Default
            };

            SetData("prognosis", prognosis.ToString());
            NextStep();
        }

        private void ProcessConfidenceSelection(string input)
        {
            var selection = GetLastChar(input);
            ConfidenceLevel confidence = selection switch
            {
                'L' => ConfidenceLevel.Low,
                'M' => ConfidenceLevel.Moderate,
                'H' => ConfidenceLevel.High,
                'C' => ConfidenceLevel.Certain,
                _ => ConfidenceLevel.Moderate // Default
            };

            SetData("confidence", confidence.ToString());
            NextStep();
        }

        private void ProcessSeveritySelection(string input)
        {
            var selection = GetLastChar(input);
            EntrySeverity severity = selection switch
            {
                'R' => EntrySeverity.Routine,
                'M' => EntrySeverity.Moderate,
                'U' => EntrySeverity.Urgent,
                'T' => EntrySeverity.Critical,
                'E' => EntrySeverity.Emergency,
                _ => EntrySeverity.Routine // Default
            };

            SetData("severity", severity.ToString());
            NextStep();
        }

        private void ProcessRequiresImmediateAction(string input)
        {
            var selection = GetLastChar(input);
            var requiresAction = selection == 'Y';
            SetData("requires_immediate_action", requiresAction.ToString().ToLower());
            NextStep();
        }

        private void ProcessContent(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                CancelWithError("Clinical impression cannot be empty");
                return;
            }

            SetData("content", input);
            Complete();
        }

        public override CommandParameters? BuildParameters(Guid documentId)
        {
            if (!IsComplete) return null;

            var parameters = new CommandParameters();
            parameters.SetParameter(AddAssessmentCommand.Parameters.DocumentId, documentId);
            parameters.SetParameter(AddAssessmentCommand.Parameters.ClinicalImpression, GetData("content"));

            if (HasData("condition") &&
                TryParseEnumByName<PatientCondition>(GetData("condition"), out var condition))
            {
                parameters.SetParameter(AddAssessmentCommand.Parameters.Condition, condition);
            }

            if (HasData("prognosis") &&
                TryParseEnumByName<Prognosis>(GetData("prognosis"), out var prognosis))
            {
                parameters.SetParameter(AddAssessmentCommand.Parameters.Prognosis, prognosis);
            }

            if (HasData("confidence") &&
                TryParseEnumByName<ConfidenceLevel>(GetData("confidence"), out var confidence))
            {
                parameters.SetParameter(AddAssessmentCommand.Parameters.Confidence, confidence);
            }

            if (HasData("severity") &&
                TryParseEnumByName<EntrySeverity>(GetData("severity"), out var severity))
            {
                parameters.SetParameter(AddAssessmentCommand.Parameters.Severity, severity);
            }

            if (HasData("requires_immediate_action") && bool.TryParse(GetData("requires_immediate_action"), out var requiresAction))
            {
                parameters.SetParameter(AddAssessmentCommand.Parameters.RequiresImmediateAction, requiresAction);
            }

            return parameters;
        }
    }
}
