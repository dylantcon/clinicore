using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Authentication;
using Core.CliniCore.Commands.Profile;
using Core.CliniCore.Commands.Scheduling;
using Core.CliniCore.Commands.Clinical;
using Core.CliniCore.Commands.Query;
using Core.CliniCore.Commands.Reports;
using Core.CliniCore.Commands.Admin;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Services;
using Core.CliniCore.Service;

namespace CLI.CliniCore.Service
{
    public class ConsoleCommandParser(IConsoleEngine console, ProfileService profileService, SchedulerService schedulerService, ClinicalDocumentService clinicalDocService)
    {
        private readonly IConsoleEngine _console = console ?? throw new ArgumentNullException(nameof(console));
        private readonly ProfileService _profileRegistry = profileService ?? throw new ArgumentNullException(nameof(profileService));
        private readonly SchedulerService _scheduleManager = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
        private readonly ClinicalDocumentService _clinicalDocumentRegistry = clinicalDocService ?? throw new ArgumentNullException(nameof(clinicalDocService));

        public CommandParameters ParseInteractive(ICommand command)
        {
            var parameters = new CommandParameters();
            
            // Use the command's CommandKey property for consistent identification
            var commandKey = command.CommandKey.ToLowerInvariant();
            
            // Parse parameters based on the command key
            switch (commandKey)
            {
                case LoginCommand.Key:
                    parameters[LoginCommand.Parameters.Username] = GetStringInput("Username");
                    parameters[LoginCommand.Parameters.Password] = GetSecureInput("Password");
                    break;

                case ChangePasswordCommand.Key:
                    parameters[ChangePasswordCommand.Parameters.OldPassword] = GetSecureInput("Current Password");
                    parameters[ChangePasswordCommand.Parameters.NewPassword] = GetSecureInput("New Password");
                    parameters[ChangePasswordCommand.Parameters.ConfirmPassword] = GetSecureInput("Confirm New Password");
                    break;

                case CreatePatientCommand.Key:
                    parameters[CreatePatientCommand.Parameters.Username] = GetStringInput("Username");
                    parameters[CreatePatientCommand.Parameters.Password] = GetSecureInput("Password");
                    parameters[CreatePatientCommand.Parameters.Name] = GetStringInput("Full Name");
                    parameters[CreatePatientCommand.Parameters.Address] = GetStringInput("Address");
                    parameters[CreatePatientCommand.Parameters.Birthdate] = GetDateInput("Birth Date");
                    parameters[CreatePatientCommand.Parameters.Gender] = GetGenderInput();
                    parameters[CreatePatientCommand.Parameters.Race] = GetRaceInput();
                    break;

                case CreatePhysicianCommand.Key:
                    parameters[CreatePhysicianCommand.Parameters.Username] = GetStringInput("Username");
                    parameters[CreatePhysicianCommand.Parameters.Password] = GetSecureInput("Password");
                    parameters[CreatePhysicianCommand.Parameters.Name] = GetStringInput("Full Name");
                    parameters[CreatePhysicianCommand.Parameters.Address] = GetStringInput("Address");
                    parameters[CreatePhysicianCommand.Parameters.Birthdate] = GetDateInput("Birth Date");
                    parameters[CreatePhysicianCommand.Parameters.LicenseNumber] = GetStringInput("License Number");
                    parameters[CreatePhysicianCommand.Parameters.GraduationDate] = GetDateInput("Graduation Date");
                    parameters[CreatePhysicianCommand.Parameters.Specializations] = GetSpecializationListInput();
                    break;

                case CreateAdministratorCommand.Key:
                    parameters[CreateAdministratorCommand.Parameters.Username] = GetStringInput("Username");
                    parameters[CreateAdministratorCommand.Parameters.Password] = GetSecureInput("Password");
                    parameters[CreateAdministratorCommand.Parameters.Name] = GetStringInput("Full Name");
                    parameters[CreateAdministratorCommand.Parameters.Address] = GetOptionalStringInput("Address (optional)");
                    parameters[CreateAdministratorCommand.Parameters.BirthDate] = GetOptionalDateInput("Birth Date (optional)");
                    parameters[CreateAdministratorCommand.Parameters.Email] = GetOptionalStringInput("Email (optional)");
                    break;

                case ScheduleAppointmentCommand.Key:
                    parameters[ScheduleAppointmentCommand.Parameters.PatientId] = GetProfileSelection(UserRole.Patient);
                    parameters[ScheduleAppointmentCommand.Parameters.PhysicianId] = GetProfileSelection(UserRole.Physician);
                    parameters[ScheduleAppointmentCommand.Parameters.StartTime] = GetDateTimeInput("Appointment Date and Time");
                    parameters[ScheduleAppointmentCommand.Parameters.DurationMinutes] = GetIntInput("Duration (minutes)", 30, 15, 240);
                    parameters[ScheduleAppointmentCommand.Parameters.Notes] = GetOptionalStringInput("Notes");
                    break;

                case CreateClinicalDocumentCommand.Key:
                    var selectedPatientId = GetProfileSelection(UserRole.Patient);
                    parameters[CreateClinicalDocumentCommand.Parameters.PatientId] = selectedPatientId;
                    parameters[CreateClinicalDocumentCommand.Parameters.AppointmentId] = GetAppointmentSelection(selectedPatientId);
                    parameters[CreateClinicalDocumentCommand.Parameters.ChiefComplaint] = GetStringInput("Chief Complaint");
                    parameters[CreateClinicalDocumentCommand.Parameters.InitialObservation] = GetOptionalStringInput("Initial Observation");
                    break;

                case AddObservationCommand.Key:
                    parameters[AddObservationCommand.Parameters.DocumentId] = GetClinicalDocumentSelection();
                    parameters[AddObservationCommand.Parameters.Observation] = GetStringInput("Observation");
                    break;

                case AddDiagnosisCommand.Key:
                    parameters[AddDiagnosisCommand.Parameters.DocumentId] = GetClinicalDocumentSelection();
                    parameters[AddDiagnosisCommand.Parameters.ICD10Code] = GetStringInput("Diagnosis Code (ICD-10)");
                    parameters[AddDiagnosisCommand.Parameters.DiagnosisDescription] = GetStringInput("Diagnosis Description");
                    break;

                case AddPrescriptionCommand.Key:
                    parameters[AddPrescriptionCommand.Parameters.DocumentId] = GetClinicalDocumentSelection();
                    parameters[AddPrescriptionCommand.Parameters.DiagnosisId] = GetGuidInput("Associated Diagnosis ID");
                    parameters[AddPrescriptionCommand.Parameters.MedicationName] = GetStringInput("Medication Name");
                    parameters[AddPrescriptionCommand.Parameters.Dosage] = GetStringInput("Dosage");
                    parameters[AddPrescriptionCommand.Parameters.Frequency] = GetStringInput("Frequency");
                    parameters[AddPrescriptionCommand.Parameters.Duration] = GetStringInput("Duration");
                    break;

                case AddAssessmentCommand.Key:
                    parameters[AddAssessmentCommand.Parameters.DocumentId] = GetClinicalDocumentSelection();
                    parameters[AddAssessmentCommand.Parameters.ClinicalImpression] = GetStringInput("Clinical Assessment");
                    break;

                case AddPlanCommand.Key:
                    parameters[AddPlanCommand.Parameters.DocumentId] = GetClinicalDocumentSelection();
                    parameters[AddPlanCommand.Parameters.PlanDescription] = GetStringInput("Plan");
                    break;

                case ViewProfileCommand.Key:
                    parameters[ViewProfileCommand.Parameters.ProfileId] = GetProfileSelection();
                    break;

                case ViewPatientProfileCommand.Key:
                    parameters[ViewPatientProfileCommand.Parameters.ProfileId] = GetProfileSelection(UserRole.Patient);
                    break;

                case ViewPhysicianProfileCommand.Key:
                    parameters[ViewPhysicianProfileCommand.Parameters.ProfileId] = GetProfileSelection(UserRole.Physician);
                    break;

                case ViewAdministratorProfileCommand.Key:
                    parameters[ViewAdministratorProfileCommand.Parameters.ProfileId] = GetProfileSelection(UserRole.Administrator);
                    break;

                case ListProfileCommand.Key:
                    parameters[ListProfileCommand.Parameters.IncludeInvalid] = GetBoolInput("Include invalid profiles?");
                    break;

                case DeleteProfileCommand.Key:
                    parameters[DeleteProfileCommand.Parameters.ProfileId] = GetProfileSelection(null);
                    parameters[DeleteProfileCommand.Parameters.Force] = GetBoolInput("Force deletion (remove all dependencies)?");
                    break;

                case UpdateProfileCommand.Key:
                    parameters[UpdateProfileCommand.Parameters.ProfileId] = GetProfileSelection();
                    // Get profile type-specific fields
                    PromptForProfileUpdateFields(parameters);
                    break;

                case UpdatePatientProfileCommand.Key:
                    parameters[UpdateProfileCommand.Parameters.ProfileId] = GetProfileSelection(UserRole.Patient);
                    // Patient-specific fields
                    parameters[UpdateProfileCommand.Parameters.Name] = GetOptionalStringInput("New Name (leave blank to keep current)");
                    parameters[UpdateProfileCommand.Parameters.Address] = GetOptionalStringInput("New Address (leave blank to keep current)");
                    parameters[UpdateProfileCommand.Parameters.BirthDate] = GetOptionalDateInput("New Birth Date (leave blank to keep current)");
                    parameters[UpdateProfileCommand.Parameters.Gender] = GetOptionalGenderInput();
                    parameters[UpdateProfileCommand.Parameters.Race] = GetOptionalStringInput("New Race (leave blank to keep current)");
                    break;

                case UpdatePhysicianProfileCommand.Key:
                    parameters[UpdateProfileCommand.Parameters.ProfileId] = GetProfileSelection(UserRole.Physician);
                    // Physician-specific fields
                    parameters[UpdateProfileCommand.Parameters.Name] = GetOptionalStringInput("New Name (leave blank to keep current)");
                    parameters[UpdateProfileCommand.Parameters.Address] = GetOptionalStringInput("New Address (leave blank to keep current)");
                    parameters[UpdateProfileCommand.Parameters.LicenseNumber] = GetOptionalStringInput("New License Number (leave blank to keep current)");
                    break;

                case ViewAppointmentCommand.Key:
                    parameters[ViewAppointmentCommand.Parameters.AppointmentId] = GetGuidInput("Appointment ID");
                    break;

                case CancelAppointmentCommand.Key:
                    parameters[CancelAppointmentCommand.Parameters.AppointmentId] = GetGuidInput("Appointment ID");
                    break;


                case ViewClinicalDocumentCommand.Key:
                    parameters[ViewClinicalDocumentCommand.Parameters.DocumentId] = GetClinicalDocumentSelection();
                    break;

                case UpdateClinicalDocumentCommand.Key:
                    parameters[UpdateClinicalDocumentCommand.Parameters.DocumentId] = GetClinicalDocumentSelection();
                    break;

                case SetPhysicianAvailabilityCommand.Key:
                    parameters[SetPhysicianAvailabilityCommand.Parameters.PhysicianId] = GetProfileSelection(UserRole.Physician);
                    parameters[SetPhysicianAvailabilityCommand.Parameters.DayOfWeek] = GetDayOfWeekInput();
                    parameters[SetPhysicianAvailabilityCommand.Parameters.StartTime] = GetTimeInput("Start Time");
                    parameters[SetPhysicianAvailabilityCommand.Parameters.EndTime] = GetTimeInput("End Time");
                    break;

                case FindPhysiciansBySpecializationCommand.Key:
                    parameters[FindPhysiciansBySpecializationCommand.Parameters.Specialization] = GetSpecializationInput();
                    break;

                case FindPhysiciansByAvailabilityCommand.Key:
                    parameters[FindPhysiciansByAvailabilityCommand.Parameters.Date] = GetDateTimeInput("Date and Time");
                    break;

                case SearchPatientsCommand.Key:
                    parameters[SearchPatientsCommand.Parameters.SearchTerm] = GetStringInput("Search Term");
                    break;

                case SearchClinicalNotesCommand.Key:
                    parameters[SearchClinicalNotesCommand.Parameters.SearchTerm] = GetStringInput("Search Term");
                    parameters[SearchClinicalNotesCommand.Parameters.PatientId] = GetOptionalGuidInput("Patient ID (optional)");
                    break;

                case AssignPatientToPhysicianCommand.Key:
                    parameters[AssignPatientToPhysicianCommand.Parameters.PatientId] = GetProfileSelection(UserRole.Patient);
                    parameters[AssignPatientToPhysicianCommand.Parameters.PhysicianId] = GetProfileSelection(UserRole.Physician);
                    parameters[AssignPatientToPhysicianCommand.Parameters.SetPrimary] = GetBoolInput("Set as primary care physician?");
                    break;

                case GetScheduleCommand.Key:
                    parameters[GetScheduleCommand.Parameters.PhysicianId] = GetProfileSelection(UserRole.Physician);
                    parameters[GetScheduleCommand.Parameters.StartDate] = GetDateInput("Start Date");
                    parameters[GetScheduleCommand.Parameters.EndDate] = GetDateInput("End Date");
                    break;

                case GetAvailableTimeSlotsCommand.Key:
                    parameters[GetAvailableTimeSlotsCommand.Parameters.PhysicianId] = GetProfileSelection(UserRole.Physician);
                    parameters[GetAvailableTimeSlotsCommand.Parameters.Date] = GetDateInput("Date");
                    parameters[GetAvailableTimeSlotsCommand.Parameters.DurationMinutes] = GetIntInput("Duration (minutes)", 30, 15, 240);
                    break;

                case CheckConflictsCommand.Key:
                    parameters[CheckConflictsCommand.Parameters.PhysicianId] = GetProfileSelection(UserRole.Physician);
                    parameters[CheckConflictsCommand.Parameters.StartTime] = GetDateTimeInput("Date and Time");
                    parameters[CheckConflictsCommand.Parameters.DurationMinutes] = GetIntInput("Duration (minutes)", 30, 15, 240);
                    break;

                case GeneratePatientReportCommand.Key:
                    parameters[GeneratePatientReportCommand.Parameters.PatientId] = GetProfileSelection(UserRole.Patient);
                    parameters[GeneratePatientReportCommand.Parameters.StartDate] = GetOptionalDateInput("Start Date (optional)");
                    parameters[GeneratePatientReportCommand.Parameters.EndDate] = GetOptionalDateInput("End Date (optional)");
                    break;

                case GeneratePhysicianReportCommand.Key:
                    parameters[GeneratePhysicianReportCommand.Parameters.PhysicianId] = GetProfileSelection(UserRole.Physician);
                    parameters[GeneratePatientReportCommand.Parameters.StartDate] = GetOptionalDateInput("Start Date (optional)");
                    parameters[GeneratePatientReportCommand.Parameters.EndDate] = GetOptionalDateInput("End Date (optional)");
                    break;

                case GenerateAppointmentReportCommand.Key:
                    parameters[GenerateAppointmentReportCommand.Parameters.StartDate] = GetDateInput("Start Date");
                    parameters[GenerateAppointmentReportCommand.Parameters.EndDate] = GetDateInput("End Date");
                    break;

                case GenerateFacilityReportCommand.Key:
                    parameters[GenerateFacilityReportCommand.Parameters.StartDate] = GetDateInput("Start Date");
                    parameters[GenerateFacilityReportCommand.Parameters.EndDate] = GetDateInput("End Date");
                    parameters[GenerateFacilityReportCommand.Parameters.ReportType] = GetReportTypeInput();
                    break;

                case CreateFacilityCommand.Key:
                    parameters[CreateFacilityCommand.Parameters.FacilityName] = GetStringInput("Facility Name");
                    parameters[CreateFacilityCommand.Parameters.FacilityAddress] = GetStringInput("Facility Address");
                    break;

                case UpdateFacilitySettingsCommand.Key:
                    parameters[UpdateFacilitySettingsCommand.Parameters.FacilityId] = GetGuidInput("Facility ID");
                    parameters[UpdateFacilitySettingsCommand.Parameters.SettingName] = GetOptionalStringInput("Setting Name (leave blank to keep current)");
                    parameters[UpdateFacilitySettingsCommand.Parameters.SettingValue] = GetOptionalStringInput("Setting Value (leave blank to keep current)");
                    break;

                case ManageUserRolesCommand.Key:
                    parameters[ManageUserRolesCommand.Parameters.UserId] = GetProfileSelection();
                    parameters[ManageUserRolesCommand.Parameters.UserRole] = GetUserRoleInput();
                    break;

                case ListPatientsCommand.Key:
                case ListPhysiciansCommand.Key:
                case ListAppointmentsCommand.Key:
                case ListClinicalDocumentsCommand.Key:
                case LogoutCommand.Key:
                case ViewAuditLogCommand.Key:
                case SystemMaintenanceCommand.Key:
                    // These commands don't require interactive parameters
                    break;

                default:
                    // For unknown commands, return empty parameters
                    _console.DisplayMessage($"No interactive parameters defined for command: {commandKey}", MessageType.Warning);
                    break;
            }
            
            return parameters;
        }

        private string GetStringInput(string prompt)
        {
            string? input;
            do
            {
                input = _console.GetUserInput($"{prompt}: ");
                if (input == null) // User pressed Escape
                {
                    throw new UserInputCancelledException($"User cancelled input for: {prompt}");
                }
                if (string.IsNullOrWhiteSpace(input))
                {
                    _console.DisplayMessage("This field is required.", MessageType.Warning);
                }
            } while (string.IsNullOrWhiteSpace(input));
            
            return input;
        }

        private string? GetOptionalStringInput(string prompt)
        {
            var input = _console.GetUserInput($"{prompt}: ");
            return string.IsNullOrWhiteSpace(input) ? null : input;
        }

        private string GetSecureInput(string prompt)
        {
            string? input;
            do
            {
                input = _console.GetSecureInput($"{prompt}: ");
                if (string.IsNullOrWhiteSpace(input))
                {
                    _console.DisplayMessage("This field is required.", MessageType.Warning);
                }
            } while (string.IsNullOrWhiteSpace(input));
            
            return input;
        }

        private DateTime GetDateInput(string prompt)
        {
            while (true)
            {
                var input = _console.GetUserInput($"{prompt} (yyyy-MM-dd): ");
                if (DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, 
                    DateTimeStyles.None, out var date))
                {
                    return date;
                }
                _console.DisplayMessage("Invalid date format. Please use yyyy-MM-dd.", MessageType.Warning);
            }
        }

        private DateTime? GetOptionalDateInput(string prompt)
        {
            var input = _console.GetUserInput($"{prompt} (yyyy-MM-dd, press Enter to skip): ");
            if (string.IsNullOrWhiteSpace(input))
                return null;
                
            if (DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, 
                DateTimeStyles.None, out var date))
            {
                return date;
            }
            
            _console.DisplayMessage("Invalid date format. Skipping this field.", MessageType.Warning);
            return null;
        }

        private DateTime GetDateTimeInput(string prompt)
        {
            while (true)
            {
                var input = _console.GetUserInput($"{prompt} (yyyy-MM-dd HH:mm): ");
                if (DateTime.TryParseExact(input, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dateTime))
                {
                    return dateTime;
                }
                _console.DisplayMessage("Invalid datetime format. Please use yyyy-MM-dd HH:mm.", MessageType.Warning);
            }
        }

        private TimeSpan GetTimeInput(string prompt)
        {
            while (true)
            {
                var input = _console.GetUserInput($"{prompt} (HH:mm): ");
                if (TimeSpan.TryParseExact(input, @"hh\:mm", CultureInfo.InvariantCulture, out var time))
                {
                    return time;
                }
                _console.DisplayMessage("Invalid time format. Please use HH:mm.", MessageType.Warning);
            }
        }

        private int GetIntInput(string prompt, int defaultValue = 0, int min = int.MinValue, int max = int.MaxValue)
        {
            while (true)
            {
                var input = _console.GetUserInput($"{prompt} [{defaultValue}]: ");
                if (string.IsNullOrWhiteSpace(input))
                {
                    return defaultValue;
                }
                
                if (int.TryParse(input, out var value))
                {
                    if (value >= min && value <= max)
                    {
                        return value;
                    }
                    _console.DisplayMessage($"Value must be between {min} and {max}.", MessageType.Warning);
                }
                else
                {
                    _console.DisplayMessage("Invalid number format.", MessageType.Warning);
                }
            }
        }

        private bool GetBoolInput(string prompt)
        {
            while (true)
            {
                var input = _console.GetUserInput($"{prompt} (Y/N): ")?.ToUpperInvariant();
                if (input == "Y" || input == "YES")
                {
                    return true;
                }
                if (input == "N" || input == "NO")
                {
                    return false;
                }
                _console.DisplayMessage("Please enter Y for Yes or N for No.", MessageType.Warning);
            }
        }

        private Guid GetGuidInput(string prompt)
        {
            while (true)
            {
                var input = _console.GetUserInput($"{prompt}: ") ??
                    throw new UserInputCancelledException($"User cancelled input for: {prompt}");
                if (Guid.TryParse(input, out var guid))
                {
                    return guid;
                }
                _console.DisplayMessage("Invalid GUID format.", MessageType.Warning);
            }
        }

        private Guid? GetOptionalGuidInput(string prompt)
        {
            var input = _console.GetUserInput($"{prompt} (press Enter to skip): ");
            if (string.IsNullOrWhiteSpace(input))
                return null;
                
            if (Guid.TryParse(input, out var guid))
            {
                return guid;
            }
            
            _console.DisplayMessage("Invalid GUID format. Skipping this field.", MessageType.Warning);
            return null;
        }

        private Gender GetGenderInput()
        {
            _console.DisplayMessage("Select Gender:", MessageType.Info);
            var genders = Enum.GetValues<Gender>();
            for (int i = 0; i < genders.Length; i++)
            {
                _console.DisplayMessage($"{i + 1}. {genders[i]}", MessageType.Info);
            }

            while (true)
            {
                var input = _console.GetUserInput("Enter selection (1-" + genders.Length + "): ");
                if (int.TryParse(input, out var selection) && selection >= 1 && selection <= genders.Length)
                {
                    return genders[selection - 1];
                }
                _console.DisplayMessage("Invalid selection.", MessageType.Warning);
            }
        }

        private Gender? GetOptionalGenderInput()
        {
            _console.DisplayMessage("Select Gender (press Enter to keep current):", MessageType.Info);
            var genders = Enum.GetValues<Gender>();
            for (int i = 0; i < genders.Length; i++)
            {
                _console.DisplayMessage($"{i + 1}. {genders[i]}", MessageType.Info);
            }

            var input = _console.GetUserInput("Enter selection (1-" + genders.Length + ", or press Enter to skip): ");

            if (string.IsNullOrWhiteSpace(input))
            {
                return null; // Keep current value
            }

            if (int.TryParse(input, out var selection) && selection >= 1 && selection <= genders.Length)
            {
                return genders[selection - 1];
            }

            _console.DisplayMessage("Invalid selection, keeping current value.", MessageType.Warning);
            return null;
        }

        private string GetRaceInput()
        {
            var commonRaces = new[]
            {
                "American Indian or Alaska Native",
                "Asian",
                "Black or African American",
                "Native Hawaiian or Other Pacific Islander",
                "White",
                "Other",
                "Prefer not to say"
            };

            _console.DisplayMessage("Select Race:", MessageType.Info);
            for (int i = 0; i < commonRaces.Length; i++)
            {
                _console.DisplayMessage($"{i + 1}. {commonRaces[i]}", MessageType.Info);
            }

            while (true)
            {
                var input = _console.GetUserInput($"Enter selection (1-{commonRaces.Length}) or type custom: ");
                
                if (int.TryParse(input, out var selection) && selection >= 1 && selection <= commonRaces.Length)
                {
                    return commonRaces[selection - 1];
                }
                else if (!string.IsNullOrWhiteSpace(input) && !char.IsDigit(input[0]))
                {
                    return input;
                }
                _console.DisplayMessage("Invalid selection.", MessageType.Warning);
            }
        }

        private MedicalSpecialization GetSpecializationInput()
        {
            _console.DisplayMessage("Select Medical Specialization:", MessageType.Info);
            var specializations = Enum.GetValues<MedicalSpecialization>();
            for (int i = 0; i < specializations.Length; i++)
            {
                _console.DisplayMessage($"{i + 1}. {specializations[i]}", MessageType.Info);
            }

            while (true)
            {
                var input = _console.GetUserInput($"Enter selection (1-{specializations.Length}): ");
                if (int.TryParse(input, out var selection) && selection >= 1 && selection <= specializations.Length)
                {
                    return specializations[selection - 1];
                }
                _console.DisplayMessage("Invalid selection.", MessageType.Warning);
            }
        }

        private List<MedicalSpecialization> GetSpecializationListInput()
        {
            List<MedicalSpecialization> msList = [];
            char sentinel = 'c';

            MessageType freeSpaceMsgType = MessageType.Info;

            while (sentinel == 'c' && msList.Count < CreatePhysicianCommand.MEDSPECMAXCOUNT)
            {
                // get their choice using helper
                MedicalSpecialization selectedMs = GetSpecializationInput();
                if (msList.Contains(selectedMs)) // ensure no duplicates
                {
                    _console.DisplayMessage(
                        $"The {selectedMs} specialization is already selected. Please try again...",
                        MessageType.Warning);
                    _console.Pause();
                    continue;
                }

                // now proven as a non-duplicate, add it and print pending selection(s)
                msList.Add(selectedMs);
                _console.DisplayMessage("Pending medical specialization(s):");

                // print their pending selections in green
                foreach (MedicalSpecialization m in msList)
                {
                    _console.DisplayMessage(
                        MedicalSpecializationExtensions.GetDisplayName(m),
                        MessageType.Success);
                }

                // get space remaining so that we can communicate it to the user
                int spaceRem = CreatePhysicianCommand.MEDSPECMAXCOUNT - msList.Count;

                if (spaceRem == 0)
                {
                    _console.DisplayMessage("Specialization list is now full. Creating profile...", freeSpaceMsgType);
                    break;
                }
                else if (spaceRem == 1)
                    freeSpaceMsgType = MessageType.Warning;

                _console.DisplayMessage($"You may add {spaceRem} more specialization(s) to your profile.", freeSpaceMsgType);
                string input = _console.GetUserInput($"[C]ontinue, or any other key to submit: ") ?? string.Empty;

                if (input == string.Empty)
                {
                    _console.DisplayMessage(
                        "Input is empty. Please provide a valid string of characters.", 
                        MessageType.Warning);
                }

                bool validParse = char.TryParse(input?[..1], out char s);
                if (!validParse)
                {
                    _console.DisplayMessage(
                        "Invalid input. Please provide a valid string of characters.", 
                        MessageType.Warning);
                    continue;
                }
                else
                {
                    sentinel = char.ToLowerInvariant(s);
                }
            }
            return msList;
        }

        private DayOfWeek GetDayOfWeekInput()
        {
            _console.DisplayMessage("Select Day of Week:", MessageType.Info);
            var days = Enum.GetValues<DayOfWeek>();
            for (int i = 0; i < days.Length; i++)
            {
                _console.DisplayMessage($"{i + 1}. {days[i]}", MessageType.Info);
            }

            while (true)
            {
                var input = _console.GetUserInput($"Enter selection (1-{days.Length}): ");
                if (int.TryParse(input, out var selection) && selection >= 1 && selection <= days.Length)
                {
                    return days[selection - 1];
                }
                _console.DisplayMessage("Invalid selection.", MessageType.Warning);
            }
        }

        private UserRole GetUserRoleInput()
        {
            _console.DisplayMessage("Select User Role:", MessageType.Info);
            var roles = Enum.GetValues<UserRole>();
            for (int i = 0; i < roles.Length; i++)
            {
                _console.DisplayMessage($"{i + 1}. {roles[i]}", MessageType.Info);
            }

            while (true)
            {
                var input = _console.GetUserInput($"Enter selection (1-{roles.Length}): ");
                if (int.TryParse(input, out var selection) && selection >= 1 && selection <= roles.Length)
                {
                    return roles[selection - 1];
                }
                _console.DisplayMessage("Invalid selection.", MessageType.Warning);
            }
        }

        private string GetReportTypeInput()
        {
            var reportTypes = new[] { "Summary", "Detailed", "Statistical" };
            
            _console.DisplayMessage("Select Report Type:", MessageType.Info);
            for (int i = 0; i < reportTypes.Length; i++)
            {
                _console.DisplayMessage($"{i + 1}. {reportTypes[i]}", MessageType.Info);
            }

            while (true)
            {
                var input = _console.GetUserInput($"Enter selection (1-{reportTypes.Length}): ");
                if (int.TryParse(input, out var selection) && selection >= 1 && selection <= reportTypes.Length)
                {
                    return reportTypes[selection - 1];
                }
                _console.DisplayMessage("Invalid selection.", MessageType.Warning);
            }
        }

        private Guid GetAppointmentSelection(Guid patientId)
        {
            var appointments = _scheduleManager.GetPatientAppointments(patientId)
                .OrderBy(a => a.Start)
                .ToList();

            if (appointments.Count == 0)
            {
                _console.DisplayMessage("No appointments found for this patient.", MessageType.Warning);
                return GetGuidInput("Enter Appointment ID manually");
            }

            _console.DisplayMessage("Select Appointment:", MessageType.Info);
            _console.DisplayTable(appointments,
                ("Date/Time", a => a.Start.ToString("yyyy-MM-dd HH:mm")),
                ("Duration", a => $"{a.Duration.TotalMinutes} min"),
                ("Status", a => a.Status.ToString()),
                ("Reason", a => a.ReasonForVisit ?? "General"),
                ("ID", a => a.Id.ToString())
            );

            while (true)
            {
                var input = _console.GetUserInput("Enter Appointment ID from list above: ");
                if (Guid.TryParse(input, out var guid))
                {
                    if (appointments.Any(a => a.Id == guid))
                    {
                        return guid;
                    }
                    _console.DisplayMessage("Appointment ID not found in list.", MessageType.Warning);
                }
                else
                {
                    _console.DisplayMessage("Invalid ID format.", MessageType.Warning);
                }
            }
        }

        private Guid GetClinicalDocumentSelection()
        {
            var documents = _clinicalDocumentRegistry.GetAllDocuments()
                .OrderByDescending(d => d.CreatedAt)
                .ToList();

            if (documents.Count == 0)
            {
                _console.DisplayMessage("No clinical documents found in the system.", MessageType.Warning);
                return GetGuidInput("Enter Clinical Document ID manually");
            }

            _console.DisplayMessage("Select Clinical Document:", MessageType.Info);
            
            // Get patient names for better display
            var documentsWithPatientInfo = documents.Select(d =>
            {
                var patient = _profileRegistry.GetProfileById(d.PatientId) as PatientProfile;
                return new
                {
                    Document = d,
                    PatientName = patient?.Name ?? "Unknown Patient"
                };
            }).ToList();

            _console.DisplayTable(documentsWithPatientInfo,
                ("Created", d => d.Document.CreatedAt.ToString("yyyy-MM-dd HH:mm")),
                ("Patient", d => d.PatientName),
                ("Chief Complaint", d => d.Document.ChiefComplaint ?? "N/A"),
                ("Status", d => d.Document.IsCompleted ? "Complete" : "Incomplete"),
                ("ID", d => d.Document.Id.ToString())
            );

            while (true)
            {
                var input = _console.GetUserInput("Enter Clinical Document ID from list above: ");
                if (Guid.TryParse(input, out var guid))
                {
                    if (documents.Any(d => d.Id == guid))
                    {
                        return guid;
                    }
                    _console.DisplayMessage("Clinical Document ID not found in list.", MessageType.Warning);
                }
                else
                {
                    _console.DisplayMessage("Invalid ID format.", MessageType.Warning);
                }
            }
        }

        private void PromptForProfileUpdateFields(CommandParameters parameters)
        {
            // First, get the profile to determine its type
            var profileId = parameters.GetParameter<Guid>(UpdateProfileCommand.Parameters.ProfileId);
            var profile = _profileRegistry.GetProfileById(profileId);

            if (profile == null)
            {
                _console.DisplayMessage("Profile not found.", MessageType.Error);
                return;
            }

            // Prompt for common fields (all profile types have these)
            parameters[UpdateProfileCommand.Parameters.Name] = GetOptionalStringInput("New Name (leave blank to keep current)");
            parameters[UpdateProfileCommand.Parameters.Address] = GetOptionalStringInput("New Address (leave blank to keep current)");
            parameters[UpdateProfileCommand.Parameters.BirthDate] = GetOptionalDateInput("New Birth Date (leave blank to keep current)");

            // Add role-specific fields based on actual profile type
            switch (profile.Role)
            {
                case UserRole.Patient:
                    // Patient-specific fields: gender, race
                    parameters[UpdateProfileCommand.Parameters.Gender] = GetOptionalGenderInput();
                    parameters[UpdateProfileCommand.Parameters.Race] = GetOptionalStringInput("New Race (leave blank to keep current)");
                    break;

                case UserRole.Physician:
                    // Physician-specific fields: license, graduation, specializations
                    parameters[UpdateProfileCommand.Parameters.LicenseNumber] = GetOptionalStringInput("New License Number (leave blank to keep current)");
                    parameters[UpdateProfileCommand.Parameters.GraduationDate] = GetOptionalDateInput("New Graduation Date (leave blank to keep current)");
                    // Note: Specializations would need special handling as it's a list
                    break;

                case UserRole.Administrator:
                    // Administrator-specific field: email
                    parameters[UpdateProfileCommand.Parameters.Email] = GetOptionalStringInput("New Email (leave blank to keep current)");
                    break;
            }
        }

        private Guid GetProfileSelection(UserRole? roleFilter = null)
        {
            var allProfiles = new List<IUserProfile>();

            // If role filter is specified, only get profiles of that type
            if (roleFilter.HasValue)
            {
                switch (roleFilter.Value)
                {
                    case UserRole.Administrator:
                        allProfiles.AddRange(_profileRegistry.GetAllAdministrators());
                        break;
                    case UserRole.Physician:
                        allProfiles.AddRange(_profileRegistry.GetAllPhysicians());
                        break;
                    case UserRole.Patient:
                        allProfiles.AddRange(_profileRegistry.GetAllPatients());
                        break;
                }
            }
            else
            {
                // No filter - get all profiles
                allProfiles.AddRange(_profileRegistry.GetAllAdministrators());
                allProfiles.AddRange(_profileRegistry.GetAllPhysicians());
                allProfiles.AddRange(_profileRegistry.GetAllPatients());
            }

            var profiles = allProfiles
                .OrderBy(p => p.Username)
                .ToList();

            if (profiles.Count == 0)
            {
                var roleMessage = roleFilter.HasValue
                    ? $"No {roleFilter.Value.ToString().ToLower()}s found in the system."
                    : "No profiles found in the system.";
                _console.DisplayMessage(roleMessage, MessageType.Warning);
                return GetGuidInput("Enter Profile ID manually");
            }

            var selectionMessage = roleFilter.HasValue
                ? $"Select {roleFilter.Value}:"
                : "Select Profile:";
            _console.DisplayMessage(selectionMessage, MessageType.Info);

            // Display role-specific information in the table
            if (roleFilter == UserRole.Patient)
            {
                var patients = profiles.Cast<PatientProfile>().ToList();
                _console.DisplayTable(patients,
                    ("Name", p => p.Name),
                    ("Username", p => p.Username),
                    ("Birth Date", p => p.BirthDate.ToString("yyyy-MM-dd")),
                    ("ID", p => p.Id.ToString())
                );
            }
            else if (roleFilter == UserRole.Physician)
            {
                var physicians = profiles.Cast<PhysicianProfile>().ToList();
                _console.DisplayTable(physicians,
                    ("Name", p => p.Name),
                    ("Username", p => p.Username),
                    ("Specializations", p => string.Join(", ", p.Specializations)),
                    ("License", p => p.LicenseNumber),
                    ("ID", p => p.Id.ToString())
                );
            }
            else
            {
                // Generic display for mixed or administrator profiles
                _console.DisplayTable(profiles,
                    ("Username", p => p.Username),
                    ("Role", p => p.Role.ToString()),
                    ("ID", p => p.Id.ToString())
                );
            }

            while (true)
            {
                var input = _console.GetUserInput("Enter Profile ID from list above: ");
                if (Guid.TryParse(input, out var guid))
                {
                    if (profiles.Any(p => p.Id == guid))
                    {
                        return guid;
                    }
                    _console.DisplayMessage("Profile ID not found in list.", MessageType.Warning);
                }
                else
                {
                    _console.DisplayMessage("Invalid ID format.", MessageType.Warning);
                }
            }
        }
    }
}