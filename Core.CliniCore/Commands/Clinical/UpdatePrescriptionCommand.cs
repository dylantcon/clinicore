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
    public class UpdatePrescriptionCommand : AbstractCommand
    {
        public const string Key = "updateprescription";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string DocumentId = "document_id";
            public const string PrescriptionId = "prescription_id";
            public const string MedicationName = "medication_name";
            public const string Dosage = "dosage";
            public const string Frequency = "frequency";
            public const string Route = "route";
            public const string Duration = "duration";
            public const string Refills = "refills";
            public const string GenericAllowed = "generic_allowed";
            public const string DEASchedule = "dea_schedule";
            public const string ExpirationDate = "expiration_date";
            public const string Instructions = "instructions";
            public const string NDCCode = "ndc_code";
            public const string Severity = "severity";
        }

        private readonly ClinicalDocumentService _documentRegistry;

        public UpdatePrescriptionCommand(ClinicalDocumentService clinicalDocService)
        {
            _documentRegistry = clinicalDocService ?? throw new ArgumentNullException(nameof(clinicalDocService));
        }

        public override string Description => "Updates a prescription entry within a clinical document";

        public override bool CanUndo => false;

        public override Permission? GetRequiredPermission()
            => Permission.UpdateClinicalDocument;

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Check required parameters
            var missingParams = parameters.GetMissingRequired(Parameters.DocumentId, Parameters.PrescriptionId);
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

            var prescriptionId = parameters.GetParameter<Guid?>(Parameters.PrescriptionId);
            if (!prescriptionId.HasValue || prescriptionId.Value == Guid.Empty)
            {
                result.AddError("Invalid prescription ID");
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

            // Check prescription exists and is correct type
            var entry = document.Entries.FirstOrDefault(e => e.Id == prescriptionId.Value);
            if (entry == null)
            {
                result.AddError($"Prescription with ID {prescriptionId.Value} not found in document");
                return result;
            }

            if (entry is not PrescriptionEntry)
            {
                result.AddError($"Entry with ID {prescriptionId.Value} is not a prescription entry");
                return result;
            }

            // Validate DEA Schedule
            var deaSchedule = parameters.GetParameter<int?>(Parameters.DEASchedule);
            if (deaSchedule.HasValue && (deaSchedule.Value < 1 || deaSchedule.Value > 5))
            {
                result.AddError("DEA Schedule must be between 1 and 5");
            }

            // Validate refills
            var refills = parameters.GetParameter<int?>(Parameters.Refills);
            if (refills.HasValue && refills.Value < 0)
            {
                result.AddError("Refills cannot be negative");
            }

            // Validate controlled substance regulations
            if (deaSchedule.HasValue && deaSchedule.Value == 2)
            {
                if (refills.HasValue && refills.Value > 5)
                {
                    result.AddError("Schedule II controlled substances cannot have more than 5 refills");
                }
            }

            // Validate EntrySeverity if provided
            var severityStr = parameters.GetParameter<string>(Parameters.Severity);
            if (!string.IsNullOrEmpty(severityStr) && !Enum.TryParse<EntrySeverity>(severityStr, true, out _))
            {
                var validSeverities = string.Join(", ", Enum.GetNames<EntrySeverity>());
                result.AddError($"Invalid severity. Valid values are: {validSeverities}");
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var documentId = parameters.GetRequiredParameter<Guid>(Parameters.DocumentId);
                var prescriptionId = parameters.GetRequiredParameter<Guid>(Parameters.PrescriptionId);

                var document = _documentRegistry.GetDocumentById(documentId);
                if (document == null)
                {
                    return CommandResult.Fail("Clinical document not found");
                }

                var prescription = document.Entries.FirstOrDefault(e => e.Id == prescriptionId) as PrescriptionEntry;
                if (prescription == null)
                {
                    return CommandResult.Fail("Prescription entry not found in document");
                }

                var fieldsUpdated = new List<string>();

                // Update medication name
                var medicationName = parameters.GetParameter<string>(Parameters.MedicationName);
                if (!string.IsNullOrEmpty(medicationName) && medicationName != prescription.MedicationName)
                {
                    prescription.MedicationName = medicationName;
                    fieldsUpdated.Add("medication_name");
                }

                // Update dosage
                var dosage = parameters.GetParameter<string>(Parameters.Dosage);
                if (dosage != null && dosage != prescription.Dosage)
                {
                    prescription.Dosage = string.IsNullOrEmpty(dosage) ? null : dosage;
                    fieldsUpdated.Add("dosage");
                }

                // Update frequency
                var frequency = parameters.GetParameter<DosageFrequency?>(Parameters.Frequency);
                if (frequency.HasValue && frequency != prescription.Frequency)
                {
                    prescription.Frequency = frequency;
                    fieldsUpdated.Add("frequency");
                }

                // Update route
                var route = parameters.GetParameter<MedicationRoute?>(Parameters.Route);
                if (route.HasValue && route.Value != prescription.Route)
                {
                    prescription.Route = route.Value;
                    fieldsUpdated.Add("route");
                }

                // Update duration
                var duration = parameters.GetParameter<string>(Parameters.Duration);
                if (duration != null && duration != prescription.Duration)
                {
                    prescription.Duration = string.IsNullOrEmpty(duration) ? null : duration;
                    fieldsUpdated.Add("duration");
                }

                // Update refills
                var refills = parameters.GetParameter<int?>(Parameters.Refills);
                if (refills.HasValue && refills.Value != prescription.Refills)
                {
                    prescription.Refills = refills.Value;
                    fieldsUpdated.Add("refills");
                }

                // Update generic allowed
                var genericAllowed = parameters.GetParameter<bool?>(Parameters.GenericAllowed);
                if (genericAllowed.HasValue && genericAllowed.Value != prescription.GenericAllowed)
                {
                    prescription.GenericAllowed = genericAllowed.Value;
                    fieldsUpdated.Add("generic_allowed");
                }

                // Update DEA Schedule
                var deaSchedule = parameters.GetParameter<int?>(Parameters.DEASchedule);
                if (deaSchedule != prescription.DEASchedule)
                {
                    prescription.DEASchedule = deaSchedule;
                    fieldsUpdated.Add("dea_schedule");
                }

                // Update expiration date
                var expirationDate = parameters.GetParameter<DateTime?>(Parameters.ExpirationDate);
                if (expirationDate != prescription.ExpirationDate)
                {
                    prescription.ExpirationDate = expirationDate;
                    fieldsUpdated.Add("expiration_date");
                }

                // Update instructions
                var instructions = parameters.GetParameter<string>(Parameters.Instructions);
                if (instructions != null && instructions != prescription.Instructions)
                {
                    prescription.Instructions = string.IsNullOrEmpty(instructions) ? null : instructions;
                    fieldsUpdated.Add("instructions");
                }

                // Update NDC code
                var ndcCode = parameters.GetParameter<string>(Parameters.NDCCode);
                if (ndcCode != null && ndcCode != prescription.NDCCode)
                {
                    prescription.NDCCode = string.IsNullOrEmpty(ndcCode) ? null : ndcCode;
                    fieldsUpdated.Add("ndc_code");
                }

                // Update severity
                var severityStr = parameters.GetParameter<string>(Parameters.Severity);
                if (!string.IsNullOrEmpty(severityStr) && Enum.TryParse<EntrySeverity>(severityStr, true, out var severity))
                {
                    if (severity != prescription.Severity)
                    {
                        prescription.Severity = severity;
                        fieldsUpdated.Add("severity");
                    }
                }

                // Validate after updates
                var errors = prescription.GetValidationErrors();
                if (errors.Any())
                {
                    return CommandResult.ValidationFailed(errors);
                }

                if (fieldsUpdated.Any())
                {
                    // Persist the changes to the repository
                    _documentRegistry.UpdateDocument(document);

                    return CommandResult.Ok(
                        $"Prescription entry updated successfully. Fields changed: {string.Join(", ", fieldsUpdated)}",
                        new {
                            DocumentId = documentId,
                            PrescriptionId = prescriptionId,
                            UpdatedFields = fieldsUpdated,
                            ModifiedAt = prescription.ModifiedAt,
                            UpdatedSig = prescription.GenerateSig()
                        });
                }
                else
                {
                    return CommandResult.Ok("No changes were made to the prescription entry", prescription);
                }
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to update prescription: {ex.Message}", ex);
            }
        }
    }
}