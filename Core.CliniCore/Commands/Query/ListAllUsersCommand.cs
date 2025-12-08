using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.CliniCore.Commands.Query
{
    /// <summary>
    /// Command that retrieves and displays all users in the system, grouped by role.
    /// Administrator-only command that provides a comprehensive system user roster
    /// including administrators, physicians, and patients.
    /// </summary>
    public class ListAllUsersCommand : AbstractCommand
    {
        /// <summary>
        /// The unique key used to identify this command.
        /// </summary>
        public const string Key = "listallusers";

        /// <inheritdoc />
        public override string CommandKey => Key;

        /// <summary>
        /// Defines the parameter keys used by <see cref="ListAllUsersCommand"/>.
        /// This command requires no parameters.
        /// </summary>
        public static class Parameters
        {
            // No parameters needed for this command
        }

        private readonly ProfileService _profileRegistry;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListAllUsersCommand"/> class.
        /// </summary>
        /// <param name="profileService">The profile service for accessing user profiles.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="profileService"/> is <see langword="null"/>.</exception>
        public ListAllUsersCommand(ProfileService profileService)
        {
            _profileRegistry = profileService ?? throw new ArgumentNullException(nameof(profileService));
        }

        /// <inheritdoc />
        public override string Description => "Lists all users in the system (administrators, physicians, and patients)";

        /// <inheritdoc />
        public override Permission? GetRequiredPermission()
            => Permission.ViewAllPatients; // Reusing existing permission - only admins have this

        protected override CommandValidationResult ValidateParameters(CommandParameters parameters)
        {
            // No parameters needed for listing all users
            return CommandValidationResult.Success();
        }

        protected override CommandValidationResult ValidateSpecific(CommandParameters parameters, SessionContext? session)
        {
            var result = CommandValidationResult.Success();

            if (session == null)
            {
                result.AddError("Must be logged in to list users");
                return result;
            }

            // Only administrators can list all users
            if (session.UserRole != UserRole.Administrator)
            {
                result.AddError("Only administrators can list all users");
            }

            return result;
        }

        protected override CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session)
        {
            try
            {
                var allUsers = new List<IUserProfile>();

                // Gather all user types
                var administrators = _profileRegistry.GetAllAdministrators();
                var physicians = _profileRegistry.GetAllPhysicians();
                var patients = _profileRegistry.GetAllPatients();

                allUsers.AddRange(administrators);
                allUsers.AddRange(physicians);
                allUsers.AddRange(patients);

                if (!allUsers.Any())
                {
                    return CommandResult.Ok("No users found in the system");
                }

                var output = FormatUserList(administrators, physicians, patients);
                return CommandResult.Ok(output, allUsers);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to list users: {ex.Message}", ex);
            }
        }

        private string FormatUserList(
            IEnumerable<AdministratorProfile> administrators,
            IEnumerable<PhysicianProfile> physicians,
            IEnumerable<PatientProfile> patients)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== ALL SYSTEM USERS ===");
            sb.AppendLine();

            // List Administrators
            if (administrators.Any())
            {
                sb.AppendLine("--- ADMINISTRATORS ---");
                sb.AppendLine($"Total: {administrators.Count()}");
                sb.AppendLine();

                foreach (var admin in administrators.OrderBy(a => a.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty))
                {
                    sb.AppendLine($"ID: {admin.Id:N}");
                    sb.AppendLine($"Name: {admin.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty}");
                    sb.AppendLine($"Username: {admin.Username}");

                    var email = admin.GetValue<string>("email");
                    if (!string.IsNullOrEmpty(email))
                    {
                        sb.AppendLine($"Email: {email}");
                    }

                    sb.AppendLine();
                }
            }

            // List Physicians
            if (physicians.Any())
            {
                sb.AppendLine("--- PHYSICIANS ---");
                sb.AppendLine($"Total: {physicians.Count()}");
                sb.AppendLine();

                foreach (var physician in physicians.OrderBy(p => p.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty))
                {
                    sb.AppendLine($"ID: {physician.Id:N}");
                    sb.AppendLine($"Name: Dr. {physician.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty}");
                    sb.AppendLine($"Username: {physician.Username}");
                    sb.AppendLine($"License Number: {physician.GetValue<string>(PhysicianEntryType.LicenseNumber.GetKey()) ?? string.Empty}");

                    var specializations = physician.GetValue<List<MedicalSpecialization>>(PhysicianEntryType.Specializations.GetKey()) ?? new List<MedicalSpecialization>();
                    if (specializations.Any())
                    {
                        var specializationNames = string.Join(", ", specializations.Select(s => s.ToString()));
                        sb.AppendLine($"Specializations: {specializationNames}");
                    }

                    sb.AppendLine($"Patients Under Care: {physician.PatientIds.Count}");
                    sb.AppendLine();
                }
            }

            // List Patients
            if (patients.Any())
            {
                sb.AppendLine("--- PATIENTS ---");
                sb.AppendLine($"Total: {patients.Count()}");
                sb.AppendLine();

                foreach (var patient in patients.OrderBy(p => p.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty))
                {
                    sb.AppendLine($"ID: {patient.Id:N}");
                    sb.AppendLine($"Name: {patient.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty}");
                    sb.AppendLine($"Username: {patient.Username}");

                    var birthDate = patient.GetValue<DateTime>("birthdate");
                    if (birthDate != default)
                    {
                        var age = DateTime.Now.Year - birthDate.Year;
                        if (DateTime.Now.DayOfYear < birthDate.DayOfYear) age--;
                        sb.AppendLine($"Age: {age} (DOB: {birthDate:yyyy-MM-dd})");
                    }

                    if (patient.PrimaryPhysicianId.HasValue && patient.PrimaryPhysicianId.Value != Guid.Empty)
                    {
                        var primaryPhysician = _profileRegistry.GetProfileById(patient.PrimaryPhysicianId.Value) as PhysicianProfile;
                        if (primaryPhysician != null)
                        {
                            sb.AppendLine($"Primary Physician: Dr. {primaryPhysician.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty}");
                        }
                    }

                    sb.AppendLine();
                }
            }

            // Summary
            sb.AppendLine("--- SUMMARY ---");
            sb.AppendLine($"Total Users: {administrators.Count() + physicians.Count() + patients.Count()}");
            sb.AppendLine($"  Administrators: {administrators.Count()}");
            sb.AppendLine($"  Physicians: {physicians.Count()}");
            sb.AppendLine($"  Patients: {patients.Count()}");

            return sb.ToString();
        }
    }
}