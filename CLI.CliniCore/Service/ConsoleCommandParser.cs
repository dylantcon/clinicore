using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Profile;
using Core.CliniCore.Commands.Scheduling;
using Core.CliniCore.Commands.Clinical;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.ClinicalDoc;
using Core.CliniCore.Scheduling.Management;

namespace CLI.CliniCore.Service
{
    public class ConsoleCommandParser
    {
        private readonly IConsoleEngine _console;
        private readonly ProfileRegistry _profileRegistry;
        private readonly ClinicalDocumentRegistry _clinicalDocumentRegistry;

        public ConsoleCommandParser(IConsoleEngine console)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _profileRegistry = ProfileRegistry.Instance;
            _clinicalDocumentRegistry = ClinicalDocumentRegistry.Instance;
        }

        public CommandParameters ParseInteractive(ICommand command)
        {
            var parameters = new CommandParameters();
            
            // Use the command's CommandKey property for consistent identification
            var commandKey = command.CommandKey.ToLowerInvariant();
            
            // Parse parameters based on the command key
            switch (commandKey)
            {
                case "login":
                    parameters[CommandParameterKeys.Username] = GetStringInput("Username");
                    parameters[CommandParameterKeys.Password] = GetSecureInput("Password");
                    break;

                case "changepassword":
                    parameters[CommandParameterKeys.OldPassword] = GetSecureInput("Current Password");
                    parameters[CommandParameterKeys.NewPassword] = GetSecureInput("New Password");
                    parameters[CommandParameterKeys.ConfirmPassword] = GetSecureInput("Confirm New Password");
                    break;

                case "createpatient":
                    parameters[CreatePatientCommand.Parameters.Username] = GetStringInput("Username");
                    parameters[CreatePatientCommand.Parameters.Password] = GetSecureInput("Password");
                    parameters[CreatePatientCommand.Parameters.Name] = GetStringInput("Full Name");
                    parameters[CreatePatientCommand.Parameters.Address] = GetStringInput("Address");
                    parameters[CreatePatientCommand.Parameters.Birthdate] = GetDateInput("Birth Date");
                    parameters[CreatePatientCommand.Parameters.Gender] = GetGenderInput();
                    parameters[CreatePatientCommand.Parameters.Race] = GetRaceInput();
                    break;

                case "createphysician":
                    parameters[CreatePhysicianCommand.Parameters.Username] = GetStringInput("Username");
                    parameters[CreatePhysicianCommand.Parameters.Password] = GetSecureInput("Password");
                    parameters[CreatePhysicianCommand.Parameters.Name] = GetStringInput("Full Name");
                    parameters[CreatePhysicianCommand.Parameters.LicenseNumber] = GetStringInput("License Number");
                    parameters[CreatePhysicianCommand.Parameters.GraduationDate] = GetDateInput("Graduation Date");
                    parameters[CreatePhysicianCommand.Parameters.Specializations] = GetSpecializationInput();
                    break;

                case "scheduleappointment":
                    parameters[ScheduleAppointmentCommand.Parameters.PatientId] = GetPatientSelection();
                    parameters[ScheduleAppointmentCommand.Parameters.PhysicianId] = GetPhysicianSelection();
                    parameters[ScheduleAppointmentCommand.Parameters.StartTime] = GetDateTimeInput("Appointment Date and Time");
                    parameters[ScheduleAppointmentCommand.Parameters.DurationMinutes] = GetIntInput("Duration (minutes)", 30, 15, 240);
                    parameters[ScheduleAppointmentCommand.Parameters.Notes] = GetOptionalStringInput("Notes");
                    break;

                case "createclinicaldocument":
                    var selectedPatientId = GetPatientSelection();
                    parameters[CreateClinicalDocumentCommand.Parameters.PatientId] = selectedPatientId;
                    parameters[CreateClinicalDocumentCommand.Parameters.AppointmentId] = GetAppointmentSelection(selectedPatientId);
                    parameters[CreateClinicalDocumentCommand.Parameters.ChiefComplaint] = GetStringInput("Chief Complaint");
                    parameters[CreateClinicalDocumentCommand.Parameters.InitialObservation] = GetOptionalStringInput("Initial Observation");
                    break;

                case "addobservation":
                    parameters[AddObservationCommand.Parameters.DocumentId] = GetClinicalDocumentSelection();
                    parameters[AddObservationCommand.Parameters.Observation] = GetStringInput("Observation");
                    break;

                case "adddiagnosis":
                    parameters[AddDiagnosisCommand.Parameters.DocumentId] = GetClinicalDocumentSelection();
                    parameters[AddDiagnosisCommand.Parameters.ICD10Code] = GetStringInput("Diagnosis Code (ICD-10)");
                    parameters[AddDiagnosisCommand.Parameters.DiagnosisDescription] = GetStringInput("Diagnosis Description");
                    break;

                case "addprescription":
                    parameters[AddPrescriptionCommand.Parameters.DocumentId] = GetClinicalDocumentSelection();
                    parameters[AddPrescriptionCommand.Parameters.DiagnosisId] = GetGuidInput("Associated Diagnosis ID");
                    parameters[AddPrescriptionCommand.Parameters.MedicationName] = GetStringInput("Medication Name");
                    parameters[AddPrescriptionCommand.Parameters.Dosage] = GetStringInput("Dosage");
                    parameters[AddPrescriptionCommand.Parameters.Frequency] = GetStringInput("Frequency");
                    parameters[AddPrescriptionCommand.Parameters.Duration] = GetStringInput("Duration");
                    break;

                case "addassessment":
                    parameters[AddAssessmentCommand.Parameters.DocumentId] = GetClinicalDocumentSelection();
                    parameters[AddAssessmentCommand.Parameters.ClinicalImpression] = GetStringInput("Clinical Assessment");
                    break;

                case "addplan":
                    parameters[AddPlanCommand.Parameters.DocumentId] = GetClinicalDocumentSelection();
                    parameters[AddPlanCommand.Parameters.PlanDescription] = GetStringInput("Plan");
                    break;

                case "viewprofile":
                case "deleteprofile":
                    parameters[ViewProfileCommand.Parameters.ProfileId] = GetProfileSelection();
                    break;

                case "updateprofile":
                    parameters[CommandParameterKeys.ProfileId] = GetProfileSelection();
                    parameters[CommandParameterKeys.Name] = GetOptionalStringInput("New Name (leave blank to keep current)");
                    parameters[CommandParameterKeys.Address] = GetOptionalStringInput("New Address (leave blank to keep current)");
                    parameters[CommandParameterKeys.Email] = GetOptionalStringInput("New Email (leave blank to keep current)");
                    parameters[CommandParameterKeys.Phone] = GetOptionalStringInput("New Phone (leave blank to keep current)");
                    break;

                case "viewappointment":
                case "cancelappointment":
                    parameters[ViewAppointmentCommand.Parameters.AppointmentId] = GetGuidInput("Appointment ID");
                    break;

                case "rescheduleappointment":
                    parameters[CommandParameterKeys.AppointmentId] = GetGuidInput("Appointment ID");
                    parameters[CommandParameterKeys.NewDateTime] = GetDateTimeInput("New Date and Time");
                    break;

                case "viewclinicaldocument":
                case "updateclinicaldocument":
                    parameters[ViewClinicalDocumentCommand.Parameters.DocumentId] = GetClinicalDocumentSelection();
                    break;

                case "setphysicianavailability":
                    parameters[CommandParameterKeys.PhysicianId] = GetPhysicianSelection();
                    parameters[CommandParameterKeys.DayOfWeek] = GetDayOfWeekInput();
                    parameters[CommandParameterKeys.StartTime] = GetTimeInput("Start Time");
                    parameters[CommandParameterKeys.EndTime] = GetTimeInput("End Time");
                    break;

                case "findphysiciansbyspecialization":
                    parameters[CommandParameterKeys.Specializations] = GetSpecializationInput();
                    break;

                case "findphysiciansbyavailability":
                    parameters[CommandParameterKeys.DateTime] = GetDateTimeInput("Date and Time");
                    break;

                case "searchpatients":
                    parameters[CommandParameterKeys.SearchTerm] = GetStringInput("Search Term");
                    break;

                case "searchclinicalnotes":
                    parameters[CommandParameterKeys.SearchTerm] = GetStringInput("Search Term");
                    parameters[CommandParameterKeys.PatientId] = GetOptionalGuidInput("Patient ID (optional)");
                    break;

                case "assignpatienttophysician":
                    parameters[CommandParameterKeys.PatientId] = GetPatientSelection();
                    parameters[CommandParameterKeys.PhysicianId] = GetPhysicianSelection();
                    break;

                case "getschedule":
                    parameters[CommandParameterKeys.PhysicianId] = GetPhysicianSelection();
                    parameters[CommandParameterKeys.StartDate] = GetDateInput("Start Date");
                    parameters[CommandParameterKeys.EndDate] = GetDateInput("End Date");
                    break;

                case "getavailabletimeslots":
                    parameters[GetAvailableTimeSlotsCommand.Parameters.PhysicianId] = GetPhysicianSelection();
                    parameters[GetAvailableTimeSlotsCommand.Parameters.Date] = GetDateInput("Date");
                    parameters[GetAvailableTimeSlotsCommand.Parameters.DurationMinutes] = GetIntInput("Duration (minutes)", 30, 15, 240);
                    break;

                case "checkconflicts":
                    parameters[CheckConflictsCommand.Parameters.PhysicianId] = GetPhysicianSelection();
                    parameters[CheckConflictsCommand.Parameters.StartTime] = GetDateTimeInput("Date and Time");
                    parameters[CheckConflictsCommand.Parameters.DurationMinutes] = GetIntInput("Duration (minutes)", 30, 15, 240);
                    break;

                case "generatepatientreport":
                    parameters[CommandParameterKeys.PatientId] = GetPatientSelection();
                    parameters[CommandParameterKeys.StartDate] = GetOptionalDateInput("Start Date (optional)");
                    parameters[CommandParameterKeys.EndDate] = GetOptionalDateInput("End Date (optional)");
                    break;

                case "generatephysicianreport":
                    parameters[CommandParameterKeys.PhysicianId] = GetPhysicianSelection();
                    parameters[CommandParameterKeys.StartDate] = GetOptionalDateInput("Start Date (optional)");
                    parameters[CommandParameterKeys.EndDate] = GetOptionalDateInput("End Date (optional)");
                    break;

                case "generateappointmentreport":
                    parameters[CommandParameterKeys.StartDate] = GetDateInput("Start Date");
                    parameters[CommandParameterKeys.EndDate] = GetDateInput("End Date");
                    break;

                case "generatefacilityreport":
                    parameters[CommandParameterKeys.StartDate] = GetDateInput("Start Date");
                    parameters[CommandParameterKeys.EndDate] = GetDateInput("End Date");
                    parameters[CommandParameterKeys.ReportType] = GetReportTypeInput();
                    break;

                case "createfacility":
                    parameters[CommandParameterKeys.FacilityName] = GetStringInput("Facility Name");
                    parameters[CommandParameterKeys.FacilityAddress] = GetStringInput("Facility Address");
                    break;

                case "updatefacilitysettings":
                    parameters[CommandParameterKeys.FacilityId] = GetGuidInput("Facility ID");
                    parameters[CommandParameterKeys.FacilityName] = GetOptionalStringInput("New Name (leave blank to keep current)");
                    parameters[CommandParameterKeys.FacilityAddress] = GetOptionalStringInput("New Address (leave blank to keep current)");
                    break;

                case "manageuserroles":
                    parameters[CommandParameterKeys.UserId] = GetProfileSelection();
                    parameters[CommandParameterKeys.UserRole] = GetUserRoleInput();
                    break;

                case "listpatients":
                case "listphysicians":
                case "listappointments":
                case "listclinicaldocuments":
                case "logout":
                case "viewauditlog":
                case "systemmaintenance":
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

        private Guid GetGuidInput(string prompt)
        {
            while (true)
            {
                var input = _console.GetUserInput($"{prompt}: ");
                if (input == null) // User pressed Escape
                {
                    throw new UserInputCancelledException($"User cancelled input for: {prompt}");
                }
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

        private Guid GetPatientSelection()
        {
            var patients = _profileRegistry.GetAllPatients()
                .OrderBy(p => p.Name)
                .ToList();

            if (patients.Count == 0)
            {
                _console.DisplayMessage("No patients found in the system.", MessageType.Warning);
                return GetGuidInput("Enter Patient ID manually");
            }

            _console.DisplayMessage("Select Patient:", MessageType.Info);
            _console.DisplayTable(patients,
                ("Name", p => p.Name),
                ("Birth Date", p => p.BirthDate.ToString("yyyy-MM-dd")),
                ("ID", p => p.Id.ToString())
            );

            while (true)
            {
                var input = _console.GetUserInput("Enter Patient ID from list above: ");
                if (input == null) // User pressed Escape
                {
                    throw new UserInputCancelledException("User cancelled patient selection");
                }
                if (Guid.TryParse(input, out var guid))
                {
                    if (patients.Any(p => p.Id == guid))
                    {
                        return guid;
                    }
                    _console.DisplayMessage("Patient ID not found in list.", MessageType.Warning);
                }
                else
                {
                    _console.DisplayMessage("Invalid ID format.", MessageType.Warning);
                }
            }
        }

        private Guid GetAppointmentSelection(Guid patientId)
        {
            var scheduleManager = ScheduleManager.Instance;
            var appointments = scheduleManager.GetPatientAppointments(patientId)
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

        private Guid GetPhysicianSelection()
        {
            var physicians = _profileRegistry.GetAllPhysicians()
                .OrderBy(p => p.Name)
                .ToList();

            if (physicians.Count == 0)
            {
                _console.DisplayMessage("No physicians found in the system.", MessageType.Warning);
                return GetGuidInput("Enter Physician ID manually");
            }

            _console.DisplayMessage("Select Physician:", MessageType.Info);
            _console.DisplayTable(physicians,
                ("Name", (Core.CliniCore.Domain.PhysicianProfile p) => p.Name),
                ("Specializations", (Core.CliniCore.Domain.PhysicianProfile p) => string.Join(", ", p.Specializations)),
                ("License", (Core.CliniCore.Domain.PhysicianProfile p) => p.LicenseNumber),
                ("ID", (Core.CliniCore.Domain.PhysicianProfile p) => p.Id.ToString())
            );

            while (true)
            {
                var input = _console.GetUserInput("Enter Physician ID from list above: ");
                if (Guid.TryParse(input, out var guid))
                {
                    if (physicians.Any(p => p.Id == guid))
                    {
                        return guid;
                    }
                    _console.DisplayMessage("Physician ID not found in list.", MessageType.Warning);
                }
                else
                {
                    _console.DisplayMessage("Invalid ID format.", MessageType.Warning);
                }
            }
        }

        private Guid GetProfileSelection()
        {
            var allProfiles = new List<IUserProfile>();
            allProfiles.AddRange(_profileRegistry.GetAllAdministrators());
            allProfiles.AddRange(_profileRegistry.GetAllPhysicians());
            allProfiles.AddRange(_profileRegistry.GetAllPatients());
            var profiles = allProfiles
                .OrderBy(p => p.Username)
                .ToList();

            if (profiles.Count == 0)
            {
                _console.DisplayMessage("No profiles found in the system.", MessageType.Warning);
                return GetGuidInput("Enter Profile ID manually");
            }

            _console.DisplayMessage("Select Profile:", MessageType.Info);
            _console.DisplayTable(profiles,
                ("Username", p => p.Username),
                ("Role", p => p.Role.ToString()),
                ("ID", p => p.Id.ToString())
            );

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