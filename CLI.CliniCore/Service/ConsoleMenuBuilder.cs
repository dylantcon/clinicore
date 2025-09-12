using System;
using System.Collections.Generic;
using System.Linq;
using CLI.CliniCore.Service.Editor;
using Core.CliniCore.ClinicalDoc;
using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Profile;
using Core.CliniCore.Commands.Reports;
using Core.CliniCore.Commands.Scheduling;
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
                    Action = () => ExecuteCommand(CommandNames.Login)
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
                    Action = () => ExecuteCommand(CommandNames.ChangePassword)
                });

                menu.Items.Add(new ConsoleMenuItem
                {
                    Key = "L",
                    Label = "Logout",
                    Description = "End current session",
                    Action = () => ExecuteCommand(CommandNames.Logout)
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
                Description = "Manage system users",
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

            menu.Items.Add(new ConsoleMenuItem
            {
                Key = "6",
                Label = "Reports",
                Description = "Generate system reports",
                SubMenuFactory = BuildReportsMenu
            });

            menu.Items.Add(new ConsoleMenuItem
            {
                Key = "7",
                Label = "System Administration",
                Description = "System settings and maintenance",
                SubMenuFactory = BuildSystemAdminMenu
            });
        }

        private void AddPhysicianMenuItems(ConsoleMenu menu)
        {
            menu.Items.Add(new ConsoleMenuItem
            {
                Key = "1",
                Label = "My Profile",
                Description = "View and manage your profile",
                Action = () => ViewOwnProfile()
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
                Label = "Scheduling",
                Description = "Manage appointments and availability",
                SubMenuFactory = BuildPhysicianSchedulingMenu
            });

            menu.Items.Add(new ConsoleMenuItem
            {
                Key = "4",
                Label = "Clinical Documents",
                Description = "Manage clinical documentation",
                SubMenuFactory = BuildClinicalDocumentsMenu
            });

            menu.Items.Add(new ConsoleMenuItem
            {
                Key = "5",
                Label = "Reports",
                Description = "Generate reports",
                SubMenuFactory = BuildPhysicianReportsMenu
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
                Action = () => ExecuteCommand(CommandNames.ListAppointments)
            });

            menu.Items.Add(new ConsoleMenuItem
            {
                Key = "3",
                Label = "My Clinical Documents",
                Description = "View your medical records",
                Action = () => ExecuteCommand(CommandNames.ListClinicalDocuments)
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
                        Action = () => ExecuteCommand(CommandNames.CreateAdministrator)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "List All Users",
                        Description = "View all system users",
                        Action = () => ListAllProfiles()
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "Delete User",
                        Description = "Remove a user from the system",
                        Action = () => ExecuteCommand(CommandNames.DeleteProfile)
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
                        Action = () => ExecuteCommand(CommandNames.CreatePatient)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "List Patients",
                        Description = "View all patients",
                        Action = () => ExecuteCommand(CommandNames.ListPatients)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "View Patient Profile",
                        Description = "View detailed patient information",
                        Action = () => ExecuteCommand(CommandNames.ViewProfile)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "Search Patients",
                        Description = "Search for patients by name",
                        Action = () => ExecuteCommand(CommandNames.SearchPatients)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "5",
                        Label = "Assign Patient to Physician",
                        Description = "Set primary physician for patient",
                        Action = () => ExecuteCommand(CommandNames.AssignPatientToPhysician)
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
                        Action = () => ExecuteCommand(CommandNames.CreatePhysician)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "List Physicians",
                        Description = "View all physicians",
                        Action = () => ExecuteCommand(CommandNames.ListPhysicians)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "View Physician Profile",
                        Description = "View detailed physician information",
                        Action = () => ExecuteCommand(CommandNames.ViewProfile)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "Find by Specialization",
                        Description = "Find physicians by medical specialization",
                        Action = () => ExecuteCommand(CommandNames.FindPhysiciansBySpecialization)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "5",
                        Label = "Find by Availability",
                        Description = "Find available physicians for a time slot",
                        Action = () => ExecuteCommand(CommandNames.FindPhysiciansByAvailability)
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
                        Action = () => ExecuteCommand(CommandNames.ScheduleAppointment)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "List Appointments",
                        Description = "View all appointments",
                        Action = () => ExecuteCommand(CommandNames.ListAppointments)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "View Appointment",
                        Description = "View appointment details",
                        Action = () => ExecuteCommand(CommandNames.ViewAppointment)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "Reschedule Appointment",
                        Description = "Change appointment time",
                        Action = () => ExecuteCommand(CommandNames.RescheduleAppointment)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "5",
                        Label = "Cancel Appointment",
                        Description = "Cancel an appointment",
                        Action = () => ExecuteCommand(CommandNames.CancelAppointment)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "6",
                        Label = "Check Available Time Slots",
                        Description = "Find available appointment times",
                        Action = () => ExecuteCommand(CommandNames.GetAvailableTimeSlots)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "7",
                        Label = "Check Conflicts",
                        Description = "Check for scheduling conflicts",
                        Action = () => ExecuteCommand(CommandNames.CheckConflicts)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "8",
                        Label = "Set Physician Availability",
                        Description = "Configure physician working hours",
                        Action = () => ExecuteCommand(CommandNames.SetPhysicianAvailability)
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
                        Action = () => ExecuteCommand(CommandNames.ScheduleAppointment)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "My Appointments",
                        Description = "View your appointments",
                        Action = () => ExecuteCommand(CommandNames.ListAppointments)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "View Appointment",
                        Description = "View appointment details",
                        Action = () => ExecuteCommand(CommandNames.ViewAppointment)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "Reschedule Appointment",
                        Description = "Change appointment time",
                        Action = () => ExecuteCommand(CommandNames.RescheduleAppointment)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "5",
                        Label = "Cancel Appointment",
                        Description = "Cancel an appointment",
                        Action = () => ExecuteCommand(CommandNames.CancelAppointment)
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
                        Action = () => ExecuteCommand(CommandNames.GetSchedule)
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
                        Action = () => ExecuteCommand(CommandNames.CreateClinicalDocument)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "List Clinical Documents",
                        Description = "View all clinical documents",
                        Action = () => ExecuteCommand(CommandNames.ListClinicalDocuments)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "View Clinical Document",
                        Description = "View document details",
                        Action = () => ExecuteCommand(CommandNames.ViewClinicalDocument)
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
                        Description = "Complete or update document status",
                        Action = () => ExecuteCommand(CommandNames.UpdateClinicalDocument)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "6",
                        Label = "Search Clinical Notes",
                        Description = "Search within clinical documents",
                        Action = () => ExecuteCommand(CommandNames.SearchClinicalNotes)
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
                        Action = () => ExecuteCommand(CommandNames.AddObservation)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "Add Diagnosis (O)",
                        Description = "Add objective diagnosis",
                        Action = () => ExecuteCommand(CommandNames.AddDiagnosis)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "Add Prescription (O)",
                        Description = "Add prescription linked to diagnosis",
                        Action = () => ExecuteCommand(CommandNames.AddPrescription)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "Add Assessment (A)",
                        Description = "Add clinical assessment",
                        Action = () => ExecuteCommand(CommandNames.AddAssessment)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "5",
                        Label = "Add Plan (P)",
                        Description = "Add treatment plan",
                        Action = () => ExecuteCommand(CommandNames.AddPlan)
                    }
                }
            };
        }

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
                        Action = () => ExecuteCommand(CommandNames.GeneratePatientReport)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "Physician Report",
                        Description = "Generate physician report",
                        Action = () => ExecuteCommand(CommandNames.GeneratePhysicianReport)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "Appointment Report",
                        Description = "Generate appointment report",
                        Action = () => ExecuteCommand(CommandNames.GenerateAppointmentReport)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "Facility Report",
                        Description = "Generate facility-wide report",
                        Action = () => ExecuteCommand(CommandNames.GenerateFacilityReport)
                    }
                }
            };
        }

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
                        Action = () => ExecuteCommand(CommandNames.GeneratePatientReport)
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
                        Action = () => ExecuteCommand(CommandNames.GenerateAppointmentReport)
                    }
                }
            };
        }

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
                        Action = () => ExecuteCommand(CommandNames.CreateFacility)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "Update Facility Settings",
                        Description = "Modify facility configuration",
                        Action = () => ExecuteCommand(CommandNames.UpdateFacilitySettings)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "Manage User Roles",
                        Description = "Manage user role assignments",
                        Action = () => ExecuteCommand(CommandNames.ManageUserRoles)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "View Audit Log",
                        Description = "View system audit trail",
                        Action = () => ExecuteCommand(CommandNames.ViewAuditLog)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "5",
                        Label = "System Maintenance",
                        Description = "Perform system maintenance tasks",
                        Action = () => ExecuteCommand(CommandNames.SystemMaintenance)
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
                    if (commandName == CommandNames.Logout)
                    {
                        _sessionManager.EndSession();
                    }
                    else if (commandName == CommandNames.Login && result.Data is SessionContext session)
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
                
                var command = _commandFactory.CreateCommand(CommandNames.ViewProfile);
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
                var command = _commandFactory.CreateCommand(CommandNames.SetPhysicianAvailability);
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
                
                var command = _commandFactory.CreateCommand(CommandNames.GeneratePhysicianReport);
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

        private void ListAllProfiles()
        {
            ExecuteCommand(CommandNames.ListPatients);
            ExecuteCommand(CommandNames.ListPhysicians);
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
    }
}