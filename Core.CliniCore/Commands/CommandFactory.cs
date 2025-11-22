using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.CliniCore.Commands.Authentication;
using Core.CliniCore.Commands.Clinical;
using Core.CliniCore.Commands.Profile;
using Core.CliniCore.Commands.Scheduling;
using Core.CliniCore.Commands.Admin;
using Core.CliniCore.Commands.Query;
using Core.CliniCore.Commands.Reports;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Scheduling.Management;
using Core.CliniCore.ClinicalDoc;

namespace Core.CliniCore.Commands
{
    /// <summary>
    /// Factory for creating command instances based on command names or types
    /// Uses CommandKey property for consistent command identification
    /// </summary>
    public class CommandFactory
    {
        private readonly IAuthenticationService _authService;
        private readonly ScheduleManager _scheduleManager;
        private readonly Dictionary<string, Type> _commandTypes;
        private readonly Dictionary<string, Func<ICommand>> _commandCreators;

        public CommandFactory(IAuthenticationService authService, ScheduleManager scheduleManager)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _scheduleManager = scheduleManager ?? throw new ArgumentNullException(nameof(scheduleManager));
            _commandTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            _commandCreators = new Dictionary<string, Func<ICommand>>(StringComparer.OrdinalIgnoreCase);

            DiscoverCommands();
            RegisterCommands();
        }

        /// <summary>
        /// Discovers all command types in the assembly
        /// </summary>
        private void DiscoverCommands()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var commandBaseType = typeof(ICommand);
            
            var commandTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract && 
                           !t.IsInterface && 
                           commandBaseType.IsAssignableFrom(t) &&
                           t.GetConstructor(Type.EmptyTypes) != null); // Has parameterless constructor for discovery

            foreach (var type in commandTypes)
            {
                try
                {
                    // Create a temporary instance to get the CommandKey
                    var tempInstance = Activator.CreateInstance(type) as ICommand;
                    if (tempInstance != null)
                    {
                        _commandTypes[tempInstance.CommandKey] = type;
                    }
                }
                catch
                {
                    // Skip commands that can't be instantiated for discovery
                    // They'll be registered manually in RegisterCommands
                }
            }
        }

        /// <summary>
        /// Registers all available commands with their specific dependencies
        /// </summary>
        private void RegisterCommands()
        {
            // Authentication Commands - register using CommandKey from instance
            RegisterCommandWithKey(() => new LoginCommand(_authService));
            RegisterCommandWithKey(() => new LogoutCommand());
            //RegisterCommandWithKey(() => new ChangePasswordCommand(_authService)); // Skip if constructor issue

            // Profile Management Commands
            RegisterCommandWithKey(() => new CreatePatientCommand(_authService));
            RegisterCommandWithKey(() => new CreatePhysicianCommand(_authService));
            RegisterCommandWithKey(() => new CreateAdministratorCommand(_authService));
            RegisterCommandWithKey(() => new AssignPatientToPhysicianCommand());
            RegisterCommandWithKey(() => new ListPatientsCommand());
            RegisterCommandWithKey(() => new ListPhysiciansCommand());
            RegisterCommandWithKey(() => new ListProfileCommand());
            RegisterCommandWithKey(() => new ViewProfileCommand());
            RegisterCommandWithKey(() => new ViewPatientProfileCommand());
            RegisterCommandWithKey(() => new ViewPhysicianProfileCommand());
            RegisterCommandWithKey(() => new ViewAdministratorProfileCommand());
            RegisterCommandWithKey(() => new UpdateProfileCommand());
            RegisterCommandWithKey(() => new UpdatePatientProfileCommand());
            RegisterCommandWithKey(() => new UpdatePhysicianProfileCommand());
            RegisterCommandWithKey(() => new UpdateAdministratorProfileCommand());
            RegisterCommandWithKey(() => new DeleteProfileCommand());

            // Scheduling Commands
            RegisterCommandWithKey(() => new ScheduleAppointmentCommand(_scheduleManager));
            RegisterCommandWithKey(() => new ListAppointmentsCommand(_scheduleManager));
            RegisterCommandWithKey(() => new ViewAppointmentCommand(_scheduleManager));
            RegisterCommandWithKey(() => new CancelAppointmentCommand(_scheduleManager));
            RegisterCommandWithKey(() => new UpdateAppointmentCommand(_scheduleManager));
            RegisterCommandWithKey(() => new DeleteAppointmentCommand(_scheduleManager));
            RegisterCommandWithKey(() => new CheckConflictsCommand(_scheduleManager));
            RegisterCommandWithKey(() => new GetAvailableTimeSlotsCommand(_scheduleManager)); // Now properly implemented
            RegisterCommandWithKey(() => new SetPhysicianAvailabilityCommand()); // Now has proper implementation

            // Clinical Documentation Commands
            RegisterCommandWithKey(() => new CreateClinicalDocumentCommand());
            RegisterCommandWithKey(() => new AddDiagnosisCommand());
            RegisterCommandWithKey(() => new AddPrescriptionCommand());
            RegisterCommandWithKey(() => new AddObservationCommand());
            RegisterCommandWithKey(() => new AddAssessmentCommand());
            RegisterCommandWithKey(() => new AddPlanCommand());
            RegisterCommandWithKey(() => new ListClinicalDocumentsCommand());
            RegisterCommandWithKey(() => new ViewClinicalDocumentCommand());
            RegisterCommandWithKey(() => new UpdateClinicalDocumentCommand());
            RegisterCommandWithKey(() => new DeleteClinicalDocumentCommand());
            RegisterCommandWithKey(() => new UpdateObservationCommand());
            RegisterCommandWithKey(() => new UpdateDiagnosisCommand());
            RegisterCommandWithKey(() => new UpdatePrescriptionCommand());
            RegisterCommandWithKey(() => new UpdateAssessmentCommand());
            RegisterCommandWithKey(() => new UpdatePlanCommand());

            // Query Commands
            RegisterCommandWithKey(() => new SearchPatientsCommand());
            RegisterCommandWithKey(() => new SearchClinicalNotesCommand());
            RegisterCommandWithKey(() => new FindPhysiciansBySpecializationCommand());
            RegisterCommandWithKey(() => new FindPhysiciansByAvailabilityCommand());
            RegisterCommandWithKey(() => new GetScheduleCommand());
            RegisterCommandWithKey(() => new ListAllUsersCommand());

            // Report Commands (unimplemented)
            //RegisterCommandWithKey(() => new GeneratePatientReportCommand());
            //RegisterCommandWithKey(() => new GeneratePhysicianReportCommand());
            //RegisterCommandWithKey(() => new GenerateAppointmentReportCommand());
            //RegisterCommandWithKey(() => new GenerateFacilityReportCommand());

            // Admin Commands (unimplemented)
            //RegisterCommandWithKey(() => new CreateFacilityCommand());
            //RegisterCommandWithKey(() => new UpdateFacilitySettingsCommand());
            //RegisterCommandWithKey(() => new ManageUserRolesCommand());
            //RegisterCommandWithKey(() => new ViewAuditLogCommand());
            //RegisterCommandWithKey(() => new SystemMaintenanceCommand());

            // Register common aliases using actual command keys
            RegisterAlias("signin", "login");
            RegisterAlias("signout", "logout");
        }

        /// <summary>
        /// Registers a command creator using the command's CommandKey property
        /// </summary>
        private void RegisterCommandWithKey(Func<ICommand> creator)
        {
            try
            {
                // Create a temporary instance to get the CommandKey
                var tempInstance = creator();
                var commandKey = tempInstance.CommandKey;
                
                _commandCreators[commandKey] = creator;
                _commandTypes[commandKey] = tempInstance.GetType();
            }
            catch (Exception ex)
            {
                // Skip commands that can't be instantiated due to missing dependencies
                Console.WriteLine($"Warning: Could not register command due to: {ex.Message}");
            }
        }

        /// <summary>
        /// Registers an alias for a command
        /// </summary>
        private void RegisterAlias(string alias, string commandKey)
        {
            if (_commandCreators.TryGetValue(commandKey, out var creator))
            {
                _commandCreators[alias] = creator;
            }
        }

        /// <summary>
        /// Creates a command by name or key
        /// </summary>
        public ICommand? CreateCommand(string commandIdentifier)
        {
            if (string.IsNullOrWhiteSpace(commandIdentifier))
                return null;

            // Normalize the identifier
            commandIdentifier = commandIdentifier.ToLowerInvariant();
            
            // Remove "Command" suffix if present
            if (commandIdentifier.EndsWith("command"))
            {
                commandIdentifier = commandIdentifier.Substring(0, commandIdentifier.Length - 7);
            }

            if (_commandCreators.TryGetValue(commandIdentifier, out var creator))
            {
                return creator();
            }

            return null;
        }

        /// <summary>
        /// Creates a command with pre-populated parameters
        /// </summary>
        public (ICommand? Command, CommandParameters Parameters) CreateCommandWithParameters(
            string commandIdentifier,
            Dictionary<string, object?> parameterValues)
        {
            var command = CreateCommand(commandIdentifier);
            if (command == null)
                return (null, new CommandParameters());

            var parameters = new CommandParameters(parameterValues);
            return (command, parameters);
        }

        /// <summary>
        /// Gets all available command keys
        /// </summary>
        public IEnumerable<string> GetAvailableCommands()
        {
            return _commandTypes.Keys
                .OrderBy(k => k)
                .ToList();
        }

        /// <summary>
        /// Gets the command type for a given key
        /// </summary>
        public Type? GetCommandType(string commandKey)
        {
            return _commandTypes.TryGetValue(commandKey, out var type) ? type : null;
        }

        /// <summary>
        /// Gets command help information
        /// </summary>
        public string GetCommandHelp(string commandIdentifier)
        {
            var command = CreateCommand(commandIdentifier);
            if (command == null)
                return $"Unknown command: {commandIdentifier}";

            return $"{command.CommandName}: {command.Description}";
        }

        /// <summary>
        /// Checks if a command exists
        /// </summary>
        public bool CommandExists(string commandIdentifier)
        {
            if (string.IsNullOrWhiteSpace(commandIdentifier))
                return false;
                
            commandIdentifier = commandIdentifier.ToLowerInvariant();
            
            if (commandIdentifier.EndsWith("command"))
            {
                commandIdentifier = commandIdentifier.Substring(0, commandIdentifier.Length - 7);
            }
            
            return _commandCreators.ContainsKey(commandIdentifier);
        }

        /// <summary>
        /// Gets commands available for a specific role
        /// </summary>
        public IEnumerable<string> GetCommandsForRole(Domain.Enumerations.UserRole role)
        {
            var availableCommands = new List<string>();

            foreach (var kvp in _commandTypes)
            {
                // Try to create instance to check permissions
                if (_commandCreators.TryGetValue(kvp.Key, out var creator))
                {
                    try
                    {
                        var command = creator();
                        var permission = command.GetRequiredPermission();

                        // Check if role has permission for this command
                        bool hasAccess = role switch
                        {
                            Domain.Enumerations.UserRole.Administrator => true, // Admins can do everything
                            Domain.Enumerations.UserRole.Physician => permission != Domain.Enumerations.Permission.ViewSystemReports,
                            Domain.Enumerations.UserRole.Patient => permission == null ||
                                permission == Domain.Enumerations.Permission.ViewOwnProfile ||
                                permission == Domain.Enumerations.Permission.EditOwnProfile ||
                                permission == Domain.Enumerations.Permission.ViewOwnAppointments ||
                                permission == Domain.Enumerations.Permission.ScheduleOwnAppointment ||
                                permission == Domain.Enumerations.Permission.ViewOwnClinicalDocuments,
                            _ => false
                        };

                        if (hasAccess)
                        {
                            availableCommands.Add(kvp.Key);
                        }
                    }
                    catch
                    {
                        // Skip commands that can't be instantiated
                    }
                }
            }

            return availableCommands.Distinct().OrderBy(c => c);
        }
    }
}