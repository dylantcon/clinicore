// Core.CliniCore/Commands/Clinical/AddPrescriptionCommand.cs
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.ClinicalDoc;
using Core.CliniCore.Service;

namespace Core.CliniCore.Commands.Clinical
{
    public class AddPrescriptionCommand : AbstractCommand
    {
        public const string Key = "addprescription";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string DocumentId = "document_id";
            public const string DiagnosisId = "diagnosis_id";
            public const string MedicationName = "medication_name";
            public const string Dosage = "dosage";
            public const string Frequency = "frequency";
            public const string Route = "route";
            public const string Duration = "duration";
            public const string Refills = "refills";
            public const string GenericAllowed = "generic_allowed";
            public const string DeaSchedule = "dea_schedule";
            public const string Instructions = "instructions";
            public const string NdcCode = "ndc_code";
        }

        private readonly ClinicalDocumentService _documentRegistry;
        private PrescriptionEntry? _addedPrescription;
        private Guid? _targetDocumentId;

        public AddPrescriptionCommand(ClinicalDocumentService clinicalDocService)
        {
            _documentRegistry = clinicalDocService ?? throw new ArgumentNullException(nameof(clinicalDocService));
        }

        public override string Description => "Adds a prescription to a clinical document (requires diagnosis)";

        public override bool CanUndo => true;

        public override Permission? GetRequiredPermission()
            => Permission.CreateClinicalDocument;

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Check required parameters
            var missingParams = parameters.GetMissingRequired(
                Parameters.DocumentId, Parameters.DiagnosisId, Parameters.MedicationName,
                Parameters.Dosage, Parameters.Frequency);

            if (missingParams.Any())
            {
                foreach (var error in missingParams)
                    result.AddError(error);
                return result;
            }

            // Validate document exists and has the diagnosis, using registry
            var documentId = parameters.GetParameter<Guid?>(Parameters.DocumentId);
            var diagnosisId = parameters.GetParameter<Guid?>(Parameters.DiagnosisId);
            if (!documentId.HasValue || documentId.Value == Guid.Empty)
            {
                result.AddError($"Clinical document {documentId} not found");
                return result;
            }
            
            var document = _documentRegistry.GetDocumentById(documentId.Value);
            if (document == null)
            {
                result.AddError($"Clinical document {documentId.Value} not found");
            }
            else 
            {
                if (document.IsCompleted)
                {
                    result.AddError("Cannot modify completed clinical document");
                }

                // CRITICAL: Validate diagnosis exists in this document
                if (diagnosisId.HasValue)
                {
                    var diagnosisExists = document.GetDiagnoses()
                        .Any(d => d.Id == diagnosisId.Value && d.IsActive);

                    if (!diagnosisExists)
                    {
                        result.AddError(
                            $"Diagnosis {diagnosisId} not found in document. " +
                            "Prescriptions must be linked to an existing diagnosis.");

                    }
                }
            }

            // Validate medication name
            var medicationName = parameters.GetParameter<string>(Parameters.MedicationName);
            if (string.IsNullOrWhiteSpace(medicationName))
            {
                result.AddError("Medication name cannot be empty");
            }

            // Validate DEA schedule if provided
            var deaSchedule = parameters.GetParameter<int?>(Parameters.DeaSchedule);
            if (deaSchedule.HasValue && (deaSchedule < 1 || deaSchedule > 5))
            {
                result.AddError("DEA Schedule must be between 1 and 5");
            }

            // Validate refills
            var refills = parameters.GetParameter<int?>(Parameters.Refills);
            if (refills.HasValue && refills < 0)
            {
                result.AddError("Refills cannot be negative");
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var documentId = parameters.GetRequiredParameter<Guid>(Parameters.DocumentId);
                var diagnosisId = parameters.GetRequiredParameter<Guid>(Parameters.DiagnosisId);
                var medicationName = parameters.GetRequiredParameter<string>(Parameters.MedicationName);
                var physicianId = session?.UserId ?? Guid.Empty;

                _targetDocumentId = documentId;
                var document = _documentRegistry.GetDocumentById(documentId);
                if (document == null)
                {
                    return CommandResult.Fail($"Clinical document {documentId} not found");
                }

                // Create the prescription
                _addedPrescription = new PrescriptionEntry(physicianId, diagnosisId, medicationName)
                {
                    Dosage = parameters.GetRequiredParameter<string>(Parameters.Dosage),
                    Frequency = parameters.GetRequiredParameter<string>(Parameters.Frequency),
                    Route = parameters.GetParameter<string>(Parameters.Route) ?? "Oral",
                    Duration = parameters.GetParameter<string>(Parameters.Duration),
                    Refills = parameters.GetParameter<int?>(Parameters.Refills) ?? 0,
                    GenericAllowed = parameters.GetParameter<bool?>(Parameters.GenericAllowed) ?? true,
                    DEASchedule = parameters.GetParameter<int?>(Parameters.DeaSchedule),
                    Instructions = parameters.GetParameter<string>(Parameters.Instructions),
                    NDCCode = parameters.GetParameter<string>(Parameters.NdcCode)
                };

                // Set expiration if controlled substance
                if (_addedPrescription.DEASchedule.HasValue)
                {
                    // Controlled substances typically expire in 6 months
                    _addedPrescription.ExpirationDate = DateTime.Now.AddMonths(6);
                }

                // Add to document (will validate diagnosis exists)
                document.AddEntry(_addedPrescription);

                // Get the diagnosis for display
                var diagnosis = document.GetDiagnoses().First(d => d.Id == diagnosisId);

                var deaStr = _addedPrescription.DEASchedule.HasValue
                    ? $" [CONTROLLED - Schedule {_addedPrescription.DEASchedule}]"
                    : "";

                return CommandResult.Ok(
                    $"Prescription added successfully:\n" +
                    $"  Medication: {medicationName}{deaStr}\n" +
                    $"  Sig: {_addedPrescription.GenerateSig()}\n" +
                    $"  For Diagnosis: {diagnosis.Content}\n" +
                    $"  Refills: {_addedPrescription.Refills}\n" +
                    $"  Generic OK: {(_addedPrescription.GenericAllowed ? "Yes" : "No")}\n" +
                    $"  Prescription ID: {_addedPrescription.Id}",
                    _addedPrescription);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("without diagnosis"))
            {
                // This shouldn't happen due to validation, but handle gracefully
                return CommandResult.Fail(
                    "Cannot add prescription: The specified diagnosis was not found in the document. " +
                    "Please add the diagnosis first.");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to add prescription: {ex.Message}", ex);
            }
        }

        protected override object? CaptureStateForUndo(CommandParameters parameters, SessionContext? session)
        {
            return new UndoState
            {
                DocumentId = _targetDocumentId ?? Guid.Empty,
                PrescriptionId = _addedPrescription?.Id ?? Guid.Empty
            };
        }

        protected override CommandResult UndoCore(object previousState, SessionContext? session)
        {
            if (previousState is UndoState state)
            {
                var document = _documentRegistry.GetDocumentById(state.DocumentId);
                if (document != null)
                {
                    var prescription = document.GetPrescriptions()
                        .FirstOrDefault(p => p.Id == state.PrescriptionId);

                    if (prescription != null)
                    {
                        prescription.IsActive = false;
                        return CommandResult.Ok($"Prescription {state.PrescriptionId} has been marked inactive");
                    }
                }
            }
            return CommandResult.Fail("Unable to undo prescription addition");
        }

        private class UndoState
        {
            public Guid DocumentId { get; set; }
            public Guid PrescriptionId { get; set; }
        }
    }
}
