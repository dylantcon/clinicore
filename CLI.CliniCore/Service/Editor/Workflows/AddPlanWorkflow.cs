using System;
using System.Collections.Generic;
using System.Linq;
using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Clinical;
using Core.CliniCore.Domain.ClinicalDocumentation.ClinicalEntries;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;

namespace CLI.CliniCore.Service.Editor.Workflows
{
    /// <summary>
    /// Workflow for creating plan entries.
    /// Steps: Type => Priority => Severity => TargetDate => FollowUpInstructions => RelatedDiagnoses => Content
    /// </summary>
    public class AddPlanWorkflow : EntryWorkflowBase
    {
        private enum Step { Type, Priority, Severity, TargetDate, FollowUpInstructions, RelatedDiagnoses, Content }

        private readonly List<DiagnosisEntry> _availableDiagnoses;

        private Step CurrentStepEnum => (Step)_currentStep;

        public override string CommandKey => AddPlanCommand.Key;

        /// <summary>
        /// Creates a new plan workflow.
        /// </summary>
        /// <param name="availableDiagnoses">List of diagnoses available to link to the plan</param>
        public AddPlanWorkflow(List<DiagnosisEntry>? availableDiagnoses = null)
        {
            _availableDiagnoses = availableDiagnoses ?? new List<DiagnosisEntry>();
        }

        public override string CurrentPrompt => CurrentStepEnum switch
        {
            Step.Type => "[T]reat, [D]iagnostic, [R]eferral, [F]ollowUp, [E]duc, [P]roc, [M]onitor, Pre[V]ent: ",
            Step.Priority => "Priority: [R]outine, [H]igh, [U]rgent, [E]mergency [default: R]: ",
            Step.Severity => "Severity: [R]outine, [M]oderate, [U]rgent, Cri[T]ical, [E]mergency [default: R]: ",
            Step.TargetDate => "Target date (YYYY-MM-DD) [optional, press Enter to skip]: ",
            Step.FollowUpInstructions => "Follow-up instructions [optional]: ",
            Step.RelatedDiagnoses => BuildRelatedDiagnosesPrompt(),
            Step.Content => "Enter plan description: ",
            _ => ""
        };

        public override string DefaultValue => CurrentStepEnum switch
        {
            Step.Priority => "R",
            Step.Severity => "R",
            _ => ""
        };

        private string BuildRelatedDiagnosesPrompt()
        {
            if (_availableDiagnoses.Count == 0)
            {
                return "No diagnoses available. Press Enter to continue: ";
            }

            var prompt = "Link to diagnoses (comma-separated, e.g., 1,2): ";
            for (int i = 0; i < Math.Min(_availableDiagnoses.Count, 9); i++)
            {
                var content = _availableDiagnoses[i].Content;
                var preview = content.Length > 15 ? content.Substring(0, 15) + "..." : content;
                prompt += $"\n  [{i + 1}] {preview}";
            }
            prompt += "\n[optional]: ";
            return prompt;
        }

        public override void ProcessInput(string input)
        {
            switch (CurrentStepEnum)
            {
                case Step.Type:
                    ProcessTypeSelection(input);
                    break;
                case Step.Priority:
                    ProcessPrioritySelection(input);
                    break;
                case Step.Severity:
                    ProcessSeveritySelection(input);
                    break;
                case Step.TargetDate:
                    ProcessTargetDate(input);
                    break;
                case Step.FollowUpInstructions:
                    ProcessFollowUpInstructions(input);
                    break;
                case Step.RelatedDiagnoses:
                    ProcessRelatedDiagnoses(input);
                    break;
                case Step.Content:
                    ProcessContent(input);
                    break;
            }
        }

        private void ProcessTypeSelection(string input)
        {
            var selection = GetLastChar(input);
            PlanType? planType = selection switch
            {
                'T' => PlanType.Treatment,
                'D' => PlanType.Diagnostic,
                'R' => PlanType.Referral,
                'F' => PlanType.FollowUp,
                'E' => PlanType.PatientEducation,
                'P' => PlanType.Procedure,
                'M' => PlanType.Monitoring,
                'V' => PlanType.Prevention,
                _ => null
            };

            if (planType == null)
            {
                CancelWithError("Invalid plan type selection");
                return;
            }

            SetData("plan_type", planType.Value.ToString());
            NextStep();
        }

        private void ProcessPrioritySelection(string input)
        {
            var selection = GetLastChar(input);
            PlanPriority priority = selection switch
            {
                'R' => PlanPriority.Routine,
                'H' => PlanPriority.High,
                'U' => PlanPriority.Urgent,
                'E' => PlanPriority.Emergency,
                _ => PlanPriority.Routine // Default
            };

            SetData("plan_priority", priority.ToString());
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

        private void ProcessTargetDate(string input)
        {
            if (!string.IsNullOrWhiteSpace(input) && DateTime.TryParse(input, out var targetDate))
            {
                SetData("target_date", targetDate.ToString("O")); // ISO 8601 format
            }
            NextStep();
        }

        private void ProcessFollowUpInstructions(string input)
        {
            if (!string.IsNullOrWhiteSpace(input))
            {
                SetData("follow_up_instructions", input.Trim());
            }
            NextStep();
        }

        private void ProcessRelatedDiagnoses(string input)
        {
            if (!string.IsNullOrWhiteSpace(input) && _availableDiagnoses.Count > 0)
            {
                var selectedIds = new List<Guid>();
                foreach (var part in input.Split(','))
                {
                    if (int.TryParse(part.Trim(), out int idx) && idx >= 1 && idx <= _availableDiagnoses.Count)
                    {
                        selectedIds.Add(_availableDiagnoses[idx - 1].Id);
                    }
                }
                if (selectedIds.Any())
                {
                    SetData("related_diagnoses", string.Join(",", selectedIds));
                }
            }
            NextStep();
        }

        private void ProcessContent(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                CancelWithError("Plan description cannot be empty");
                return;
            }

            SetData("content", input);
            Complete();
        }

        public override CommandParameters? BuildParameters(Guid documentId)
        {
            if (!IsComplete) return null;

            var parameters = new CommandParameters();
            parameters.SetParameter(AddPlanCommand.Parameters.DocumentId, documentId);
            parameters.SetParameter(AddPlanCommand.Parameters.PlanDescription, GetData("content"));

            if (HasData("plan_type") &&
                TryParseEnumByName<PlanType>(GetData("plan_type"), out var planType))
            {
                parameters.SetParameter(AddPlanCommand.Parameters.PlanType, planType);
            }

            if (HasData("plan_priority") &&
                TryParseEnumByName<PlanPriority>(GetData("plan_priority"), out var priority))
            {
                parameters.SetParameter(AddPlanCommand.Parameters.Priority, priority);
            }

            if (HasData("severity") &&
                TryParseEnumByName<EntrySeverity>(GetData("severity"), out var severity))
            {
                parameters.SetParameter(AddPlanCommand.Parameters.Severity, severity);
            }

            if (HasData("target_date") && DateTime.TryParse(GetData("target_date"), out var targetDate))
            {
                parameters.SetParameter(AddPlanCommand.Parameters.TargetDate, targetDate);
            }

            if (HasData("follow_up_instructions"))
            {
                parameters.SetParameter(AddPlanCommand.Parameters.FollowUpInstructions, GetData("follow_up_instructions"));
            }

            if (HasData("related_diagnoses"))
            {
                var diagnosisIds = GetData("related_diagnoses")
                    .Split(',')
                    .Select(s => Guid.Parse(s))
                    .ToList();
                parameters.SetParameter(AddPlanCommand.Parameters.RelatedDiagnoses, diagnosisIds);
            }

            return parameters;
        }
    }
}
