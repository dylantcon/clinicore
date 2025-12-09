using System;
using System.Collections.Generic;
using CLI.CliniCore.Service.Menu;
using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Authentication;
using Core.CliniCore.Commands.Clinical;
using Core.CliniCore.Commands.Profile;
using Core.CliniCore.Commands.Query;
using Core.CliniCore.Commands.Reports;
using Core.CliniCore.Commands.Scheduling;
using Core.CliniCore.Commands.Admin;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Service;

namespace CLI.CliniCore.Service
{
    /// <summary>
    /// Builds console menus based on user role and session state.
    /// Delegates command execution to MenuExecutor.
    /// </summary>
    public class ConsoleMenuBuilder
    {
        private readonly ConsoleSessionManager _sessionManager;
        private readonly IConsoleEngine _console;
        private readonly MenuExecutor _menuExecutor;

        public ConsoleMenuBuilder(
            CommandInvoker commandInvoker,
            CommandFactory commandFactory,
            ConsoleSessionManager sessionManager,
            ConsoleCommandParser commandParser,
            IConsoleEngine console,
            ClinicalDocumentService clinicalDocService)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _menuExecutor = new MenuExecutor(commandInvoker, commandFactory, sessionManager, commandParser, console, clinicalDocService);
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
                    Action = () => _menuExecutor.ExecuteCommand(LoginCommand.Key)
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
                    Action = () => _menuExecutor.ExecuteCommand(ChangePasswordCommand.Key)
                });

                menu.Items.Add(new ConsoleMenuItem
                {
                    Key = "L",
                    Label = "Logout",
                    Description = "End current session",
                    Action = () => _menuExecutor.ExecuteCommand(LogoutCommand.Key)
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
                Action = () => _menuExecutor.ViewOwnProfile()
            });
        }

        private void AddPatientMenuItems(ConsoleMenu menu)
        {
            menu.Items.Add(new ConsoleMenuItem
            {
                Key = "1",
                Label = "My Profile",
                Description = "View your profile information",
                Action = () => _menuExecutor.ViewOwnProfile()
            });

            menu.Items.Add(new ConsoleMenuItem
            {
                Key = "2",
                Label = "My Appointments",
                Description = "View your appointments",
                Action = () => _menuExecutor.ExecuteCommand(ListAppointmentsCommand.Key)
            });

            menu.Items.Add(new ConsoleMenuItem
            {
                Key = "3",
                Label = "My Clinical Documents",
                Description = "View your medical records",
                Action = () => _menuExecutor.ExecuteCommand(ListClinicalDocumentsCommand.Key)
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
                        Action = () => _menuExecutor.ExecuteCommand(CreateAdministratorCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "List All Users",
                        Description = "View all system users",
                        Action = () => _menuExecutor.ExecuteCommand(ListAllUsersCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "Update User Profile",
                        Description = "Update any user profile",
                        Action = () => _menuExecutor.ExecuteCommand(UpdateProfileCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "Delete User",
                        Description = "Remove a user from the system",
                        Action = () => _menuExecutor.ExecuteCommand(DeleteProfileCommand.Key)
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
                        Action = () => _menuExecutor.ExecuteCommand(CreatePatientCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "List Patients",
                        Description = "View all patients",
                        Action = () => _menuExecutor.ExecuteCommand(ListPatientsCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "View Patient Profile",
                        Description = "View detailed patient information",
                        Action = () => _menuExecutor.ExecuteCommand(ViewPatientProfileCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "Update Patient Profile",
                        Description = "Update patient information",
                        Action = () => _menuExecutor.ExecuteCommand("updatepatientprofile")
                    },
                    new ConsoleMenuItem
                    {
                        Key = "5",
                        Label = "Search Patients",
                        Description = "Search for patients by name",
                        Action = () => _menuExecutor.ExecuteCommand(SearchPatientsCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "6",
                        Label = "Assign Patient to Physician",
                        Description = "Set primary physician for patient",
                        Action = () => _menuExecutor.ExecuteCommand(AssignPatientToPhysicianCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "7",
                        Label = "Delete Patient",
                        Description = "Remove a patient from the system",
                        Action = () => _menuExecutor.ExecuteCommand(DeleteProfileCommand.Key)
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
                        Action = () => _menuExecutor.ExecuteCommand(CreatePhysicianCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "List Physicians",
                        Description = "View all physicians",
                        Action = () => _menuExecutor.ExecuteCommand(ListPhysiciansCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "View Physician Profile",
                        Description = "View detailed physician information",
                        Action = () => _menuExecutor.ExecuteCommand(ViewPhysicianProfileCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "Update Physician Profile",
                        Description = "Update physician information",
                        Action = () => _menuExecutor.ExecuteCommand("updatephysicianprofile")
                    },
                    new ConsoleMenuItem
                    {
                        Key = "5",
                        Label = "Find by Specialization",
                        Description = "Find physicians by medical specialization",
                        Action = () => _menuExecutor.ExecuteCommand(FindPhysiciansBySpecializationCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "6",
                        Label = "Find by Availability",
                        Description = "Find available physicians for a time slot",
                        Action = () => _menuExecutor.ExecuteCommand(FindPhysiciansByAvailabilityCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "7",
                        Label = "Delete Physician",
                        Description = "Remove a physician from the system",
                        Action = () => _menuExecutor.ExecuteCommand(DeleteProfileCommand.Key)
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
                        Action = () => _menuExecutor.ExecuteCommand(ScheduleAppointmentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "List Appointments",
                        Description = "View all appointments",
                        Action = () => _menuExecutor.ExecuteCommand(ListAppointmentsCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "View Appointment",
                        Description = "View appointment details",
                        Action = () => _menuExecutor.ExecuteCommand(ViewAppointmentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "Update Appointment",
                        Description = "Change appointment time, duration, or details",
                        Action = () => _menuExecutor.ExecuteCommand(UpdateAppointmentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "5",
                        Label = "Cancel Appointment",
                        Description = "Cancel an appointment",
                        Action = () => _menuExecutor.ExecuteCommand(CancelAppointmentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "6",
                        Label = "Check Available Time Slots",
                        Description = "Find available appointment times",
                        Action = () => _menuExecutor.ExecuteCommand(GetAvailableTimeSlotsCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "7",
                        Label = "Check Conflicts",
                        Description = "Check for scheduling conflicts",
                        Action = () => _menuExecutor.ExecuteCommand(CheckConflictsCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "8",
                        Label = "Set Physician Availability",
                        Description = "Configure physician working hours",
                        Action = () => _menuExecutor.ExecuteCommand(SetPhysicianAvailabilityCommand.Key)
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
                        Action = () => _menuExecutor.ExecuteCommand(ScheduleAppointmentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "My Appointments",
                        Description = "View your appointments",
                        Action = () => _menuExecutor.ExecuteCommand(ListAppointmentsCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "View Appointment",
                        Description = "View appointment details",
                        Action = () => _menuExecutor.ExecuteCommand(ViewAppointmentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "Update Appointment",
                        Description = "Change appointment time, duration, or details",
                        Action = () => _menuExecutor.ExecuteCommand(UpdateAppointmentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "5",
                        Label = "Cancel Appointment",
                        Description = "Cancel an appointment",
                        Action = () => _menuExecutor.ExecuteCommand(CancelAppointmentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "6",
                        Label = "Set My Availability",
                        Description = "Configure your working hours",
                        Action = () => _menuExecutor.SetOwnAvailability()
                    },
                    new ConsoleMenuItem
                    {
                        Key = "7",
                        Label = "View My Schedule",
                        Description = "View your schedule",
                        Action = () => _menuExecutor.ExecuteCommand(GetScheduleCommand.Key)
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
                        Action = () => _menuExecutor.LaunchDocumentEditor()
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
                        Action = () => _menuExecutor.ExecuteCommand(CreateClinicalDocumentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "List Clinical Documents",
                        Description = "View all clinical documents",
                        Action = () => _menuExecutor.ExecuteCommand(ListClinicalDocumentsCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "View Clinical Document",
                        Description = "View document details",
                        Action = () => _menuExecutor.ExecuteCommand(ViewClinicalDocumentCommand.Key)
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
                        Action = () => _menuExecutor.ExecuteUpdateClinicalDocument()
                    },
                    new ConsoleMenuItem
                    {
                        Key = "7",
                        Label = "Finalize Clinical Document",
                        Description = "Complete and finalize a clinical document",
                        Action = () => _menuExecutor.ExecuteFinalizeClinicalDocument()
                    },
                    new ConsoleMenuItem
                    {
                        Key = "8",
                        Label = "Delete Clinical Document",
                        Description = "Delete a clinical document",
                        Action = () => _menuExecutor.ExecuteCommand(DeleteClinicalDocumentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "9",
                        Label = "Search Clinical Notes",
                        Description = "Search within clinical documents",
                        Action = () => _menuExecutor.ExecuteCommand(SearchClinicalNotesCommand.Key)
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
                        Action = () => _menuExecutor.ExecuteCommand(AddObservationCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "Add Diagnosis (O)",
                        Description = "Add objective diagnosis",
                        Action = () => _menuExecutor.ExecuteCommand(AddDiagnosisCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "Add Prescription (O)",
                        Description = "Add prescription linked to diagnosis",
                        Action = () => _menuExecutor.ExecuteCommand(AddPrescriptionCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "Add Assessment (A)",
                        Description = "Add clinical assessment",
                        Action = () => _menuExecutor.ExecuteCommand(AddAssessmentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "5",
                        Label = "Add Plan (P)",
                        Description = "Add treatment plan",
                        Action = () => _menuExecutor.ExecuteCommand(AddPlanCommand.Key)
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
                        Action = () => _menuExecutor.ExecuteCommand(GeneratePatientReportCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "Physician Report",
                        Description = "Generate physician report",
                        Action = () => _menuExecutor.ExecuteCommand(GeneratePhysicianReportCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "Appointment Report",
                        Description = "Generate appointment report",
                        Action = () => _menuExecutor.ExecuteCommand(GenerateAppointmentReportCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "Facility Report",
                        Description = "Generate facility-wide report",
                        Action = () => _menuExecutor.ExecuteCommand(GenerateFacilityReportCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "5",
                        Label = "List All Profiles",
                        Description = "View all user profiles in the system",
                        Action = () => _menuExecutor.ExecuteCommand(ListProfileCommand.Key)
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
                        Action = () => _menuExecutor.ExecuteCommand(GeneratePatientReportCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "My Performance Report",
                        Description = "Generate your performance report",
                        Action = () => _menuExecutor.GenerateOwnPhysicianReport()
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "Appointment Report",
                        Description = "Generate appointment report",
                        Action = () => _menuExecutor.ExecuteCommand(GenerateAppointmentReportCommand.Key)
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
                        Action = () => _menuExecutor.ExecuteCommand(CreateFacilityCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "Update Facility Settings",
                        Description = "Modify facility configuration",
                        Action = () => _menuExecutor.ExecuteCommand(UpdateFacilitySettingsCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "Manage User Roles",
                        Description = "Manage user role assignments",
                        Action = () => _menuExecutor.ExecuteCommand(ManageUserRolesCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "View Audit Log",
                        Description = "View system audit trail",
                        Action = () => _menuExecutor.ExecuteCommand(ViewAuditLogCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "5",
                        Label = "System Maintenance",
                        Description = "Perform system maintenance tasks",
                        Action = () => _menuExecutor.ExecuteCommand(SystemMaintenanceCommand.Key)
                    }
                }
            };
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
                        Action = () => _menuExecutor.ExecuteCommand(UpdateObservationCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "2",
                        Label = "Update Diagnosis",
                        Description = "Update existing diagnosis entry",
                        Action = () => _menuExecutor.ExecuteCommand(UpdateDiagnosisCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "3",
                        Label = "Update Prescription",
                        Description = "Update existing prescription entry",
                        Action = () => _menuExecutor.ExecuteCommand(UpdatePrescriptionCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "4",
                        Label = "Update Assessment",
                        Description = "Update existing assessment entry",
                        Action = () => _menuExecutor.ExecuteCommand(UpdateAssessmentCommand.Key)
                    },
                    new ConsoleMenuItem
                    {
                        Key = "5",
                        Label = "Update Plan",
                        Description = "Update existing plan entry",
                        Action = () => _menuExecutor.ExecuteCommand(UpdatePlanCommand.Key)
                    }
                }
            };
        }

    }
}