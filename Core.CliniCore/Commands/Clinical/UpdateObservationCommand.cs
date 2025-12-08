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
    /// Command that updates an existing observation entry within a clinical document.
    /// </summary>
    public class UpdateObservationCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "updateobservation";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="UpdateObservationCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the clinical document identifier that owns the observation.
            /// </summary>
            public const string DocumentId = "document_id";

            /// <summary>
            /// Parameter key for the observation entry identifier.
            /// </summary>
            public const string ObservationId = "observation_id";

            /// <summary>
            /// Parameter key for the updated observation content.
            /// </summary>
            public const string Content = "content";

            /// <summary>
            /// Parameter key for the observation type.
            /// </summary>
            public const string Type = "type";

            /// <summary>
            /// Parameter key for the affected body system.
            /// </summary>
            public const string BodySystem = "body_system";

            /// <summary>
            /// Parameter key indicating whether the observation is abnormal.
            /// </summary>
            public const string IsAbnormal = "is_abnormal";

            /// <summary>
            /// Parameter key for a numeric measurement value associated with the observation.
            /// </summary>
            public const string NumericValue = "numeric_value";

            /// <summary>
            /// Parameter key for the unit of the numeric measurement.
            /// </summary>
            public const string Unit = "unit";

            /// <summary>
            /// Parameter key for the textual reference range.
            /// </summary>
            public const string ReferenceRange = "reference_range";

            /// <summary>
            /// Parameter key for a set of vital sign measurements.
            /// </summary>
            public const string VitalSigns = "vital_signs";

            /// <summary>
            /// Parameter key for a coding system identifier associated with the observation.
            /// </summary>
            public const string Code = "code";

            /// <summary>
            /// Parameter key for the severity associated with the observation.
            /// </summary>
            public const string Severity = "severity";
        }

        private readonly ClinicalDocumentService _documentRegistry;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateObservationCommand"/> class.
        /// </summary>
        /// <param name="clinicalDocService">The clinical document service used to access and update documents.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="clinicalDocService"/> is <c>null</c>.</exception>
        public UpdateObservationCommand(ClinicalDocumentService clinicalDocService)
        {
            _documentRegistry = clinicalDocService ?? throw new ArgumentNullException(nameof(clinicalDocService));
        }

        /// <inheritdoc />
        public override string Description => "Updates an observation entry within a clinical document";

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
            var missingParams = parameters.GetMissingRequired(Parameters.DocumentId, Parameters.ObservationId);
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

            var observationId = parameters.GetParameter<Guid?>(Parameters.ObservationId);
            if (!observationId.HasValue || observationId.Value == Guid.Empty)
            {
                result.AddError("Invalid observation ID");
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

            // Check observation exists and is correct type
            var entry = document.Entries.FirstOrDefault(e => e.Id == observationId.Value);
            if (entry == null)
            {
                result.AddError($"Observation with ID {observationId.Value} not found in document");
                return result;
            }

            if (entry is not ObservationEntry)
            {
                result.AddError($"Entry with ID {observationId.Value} is not an observation entry");
                return result;
            }

            // Validate ObservationType if provided
            var typeStr = parameters.GetParameter<string>(Parameters.Type);
            if (!string.IsNullOrEmpty(typeStr) && !Enum.TryParse<ObservationType>(typeStr, true, out _))
            {
                var validTypes = string.Join(", ", Enum.GetNames<ObservationType>());
                result.AddError($"Invalid observation type. Valid values are: {validTypes}");
            }

            // Validate EntrySeverity if provided
            var severityStr = parameters.GetParameter<string>(Parameters.Severity);
            if (!string.IsNullOrEmpty(severityStr) && !Enum.TryParse<EntrySeverity>(severityStr, true, out _))
            {
                var validSeverities = string.Join(", ", Enum.GetNames<EntrySeverity>());
                result.AddError($"Invalid severity. Valid values are: {validSeverities}");
            }

            // Validate numeric value consistency
            var numericValue = parameters.GetParameter<double?>(Parameters.NumericValue);
            var unit = parameters.GetParameter<string>(Parameters.Unit);
            if (numericValue.HasValue && numericValue.Value < 0)
            {
                result.AddWarning("Numeric value is negative - ensure this is intentional");
            }

            return result;
        }

        /// <inheritdoc />
        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var documentId = parameters.GetRequiredParameter<Guid>(Parameters.DocumentId);
                var observationId = parameters.GetRequiredParameter<Guid>(Parameters.ObservationId);

                var document = _documentRegistry.GetDocumentById(documentId);
                if (document == null)
                {
                    return CommandResult.Fail("Clinical document not found");
                }

                var observation = document.Entries.FirstOrDefault(e => e.Id == observationId) as ObservationEntry;
                if (observation == null)
                {
                    return CommandResult.Fail("Observation entry not found in document");
                }

                var fieldsUpdated = new List<string>();

                // Update content
                var content = parameters.GetParameter<string>(Parameters.Content);
                if (!string.IsNullOrEmpty(content) && content != observation.Content)
                {
                    observation.Update(content);
                    fieldsUpdated.Add("content");
                }

                // Update type
                var typeStr = parameters.GetParameter<string>(Parameters.Type);
                if (!string.IsNullOrEmpty(typeStr) && Enum.TryParse<ObservationType>(typeStr, true, out var type))
                {
                    if (type != observation.Type)
                    {
                        observation.Type = type;
                        fieldsUpdated.Add("type");
                    }
                }

                // Update body system
                var bodySystem = parameters.GetParameter<BodySystem?>(Parameters.BodySystem);
                if (bodySystem.HasValue && bodySystem != observation.BodySystem)
                {
                    observation.BodySystem = bodySystem;
                    fieldsUpdated.Add("body_system");
                }

                // Update abnormal flag
                var isAbnormal = parameters.GetParameter<bool?>(Parameters.IsAbnormal);
                if (isAbnormal.HasValue && isAbnormal.Value != observation.IsAbnormal)
                {
                    observation.IsAbnormal = isAbnormal.Value;
                    fieldsUpdated.Add("is_abnormal");
                }

                // Update numeric value
                var numericValue = parameters.GetParameter<double?>(Parameters.NumericValue);
                if (numericValue.HasValue && numericValue != observation.NumericValue)
                {
                    observation.NumericValue = numericValue.Value;
                    fieldsUpdated.Add("numeric_value");
                }

                // Update unit
                var unit = parameters.GetParameter<string>(Parameters.Unit);
                if (unit != null && unit != observation.Unit)
                {
                    observation.Unit = string.IsNullOrEmpty(unit) ? null : unit;
                    fieldsUpdated.Add("unit");
                }

                // Update reference range
                var referenceRange = parameters.GetParameter<string>(Parameters.ReferenceRange);
                if (referenceRange != null && referenceRange != observation.ReferenceRange)
                {
                    observation.ReferenceRange = string.IsNullOrEmpty(referenceRange) ? null : referenceRange;
                    fieldsUpdated.Add("reference_range");
                }

                // Update code
                var code = parameters.GetParameter<string>(Parameters.Code);
                if (code != null && code != observation.Code)
                {
                    observation.Code = string.IsNullOrEmpty(code) ? null : code;
                    fieldsUpdated.Add("code");
                }

                // Update severity
                var severityStr = parameters.GetParameter<string>(Parameters.Severity);
                if (!string.IsNullOrEmpty(severityStr) && Enum.TryParse<EntrySeverity>(severityStr, true, out var severity))
                {
                    if (severity != observation.Severity)
                    {
                        observation.Severity = severity;
                        fieldsUpdated.Add("severity");
                    }
                }

                // Update vital signs (expects dictionary)
                var vitalSigns = parameters.GetParameter<Dictionary<string, string>>(Parameters.VitalSigns);
                if (vitalSigns != null)
                {
                    observation.VitalSigns.Clear();
                    foreach (var kvp in vitalSigns)
                    {
                        observation.AddVitalSign(kvp.Key, kvp.Value);
                    }
                    fieldsUpdated.Add("vital_signs");
                }

                if (fieldsUpdated.Any())
                {
                    // Persist the changes to the repository
                    _documentRegistry.UpdateDocument(document);

                    return CommandResult.Ok(
                        $"Observation entry updated successfully. Fields changed: {string.Join(", ", fieldsUpdated)}",
                        new {
                            DocumentId = documentId,
                            ObservationId = observationId,
                            UpdatedFields = fieldsUpdated,
                            ModifiedAt = observation.ModifiedAt
                        });
                }
                else
                {
                    return CommandResult.Ok("No changes were made to the observation entry", observation);
                }
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to update observation: {ex.Message}", ex);
            }
        }
    }
}