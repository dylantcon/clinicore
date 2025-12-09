using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Clinical;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;

namespace CLI.CliniCore.Service.Editor.Workflows
{
    /// <summary>
    /// Workflow for creating diagnosis entries.
    /// Steps: Type => ICD-10 => Status => Severity => IsPrimary => OnsetDate => Content
    /// </summary>
    public class AddDiagnosisWorkflow : EntryWorkflowBase
    {
        private enum Step { Type, ICD10, Status, Severity, IsPrimary, OnsetDate, Content }

        private Step CurrentStepEnum => (Step)_currentStep;

        public override string CommandKey => AddDiagnosisCommand.Key;

        public override string CurrentPrompt => CurrentStepEnum switch
        {
            Step.Type => "Type: [D]ifferential, [W]orking, [F]inal, [R]uled Out: ",
            Step.ICD10 => GetICD10Prompt(),
            Step.Status => "Status: [A]ctive, Resol[V]ed, [C]hronic, Re[M]ission, Recurr[E]nce: ",
            Step.Severity => "Severity: [R]outine, [M]oderate, [U]rgent, Cri[T]ical, [E]mergency [default: R]: ",
            Step.IsPrimary => "Is this the primary diagnosis? [Y]es/[N]o [default: N]: ",
            Step.OnsetDate => "Onset date (YYYY-MM-DD) [optional, press Enter to skip]: ",
            Step.Content => "Enter diagnosis description: ",
            _ => ""
        };

        public override string DefaultValue => CurrentStepEnum switch
        {
            Step.Severity => "R",
            Step.IsPrimary => "N",
            _ => ""
        };

        private string GetICD10Prompt()
        {
            var required = GetData("diagnosis_type") == DiagnosisType.Final.ToString() ? " *required*" : " [optional]";
            return $"ICD-10 code{required}: ";
        }

        public override void ProcessInput(string input)
        {
            switch (CurrentStepEnum)
            {
                case Step.Type:
                    ProcessTypeSelection(input);
                    break;
                case Step.ICD10:
                    ProcessICD10Input(input);
                    break;
                case Step.Status:
                    ProcessStatusSelection(input);
                    break;
                case Step.Severity:
                    ProcessSeveritySelection(input);
                    break;
                case Step.IsPrimary:
                    ProcessIsPrimary(input);
                    break;
                case Step.OnsetDate:
                    ProcessOnsetDate(input);
                    break;
                case Step.Content:
                    ProcessContent(input);
                    break;
            }
        }

        private void ProcessTypeSelection(string input)
        {
            var selection = GetLastChar(input);
            DiagnosisType? dxType = selection switch
            {
                'D' => DiagnosisType.Differential,
                'W' => DiagnosisType.Working,
                'F' => DiagnosisType.Final,
                'R' => DiagnosisType.RuledOut,
                _ => null
            };

            if (dxType == null)
            {
                CancelWithError("Invalid diagnosis type selection");
                return;
            }

            SetData("diagnosis_type", dxType.Value.ToString());
            NextStep();
        }

        private void ProcessICD10Input(string input)
        {
            var trimmed = input?.Trim() ?? "";

            // Final diagnoses require ICD-10
            if (GetData("diagnosis_type") == DiagnosisType.Final.ToString() &&
                string.IsNullOrWhiteSpace(trimmed))
            {
                SetError("ICD-10 code required for Final diagnoses");
                return;
            }

            if (!string.IsNullOrWhiteSpace(trimmed))
            {
                SetData("icd10_code", trimmed);
            }

            NextStep();
        }

        private void ProcessStatusSelection(string input)
        {
            var selection = GetLastChar(input);
            DiagnosisStatus status = selection switch
            {
                'A' => DiagnosisStatus.Active,
                'V' => DiagnosisStatus.Resolved,
                'C' => DiagnosisStatus.Chronic,
                'M' => DiagnosisStatus.Remission,
                'E' => DiagnosisStatus.Recurrence,
                _ => DiagnosisStatus.Active // Default
            };

            SetData("diagnosis_status", status.ToString());
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

        private void ProcessIsPrimary(string input)
        {
            var selection = GetLastChar(input);
            var isPrimary = selection == 'Y';
            SetData("is_primary", isPrimary.ToString().ToLower());
            NextStep();
        }

        private void ProcessOnsetDate(string input)
        {
            if (!string.IsNullOrWhiteSpace(input) && DateTime.TryParse(input, out var onsetDate))
            {
                SetData("onset_date", onsetDate.ToString("O")); // ISO 8601 format
            }
            NextStep();
        }

        private void ProcessContent(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                CancelWithError("Diagnosis description cannot be empty");
                return;
            }

            SetData("content", input);
            Complete();
        }

        public override CommandParameters? BuildParameters(Guid documentId)
        {
            if (!IsComplete) return null;

            var parameters = new CommandParameters();
            parameters.SetParameter(AddDiagnosisCommand.Parameters.DocumentId, documentId);
            parameters.SetParameter(AddDiagnosisCommand.Parameters.DiagnosisDescription, GetData("content"));

            if (HasData("diagnosis_type") &&
                TryParseEnumByName<DiagnosisType>(GetData("diagnosis_type"), out var dxType))
            {
                parameters.SetParameter(AddDiagnosisCommand.Parameters.DiagnosisType, dxType);
            }

            if (HasData("icd10_code"))
            {
                parameters.SetParameter(AddDiagnosisCommand.Parameters.ICD10Code, GetData("icd10_code"));
            }

            if (HasData("diagnosis_status") &&
                TryParseEnumByName<DiagnosisStatus>(GetData("diagnosis_status"), out var status))
            {
                parameters.SetParameter(AddDiagnosisCommand.Parameters.DiagnosisStatus, status);
            }

            if (HasData("severity") &&
                TryParseEnumByName<EntrySeverity>(GetData("severity"), out var severity))
            {
                parameters.SetParameter(AddDiagnosisCommand.Parameters.Severity, severity);
            }

            if (HasData("is_primary") && bool.TryParse(GetData("is_primary"), out var isPrimary))
            {
                parameters.SetParameter(AddDiagnosisCommand.Parameters.IsPrimary, isPrimary);
            }

            if (HasData("onset_date") && DateTime.TryParse(GetData("onset_date"), out var onsetDate))
            {
                parameters.SetParameter(AddDiagnosisCommand.Parameters.OnsetDate, onsetDate);
            }

            return parameters;
        }
    }
}
