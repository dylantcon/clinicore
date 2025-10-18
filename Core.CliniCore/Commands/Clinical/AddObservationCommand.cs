// Core.CliniCore/Commands/Clinical/AddObservationCommand.cs
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.ClinicalDoc;

namespace Core.CliniCore.Commands.Clinical
{
    public class AddObservationCommand : AbstractCommand
    {
        public const string Key = "addobservation";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string DocumentId = "document_id";
            public const string Observation = "observation";
            public const string VitalSigns = "vital_signs";
            public const string NumericValue = "numeric_value";
            public const string Unit = "unit";
            public const string ObservationType = "observation_type";
            public const string BodySystem = "body_system";
            public const string IsAbnormal = "is_abnormal";
            public const string Severity = "severity";
            public const string ReferenceRange = "reference_range";
            public const string LoincCode = "loinc_code";
        }

        private readonly ClinicalDocumentRegistry _documentRegistry = ClinicalDocumentRegistry.Instance;
        private ObservationEntry? _addedObservation;
        private Guid? _targetDocumentId;

        public AddObservationCommand() {}

        public override string Description => "Adds a clinical observation to a document";

        public override bool CanUndo => true;

        public override Permission? GetRequiredPermission()
            => Permission.CreateClinicalDocument;

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Check required parameters
            var missingParams = parameters.GetMissingRequired(
                Parameters.DocumentId, Parameters.Observation);

            if (missingParams.Any())
            {
                foreach (var error in missingParams)
                    result.AddError(error);
                return result;
            }

            // Validate document exists
            var documentId = parameters.GetParameter<Guid?>(Parameters.DocumentId);
            if (!documentId.HasValue || documentId.Value == Guid.Empty)
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

            // Validate observation content
            var observation = parameters.GetParameter<string>(Parameters.Observation);
            if (string.IsNullOrWhiteSpace(observation))
            {
                result.AddError("Observation cannot be empty");
            }

            // Validate vital signs format if provided
            var vitals = parameters.GetParameter<Dictionary<string, string>>(Parameters.VitalSigns);
            if (vitals != null)
            {
                foreach (var vital in vitals)
                {
                    if (string.IsNullOrWhiteSpace(vital.Key) || string.IsNullOrWhiteSpace(vital.Value))
                    {
                        result.AddWarning($"Invalid vital sign entry: {vital.Key}");
                    }
                }
            }

            // Validate numeric values if provided
            var numericValue = parameters.GetParameter<double?>(Parameters.NumericValue);
            var unit = parameters.GetParameter<string>(Parameters.Unit);
            if (numericValue.HasValue && string.IsNullOrWhiteSpace(unit))
            {
                result.AddWarning("Numeric value provided without unit of measurement");
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var documentId = parameters.GetRequiredParameter<Guid>(Parameters.DocumentId);
                var observationText = parameters.GetRequiredParameter<string>(Parameters.Observation);
                var physicianId = session?.UserId ?? Guid.Empty;

                _targetDocumentId = documentId;
                var document = _documentRegistry.GetDocumentById(documentId);
                if (document == null)
                {
                    return CommandResult.Fail($"Clinical document {documentId} not found");
                }

                // Create the observation
                _addedObservation = new ObservationEntry(physicianId, observationText)
                {
                    Type = parameters.GetParameter<ObservationType?>(Parameters.ObservationType)
                        ?? ObservationType.PhysicalExam,
                    BodySystem = parameters.GetParameter<string>(Parameters.BodySystem),
                    IsAbnormal = parameters.GetParameter<bool?>(Parameters.IsAbnormal) ?? false,
                    Severity = parameters.GetParameter<EntrySeverity?>(Parameters.Severity)
                        ?? EntrySeverity.Routine,
                    NumericValue = parameters.GetParameter<double?>(Parameters.NumericValue),
                    Unit = parameters.GetParameter<string>(Parameters.Unit),
                    ReferenceRange = parameters.GetParameter<string>(Parameters.ReferenceRange),
                    Code = parameters.GetParameter<string>(Parameters.LoincCode) // LOINC codes for lab observations
                };

                // Add vital signs if provided
                var vitals = parameters.GetParameter<Dictionary<string, string>>(Parameters.VitalSigns);
                if (vitals != null)
                {
                    foreach (var vital in vitals)
                    {
                        _addedObservation.AddVitalSign(vital.Key, vital.Value);
                    }
                }

                // Add to document
                document.AddEntry(_addedObservation);

                // Build confirmation message
                var abnormalStr = _addedObservation.IsAbnormal ? " [ABNORMAL]" : "";
                var systemStr = !string.IsNullOrEmpty(_addedObservation.BodySystem)
                    ? $"\n  Body System: {_addedObservation.BodySystem}"
                    : "";
                var vitalsStr = _addedObservation.VitalSigns.Any()
                    ? $"\n  Vital Signs: {_addedObservation.GetVitalSignsDisplay()}"
                    : "";

                return CommandResult.Ok(
                    $"Observation added successfully:{abnormalStr}\n" +
                    $"  Type: {_addedObservation.Type}\n" +
                    $"  Content: {observationText}{systemStr}{vitalsStr}\n" +
                    $"  Severity: {_addedObservation.Severity}\n" +
                    $"  Observation ID: {_addedObservation.Id}",
                    _addedObservation);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to add observation: {ex.Message}", ex);
            }
        }

        protected override object? CaptureStateForUndo(CommandParameters parameters, SessionContext? session)
        {
            return new UndoState
            {
                DocumentId = _targetDocumentId ?? Guid.Empty,
                ObservationId = _addedObservation?.Id ?? Guid.Empty
            };
        }

        protected override CommandResult UndoCore(object previousState, SessionContext? session)
        {
            if (previousState is UndoState state)
            {
                var document = _documentRegistry.GetDocumentById(state.DocumentId);
                if (document != null)
                {
                    var observation = document.GetObservations()
                        .FirstOrDefault(o => o.Id == state.ObservationId);

                    if (observation != null)
                    {
                        observation.IsActive = false;
                        return CommandResult.Ok($"Observation {state.ObservationId} has been marked inactive");
                    }
                }
            }
            return CommandResult.Fail("Unable to undo observation addition");
        }

        private class UndoState
        {
            public Guid DocumentId { get; set; }
            public Guid ObservationId { get; set; }
        }
    }
}
