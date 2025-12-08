using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Service;

namespace Core.CliniCore.Commands
{
    /// <summary>
    /// Base implementation for all commands providing common infrastructure
    /// </summary>
    public abstract class AbstractCommand : ICommand
    {
        private readonly RoleBasedAuthorizationService _authorizationService;

        /// <summary>
        /// Stores the parameters used in the most recent command execution.
        /// </summary>
        /// <remarks>This field is intended for use by derived classes to access or track the parameters
        /// passed to the last executed command. The value may be <c>null</c> if no command has been executed yet or if
        /// the parameters were not specified.</remarks>
        protected CommandParameters? _lastExecutionParameters;

        /// <summary>
        /// Stores the session context from the most recent execution.
        /// </summary>
        /// <remarks>This field is intended for use by derived classes to access or track the session
        /// associated with the last operation. The value may be <c>null</c> if no execution has occurred or if the
        /// session is unavailable.</remarks>
        protected SessionContext? _lastExecutionSession;

        /// <summary>
        /// Stores the previous state of the object to support undo operations.
        /// </summary>
        /// <remarks>This field is used to retain the state prior to a change, enabling the ability to
        /// revert to an earlier state if needed.</remarks>
        protected object? _previousState; // For undo functionality

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractCommand"/> class with a unique command identifier and
        /// default role-based authorization service.
        /// </summary>
        /// <remarks>The <see cref="CommandId"/> property is assigned a new <see cref="Guid"/> value for
        /// each instance. The command is configured to use a role-based authorization service by default.</remarks>
        protected AbstractCommand()
        {
            CommandId = Guid.NewGuid();
            _authorizationService = new RoleBasedAuthorizationService();
        }

        /// <summary>
        /// Unique identifier for this command instance
        /// </summary>
        public Guid CommandId { get; }

        /// <summary>
        /// Name of the command - defaults to class name without "Command" suffix
        /// </summary>
        public virtual string CommandName
        {
            get
            {
                var typeName = GetType().Name;
                return typeName.EndsWith("Command")
                    ? typeName.Substring(0, typeName.Length - 7)
                    : typeName;
            }
        }

        /// <summary>
        /// Unique key identifier for this command type
        /// Generated from the class name, converted to lowercase without "Command" suffix
        /// E.g., "LoginCommand" becomes "login", "CreatePatientCommand" becomes "createpatient"
        /// </summary>
        public virtual string CommandKey
        {
            get
            {
                var typeName = GetType().Name;
                var nameWithoutCommand = typeName.EndsWith("Command")
                    ? typeName.Substring(0, typeName.Length - 7)
                    : typeName;
                return nameWithoutCommand.ToLowerInvariant();
            }
        }

        /// <summary>
        /// Description of what this command does
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Whether this command can be undone - defaults to false
        /// </summary>
        public virtual bool CanUndo => false;

        /// <summary>
        /// Gets the required permission to execute this command
        /// </summary>
        public abstract Permission? GetRequiredPermission();

        /// <summary>
        /// Validates that the command can be executed
        /// </summary>
        public virtual CommandValidationResult Validate(CommandParameters parameters, SessionContext? session)
        {
            var result = CommandValidationResult.Success();

            // Check authorization first
            var authResult = ValidateAuthorization(session);
            if (!authResult.IsValid)
            {
                return authResult;
            }

            // Validate session is not expired
            if (session != null && session.IsExpired())
            {
                return CommandValidationResult.Failure("Session has expired. Please log in again.");
            }

            // Validate parameters
            var paramResult = ValidateParameters(parameters);
            result.Merge(paramResult);

            // Perform command-specific validation
            var specificResult = ValidateSpecific(parameters, session);
            result.Merge(specificResult);

            return result;
        }

        /// <summary>
        /// Executes the command
        /// </summary>
        public CommandResult Execute(CommandParameters parameters, SessionContext? session)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Ensure we have non-null parameters to work with
                parameters = parameters ?? new CommandParameters();

                // Store for potential undo
                _lastExecutionParameters = parameters.Clone();
                _lastExecutionSession = session;

                // Validate first
                var validationResult = Validate(parameters, session);
                if (!validationResult.IsValid)
                {
                    return CommandResult.ValidationFailed(validationResult.Errors);
                }

                // Update session activity
                session?.UpdateActivity();

                // Log command execution (would go to audit log in production)
                LogExecution(session);

                // Store state for undo if supported
                if (CanUndo)
                {
                    _previousState = CaptureStateForUndo(parameters, session);
                }

                // Execute the actual command logic
                var result = ExecuteCore(parameters, session);

                // Add any validation warnings to the result
                foreach (var warning in validationResult.Warnings)
                {
                    result.AddWarning(warning);
                }

                // Set execution metadata
                result.CommandId = CommandId;
                result.ExecutionTime = stopwatch.Elapsed;

                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                return CommandResult.Unauthorized(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return CommandResult.Fail($"Invalid arguments: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Command execution failed: {ex.Message}", ex);
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        /// <summary>
        /// Undoes the command if possible
        /// </summary>
        public virtual CommandResult Undo(SessionContext? session)
        {
            if (!CanUndo)
            {
                return CommandResult.Fail("This command does not support undo.");
            }

            if (_previousState == null)
            {
                return CommandResult.Fail("No previous state available for undo.");
            }

            try
            {
                // Check authorization for undo
                var permission = GetRequiredPermission();
                if (permission.HasValue && !_authorizationService.Authorize(session, permission.Value))
                {
                    return CommandResult.Unauthorized("Not authorized to undo this command.");
                }

                // Perform the undo
                var result = UndoCore(_previousState, session);

                // Clear the stored state after successful undo
                if (result.Success)
                {
                    _previousState = null;
                }

                return result;
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Undo failed: {ex.Message}", ex);
            }
        }

        #region Protected Methods for Derived Classes

        /// <summary>
        /// Validates authorization for the command
        /// </summary>
        protected virtual CommandValidationResult ValidateAuthorization(SessionContext? session)
        {
            var permission = GetRequiredPermission();

            // If no specific permission required, allow execution
            if (!permission.HasValue)
            {
                return CommandValidationResult.Success();
            }

            // Check if session exists
            if (session == null)
            {
                return CommandValidationResult.Failure("Authentication required. Please log in.");
            }

            // Check permission
            if (!_authorizationService.Authorize(session, permission.Value))
            {
                return CommandValidationResult.Failure(
                    $"You do not have permission to execute {CommandName}. " +
                    $"Required permission: {permission.Value}");
            }

            return CommandValidationResult.Success();
        }

        /// <summary>
        /// Validates command parameters - override in derived classes
        /// </summary>
        /// <param name="parameters">The parameters to validate (never null, but may be empty)</param>
        /// <returns>Validation result indicating success or failure with error messages</returns>
        protected abstract CommandValidationResult ValidateParameters(CommandParameters parameters);

        /// <summary>
        /// Performs command-specific validation - override in derived classes if needed
        /// </summary>
        protected virtual CommandValidationResult ValidateSpecific(CommandParameters parameters, SessionContext? session)
        {
            return CommandValidationResult.Success();
        }

        /// <summary>
        /// Executes the core command logic - must be implemented by derived classes
        /// </summary>
        protected abstract CommandResult ExecuteCore(CommandParameters parameters, SessionContext? session);

        /// <summary>
        /// Captures state before execution for undo - override if undo is supported
        /// </summary>
        protected virtual object? CaptureStateForUndo(CommandParameters parameters, SessionContext? session)
        {
            return null;
        }

        /// <summary>
        /// Performs the undo operation - override if undo is supported
        /// </summary>
        protected virtual CommandResult UndoCore(object previousState, SessionContext? session)
        {
            return CommandResult.Fail("Undo not implemented for this command.");
        }

        /// <summary>
        /// Logs command execution for audit trail
        /// </summary>
        protected virtual void LogExecution(SessionContext? session)
        {
            // In production, this would write to an audit log
            var username = session?.Username ?? "Anonymous";
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            // TODO: remove Console.WriteLine usage here
            Console.WriteLine($"[{timestamp}] Command '{CommandName}' executed by user '{username}'");
        }

        #endregion

        /// <summary>
        /// Returns a string that represents the current command, including its name and identifier in a formatted form.
        /// </summary>
        /// <returns>A string containing the command name followed by the command identifier in a hyphenated, 32-digit format.</returns>
        public override string ToString()
        {
            return $"{CommandName} [{CommandId:N}]";
        }
    }
}