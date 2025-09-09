using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Commands
{
    /// <summary>
    /// Represents the result of a command execution
    /// </summary>
    public class CommandResult
    {
        /// <summary>
        /// Whether the command executed successfully
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Message describing the result
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Any data returned by the command
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// Error details if the command failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Exception if one occurred
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// Validation errors if validation failed
        /// </summary>
        public List<string> ValidationErrors { get; set; } = new List<string>();

        /// <summary>
        /// The ID of the command that produced this result
        /// </summary>
        public Guid CommandId { get; set; }

        /// <summary>
        /// When the command was executed
        /// </summary>
        public DateTime ExecutedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// How long the command took to execute
        /// </summary>
        public TimeSpan? ExecutionTime { get; set; }

        /// <summary>
        /// Any warnings that occurred during execution
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// Gets typed data from the result
        /// </summary>
        public T? GetData<T>()
        {
            if (Data == null)
                return default;

            if (Data is T typedData)
                return typedData;

            try
            {
                return (T)Convert.ChangeType(Data, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Factory method for successful result
        /// </summary>
        public static CommandResult Ok(string message = "Command executed successfully.", object? data = null)
        {
            return new CommandResult
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// Factory method for failed result
        /// </summary>
        public static CommandResult Fail(string errorMessage, Exception? exception = null)
        {
            return new CommandResult
            {
                Success = false,
                Message = "Command execution failed.",
                ErrorMessage = errorMessage,
                Exception = exception
            };
        }

        /// <summary>
        /// Factory method for validation failure
        /// </summary>
        public static CommandResult ValidationFailed(List<string> errors)
        {
            return new CommandResult
            {
                Success = false,
                Message = "Command validation failed.",
                ValidationErrors = errors ?? new List<string>(),
                ErrorMessage = errors?.FirstOrDefault() ?? "Validation failed."
            };
        }

        /// <summary>
        /// Factory method for authorization failure
        /// </summary>
        public static CommandResult Unauthorized(string message = "Unauthorized to execute this command.")
        {
            return new CommandResult
            {
                Success = false,
                Message = message,
                ErrorMessage = message
            };
        }

        /// <summary>
        /// Adds a warning to the result
        /// </summary>
        public CommandResult AddWarning(string warning)
        {
            Warnings.Add(warning);
            return this;
        }

        /// <summary>
        /// Gets a user-friendly message about the result
        /// </summary>
        public string GetDisplayMessage()
        {
            if (Success)
            {
                return Message;
            }
            else if (ValidationErrors.Any())
            {
                return $"Validation failed: {string.Join("; ", ValidationErrors)}";
            }
            else if (!string.IsNullOrWhiteSpace(ErrorMessage))
            {
                return ErrorMessage;
            }
            else
            {
                return Message;
            }
        }

        public override string ToString()
        {
            return $"CommandResult[Success={Success}, Message={Message}]";
        }
    }
}