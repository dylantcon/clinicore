using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.CliniCore.Domain.Authentication;

namespace Core.CliniCore.Commands
{
    /// <summary>
    /// Defines the contract for all commands in the CliniCore system
    /// Implements Command Pattern for encapsulating requests
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Unique identifier for this command instance
        /// </summary>
        Guid CommandId { get; }

        /// <summary>
        /// Name of the command for display/logging purposes
        /// </summary>
        string CommandName { get; }

        /// <summary>
        /// Description of what this command does
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Whether this command can be undone
        /// </summary>
        bool CanUndo { get; }

        /// <summary>
        /// Validates that the command can be executed with given parameters
        /// </summary>
        /// <param name="parameters">The parameters for the command (will never be null, but may be empty)</param>
        /// <param name="session">The current session context (may be null if not authenticated)</param>
        /// <returns>Validation result with any error messages</returns>
        CommandValidationResult Validate(CommandParameters parameters, SessionContext? session);

        /// <summary>
        /// Executes the command with the given parameters
        /// </summary>
        /// <param name="parameters">The parameters for the command (will never be null, but may be empty)</param>
        /// <param name="session">The current session context (may be null if not authenticated)</param>
        /// <returns>Result of the command execution</returns>
        CommandResult Execute(CommandParameters parameters, SessionContext? session);

        /// <summary>
        /// Undoes the command if possible
        /// </summary>
        /// <param name="session">The current session context</param>
        /// <returns>Result of the undo operation</returns>
        CommandResult Undo(SessionContext? session);

        /// <summary>
        /// Gets the required permission to execute this command
        /// </summary>
        /// <returns>The permission required, or null if no specific permission needed</returns>
        Domain.Enumerations.Permission? GetRequiredPermission();
    }
}