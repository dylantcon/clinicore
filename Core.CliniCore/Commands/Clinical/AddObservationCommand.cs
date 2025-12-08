// Core.CliniCore/Commands/Clinical/AddObservationCommand.cs
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Service;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.ClinicalDocumentation.ClinicalEntries;

namespace Core.CliniCore.Commands.Clinical
{
    /// <summary>
    /// Command that adds a clinical observation entry to an existing clinical document.
    /// </summary>
    public class AddObservationCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "addobservation";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="AddObservationCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the target clinical document identifier.
            /// </summary>
            public const string DocumentId = "document_id";

            /// <summary>
            /// Parameter key for the observation content.
            /// </summary>
            public const string Observation = "observation";

            /// <summary>
            /// Parameter key for a collection of vital sign measurements.
            /// </summary>
            public const string VitalSigns = "vital_signs";

            /// <summary>
            /// Parameter key for a numeric measurement value associated with the observation.
            /// </summary>
            public const string NumericValue = "numeric_value";

            /// <summary>
            /// Parameter key for the unit of the numeric measurement.
            /// </summary>
            public const string Unit = "unit";

            /// <summary>
            /// Parameter key for the observation type.
            /// </summary>
            public const string ObservationType = "observation_type";

            /// <summary>
            /// Parameter key for the affected body system.
            /// </summary>
            public const string BodySystem = "body_system";

            /// <summary>
            /// Parameter key indicating whether the observation is abnormal.
            /// </summary>
            public const string IsAbnormal = "is_abnormal";

            /// <summary>
            /// Parameter key for the severity of the observation.
            /// </summary>
            public const string Severity = "severity";

            /// <summary>
            /// Parameter key for a textual reference range.
            /// </summary>
            public const string ReferenceRange = "reference_range";

            /// <summary>
            /// Parameter key for the LOINC code associated with the observation.
            /// </summary>
            public const string LoincCode = "loinc_code";
        }

        private readonly ClinicalDocumentService _documentRegistry;
        private ObservationEntry? _addedObservation;
        private Guid? _targetDocumentId;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddObservationCommand"/> class.
        /// </summary>
        /// <param name="clinicalDocService">The clinical document service used to access and modify documents.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="clinicalDocService"/> is <c>null</c>.</exception>
        public AddObservationCommand(ClinicalDocumentService clinicalDocService)
        {
            _documentRegistry = clinicalDocService ?? throw new ArgumentNullException(nameof(clinicalDocService));
        }

        /// <inheritdoc />
        public override string Description => "Adds a clinical observation to a document";

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

        /// <inheritdoc />
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
                    BodySystem = parameters.GetParameter<BodySystem?>(Parameters.BodySystem),
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

                // Persist the observation via the service's entry-level method
                _documentRegistry.AddObservation(documentId, _addedObservation);

                // Build confirmation message
                var abnormalStr = _addedObservation.IsAbnormal ? " [ABNORMAL]" : "";
                var systemStr = _addedObservation.BodySystem.HasValue
                    ? $"\n  Body System: {_addedObservation.BodySystem.Value}"
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

        /// <inheritdoc />
        protected override object? CaptureStateForUndo(CommandParameters parameters, SessionContext? session)
        {
            return new UndoState
            {
                DocumentId = _targetDocumentId ?? Guid.Empty,
                ObservationId = _addedObservation?.Id ?? Guid.Empty
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
            /// <summary>
            /// Gets or sets the identifier of the clinical document that owns the observation.
            /// </summary>
            public Guid DocumentId { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the observation entry that was added.
            /// </summary>
            public Guid ObservationId { get; set; }
        }
    }
}
