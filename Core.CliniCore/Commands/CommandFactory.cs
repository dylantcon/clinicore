using System.Reflection;
using Core.CliniCore.Commands.Authentication;
using Core.CliniCore.Commands.Clinical;
using Core.CliniCore.Commands.Profile;
using Core.CliniCore.Commands.Scheduling;
using Core.CliniCore.Commands.Query;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Service;

namespace Core.CliniCore.Commands
{
    /// <summary>
    /// Factory for creating command instances based on command names or types
    /// Uses CommandKey property for consistent command identification
    /// </summary>
    public class CommandFactory
    {
        private readonly IAuthenticationService _authService;
        private readonly SchedulerService _schedulerService;
        private readonly ProfileService _profileService;
        private readonly ClinicalDocumentService _clinicalDocService;
        private readonly Dictionary<string, Type> _commandTypes;
        private readonly Dictionary<string, Func<ICommand>> _commandCreators;

        public CommandFactory(
            IAuthenticationService authService,
            SchedulerService scheduleManager,
            ProfileService profileService,
            ClinicalDocumentService clinicalDocService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _schedulerService = scheduleManager ?? throw new ArgumentNullException(nameof(scheduleManager));
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _clinicalDocService = clinicalDocService ?? throw new ArgumentNullException(nameof(clinicalDocService));
            _commandTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            _commandCreators = new Dictionary<string, Func<ICommand>>(StringComparer.OrdinalIgnoreCase);

            DiscoverCommands();
            RegisterCommands();
        }

        /// <summary>
        /// Discovers all command types in the assembly by reading static Key field
        /// </summary>
        private void DiscoverCommands()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var commandBaseType = typeof(ICommand);

            var commandTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract &&
                           !t.IsInterface &&
                           commandBaseType.IsAssignableFrom(t));

            foreach (var type in commandTypes)
            {
                // Read the static Key field without instantiation
                var key = GetCommandKeyFromType(type);
                if (key != null)
                {
                    _commandTypes[key] = type;
                }
            }
        }

        /// <summary>
        /// Gets the command key from a type's static Key field without instantiation
        /// </summary>
        private static string? GetCommandKeyFromType(Type type)
        {
            // Look for const or static field named "Key"
            var keyField = type.GetField("Key", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (keyField != null)
            {
                return keyField.GetRawConstantValue() as string ?? keyField.GetValue(null) as string;
            }
            return null;
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
            RegisterCommandWithKey(() => new CreatePatientCommand(_authService, _profileService));
            RegisterCommandWithKey(() => new CreatePhysicianCommand(_authService, _profileService));
            RegisterCommandWithKey(() => new CreateAdministratorCommand(_authService, _profileService));
            RegisterCommandWithKey(() => new AssignPatientToPhysicianCommand(_profileService));
            RegisterCommandWithKey(() => new ListPatientsCommand(_profileService));
            RegisterCommandWithKey(() => new ListPhysiciansCommand(_profileService));
            RegisterCommandWithKey(() => new ListProfileCommand(_profileService));
            RegisterCommandWithKey(() => new ViewProfileCommand(_profileService));
            RegisterCommandWithKey(() => new ViewPatientProfileCommand(_profileService));
            RegisterCommandWithKey(() => new ViewPhysicianProfileCommand(_profileService, _schedulerService));
            RegisterCommandWithKey(() => new ViewAdministratorProfileCommand(_profileService));
            RegisterCommandWithKey(() => new UpdateProfileCommand(_profileService));
            RegisterCommandWithKey(() => new UpdatePatientProfileCommand(_profileService));
            RegisterCommandWithKey(() => new UpdatePhysicianProfileCommand(_profileService));
            RegisterCommandWithKey(() => new UpdateAdministratorProfileCommand(_profileService));
            RegisterCommandWithKey(() => new DeleteProfileCommand(_profileService, _schedulerService, _clinicalDocService));

            // Scheduling Commands
            RegisterCommandWithKey(() => new ScheduleAppointmentCommand(_schedulerService, _profileService));
            RegisterCommandWithKey(() => new ListAppointmentsCommand(_schedulerService, _profileService));
            RegisterCommandWithKey(() => new ViewAppointmentCommand(_schedulerService, _profileService));
            RegisterCommandWithKey(() => new CancelAppointmentCommand(_schedulerService));
            RegisterCommandWithKey(() => new UpdateAppointmentCommand(_schedulerService));
            RegisterCommandWithKey(() => new DeleteAppointmentCommand(_schedulerService));
            RegisterCommandWithKey(() => new CheckConflictsCommand(_schedulerService));
            RegisterCommandWithKey(() => new GetAvailableTimeSlotsCommand(_schedulerService, _profileService));
            RegisterCommandWithKey(() => new SetPhysicianAvailabilityCommand());

            // Clinical Documentation Commands
            RegisterCommandWithKey(() => new CreateClinicalDocumentCommand(_profileService, _clinicalDocService));
            RegisterCommandWithKey(() => new AddDiagnosisCommand(_clinicalDocService));
            RegisterCommandWithKey(() => new AddPrescriptionCommand(_clinicalDocService));
            RegisterCommandWithKey(() => new AddObservationCommand(_clinicalDocService));
            RegisterCommandWithKey(() => new AddAssessmentCommand(_clinicalDocService));
            RegisterCommandWithKey(() => new AddPlanCommand(_clinicalDocService));
            RegisterCommandWithKey(() => new ListClinicalDocumentsCommand(_profileService, _clinicalDocService));
            RegisterCommandWithKey(() => new ViewClinicalDocumentCommand(_profileService, _clinicalDocService));
            RegisterCommandWithKey(() => new UpdateClinicalDocumentCommand(_clinicalDocService));
            RegisterCommandWithKey(() => new DeleteClinicalDocumentCommand(_clinicalDocService));
            RegisterCommandWithKey(() => new UpdateObservationCommand(_clinicalDocService));
            RegisterCommandWithKey(() => new UpdateDiagnosisCommand(_clinicalDocService));
            RegisterCommandWithKey(() => new UpdatePrescriptionCommand(_clinicalDocService));
            RegisterCommandWithKey(() => new UpdateAssessmentCommand(_clinicalDocService));
            RegisterCommandWithKey(() => new UpdatePlanCommand(_clinicalDocService));

            // Query Commands
            RegisterCommandWithKey(() => new SearchPatientsCommand(_profileService));
            RegisterCommandWithKey(() => new SearchClinicalNotesCommand(_profileService, _clinicalDocService));
            RegisterCommandWithKey(() => new FindPhysiciansBySpecializationCommand(_profileService));
            RegisterCommandWithKey(() => new FindPhysiciansByAvailabilityCommand(_profileService, _schedulerService));
            RegisterCommandWithKey(() => new GetScheduleCommand(_profileService, _schedulerService));
            RegisterCommandWithKey(() => new ListAllUsersCommand(_profileService));

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
        /// Registers a command creator using the command type's static Key field
        /// </summary>
        private void RegisterCommandWithKey<TCommand>(Func<TCommand> creator) where TCommand : ICommand
        {
            var commandType = typeof(TCommand);
            var key = GetCommandKeyFromType(commandType);

            if (key != null)
            {
                _commandCreators[key] = () => creator();
                _commandTypes[key] = commandType;
            }
            else
            {
                Console.WriteLine($"Warning: Command {commandType.Name} missing static Key field");
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