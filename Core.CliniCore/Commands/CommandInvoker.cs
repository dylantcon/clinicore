using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.CliniCore.Domain.Authentication;

namespace Core.CliniCore.Commands
{
    /// <summary>
    /// Orchestrates command execution, manages command history, and handles undo/redo operations
    /// </summary>
    public class CommandInvoker
    {
        private readonly Stack<ICommand> _executedCommands;
        private readonly Stack<ICommand> _undoneCommands;
        private readonly List<CommandExecutionRecord> _commandHistory;
        private readonly object _lock = new object();

        public CommandInvoker()
        {
            _executedCommands = new Stack<ICommand>();
            _undoneCommands = new Stack<ICommand>();
            _commandHistory = new List<CommandExecutionRecord>();
        }

        /// <summary>
        /// Gets the command execution history
        /// </summary>
        public IReadOnlyList<CommandExecutionRecord> History => _commandHistory.AsReadOnly();

        /// <summary>
        /// Whether there are commands that can be undone
        /// </summary>
        public bool CanUndo => _executedCommands.Any(cmd => cmd.CanUndo);

        /// <summary>
        /// Whether there are commands that can be redone
        /// </summary>
        public bool CanRedo => _undoneCommands.Any();

        /// <summary>
        /// Executes a command with the given parameters
        /// </summary>
        public CommandResult Execute(ICommand command, CommandParameters parameters, SessionContext? session)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            lock (_lock)
            {
                // Ensure we have non-null parameters
                parameters = parameters ?? new CommandParameters();

                // Execute the command
                var result = command.Execute(parameters, session);

                // Record the execution
                var record = new CommandExecutionRecord
                {
                    CommandId = command.CommandId,
                    CommandName = command.CommandName,
                    ExecutedAt = DateTime.Now,
                    ExecutedBy = session?.Username ?? "System",
                    Success = result.Success,
                    Message = result.GetDisplayMessage(),
                    ExecutionTime = result.ExecutionTime
                };
                _commandHistory.Add(record);

                // If successful, add to executed stack (for undo)
                if (result.Success)
                {
                    _executedCommands.Push(command);

                    // Clear redo stack when a new command is executed
                    _undoneCommands.Clear();
                }

                return result;
            }
        }

        /// <summary>
        /// Executes a command asynchronously
        /// </summary>
        public async Task<CommandResult> ExecuteAsync(ICommand command, CommandParameters parameters, SessionContext? session)
        {
            return await Task.Run(() => Execute(command, parameters, session));
        }

        /// <summary>
        /// Undoes the last executed command that supports undo
        /// </summary>
        public CommandResult Undo(SessionContext? session)
        {
            lock (_lock)
            {
                // Find the last command that can be undone
                ICommand? commandToUndo = null;
                var tempStack = new Stack<ICommand>();

                while (_executedCommands.Count > 0)
                {
                    var cmd = _executedCommands.Pop();
                    if (cmd.CanUndo)
                    {
                        commandToUndo = cmd;
                        break;
                    }
                    tempStack.Push(cmd);
                }

                // Restore commands that can't be undone
                while (tempStack.Count > 0)
                {
                    _executedCommands.Push(tempStack.Pop());
                }

                if (commandToUndo == null)
                {
                    return CommandResult.Fail("No commands available to undo.");
                }

                // Perform the undo
                var result = commandToUndo.Undo(session);

                // Record the undo
                var record = new CommandExecutionRecord
                {
                    CommandId = commandToUndo.CommandId,
                    CommandName = $"UNDO: {commandToUndo.CommandName}",
                    ExecutedAt = DateTime.Now,
                    ExecutedBy = session?.Username ?? "System",
                    Success = result.Success,
                    Message = result.GetDisplayMessage(),
                    ExecutionTime = result.ExecutionTime
                };
                _commandHistory.Add(record);

                // If successful, move to undone stack (for redo)
                if (result.Success)
                {
                    _undoneCommands.Push(commandToUndo);
                }
                else
                {
                    // If undo failed, restore to executed stack
                    _executedCommands.Push(commandToUndo);
                }

                return result;
            }
        }

        /// <summary>
        /// Redoes the last undone command
        /// </summary>
        public CommandResult Redo(CommandParameters parameters, SessionContext? session)
        {
            lock (_lock)
            {
                if (_undoneCommands.Count == 0)
                {
                    return CommandResult.Fail("No commands available to redo.");
                }

                var commandToRedo = _undoneCommands.Pop();

                // Re-execute the command
                var result = Execute(commandToRedo, parameters, session);

                // Record as a redo in history
                if (_commandHistory.Count > 0)
                {
                    var lastRecord = _commandHistory[_commandHistory.Count - 1];
                    lastRecord.CommandName = $"REDO: {commandToRedo.CommandName}";
                }

                return result;
            }
        }

        /// <summary>
        /// Executes multiple commands as a batch (transaction-like)
        /// </summary>
        public BatchCommandResult ExecuteBatch(
            IEnumerable<(ICommand Command, CommandParameters Parameters)> commands,
            SessionContext? session,
            bool stopOnFirstFailure = true)
        {
            var results = new List<CommandResult>();
            var executedCommands = new List<ICommand>();

            lock (_lock)
            {
                foreach (var (command, parameters) in commands)
                {
                    var result = Execute(command, parameters, session);
                    results.Add(result);

                    if (result.Success)
                    {
                        executedCommands.Add(command);
                    }
                    else if (stopOnFirstFailure)
                    {
                        // Rollback: undo all successfully executed commands in reverse order
                        for (int i = executedCommands.Count - 1; i >= 0; i--)
                        {
                            if (executedCommands[i].CanUndo)
                            {
                                executedCommands[i].Undo(session);
                            }
                        }
                        break;
                    }
                }
            }

            return new BatchCommandResult
            {
                IndividualResults = results,
                AllSucceeded = results.All(r => r.Success),
                SuccessCount = results.Count(r => r.Success),
                FailureCount = results.Count(r => !r.Success)
            };
        }

        /// <summary>
        /// Clears the command history
        /// </summary>
        public void ClearHistory()
        {
            lock (_lock)
            {
                _executedCommands.Clear();
                _undoneCommands.Clear();
                _commandHistory.Clear();
            }
        }

        /// <summary>
        /// Gets commands available for undo
        /// </summary>
        public IEnumerable<string> GetUndoableCommands()
        {
            lock (_lock)
            {
                return _executedCommands
                    .Where(cmd => cmd.CanUndo)
                    .Select(cmd => cmd.CommandName)
                    .ToList();
            }
        }

        /// <summary>
        /// Gets commands available for redo
        /// </summary>
        public IEnumerable<string> GetRedoableCommands()
        {
            lock (_lock)
            {
                return _undoneCommands
                    .Select(cmd => cmd.CommandName)
                    .ToList();
            }
        }
    }

    /// <summary>
    /// Record of a command execution for history/audit
    /// </summary>
    public class CommandExecutionRecord
    {
        public Guid CommandId { get; set; }
        public string CommandName { get; set; } = string.Empty;
        public DateTime ExecutedAt { get; set; }
        public string ExecutedBy { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public TimeSpan? ExecutionTime { get; set; }

        public override string ToString()
        {
            var status = Success ? "SUCCESS" : "FAILED";
            var time = ExecutionTime?.TotalMilliseconds ?? 0;
            return $"[{ExecutedAt:yyyy-MM-dd HH:mm:ss}] {CommandName} by {ExecutedBy} - {status} ({time:F0}ms)";
        }
    }

    /// <summary>
    /// Result of executing multiple commands as a batch
    /// </summary>
    public class BatchCommandResult
    {
        public List<CommandResult> IndividualResults { get; set; } = new List<CommandResult>();
        public bool AllSucceeded { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }

        public string GetSummary()
        {
            if (AllSucceeded)
            {
                return $"All {SuccessCount} commands executed successfully.";
            }
            else
            {
                return $"Batch execution completed: {SuccessCount} succeeded, {FailureCount} failed.";
            }
        }
    }
}
