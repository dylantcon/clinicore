// Core.CliniCore/Commands/Profile/AssignPatientToPhysicianCommand.cs
using System;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;

namespace Core.CliniCore.Commands.Profile
{
    public class AssignPatientToPhysicianCommand : AbstractCommand
    {
        public const string Key = "assignpatienttophysician";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string PatientId = "patient_id";
            public const string PhysicianId = "physician_id";
            public const string SetPrimary = "set_primary";
        }

        private readonly ProfileRegistry _registry = ProfileRegistry.Instance;
        private Guid? _patientId;
        private Guid? _physicianId;
        private bool _wasPrimary;

        public override string Description => "Establishes or updates a physician-patient care relationship";

        public override bool CanUndo => true;

        public override Permission? GetRequiredPermission()
            => Permission.CreatePatientProfile; // Physicians and admins can assign patients

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Check required parameters
            var missingParams = parameters.GetMissingRequired(Parameters.PatientId, Parameters.PhysicianId);
            if (missingParams.Any())
            {
                foreach (var error in missingParams)
                    result.AddError(error);
                return result;
            }

            // Validate patient exists and is actually a patient
            var patientId = parameters.GetParameter<Guid?>(Parameters.PatientId);
            if (!patientId.HasValue || patientId.Value == Guid.Empty)
            {
                result.AddError("Invalid patient ID");
            }
            else
            {
                var patient = _registry.GetProfileById(patientId.Value);
                if (patient == null)
                {
                    result.AddError($"Patient with ID {patientId.Value} not found");
                }
                else if (patient.Role != UserRole.Patient)
                {
                    result.AddError($"Profile {patientId.Value} is not a patient");
                }
            }

            // Validate physician exists and is actually a physician
            var physicianId = parameters.GetParameter<Guid?>(Parameters.PhysicianId);
            if (!physicianId.HasValue || physicianId.Value == Guid.Empty)
            {
                result.AddError("Invalid physician ID");
            }
            else
            {
                var physician = _registry.GetProfileById(physicianId.Value);
                if (physician == null)
                {
                    result.AddError($"Physician with ID {physicianId.Value} not found");
                }
                else if (physician.Role != UserRole.Physician)
                {
                    result.AddError($"Profile {physicianId.Value} is not a physician");
                }
            }

            // Check if relationship already exists
            if (patientId.HasValue && physicianId.HasValue)
            {
                var physician = _registry.GetProfileById(physicianId.Value) as PhysicianProfile;
                if (physician?.PatientIds.Contains(patientId.Value) == true)
                {
                    result.AddWarning("Patient is already assigned to this physician");
                }
            }

            return result;
        }

        protected override CommandValidationResult ValidateSpecific(CommandParameters parameters, SessionContext? session)
        {
            var result = CommandValidationResult.Success();

            // Physicians can only assign patients to themselves
            if (session?.UserRole == UserRole.Physician)
            {
                var physicianId = parameters.GetParameter<Guid?>(Parameters.PhysicianId);
                if (physicianId.HasValue && physicianId.Value != session.UserId)
                {
                    result.AddError("Physicians can only assign patients to themselves");
                }
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                _patientId = parameters.GetRequiredParameter<Guid>(Parameters.PatientId);
                _physicianId = parameters.GetRequiredParameter<Guid>(Parameters.PhysicianId);
                var setPrimary = parameters.GetParameter<bool?>(Parameters.SetPrimary) ?? false;

                // Get profiles for display
                var patient = _registry.GetProfileById(_patientId.Value) as PatientProfile;
                var physician = _registry.GetProfileById(_physicianId.Value) as PhysicianProfile;

                if (patient == null || physician == null)
                {
                    return CommandResult.Fail("Failed to retrieve patient or physician profile");
                }

                // Store previous primary physician for undo
                _wasPrimary = patient.PrimaryPhysicianId == _physicianId;

                // Establish the relationship
                if (!_registry.AssignPatientToPhysician(_patientId.Value, _physicianId.Value, setPrimary))
                {
                    return CommandResult.Fail("Failed to establish physician-patient relationship");
                }

                // Build success message
                var message = $"Patient '{patient.Name}' assigned to Dr. {physician.Name}";
                if (setPrimary)
                {
                    message += " as PRIMARY physician";
                }

                // Include statistics
                var patientCount = physician.PatientIds.Count;
                message += $"\nDr. {physician.Name} now has {patientCount} patient(s) under care";

                return CommandResult.Ok(message, new { PatientId = _patientId, PhysicianId = _physicianId });
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to assign patient to physician: {ex.Message}", ex);
            }
        }

        protected override object? CaptureStateForUndo(CommandParameters parameters, SessionContext? session)
        {
            return new UndoState
            {
                PatientId = _patientId ?? Guid.Empty,
                PhysicianId = _physicianId ?? Guid.Empty,
                WasPrimary = _wasPrimary
            };
        }

        protected override CommandResult UndoCore(object previousState, SessionContext? session)
        {
            if (previousState is UndoState state)
            {
                var patient = _registry.GetProfileById(state.PatientId) as PatientProfile;
                var physician = _registry.GetProfileById(state.PhysicianId) as PhysicianProfile;

                if (patient != null && physician != null)
                {
                    // Remove patient from physician's list
                    physician.PatientIds.Remove(state.PatientId);

                    // Clear primary physician if it was set
                    if (patient.PrimaryPhysicianId == state.PhysicianId)
                    {
                        patient.PrimaryPhysicianId = null;
                    }

                    return CommandResult.Ok(
                        $"Physician-patient relationship between Dr. {physician.Name} and {patient.Name} has been removed");
                }
            }
            return CommandResult.Fail("Unable to undo physician-patient assignment");
        }

        private class UndoState
        {
            public Guid PatientId { get; set; }
            public Guid PhysicianId { get; set; }
            public bool WasPrimary { get; set; }
        }
    }
}
