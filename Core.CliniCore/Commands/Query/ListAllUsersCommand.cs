using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.CliniCore.Commands.Query
{
    public class ListAllUsersCommand : AbstractCommand
    {
        public const string Key = "listallusers";
        public override string CommandKey => Key;

        public static class Parameters
        {
            // No parameters needed for this command
        }

        private readonly ProfileService _profileRegistry;

        public ListAllUsersCommand(ProfileService profileService)
        {
            _profileRegistry = profileService ?? throw new ArgumentNullException(nameof(profileService));
        }

        public override string Description => "Lists all users in the system (administrators, physicians, and patients)";

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

                foreach (var admin in administrators.OrderBy(a => a.Name))
                {
                    sb.AppendLine($"ID: {admin.Id:N}");
                    sb.AppendLine($"Name: {admin.Name}");
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

                foreach (var physician in physicians.OrderBy(p => p.Name))
                {
                    sb.AppendLine($"ID: {physician.Id:N}");
                    sb.AppendLine($"Name: Dr. {physician.Name}");
                    sb.AppendLine($"Username: {physician.Username}");
                    sb.AppendLine($"License Number: {physician.LicenseNumber}");

                    if (physician.Specializations.Any())
                    {
                        var specializationNames = string.Join(", ", physician.Specializations.Select(s => s.ToString()));
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

                foreach (var patient in patients.OrderBy(p => p.Name))
                {
                    sb.AppendLine($"ID: {patient.Id:N}");
                    sb.AppendLine($"Name: {patient.Name}");
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
                            sb.AppendLine($"Primary Physician: Dr. {primaryPhysician.Name}");
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