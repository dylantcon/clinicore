// Core.CliniCore/Commands/Clinical/CreateClinicalDocumentCommand.cs - Complete updated version
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.ClinicalDoc;

namespace Core.CliniCore.Commands.Clinical
{
    public class CreateClinicalDocumentCommand : AbstractCommand
    {
        public const string Key = "createclinicaldocument";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string PatientId = "patient_id";
            public const string AppointmentId = "appointment_id";
            public const string ChiefComplaint = "chief_complaint";
            public const string InitialObservation = "initial_observation";
        }

        private readonly ClinicalDocumentRegistry _documentRegistry = ClinicalDocumentRegistry.Instance;
        private readonly ProfileRegistry _profileRegistry = ProfileRegistry.Instance;
        private ClinicalDocument? _createdDocument;

        // Constructor no longer needs dictionary parameter
        public CreateClinicalDocumentCommand()
        {
        }

        public override string Description => "Creates a new clinical document for a patient encounter";

        public override bool CanUndo => true;

        public override Permission? GetRequiredPermission()
            => Permission.CreateClinicalDocument;

        // ValidateParameters updated to use registry
        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Check required parameters
            var missingParams = parameters.GetMissingRequired(
                Parameters.PatientId, Parameters.AppointmentId, Parameters.ChiefComplaint);

            if (missingParams.Any())
            {
                foreach (var error in missingParams)
                    result.AddError(error);
                return result;
            }

            // Validate patient exists
            var patientId = parameters.GetParameter<Guid?>(Parameters.PatientId);
            if (!patientId.HasValue || patientId.Value == Guid.Empty)
            {
                result.AddError("Invalid patient ID");
            }
            else
            {
                var patient = _profileRegistry.GetProfileById(patientId.Value);
                if (patient == null || patient.Role != UserRole.Patient)
                {
                    result.AddError($"Patient with ID {patientId.Value} not found");
                }
            }

            // Validate chief complaint
            var chiefComplaint = parameters.GetParameter<string>(Parameters.ChiefComplaint);
            if (string.IsNullOrWhiteSpace(chiefComplaint))
            {
                result.AddError("Chief complaint cannot be empty");
            }
            else if (chiefComplaint.Length > 500)
            {
                result.AddError("Chief complaint cannot exceed 500 characters");
            }

            // Check for duplicate document for same appointment using registry
            var appointmentId = parameters.GetParameter<Guid?>(Parameters.AppointmentId);
            if (appointmentId.HasValue && _documentRegistry.AppointmentHasDocument(appointmentId.Value))
            {
                result.AddError($"Clinical document already exists for appointment {appointmentId.Value}");
            }

            return result;
        }

        // ValidateSpecific stays the same
        protected override CommandValidationResult ValidateSpecific(CommandParameters parameters, SessionContext? session)
        {
            var result = CommandValidationResult.Success();

            // Only physicians can create clinical documents
            if (session == null)
            {
                result.AddError("Must be logged in to create clinical documents");
            }
            else if (session.UserRole != UserRole.Physician && session.UserRole != UserRole.Administrator)
            {
                result.AddError("Only physicians can create clinical documents");
            }

            return result;
        }

        // ExecuteCore updated to use registry's AddDocument method
        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var patientId = parameters.GetRequiredParameter<Guid>(Parameters.PatientId);
                var appointmentId = parameters.GetRequiredParameter<Guid>(Parameters.AppointmentId);
                var chiefComplaint = parameters.GetRequiredParameter<string>(Parameters.ChiefComplaint);

                // Get physician ID from session
                var physicianId = session?.UserId ?? Guid.Empty;

                // Create the document
                _createdDocument = new ClinicalDocument(patientId, physicianId, appointmentId)
                {
                    ChiefComplaint = chiefComplaint
                };

                // Add any initial observations if provided
                var initialObservation = parameters.GetParameter<string>(Parameters.InitialObservation);
                if (!string.IsNullOrWhiteSpace(initialObservation))
                {
                    var observation = new ObservationEntry(physicianId, initialObservation)
                    {
                        Type = ObservationType.ChiefComplaint
                    };
                    _createdDocument.AddEntry(observation);
                }

                // Register the document using the registry
                if (!_documentRegistry.AddDocument(_createdDocument))
                {
                    return CommandResult.Fail(
                        "Failed to register clinical document. " +
                        "Appointment may already have a document or a system error occurred.");
                }

                // Get patient name for confirmation
                var patient = _profileRegistry.GetProfileById(patientId) as PatientProfile;

                return CommandResult.Ok(
                    $"Clinical document created successfully:\n" +
                    $"  Document ID: {_createdDocument.Id}\n" +
                    $"  Patient: {patient?.Name ?? "Unknown"}\n" +
                    $"  Chief Complaint: {chiefComplaint}\n" +
                    $"  Created: {_createdDocument.CreatedAt:yyyy-MM-dd HH:mm}",
                    _createdDocument);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to create clinical document: {ex.Message}", ex);
            }
        }

        // CaptureStateForUndo stays the same
        protected override object? CaptureStateForUndo(CommandParameters parameters, SessionContext? session)
        {
            return _createdDocument?.Id;
        }

        // UndoCore updated to use registry's RemoveDocument method
        protected override CommandResult UndoCore(object previousState, SessionContext? session)
        {
            if (previousState is Guid documentId)
            {
                if (_documentRegistry.RemoveDocument(documentId))
                {
                    return CommandResult.Ok($"Clinical document {documentId} has been removed");
                }
            }
            return CommandResult.Fail("Unable to undo document creation");
        }
    }
}