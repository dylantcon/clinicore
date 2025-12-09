using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Clinical;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;

namespace CLI.CliniCore.Service.Editor.Workflows
{
    /// <summary>
    /// Workflow for creating observation entries (subjective and objective observations).
    /// Steps: Category => Type => Content => BodySystem => Severity => IsAbnormal => (NumericValue/Unit for objective)
    /// </summary>
    public class AddObservationWorkflow : EntryWorkflowBase
    {
        private enum Step { Category, SubjectiveType, ObjectiveType, Content, BodySystem, Severity, IsAbnormal, NumericValue, Unit }

        private Step CurrentStepEnum => (Step)_currentStep;

        public override string CommandKey => AddObservationCommand.Key;

        public override string CurrentPrompt => CurrentStepEnum switch
        {
            Step.Category => "Category - [S]ubjective or [O]bjective (Esc=cancel): ",
            Step.SubjectiveType => "[C]hief Complaint, [H]PI, [S]ocial Hx, [F]amily Hx, [A]llergy: ",
            Step.ObjectiveType => "[P]hysical Exam, [V]ital Signs, [L]ab, [I]maging, [R]eview of Systems: ",
            Step.Content => "Enter observation content: ",
            Step.BodySystem => BuildBodySystemPrompt(),
            Step.Severity => "[R]outine, [M]oderate, [U]rgent, [C]ritical, [E]mergency [default: R]: ",
            Step.IsAbnormal => "Is this finding abnormal? [Y]es/[N]o [default: N]: ",
            Step.NumericValue => "Numeric value (e.g., 120) [optional, press Enter to skip]: ",
            Step.Unit => "Unit (e.g., mmHg, mg/dL) [optional]: ",
            _ => ""
        };

        public override string DefaultValue => CurrentStepEnum switch
        {
            Step.Severity => "R",
            Step.IsAbnormal => "N",
            _ => ""
        };

        private static string BuildBodySystemPrompt()
        {
            return "[G]eneral, H[E]ENT, [C]ardio, [R]esp, G[I], G[U], M[S]cul, [N]euro, Inte[G]um, En[D]ocr, [H]eme, I[M]mun, [P]sych [opt]: ";
        }

        public override void ProcessInput(string input)
        {
            switch (CurrentStepEnum)
            {
                case Step.Category:
                    ProcessCategorySelection(input);
                    break;
                case Step.SubjectiveType:
                    ProcessSubjectiveType(input);
                    break;
                case Step.ObjectiveType:
                    ProcessObjectiveType(input);
                    break;
                case Step.Content:
                    ProcessContent(input);
                    break;
                case Step.BodySystem:
                    ProcessBodySystem(input);
                    break;
                case Step.Severity:
                    ProcessSeverity(input);
                    break;
                case Step.IsAbnormal:
                    ProcessIsAbnormal(input);
                    break;
                case Step.NumericValue:
                    ProcessNumericValue(input);
                    break;
                case Step.Unit:
                    ProcessUnit(input);
                    break;
            }
        }

        private void ProcessCategorySelection(string input)
        {
            var selection = GetLastChar(input);
            switch (selection)
            {
                case 'S':
                    SetData("category", "subjective");
                    _currentStep = (int)Step.SubjectiveType;
                    break;
                case 'O':
                    SetData("category", "objective");
                    _currentStep = (int)Step.ObjectiveType;
                    break;
                default:
                    CancelWithError("Invalid category selection");
                    break;
            }
        }

        private void ProcessSubjectiveType(string input)
        {
            var selection = GetLastChar(input);
            ObservationType? obsType = selection switch
            {
                'C' => ObservationType.ChiefComplaint,
                'H' => ObservationType.HistoryOfPresentIllness,
                'S' => ObservationType.SocialHistory,
                'F' => ObservationType.FamilyHistory,
                'A' => ObservationType.Allergy,
                _ => null
            };

            if (obsType == null)
            {
                CancelWithError("Invalid subjective type selection");
                return;
            }

            SetData("observation_type", obsType.Value.ToString());
            _currentStep = (int)Step.Content;
        }

        private void ProcessObjectiveType(string input)
        {
            var selection = GetLastChar(input);
            ObservationType? obsType = selection switch
            {
                'P' => ObservationType.PhysicalExam,
                'V' => ObservationType.VitalSigns,
                'L' => ObservationType.LabResult,
                'I' => ObservationType.ImagingResult,
                'R' => ObservationType.ReviewOfSystems,
                _ => null
            };

            if (obsType == null)
            {
                CancelWithError("Invalid objective observation type selection");
                return;
            }

            SetData("observation_type", obsType.Value.ToString());
            _currentStep = (int)Step.Content;
        }

        private void ProcessContent(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                CancelWithError("Content cannot be empty");
                return;
            }

            SetData("content", input);
            _currentStep = (int)Step.BodySystem;
        }

        private void ProcessBodySystem(string input)
        {
            var selection = GetLastChar(input);
            BodySystem? bodySystem = selection switch
            {
                'G' => BodySystem.General,
                'E' => BodySystem.HEENT,
                'C' => BodySystem.Cardiovascular,
                'R' => BodySystem.Respiratory,
                'I' => BodySystem.Gastrointestinal,
                'U' => BodySystem.Genitourinary,
                'S' => BodySystem.Musculoskeletal,
                'N' => BodySystem.Neurological,
                'T' => BodySystem.Integumentary,
                'D' => BodySystem.Endocrine,
                'H' => BodySystem.Hematologic,
                'M' => BodySystem.Immunologic,
                'P' => BodySystem.Psychiatric,
                _ => null
            };

            if (bodySystem.HasValue)
            {
                SetData("body_system", bodySystem.Value.ToString());
            }

            _currentStep = (int)Step.Severity;
        }

        private void ProcessSeverity(string input)
        {
            var selection = GetLastChar(input);
            EntrySeverity severity = selection switch
            {
                'R' => EntrySeverity.Routine,
                'M' => EntrySeverity.Moderate,
                'U' => EntrySeverity.Urgent,
                'C' => EntrySeverity.Critical,
                'E' => EntrySeverity.Emergency,
                _ => EntrySeverity.Routine
            };

            SetData("severity", severity.ToString());
            _currentStep = (int)Step.IsAbnormal;
        }

        private void ProcessIsAbnormal(string input)
        {
            var selection = GetLastChar(input);
            var isAbnormal = selection == 'Y';
            SetData("is_abnormal", isAbnormal.ToString().ToLower());

            // For objective observations, offer numeric value entry
            if (GetData("category") == "objective")
            {
                _currentStep = (int)Step.NumericValue;
            }
            else
            {
                Complete();
            }
        }

        private void ProcessNumericValue(string input)
        {
            if (!string.IsNullOrWhiteSpace(input) && double.TryParse(input, out var numericValue))
            {
                SetData("numeric_value", numericValue.ToString());
                _currentStep = (int)Step.Unit;
            }
            else
            {
                // Skip numeric value and unit
                Complete();
            }
        }

        private void ProcessUnit(string input)
        {
            if (!string.IsNullOrWhiteSpace(input))
            {
                SetData("unit", input.Trim());
            }
            Complete();
        }

        public override CommandParameters? BuildParameters(Guid documentId)
        {
            if (!IsComplete) return null;

            var parameters = new CommandParameters();
            parameters.SetParameter(AddObservationCommand.Parameters.DocumentId, documentId);
            parameters.SetParameter(AddObservationCommand.Parameters.Observation, GetData("content"));

            if (HasData("observation_type") &&
                TryParseEnumByName<ObservationType>(GetData("observation_type"), out var obsType))
            {
                parameters.SetParameter(AddObservationCommand.Parameters.ObservationType, obsType);
            }

            if (HasData("body_system") &&
                TryParseEnumByName<BodySystem>(GetData("body_system"), out var bodySystem))
            {
                parameters.SetParameter(AddObservationCommand.Parameters.BodySystem, bodySystem);
            }

            if (HasData("severity") &&
                TryParseEnumByName<EntrySeverity>(GetData("severity"), out var severity))
            {
                parameters.SetParameter(AddObservationCommand.Parameters.Severity, severity);
            }

            if (HasData("is_abnormal") && bool.TryParse(GetData("is_abnormal"), out var isAbnormal))
            {
                parameters.SetParameter(AddObservationCommand.Parameters.IsAbnormal, isAbnormal);
            }

            if (HasData("numeric_value") && double.TryParse(GetData("numeric_value"), out var numericValue))
            {
                parameters.SetParameter(AddObservationCommand.Parameters.NumericValue, numericValue);
            }

            if (HasData("unit"))
            {
                parameters.SetParameter(AddObservationCommand.Parameters.Unit, GetData("unit"));
            }

            return parameters;
        }
    }
}
