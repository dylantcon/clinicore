using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Commands
{
    /// <summary>
    /// Represents the result of a command execution.
    /// </summary>
    public class CommandResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the command executed successfully.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets a message describing the result of the command.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets any data returned by the command.
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// Gets or sets error details if the command failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the exception that occurred during command execution, if any.
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// Gets or sets the collection of validation errors produced during command execution.
        /// </summary>
        public List<string> ValidationErrors { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the ID of the command that produced this result.
        /// </summary>
        public Guid CommandId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the command was executed.
        /// </summary>
        public DateTime ExecutedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the duration of the command execution.
        /// </summary>
        public TimeSpan? ExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the collection of warnings generated during command execution.
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// Additional named data returned by the command, keyed by a descriptive name.
        /// </summary>
        private readonly Dictionary<string, object?> _additionalData = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Sets a named data value in the result.
        /// </summary>
        /// <param name="key">The key under which the data value is stored.</param>
        /// <param name="value">The value to store.</param>
        /// <returns>The current <see cref="CommandResult"/> instance for method chaining.</returns>
        public CommandResult SetData(string key, object? value)
        {
            _additionalData[key] = value;
            return this;
        }

        /// <summary>
        /// Gets a named data value from the result.
        /// </summary>
        /// <typeparam name="T">The expected type of the stored value.</typeparam>
        /// <param name="key">The key associated with the stored value.</param>
        /// <returns>The value converted to <typeparamref name="T"/>, or the default value of <typeparamref name="T"/> if not found or not convertible.</returns>
        public T? GetData<T>(string key)
        {
            if (!_additionalData.TryGetValue(key, out var value) || value == null)
                return default;

            if (value is T typedValue)
                return typedValue;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Gets the primary data payload of the result, converted to the specified type.
        /// </summary>
        /// <typeparam name="T">The expected type of the primary data.</typeparam>
        /// <returns>The data converted to <typeparamref name="T"/>, or the default value of <typeparamref name="T"/> if not set or not convertible.</returns>
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
        /// Creates a successful <see cref="CommandResult"/> instance.
        /// </summary>
        /// <param name="message">An optional message describing the success.</param>
        /// <param name="data">Optional data returned by the command.</param>
        /// <returns>A configured <see cref="CommandResult"/> indicating success.</returns>
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
        /// Creates a failed <see cref="CommandResult"/> instance.
        /// </summary>
        /// <param name="errorMessage">A message describing the cause of the failure.</param>
        /// <param name="exception">The exception that triggered the failure, if available.</param>
        /// <returns>A configured <see cref="CommandResult"/> indicating failure.</returns>
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
        /// Creates a <see cref="CommandResult"/> representing a validation failure.
        /// </summary>
        /// <param name="errors">The collection of validation errors.</param>
        /// <returns>A configured <see cref="CommandResult"/> indicating validation failure.</returns>
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
        /// Creates a <see cref="CommandResult"/> representing an authorization failure.
        /// </summary>
        /// <param name="message">An optional message describing the authorization issue.</param>
        /// <returns>A configured <see cref="CommandResult"/> indicating unauthorized access.</returns>
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
        /// Adds a warning message to the result.
        /// </summary>
        /// <param name="warning">The warning message to add.</param>
        /// <returns>The current <see cref="CommandResult"/> instance for method chaining.</returns>
        public CommandResult AddWarning(string warning)
        {
            Warnings.Add(warning);
            return this;
        }

        /// <summary>
        /// Gets a user-friendly message summarizing the outcome of the command execution.
        /// </summary>
        /// <returns>A human-readable message that describes the result.</returns>
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

        /// <summary>
        /// Returns a string that represents the current <see cref="CommandResult"/>.
        /// </summary>
        /// <returns>A formatted string containing the success status and primary message.</returns>
        public override string ToString()
        {
            return $"CommandResult[Success={Success}, Message={Message}]";
        }
    }
}