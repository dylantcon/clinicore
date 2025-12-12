using Core.CliniCore.Commands;
using Core.CliniCore.Service;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Users;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Repositories;

namespace Core.CliniCore.Commands.Profile
{
    /// <summary>
    /// Command that permanently deletes a user profile from the system.
    /// Performs dependency checks for clinical documents, appointments, and patient assignments
    /// before deletion. Supports force deletion with automatic cleanup of dependencies.
    /// </summary>
    /// <remarks>
    /// Creates a DeleteProfileCommand with the supplied parameters.
    /// </remarks>
    /// <param name="profileService"></param>
    /// <param name="schedulerService"></param>
    /// <param name="clinicalDocService"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public class DeleteProfileCommand(ProfileService profileService, SchedulerService schedulerService, ClinicalDocumentService clinicalDocService) : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "deleteprofile";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="DeleteProfileCommand"/>.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Parameter key for the unique identifier of the profile to delete.
            /// </summary>
            public const string ProfileId = "profileId";

            /// <summary>
            /// Parameter key indicating whether to skip dependency checks and force deletion.
            /// When true, all dependencies (documents, appointments, assignments) are cleaned up automatically.
            /// </summary>
            public const string Force = "force";
        }

        private readonly ProfileService _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
        private readonly ClinicalDocumentService _documentRegistry = clinicalDocService ?? throw new ArgumentNullException(nameof(clinicalDocService));
        private readonly SchedulerService _scheduleManager = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));

        /// <inheritdoc />
        public override string Description => "Permanently deletes a user profile from the system";

        /// <inheritdoc />
        public override bool CanUndo => false; // Deletions cannot be undone

        /// <inheritdoc />
        public override Permission? GetRequiredPermission()
            => Permission.DeletePatientProfile;

        /// <inheritdoc />
        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            var result = CommandValidationResult.Success();

            // Check required ProfileId parameter
            var missingParams = parameters.GetMissingRequired(Parameters.ProfileId);
            if (missingParams.Count != 0)
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

            var profile = _profileService.GetProfileById(profileId.Value);
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
                if (dependencies.Count != 0)
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

        /// <inheritdoc />
        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var profileId = parameters.GetRequiredParameter<Guid>(Parameters.ProfileId);
                var force = parameters.GetParameter<bool?>(Parameters.Force) ?? false;

                var profile = _profileService.GetProfileById(profileId);
                if (profile == null)
                {
                    return CommandResult.Fail("Profile not found");
                }

                // Store profile info for result message
                var profileName = profile.GetValue<string>(CommonEntryType.Name.GetKey()) ?? "Unknown";
                var profileRole = profile.Role;

                // if force requested and possible, clean up dependencies
                if (force)
                {
                    try
                    {
                        if (_profileService.CanForceDeleteProfile(profileId))
                            CleanupDependencies(profileId, profile);
                    }
                    catch (RepositoryOperationException e)
                    {
                        return CommandResult.Fail($"Failed to clean dependencies for force delete: {e.Message}", e);
                    }
                }

                // Remove the profile from registry
                var error = _profileService.RemoveProfile(profileId);
                if (error != null)
                {
                    return CommandResult.Fail($"Failed to remove profile: {error}");
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
                if (profile is PhysicianProfile physician && physician.PatientIds.Count != 0)
                {
                    dependencies.Add($"{physician.PatientIds.Count} assigned patient(s)");
                }
            }

            // Check for active appointments
            var patientAppointments = _scheduleManager.GetPatientAppointments(profileId)?.Where(a => a.Status == AppointmentStatus.Scheduled).ToList();
            if (patientAppointments?.Count != 0)
            {
                dependencies.Add($"{patientAppointments?.Count ?? -1} scheduled appointment(s)");
            }

            // If physician, check for appointments they're scheduled to provide
            if (profile.Role == UserRole.Physician)
            {
                var physicianAppointments = _scheduleManager.GetScheduleInRange(profileId, DateTime.Now, DateTime.Now.AddYears(1))
                    .Where(a => a.Status == AppointmentStatus.Scheduled).ToList();
                if (physicianAppointments.Count != 0)
                {
                    dependencies.Add($"{physicianAppointments.Count} physician appointment(s) to provide");
                }
            }

            return dependencies;
        }

        private void CleanupDependencies(Guid profileId, IUserProfile profile)
        {
            try
            {
                // Clean up clinical documents (unlink from appointments first)
                var documents = _documentRegistry.GetPatientDocuments(profileId)?.ToList();
                if (documents != null)
                {
                    foreach (var document in documents)
                    {
                        // Unlink from appointment before deleting
                        _scheduleManager.LinkClinicalDocument(document.AppointmentId, null);
                        _documentRegistry.RemoveDocument(document.Id);
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
                            // Unlink from appointment before deleting
                            _scheduleManager.LinkClinicalDocument(document.AppointmentId, null);
                            _documentRegistry.RemoveDocument(document.Id);
                        }
                    }

                    // Remove physician from all patients' primary physician assignments
                    if (profile is PhysicianProfile physician)
                    {
                        foreach (var patientId in physician.PatientIds.ToList())
                        {
                            var patient = _profileService.GetProfileById(patientId) as PatientProfile;
                            if (patient?.PrimaryPhysicianId == profileId)
                            {
                                patient.PrimaryPhysicianId = null;
                                _profileService.UpdateProfile(patient);
                            }
                        }
                        physician.PatientIds.Clear();
                        _profileService.UpdateProfile(physician);
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
            catch (RepositoryOperationException e)
            {
                throw new RepositoryOperationException("DeleteProfileCommand", "CleanupDependencies", profileId, "Failed to clean up dependencies for profile deletion", e);
            }
        }
    }
}
