using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.CliniCore.Commands;
using Core.CliniCore.Service;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Services;

namespace Core.CliniCore.Commands.Profile
{
    public class DeleteProfileCommand : AbstractCommand
    {
        public const string Key = "deleteprofile";
        public override string CommandKey => Key;

        public static class Parameters
        {
            public const string ProfileId = "profileId";
            public const string Force = "force"; // Skip dependency checks if true
        }

        private readonly ProfileService _profileRegistry;
        private readonly ClinicalDocumentService _documentRegistry;
        private readonly SchedulerService _scheduleManager;

        public DeleteProfileCommand(ProfileService profileService, SchedulerService schedulerService, ClinicalDocumentService clinicalDocService)
        {
            _profileRegistry = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _scheduleManager = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
            _documentRegistry = clinicalDocService ?? throw new ArgumentNullException(nameof(clinicalDocService));
        }

        public override string Description => "Permanently deletes a user profile from the system";

        public override bool CanUndo => false; // Deletions cannot be undone

        public override Permission? GetRequiredPermission()
            => Permission.DeletePatientProfile;

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Check required ProfileId parameter
            var missingParams = parameters.GetMissingRequired(Parameters.ProfileId);
            if (missingParams.Any())
            {
                foreach (var error in missingParams)
                    result.AddError(error);
                return result;
            }

            // Validate profile exists
            var profileId = parameters.GetParameter<Guid?>(Parameters.ProfileId);
            if (!profileId.HasValue || profileId.Value == Guid.Empty)
            {
                result.AddError("Invalid profile ID");
                return result;
            }

            var profile = _profileRegistry.GetProfileById(profileId.Value);
            if (profile == null)
            {
                result.AddError($"Profile with ID {profileId.Value} not found");
                return result;
            }

            // Check for dependencies unless force delete is specified
            var force = parameters.GetParameter<bool?>(Parameters.Force) ?? false;
            if (!force)
            {
                var dependencies = CheckDependencies(profileId.Value, profile);
                if (dependencies.Any())
                {
                    foreach (var dependency in dependencies)
                    {
                        result.AddWarning($"Profile has active {dependency} - consider using force=true to override");
                    }
                    result.AddError("Cannot delete profile with active dependencies. Use force parameter to override.");
                }
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var profileId = parameters.GetRequiredParameter<Guid>(Parameters.ProfileId);
                var force = parameters.GetParameter<bool?>(Parameters.Force) ?? false;

                var profile = _profileRegistry.GetProfileById(profileId);
                if (profile == null)
                {
                    return CommandResult.Fail("Profile not found");
                }

                // Store profile info for result message
                var profileName = profile.GetValue<string>("name") ?? "Unknown";
                var profileRole = profile.Role;

                // If force delete, clean up dependencies first
                if (force)
                {
                    CleanupDependencies(profileId, profile);
                }

                // Remove the profile from registry
                var success = _profileRegistry.RemoveProfile(profileId);
                if (!success)
                {
                    return CommandResult.Fail("Failed to remove profile from registry");
                }

                return CommandResult.Ok(
                    $"{profileRole} profile '{profileName}' (ID: {profileId}) has been permanently deleted",
                    new { ProfileId = profileId, ProfileName = profileName, Role = profileRole });
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to delete profile: {ex.Message}", ex);
            }
        }

        private List<string> CheckDependencies(Guid profileId, IUserProfile profile)
        {
            var dependencies = new List<string>();

            // Check for clinical documents
            var documents = _documentRegistry.GetPatientDocuments(profileId);
            if (documents?.Any() == true)
            {
                dependencies.Add($"{documents.Count()} clinical document(s)");
            }

            // Check for authored documents (if physician)
            if (profile.Role == UserRole.Physician)
            {
                var authoredDocs = _documentRegistry.GetPhysicianDocuments(profileId);
                if (authoredDocs?.Any() == true)
                {
                    dependencies.Add($"{authoredDocs.Count()} authored clinical document(s)");
                }

                // Check for assigned patients
                if (profile is PhysicianProfile physician && physician.PatientIds.Any())
                {
                    dependencies.Add($"{physician.PatientIds.Count} assigned patient(s)");
                }
            }

            // Check for active appointments
            var patientAppointments = _scheduleManager.GetPatientAppointments(profileId)?.Where(a => a.Status == AppointmentStatus.Scheduled).ToList();
            if (patientAppointments?.Any() == true)
            {
                dependencies.Add($"{patientAppointments.Count} scheduled appointment(s)");
            }

            // If physician, check for appointments they're scheduled to provide
            if (profile.Role == UserRole.Physician)
            {
                var physicianAppointments = _scheduleManager.GetScheduleInRange(profileId, DateTime.Now, DateTime.Now.AddYears(1))
                    .Where(a => a.Status == AppointmentStatus.Scheduled).ToList();
                if (physicianAppointments.Any())
                {
                    dependencies.Add($"{physicianAppointments.Count} physician appointment(s) to provide");
                }
            }

            return dependencies;
        }

        private void CleanupDependencies(Guid profileId, IUserProfile profile)
        {
            // Clean up clinical documents
            var documents = _documentRegistry.GetPatientDocuments(profileId)?.ToList();
            if (documents != null)
            {
                foreach (var document in documents)
                {
                    _documentRegistry.RemoveDocument(document.Id); // Assuming this method exists
                }
            }

            // If physician, clean up authored documents and patient assignments
            if (profile.Role == UserRole.Physician)
            {
                var authoredDocs = _documentRegistry.GetPhysicianDocuments(profileId)?.ToList();
                if (authoredDocs != null)
                {
                    foreach (var document in authoredDocs)
                    {
                        _documentRegistry.RemoveDocument(document.Id);
                    }
                }

                // Remove physician from all patients' primary physician assignments
                if (profile is PhysicianProfile physician)
                {
                    foreach (var patientId in physician.PatientIds.ToList())
                    {
                        var patient = _profileRegistry.GetProfileById(patientId) as PatientProfile;
                        if (patient?.PrimaryPhysicianId == profileId)
                        {
                            patient.PrimaryPhysicianId = null;
                        }
                    }
                }
            }

            // Cancel active appointments for patient
            var patientAppointments = _scheduleManager.GetPatientAppointments(profileId)?.Where(a => a.Status == AppointmentStatus.Scheduled).ToList();
            if (patientAppointments != null)
            {
                foreach (var appointment in patientAppointments)
                {
                    _scheduleManager.CancelAppointment(appointment.PhysicianId, appointment.Id, $"Patient profile {profileId} deleted");
                }
            }

            // Cancel appointments physician was scheduled to provide
            if (profile.Role == UserRole.Physician)
            {
                var physicianAppointments = _scheduleManager.GetScheduleInRange(profileId, DateTime.Now, DateTime.Now.AddYears(1))
                    .Where(a => a.Status == AppointmentStatus.Scheduled).ToList();
                foreach (var appointment in physicianAppointments)
                {
                    _scheduleManager.CancelAppointment(profileId, appointment.Id, $"Physician profile {profileId} deleted");
                }
            }
        }
    }
}
