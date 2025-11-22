using System;
using System.Collections.Generic;
using System.Linq;
using CLI.CliniCore.Service.Editor;
using Core.CliniCore.ClinicalDoc;
using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Authentication;
using Core.CliniCore.Commands.Clinical;
using Core.CliniCore.Commands.Profile;
using Core.CliniCore.Commands.Query;
using Core.CliniCore.Commands.Reports;
using Core.CliniCore.Commands.Scheduling;
using Core.CliniCore.Commands.Admin;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Authentication;

namespace CLI.CliniCore.Service
{
    public class ConsoleMenuBuilder
    {
        private readonly CommandInvoker _commandInvoker;
        private readonly CommandFactory _commandFactory;
        private readonly ConsoleSessionManager _sessionManager;
        private readonly ConsoleCommandParser _commandParser;
        private readonly IConsoleEngine _console;
        private readonly Dictionary<Type, string> _commandKeyCache = new Dictionary<Type, string>();

        public ConsoleMenuBuilder(
            CommandInvoker commandInvoker,
            CommandFactory commandFactory,
            ConsoleSessionManager sessionManager,
            ConsoleCommandParser commandParser,
            IConsoleEngine console)
        {
            _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _commandParser = commandParser ?? throw new ArgumentNullException(nameof(commandParser));
            _console = console ?? throw new ArgumentNullException(nameof(console));
        }

        public ConsoleMenu BuildMainMenu()
        {
            var menu = new ConsoleMenu
            {
                Title = "CliniCore Medical Practice Management System",
                Subtitle = _sessionManager.GetSessionInfo(),
                ShowBackOption = false,
                HelpText = "Enter the number or letter of your choice"
            };

            if (!_sessionManager.IsAuthenticated)
            {
                menu.Items.Add(new ConsoleMenuItem
                {
                    Key = "1",
                    Label = "Login",
                    Description = "Authenticate to the system",
                    Action = () => ExecuteCommand(LoginCommand.Key)
                });

                menu.Items.Add(new ConsoleMenuItem
                {
                    Key = "X",
                    Label = "Exit",
                    Description = "Exit the application",
                    Action = null
                });
            }
            else
            {
                var role = _sessionManager.CurrentSession?.UserRole ?? UserRole.Patient;

                switch (role)
                {
                    case UserRole.Administrator:
                        AddAdministratorMenuItems(menu);
                        break;
                    case UserRole.Physician:
                        AddPhysicianMenuItems(menu);
                        break;
                    case UserRole.Patient:
                        AddPatientMenuItems(menu);
                        break;
                }

                menu.Items.Add(new ConsoleMenuItem
                {
                    Key = "P",
                    Label = "Change Password",
                    Description = "Change your account password",
                    Action = () => ExecuteCommand(ChangePasswordCommand.Key)
                });

                menu.Items.Add(new ConsoleMenuItem
                {
                    Key = "L",
                    Label = "Logout",
                    Description = "End current session",
                    Action = () => ExecuteCommand(LogoutCommand.Key)
                });

                menu.Items.Add(new ConsoleMenuItem
                {
                    Key = "X",
                    Label = "Exit",
                    Description = "Exit the application",
                    Action = null
                });
            }

            return menu;
        }

        private void AddAdministratorMenuItems(ConsoleMenu menu)
        {
            menu.Items.Add(new ConsoleMenuItem
            {
                Key = "1",
                Label = "User Management",
                Description = "Manage all system users",
                SubMenuFactory = BuildUserManagementMenu
            });

            menu.Items.Add(new ConsoleMenuItem
            {
                Key = "2",
                Label = "Patient Management",
                Description = "Manage patient profiles",
                SubMenuFactory = BuildPatientManagementMenu
            });

            menu.Items.Add(new ConsoleMenuItem
            {
                Key = "3",
                Label = "Physician Management",
                Description = "Manage physician profiles",
                SubMenuFactory = BuildPhysicianManagementMenu
            });

            menu.Items.Add(new ConsoleMenuItem
            {
                Key = "4",
                Label = "Scheduling",
                Description = "Manage appointments and schedules",
                SubMenuFactory = BuildSchedulingMenu
            });

            menu.Items.Add(new ConsoleMenuItem
            {
                Key = "5",
                Label = "Clinical Documents",
                Description = "Manage clinical documentation",
                SubMenuFactory = BuildClinicalDocumentsMenu
            });
        }

        private void AddPhysicianMenuItems(ConsoleMenu menu)
        {
            menu.Items.Add(new ConsoleMenuItem
            {
                Key = "1",
                Label = "My Patients",
                Description = "View and manage patient profiles",
                SubMenuFactory = BuildPatientManagementMenu
            });

            menu.Items.Add(new ConsoleMenuItem
            {
                Key = "2",
                Label = "Scheduling",
                Description = "Manage appointments and availability",
                SubMenuFactory = BuildPhysicianSchedulingMenu
            });

            menu.Items.Add(new ConsoleMenuItem
            {
                Key = "3",
                Label = "Clinical Documents",
                Description = "Manage patient medical records",
                SubMenuFactory = BuildClinicalDocumentsMenu
            });

            menu.Items.Add(new ConsoleMenuItem
            {
                Key = "4",
                Label = "My Profile",
                Description = "View and update your profile",
                Action = () => ViewOwnProfile()
            });
        }

        private void AddPatientMenuItems(ConsoleMenu menu)
        {
            menu.Items.Add(new ConsoleMenuItem
            {
                Key = "1",
                Label = "My Profile",
                Description = "View your profile information",
                Action = () => ViewOwnProfile()
            });

            menu.Items.Add(new ConsoleMenuItem
            {
                Key = "2",
                Label = "My Appointments",
                Description = "View your appointments",
                Action = () => ExecuteCommand(ListAppointmentsCommand.Key)
            });

            menu.Items.Add(new ConsoleMenuItem
            {
                Key = "3",
                Label = "My Clinical Documents",
                Description = "View your medical records",
                Action = () => ExecuteCommand(ListClinicalDocumentsCommand.Key)
            });
        }

        private ConsoleMenu BuildUserManagementMenu()
        {
            return new ConsoleMenu
            {
                Title = "User Management",
                Items = new List<ConsoleMenuItem>
                {
                    new ConsoleMenuItem
                    {
                        Key = "1",
                        Label = "Create Administrator",
                        Description = "Create a new administrator account",
                        Action = () => ExecuteCommand(CreateAdministratorCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "List All Users",
                        Description = "View all system users",
                        Action = () => ExecuteCommand(ListAllUsersCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "Update User Profile",
                        Description = "Update any user profile",
                        Action = () => ExecuteCommand(UpdateProfileCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "Delete User",
                        Description = "Remove a user from the system",
                        Action = () => ExecuteCommand(DeleteProfileCommand.Key)
                    }
                }
            };
        }

        private ConsoleMenu BuildPatientManagementMenu()
        {
            return new ConsoleMenu
            {
                Title = "Patient Management",
                Items = new List<ConsoleMenuItem>
                {
                    new ConsoleMenuItem
                    {
                        Key = "1",
                        Label = "Create Patient",
                        Description = "Register a new patient",
                        Action = () => ExecuteCommand(CreatePatientCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "List Patients",
                        Description = "View all patients",
                        Action = () => ExecuteCommand(ListPatientsCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "View Patient Profile",
                        Description = "View detailed patient information",
                        Action = () => ExecuteCommand(ViewPatientProfileCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "Update Patient Profile",
                        Description = "Update patient information",
                        Action = () => ExecuteCommand("updatepatientprofile")
                    },
                    new ConsoleMenuItem
                    {
                        Key = "5",
                        Label = "Search Patients",
                        Description = "Search for patients by name",
                        Action = () => ExecuteCommand(SearchPatientsCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "6",
                        Label = "Assign Patient to Physician",
                        Description = "Set primary physician for patient",
                        Action = () => ExecuteCommand(AssignPatientToPhysicianCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "7",
                        Label = "Delete Patient",
                        Description = "Remove a patient from the system",
                        Action = () => ExecuteCommand(DeleteProfileCommand.Key)
                    }
                }
            };
        }

        private ConsoleMenu BuildPhysicianManagementMenu()
        {
            return new ConsoleMenu
            {
                Title = "Physician Management",
                Items = new List<ConsoleMenuItem>
                {
                    new ConsoleMenuItem
                    {
                        Key = "1",
                        Label = "Create Physician",
                        Description = "Register a new physician",
                        Action = () => ExecuteCommand(CreatePhysicianCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "List Physicians",
                        Description = "View all physicians",
                        Action = () => ExecuteCommand(ListPhysiciansCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "View Physician Profile",
                        Description = "View detailed physician information",
                        Action = () => ExecuteCommand(ViewPhysicianProfileCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "Update Physician Profile",
                        Description = "Update physician information",
                        Action = () => ExecuteCommand("updatephysicianprofile")
                    },
                    new ConsoleMenuItem
                    {
                        Key = "5",
                        Label = "Find by Specialization",
                        Description = "Find physicians by medical specialization",
                        Action = () => ExecuteCommand(FindPhysiciansBySpecializationCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "6",
                        Label = "Find by Availability",
                        Description = "Find available physicians for a time slot",
                        Action = () => ExecuteCommand(FindPhysiciansByAvailabilityCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "7",
                        Label = "Delete Physician",
                        Description = "Remove a physician from the system",
                        Action = () => ExecuteCommand(DeleteProfileCommand.Key)
                    }
                }
            };
        }

        private ConsoleMenu BuildSchedulingMenu()
        {
            return new ConsoleMenu
            {
                Title = "Scheduling",
                Items = new List<ConsoleMenuItem>
                {
                    new ConsoleMenuItem
                    {
                        Key = "1",
                        Label = "Schedule Appointment",
                        Description = "Create a new appointment",
                        Action = () => ExecuteCommand(ScheduleAppointmentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "List Appointments",
                        Description = "View all appointments",
                        Action = () => ExecuteCommand(ListAppointmentsCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "View Appointment",
                        Description = "View appointment details",
                        Action = () => ExecuteCommand(ViewAppointmentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "Update Appointment",
                        Description = "Change appointment time, duration, or details",
                        Action = () => ExecuteCommand(UpdateAppointmentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "5",
                        Label = "Cancel Appointment",
                        Description = "Cancel an appointment",
                        Action = () => ExecuteCommand(CancelAppointmentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "6",
                        Label = "Check Available Time Slots",
                        Description = "Find available appointment times",
                        Action = () => ExecuteCommand(GetAvailableTimeSlotsCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "7",
                        Label = "Check Conflicts",
                        Description = "Check for scheduling conflicts",
                        Action = () => ExecuteCommand(CheckConflictsCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "8",
                        Label = "Set Physician Availability",
                        Description = "Configure physician working hours",
                        Action = () => ExecuteCommand(SetPhysicianAvailabilityCommand.Key)
                    }
                }
            };
        }

        private ConsoleMenu BuildPhysicianSchedulingMenu()
        {
            return new ConsoleMenu
            {
                Title = "Scheduling",
                Items = new List<ConsoleMenuItem>
                {
                    new ConsoleMenuItem
                    {
                        Key = "1",
                        Label = "Schedule Appointment",
                        Description = "Create a new appointment",
                        Action = () => ExecuteCommand(ScheduleAppointmentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "My Appointments",
                        Description = "View your appointments",
                        Action = () => ExecuteCommand(ListAppointmentsCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "View Appointment",
                        Description = "View appointment details",
                        Action = () => ExecuteCommand(ViewAppointmentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "Update Appointment",
                        Description = "Change appointment time, duration, or details",
                        Action = () => ExecuteCommand(UpdateAppointmentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "5",
                        Label = "Cancel Appointment",
                        Description = "Cancel an appointment",
                        Action = () => ExecuteCommand(CancelAppointmentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "6",
                        Label = "Set My Availability",
                        Description = "Configure your working hours",
                        Action = () => SetOwnAvailability()
                    },
                    new ConsoleMenuItem
                    {
                        Key = "7",
                        Label = "View My Schedule",
                        Description = "View your schedule",
                        Action = () => ExecuteCommand(GetScheduleCommand.Key)
                    }
                }
            };
        }

        private ConsoleMenu BuildClinicalDocumentsMenu()
        {
            return new ConsoleMenu
            {
                Title = "Clinical Documents",
                Subtitle = "Comprehensive document management and editing",
                Items = new List<ConsoleMenuItem>
                {
                    new ConsoleMenuItem
                    {
                        Key = "1",
                        Label = "Document Editor",
                        Description = "Comprehensive editing environment",
                        Color = ConsoleColor.Green,
                        Action = () => LaunchDocumentEditor()
                    },
                    new ConsoleMenuItem
                    {
                        Key = "",
                        Label = "━━━ Individual Commands ━━━",
                        Description = "",
                        IsEnabled = false,
                        Color = ConsoleColor.DarkGray
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "Create Clinical Document",
                        Description = "Start a new clinical document",
                        Action = () => ExecuteCommand(CreateClinicalDocumentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "List Clinical Documents",
                        Description = "View all clinical documents",
                        Action = () => ExecuteCommand(ListClinicalDocumentsCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "View Clinical Document",
                        Description = "View document details",
                        Action = () => ExecuteCommand(ViewClinicalDocumentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "5",
                        Label = "Add SOAP Note Entries",
                        Description = "Add entries to existing document",
                        SubMenuFactory = BuildSOAPMenu
                    },
                    new ConsoleMenuItem
                    {
                        Key = "6",
                        Label = "Update Clinical Document",
                        Description = "Update document chief complaint",
                        Action = () => ExecuteUpdateClinicalDocument()
                    },
                    new ConsoleMenuItem
                    {
                        Key = "7",
                        Label = "Finalize Clinical Document",
                        Description = "Complete and finalize a clinical document",
                        Action = () => ExecuteFinalizeClinicalDocument()
                    },
                    new ConsoleMenuItem
                    {
                        Key = "8",
                        Label = "Delete Clinical Document",
                        Description = "Delete a clinical document",
                        Action = () => ExecuteCommand(DeleteClinicalDocumentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "9",
                        Label = "Search Clinical Notes",
                        Description = "Search within clinical documents",
                        Action = () => ExecuteCommand(SearchClinicalNotesCommand.Key)
                    }
                }
            };
        }

        private ConsoleMenu BuildSOAPMenu()
        {
            return new ConsoleMenu
            {
                Title = "SOAP Note Management",
                Items = new List<ConsoleMenuItem>
                {
                    new ConsoleMenuItem
                    {
                        Key = "1",
                        Label = "Add Observation (S)",
                        Description = "Add subjective observation",
                        Action = () => ExecuteCommand(AddObservationCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "Add Diagnosis (O)",
                        Description = "Add objective diagnosis",
                        Action = () => ExecuteCommand(AddDiagnosisCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "Add Prescription (O)",
                        Description = "Add prescription linked to diagnosis",
                        Action = () => ExecuteCommand(AddPrescriptionCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "Add Assessment (A)",
                        Description = "Add clinical assessment",
                        Action = () => ExecuteCommand(AddAssessmentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "5",
                        Label = "Add Plan (P)",
                        Description = "Add treatment plan",
                        Action = () => ExecuteCommand(AddPlanCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "",
                        Label = "━━━ Update Entries ━━━",
                        Description = "",
                        IsEnabled = false,
                        Color = ConsoleColor.DarkGray
                    },
                    new ConsoleMenuItem
                    {
                        Key = "6",
                        Label = "Update SOAP Entry",
                        Description = "Update existing clinical entry",
                        SubMenuFactory = BuildSOAPUpdateMenu
                    }
                }
            };
        }

        /// <summary>
        /// Currently unutilized, will be put to use when facilities are implemented
        /// </summary>
        /// <returns></returns>
        private ConsoleMenu BuildReportsMenu()
        {
            return new ConsoleMenu
            {
                Title = "Reports",
                Items = new List<ConsoleMenuItem>
                {
                    new ConsoleMenuItem
                    {
                        Key = "1",
                        Label = "Patient Report",
                        Description = "Generate patient report",
                        Action = () => ExecuteCommand(GeneratePatientReportCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "Physician Report",
                        Description = "Generate physician report",
                        Action = () => ExecuteCommand(GeneratePhysicianReportCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "Appointment Report",
                        Description = "Generate appointment report",
                        Action = () => ExecuteCommand(GenerateAppointmentReportCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "Facility Report",
                        Description = "Generate facility-wide report",
                        Action = () => ExecuteCommand(GenerateFacilityReportCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "5",
                        Label = "List All Profiles",
                        Description = "View all user profiles in the system",
                        Action = () => ExecuteCommand(ListProfileCommand.Key)
                    }
                }
            };
        }

        /// <summary>
        /// Currently unutilized, will be put to use when facilities are implemented
        /// </summary>
        /// <returns></returns>
        private ConsoleMenu BuildPhysicianReportsMenu()
        {
            return new ConsoleMenu
            {
                Title = "Reports",
                Items = new List<ConsoleMenuItem>
                {
                    new ConsoleMenuItem
                    {
                        Key = "1",
                        Label = "Patient Report",
                        Description = "Generate patient report",
                        Action = () => ExecuteCommand(GeneratePatientReportCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "My Performance Report",
                        Description = "Generate your performance report",
                        Action = () => GenerateOwnPhysicianReport()
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "Appointment Report",
                        Description = "Generate appointment report",
                        Action = () => ExecuteCommand(GenerateAppointmentReportCommand.Key)
                    }
                }
            };
        }

        /// <summary>
        /// Currently unutilized, will be put to use when facilities are implemented
        /// </summary>
        /// <returns></returns>
        private ConsoleMenu BuildSystemAdminMenu()
        {
            return new ConsoleMenu
            {
                Title = "System Administration",
                Items = new List<ConsoleMenuItem>
                {
                    new ConsoleMenuItem
                    {
                        Key = "1",
                        Label = "Create Facility",
                        Description = "Create a new facility",
                        Action = () => ExecuteCommand(CreateFacilityCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "Update Facility Settings",
                        Description = "Modify facility configuration",
                        Action = () => ExecuteCommand(UpdateFacilitySettingsCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "Manage User Roles",
                        Description = "Manage user role assignments",
                        Action = () => ExecuteCommand(ManageUserRolesCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "View Audit Log",
                        Description = "View system audit trail",
                        Action = () => ExecuteCommand(ViewAuditLogCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "5",
                        Label = "System Maintenance",
                        Description = "Perform system maintenance tasks",
                        Action = () => ExecuteCommand(SystemMaintenanceCommand.Key)
                    }
                }
            };
        }

        private void ExecuteCommand(string commandName)
        {
            try
            {
                _sessionManager.UpdateActivity();
                var command = _commandFactory.CreateCommand(commandName);
                if (command == null)
                {
                    _console.DisplayMessage($"Command '{commandName}' not found.", MessageType.Error);
                    return;
                }

                CommandParameters parameters;
                try
                {
                    parameters = _commandParser.ParseInteractive(command);
                }
                catch (UserInputCancelledException)
                {
                    _console.DisplayMessage("Operation cancelled by user.", MessageType.Info);
                    _console.Pause();
                    return;
                }
                
                var result = _commandInvoker.Execute(command, parameters, _sessionManager.CurrentSession);
                
                if (result.Success)
                {
                    // Commands handle their own formatting - just display their message
                    _console.DisplayMessage(result.Message ?? "Command executed successfully.", MessageType.Success);

                    // Handle special session management cases
                    if (commandName == LogoutCommand.Key)
                    {
                        _sessionManager.EndSession();
                    }
                    else if (commandName == LoginCommand.Key && result.Data is SessionContext session)
                    {
                        _sessionManager.StartSession(session);
                    }
                }
                else
                {
                    _console.DisplayMessage($"Command failed: {result.Message}", MessageType.Error);
                    if (result.ValidationErrors.Any())
                    {
                        foreach (var error in result.ValidationErrors)
                        {
                            _console.DisplayMessage($"  - {error}", MessageType.Error);
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _console.DisplayMessage($"Authorization failed: {ex.Message}", MessageType.Error);
            }
            catch (Exception ex)
            {
                _console.DisplayMessage($"Error executing command: {ex.Message}", MessageType.Error);
            }
            
            _console.Pause();
        }


        private void ViewOwnProfile()
        {
            if (_sessionManager.CurrentUserId.HasValue)
            {
                var parameters = new CommandParameters();
                parameters[ViewProfileCommand.Parameters.ProfileId] = _sessionManager.CurrentUserId.Value;
                
                var command = _commandFactory.CreateCommand(ViewProfileCommand.Key);
                if (command == null)
                {
                    _console.DisplayMessage("ViewProfile command not found.", MessageType.Error);
                    _console.Pause();
                    return;
                }
                var result = _commandInvoker.Execute(command, parameters, _sessionManager.CurrentSession);
                
                if (result.Success)
                {
                    // Command handles its own display formatting
                    _console.DisplayMessage(result.Message ?? "Profile retrieved successfully.", MessageType.Success);
                }
                else
                {
                    _console.DisplayMessage($"Failed to retrieve profile: {result.Message}", MessageType.Error);
                }
                _console.Pause();
            }
        }

        private void SetOwnAvailability()
        {
            if (_sessionManager.CurrentUserId.HasValue)
            {
                var command = _commandFactory.CreateCommand(SetPhysicianAvailabilityCommand.Key);
                if (command == null)
                {
                    _console.DisplayMessage("SetPhysicianAvailability command not found.", MessageType.Error);
                    _console.Pause();
                    return;
                }
                CommandParameters parameters;
                try
                {
                    parameters = _commandParser.ParseInteractive(command);
                }
                catch (UserInputCancelledException)
                {
                    _console.DisplayMessage("Operation cancelled by user.", MessageType.Info);
                    _console.Pause();
                    return;
                }
                parameters[SetPhysicianAvailabilityCommand.Parameters.PhysicianId] = _sessionManager.CurrentUserId.Value;
                
                var result = _commandInvoker.Execute(command, parameters, _sessionManager.CurrentSession);
                
                if (result.Success)
                {
                    _console.DisplayMessage("Availability updated successfully.", MessageType.Success);
                }
                else
                {
                    _console.DisplayMessage($"Failed to update availability: {result.Message}", MessageType.Error);
                }
                _console.Pause();
            }
        }

        private void GenerateOwnPhysicianReport()
        {
            if (_sessionManager.CurrentUserId.HasValue)
            {
                var parameters = new CommandParameters();
                parameters[GeneratePhysicianReportCommand.Parameters.PhysicianId] = _sessionManager.CurrentUserId.Value;
                
                var command = _commandFactory.CreateCommand(GeneratePhysicianReportCommand.Key);
                if (command == null)
                {
                    _console.DisplayMessage("GeneratePhysicianReport command not found.", MessageType.Error);
                    _console.Pause();
                    return;
                }
                var result = _commandInvoker.Execute(command, parameters, _sessionManager.CurrentSession);
                
                if (result.Success)
                {
                    // Command handles its own display formatting
                    _console.DisplayMessage(result.Message ?? "Report generated successfully.", MessageType.Success);
                }
                else
                {
                    _console.DisplayMessage($"Failed to generate report: {result.Message}", MessageType.Error);
                }
                _console.Pause();
            }
        }


        private ConsoleMenu BuildSOAPUpdateMenu()
        {
            return new ConsoleMenu
            {
                Title = "Update SOAP Entry",
                Items = new List<ConsoleMenuItem>
                {
                    new ConsoleMenuItem
                    {
                        Key = "1",
                        Label = "Update Observation",
                        Description = "Update existing observation entry",
                        Action = () => ExecuteCommand(UpdateObservationCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "Update Diagnosis",
                        Description = "Update existing diagnosis entry",
                        Action = () => ExecuteCommand(UpdateDiagnosisCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "Update Prescription",
                        Description = "Update existing prescription entry",
                        Action = () => ExecuteCommand(UpdatePrescriptionCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "Update Assessment",
                        Description = "Update existing assessment entry",
                        Action = () => ExecuteCommand(UpdateAssessmentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "5",
                        Label = "Update Plan",
                        Description = "Update existing plan entry",
                        Action = () => ExecuteCommand(UpdatePlanCommand.Key)
                    }
                }
            };
        }

        private void LaunchDocumentEditor()
        {
            try
            {
                // Get document selection (similar to existing GetClinicalDocumentSelection pattern)
                var documentId = GetClinicalDocumentSelection();
                if (documentId == Guid.Empty)
                {
                    _console.DisplayMessage("No document selected for editing.", MessageType.Warning);
                    _console.Pause();
                    return;
                }

                // Get the document from registry
                var registry = ClinicalDocumentRegistry.Instance;
                var document = registry.GetDocumentById(documentId);
                
                if (document == null)
                {
                    _console.DisplayMessage($"Document with ID {documentId} not found.", MessageType.Error);
                    _console.Pause();
                    return;
                }

                // Launch the editor
                var editor = new ClinicalDocumentEditor(_sessionManager, _commandInvoker, _commandParser);
                editor.EditDocument(document);
            }
            catch (Exception ex)
            {
                _console.DisplayMessage($"Failed to launch document editor: {ex.Message}", MessageType.Error);
                _console.Pause();
            }
        }

        private Guid GetClinicalDocumentSelection()
        {
            // Reuse the existing clinical document selection logic from ConsoleCommandParser
            try
            {
                var registry = ClinicalDocumentRegistry.Instance;
                var allDocuments = registry.GetAllDocuments().ToList();
                
                if (allDocuments.Count == 0)
                {
                    _console.DisplayMessage("No clinical documents found. Create one first.", MessageType.Warning);
                    return Guid.Empty;
                }

                _console.DisplayMessage("Select Clinical Document:", MessageType.Info);
                _console.DisplayTable(allDocuments,
                    ("Date", d => d.CreatedAt.ToString("yyyy-MM-dd")),
                    ("Patient", d => d.PatientId.ToString("N")[..8] + "..."),
                    ("Status", d => d.IsCompleted ? "Completed" : "In Progress"),
                    ("Entries", d => d.Entries.Count.ToString()),
                    ("ID", d => d.Id.ToString())
                );

                _console.DisplayMessage("Enter selection (1-" + allDocuments.Count + "): ", MessageType.Info);
                var input = _console.GetUserInput("");
                
                if (string.IsNullOrEmpty(input))
                {
                    return Guid.Empty;
                }

                if (int.TryParse(input, out int selection) && 
                    selection >= 1 && selection <= allDocuments.Count)
                {
                    return allDocuments[selection - 1].Id;
                }

                _console.DisplayMessage("Invalid selection.", MessageType.Error);
                return Guid.Empty;
            }
            catch (Exception ex)
            {
                _console.DisplayMessage($"Error selecting document: {ex.Message}", MessageType.Error);
                return Guid.Empty;
            }
        }

        private void ExecuteUpdateClinicalDocument()
        {
            try
            {
                // Get document selection
                var documentId = GetClinicalDocumentSelection();
                if (documentId == Guid.Empty)
                {
                    _console.Pause();
                    return;
                }

                // Get the document
                var registry = ClinicalDocumentRegistry.Instance;
                var document = registry.GetDocumentById(documentId);
                if (document == null)
                {
                    _console.DisplayMessage("Document not found.", MessageType.Error);
                    _console.Pause();
                    return;
                }

                if (document.IsCompleted)
                {
                    _console.DisplayMessage("Cannot modify a completed clinical document.", MessageType.Error);
                    _console.Pause();
                    return;
                }

                // Show current chief complaint
                _console.DisplayMessage("\nCurrent Chief Complaint:", MessageType.Info);
                _console.DisplayMessage($"  {document.ChiefComplaint ?? "(not set)"}", MessageType.Debug);
                _console.DisplayMessage("");

                // Prompt for new chief complaint
                _console.DisplayMessage("Enter new Chief Complaint (or press Enter to keep current): ", MessageType.Info);
                var newChiefComplaint = _console.GetUserInput("");

                if (string.IsNullOrWhiteSpace(newChiefComplaint))
                {
                    _console.DisplayMessage("No changes made.", MessageType.Info);
                    _console.Pause();
                    return;
                }

                // Get command from factory
                var command = _commandFactory.CreateCommand(UpdateClinicalDocumentCommand.Key);
                if (command == null)
                {
                    _console.DisplayMessage("Update command not available.", MessageType.Error);
                    _console.Pause();
                    return;
                }

                // Build parameters
                var parameters = new CommandParameters()
                    .SetParameter(UpdateClinicalDocumentCommand.Parameters.DocumentId, documentId)
                    .SetParameter(UpdateClinicalDocumentCommand.Parameters.ChiefComplaint, newChiefComplaint.Trim());

                // Execute command
                var result = _commandInvoker.Execute(command, parameters, _sessionManager.CurrentSession);

                // Display result
                if (result.Success)
                {
                    _console.DisplayMessage("Chief complaint updated successfully!", MessageType.Success);
                }
                else
                {
                    _console.DisplayMessage($"Failed to update document: {result.Message}", MessageType.Error);
                }

                _console.Pause();
            }
            catch (Exception ex)
            {
                _console.DisplayMessage($"Error updating document: {ex.Message}", MessageType.Error);
                _console.Pause();
            }
        }

        private void ExecuteFinalizeClinicalDocument()
        {
            try
            {
                // Get document selection
                var documentId = GetClinicalDocumentSelection();
                if (documentId == Guid.Empty)
                {
                    _console.Pause();
                    return;
                }

                // Check if document is already completed
                var registry = ClinicalDocumentRegistry.Instance;
                var document = registry.GetDocumentById(documentId);
                if (document == null)
                {
                    _console.DisplayMessage("Document not found.", MessageType.Error);
                    _console.Pause();
                    return;
                }

                if (document.IsCompleted)
                {
                    _console.DisplayMessage("Document is already finalized.", MessageType.Warning);
                    _console.Pause();
                    return;
                }

                // Show completion status
                _console.DisplayMessage("\nDocument Completion Check:", MessageType.Info);
                var validationErrors = document.GetValidationErrors();
                if (validationErrors.Any())
                {
                    _console.DisplayMessage("Document cannot be finalized. Missing required entries:", MessageType.Warning);
                    foreach (var error in validationErrors)
                    {
                        _console.DisplayMessage($"  - {error}", MessageType.Error);
                    }
                    _console.Pause();
                    return;
                }

                // Confirm finalization
                _console.DisplayMessage("\nDocument is ready to be finalized.", MessageType.Success);
                _console.DisplayMessage("Once finalized, the document cannot be modified.", MessageType.Warning);
                _console.DisplayMessage("Finalize this document? (y/n): ", MessageType.Info);
                var confirm = _console.GetUserInput("");

                if (confirm?.ToLower() != "y")
                {
                    _console.DisplayMessage("Finalization cancelled.", MessageType.Info);
                    _console.Pause();
                    return;
                }

                // Get command from factory
                var command = _commandFactory.CreateCommand(UpdateClinicalDocumentCommand.Key);
                if (command == null)
                {
                    _console.DisplayMessage("Update command not available.", MessageType.Error);
                    _console.Pause();
                    return;
                }

                // Build parameters
                var parameters = new CommandParameters()
                    .SetParameter(UpdateClinicalDocumentCommand.Parameters.DocumentId, documentId)
                    .SetParameter(UpdateClinicalDocumentCommand.Parameters.Complete, true);

                // Execute command
                var result = _commandInvoker.Execute(command, parameters, _sessionManager.CurrentSession);

                // Display result
                if (result.Success)
                {
                    _console.DisplayMessage("Clinical document finalized successfully!", MessageType.Success);
                }
                else
                {
                    _console.DisplayMessage($"Failed to finalize document: {result.Message}", MessageType.Error);
                }

                _console.Pause();
            }
            catch (Exception ex)
            {
                _console.DisplayMessage($"Error finalizing document: {ex.Message}", MessageType.Error);
                _console.Pause();
            }
        }
    }
}