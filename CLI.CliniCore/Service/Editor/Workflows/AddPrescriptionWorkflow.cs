using System;
using System.Collections.Generic;
using System.Linq;
using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Clinical;
using Core.CliniCore.Domain.ClinicalDocumentation.ClinicalEntries;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.Extensions;

namespace CLI.CliniCore.Service.Editor.Workflows
{
    /// <summary>
    /// Workflow for creating prescription entries.
    /// Steps: DiagnosisSelection => MedicationName => Dosage => Frequency => Route => Duration => Refills => GenericAllowed => DEASchedule => Instructions
    /// </summary>
    public class AddPrescriptionWorkflow : EntryWorkflowBase
    {
        private enum Step
        {
            DiagnosisSelection,
            MedicationName,
            Dosage,
            Frequency,
            Route,
            Duration,
            Refills,
            GenericAllowed,
            DEASchedule,
            Instructions
        }

        private readonly List<DiagnosisEntry> _availableDiagnoses;

        private Step CurrentStepEnum => (Step)_currentStep;

        public override string CommandKey => AddPrescriptionCommand.Key;

        /// <summary>
        /// Creates a new prescription workflow.
        /// </summary>
        /// <param name="availableDiagnoses">List of diagnoses to link the prescription to</param>
        public AddPrescriptionWorkflow(List<DiagnosisEntry> availableDiagnoses)
        {
            _availableDiagnoses = availableDiagnoses ?? new List<DiagnosisEntry>();

            if (_availableDiagnoses.Count == 0)
            {
                CancelWithError("No diagnoses found. Create a diagnosis entry first before adding prescriptions.");
            }
        }

        public override string CurrentPrompt => CurrentStepEnum switch
        {
            Step.DiagnosisSelection => BuildDiagnosisSelectionPrompt(),
            Step.MedicationName => "Medication name: ",
            Step.Dosage => "Dosage: ",
            Step.Frequency => BuildFrequencyPrompt(),
            Step.Route => BuildRoutePrompt(),
            Step.Duration => "Duration (e.g., '7 days', '2 weeks') [optional]: ",
            Step.Refills => "Number of refills [default: 0]: ",
            Step.GenericAllowed => "Allow generic substitution? [Y]es/[N]o [default: Yes]: ",
            Step.DEASchedule => "DEA Schedule (1-5, for controlled substances) [optional]: ",
            Step.Instructions => "Instructions (e.g., 'Take with food') [optional]: ",
            _ => ""
        };

        public override string DefaultValue => CurrentStepEnum switch
        {
            Step.Route => "PO",
            Step.Refills => "0",
            Step.GenericAllowed => "Y",
            _ => ""
        };

        private string BuildDiagnosisSelectionPrompt()
        {
            var prompt = "Select diagnosis: ";
            for (int i = 0; i < Math.Min(_availableDiagnoses.Count, 9); i++)
            {
                var content = _availableDiagnoses[i].Content;
                var preview = content.Length > 15 ? content.Substring(0, 15) + "..." : content;
                prompt += $"[{i + 1}]{preview} ";
            }
            prompt += "(1-9): ";
            return prompt;
        }

        private static string BuildFrequencyPrompt()
        {
            var options = string.Join(", ", DosageFrequencyExtensions.All.Take(6).Select(f => f.GetAbbreviation()));
            return $"Frequency ({options}...) *: ";
        }

        private static string BuildRoutePrompt()
        {
            var options = string.Join(", ", MedicationRouteExtensions.All.Take(6).Select(r => r.GetAbbreviation()));
            return $"Route ({options}...) [default: PO]: ";
        }

        public override void ProcessInput(string input)
        {
            switch (CurrentStepEnum)
            {
                case Step.DiagnosisSelection:
                    ProcessDiagnosisSelection(input);
                    break;
                case Step.MedicationName:
                    ProcessMedicationName(input);
                    break;
                case Step.Dosage:
                    ProcessDosage(input);
                    break;
                case Step.Frequency:
                    ProcessFrequency(input);
                    break;
                case Step.Route:
                    ProcessRoute(input);
                    break;
                case Step.Duration:
                    ProcessDuration(input);
                    break;
                case Step.Refills:
                    ProcessRefills(input);
                    break;
                case Step.GenericAllowed:
                    ProcessGenericAllowed(input);
                    break;
                case Step.DEASchedule:
                    ProcessDEASchedule(input);
                    break;
                case Step.Instructions:
                    ProcessInstructions(input);
                    break;
            }
        }

        private void ProcessDiagnosisSelection(string input)
        {
            var lastChar = GetLastChar(input);
            if (char.IsDigit(lastChar))
            {
                var selection = int.Parse(lastChar.ToString()) - 1; // 0-based
                if (selection >= 0 && selection < _availableDiagnoses.Count)
                {
                    SetData("diagnosis_id", _availableDiagnoses[selection].Id.ToString());
                    NextStep();
                    return;
                }
            }

            CancelWithError("Invalid diagnosis selection");
        }

        private void ProcessMedicationName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                CancelWithError("Medication name cannot be empty");
                return;
            }

            SetData("medication_name", input.Trim());
            NextStep();
        }

        private void ProcessDosage(string input)
        {
            SetData("dosage", input?.Trim() ?? "");
            NextStep();
        }

        private void ProcessFrequency(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                SetError("Frequency is required for prescriptions");
                return;
            }

            var trimmed = input.Trim();
            DosageFrequency? parsed = DosageFrequencyExtensions.All
                .FirstOrDefault(f => f.ToString().Equals(trimmed, StringComparison.OrdinalIgnoreCase) ||
                                    f.GetDisplayName().Equals(trimmed, StringComparison.OrdinalIgnoreCase) ||
                                    f.GetAbbreviation().Equals(trimmed, StringComparison.OrdinalIgnoreCase));

            if (parsed == null)
            {
                var validOptions = string.Join(", ", DosageFrequencyExtensions.All.Select(f => f.GetAbbreviation()));
                SetError($"Invalid frequency. Valid: {validOptions}");
                return;
            }

            SetData("frequency", parsed.Value.ToString());
            NextStep();
        }

        private void ProcessRoute(string input)
        {
            var trimmed = input?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(trimmed))
            {
                SetData("route", MedicationRoute.Oral.ToString());
            }
            else
            {
                MedicationRoute? parsed = MedicationRouteExtensions.All
                    .FirstOrDefault(r => r.ToString().Equals(trimmed, StringComparison.OrdinalIgnoreCase) ||
                                        r.GetDisplayName().Equals(trimmed, StringComparison.OrdinalIgnoreCase) ||
                                        r.GetAbbreviation().Equals(trimmed, StringComparison.OrdinalIgnoreCase));

                if (parsed == null)
                {
                    var validOptions = string.Join(", ", MedicationRouteExtensions.All.Select(r => r.GetAbbreviation()));
                    SetError($"Invalid route. Valid: {validOptions}");
                    return;
                }

                SetData("route", parsed.Value.ToString());
            }

            NextStep();
        }

        private void ProcessDuration(string input)
        {
            if (!string.IsNullOrWhiteSpace(input))
            {
                SetData("duration", input.Trim());
            }
            NextStep();
        }

        private void ProcessRefills(string input)
        {
            var trimmed = input?.Trim() ?? "0";
            if (int.TryParse(trimmed, out var refills) && refills >= 0)
            {
                SetData("refills", refills.ToString());
            }
            else
            {
                SetData("refills", "0");
            }
            NextStep();
        }

        private void ProcessGenericAllowed(string input)
        {
            var lastChar = input.Length > 0 ? char.ToUpper(input[input.Length - 1]) : 'Y';
            SetData("generic_allowed", (lastChar != 'N').ToString());
            NextStep();
        }

        private void ProcessDEASchedule(string input)
        {
            if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input.Trim(), out int schedule))
            {
                if (schedule >= 1 && schedule <= 5)
                {
                    SetData("dea_schedule", schedule.ToString());
                }
            }
            NextStep();
        }

        private void ProcessInstructions(string input)
        {
            SetData("instructions", input ?? "");
            Complete();
        }

        public override CommandParameters? BuildParameters(Guid documentId)
        {
            if (!IsComplete) return null;

            var parameters = new CommandParameters();
            parameters.SetParameter("document_id", documentId);

            if (HasData("diagnosis_id") && Guid.TryParse(GetData("diagnosis_id"), out var dxId))
            {
                parameters.SetParameter(AddPrescriptionCommand.Parameters.DiagnosisId, dxId);
            }

            parameters.SetParameter(AddPrescriptionCommand.Parameters.MedicationName, GetData("medication_name"));
            parameters.SetParameter(AddPrescriptionCommand.Parameters.Dosage, GetData("dosage"));

            if (HasData("frequency") && TryParseEnumByName<DosageFrequency>(GetData("frequency"), out var freq))
            {
                parameters.SetParameter(AddPrescriptionCommand.Parameters.Frequency, freq);
            }

            if (HasData("route") && TryParseEnumByName<MedicationRoute>(GetData("route"), out var route))
            {
                parameters.SetParameter(AddPrescriptionCommand.Parameters.Route, route);
            }

            if (HasData("duration"))
            {
                parameters.SetParameter(AddPrescriptionCommand.Parameters.Duration, GetData("duration"));
            }

            if (HasData("refills") && int.TryParse(GetData("refills"), out var refills))
            {
                parameters.SetParameter(AddPrescriptionCommand.Parameters.Refills, refills);
            }

            if (HasData("generic_allowed") && bool.TryParse(GetData("generic_allowed"), out var generic))
            {
                parameters.SetParameter(AddPrescriptionCommand.Parameters.GenericAllowed, generic);
            }

            if (HasData("dea_schedule") && int.TryParse(GetData("dea_schedule"), out var deaSchedule))
            {
                parameters.SetParameter(AddPrescriptionCommand.Parameters.DeaSchedule, deaSchedule);
            }

            if (HasData("instructions"))
            {
                parameters.SetParameter(AddPrescriptionCommand.Parameters.Instructions, GetData("instructions"));
            }

            return parameters;
        }
    }
}
