using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Commands
{
    /// <summary>
    /// Represents the result of command validation.
    /// </summary>
    public class CommandValidationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the validation passed.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the list of validation errors.
        /// </summary>
        public List<string> Errors { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of non-blocking validation warnings.
        /// </summary>
        public List<string> Warnings { get; set; } = [];

        /// <summary>
        /// Adds an error to the validation result.
        /// </summary>
        /// <param name="error">The error message to add.</param>
        /// <returns>The current <see cref="CommandValidationResult"/> instance for method chaining.</returns>
        public CommandValidationResult AddError(string error)
        {
            Errors.Add(error);
            IsValid = false;
            return this;
        }

        /// <summary>
        /// Adds a warning to the validation result.
        /// </summary>
        /// <param name="warning">The warning message to add.</param>
        /// <returns>The current <see cref="CommandValidationResult"/> instance for method chaining.</returns>
        public CommandValidationResult AddWarning(string warning)
        {
            Warnings.Add(warning);
            return this;
        }

        /// <summary>
        /// Creates a successful validation result.
        /// </summary>
        /// <returns>A <see cref="CommandValidationResult"/> indicating validation success.</returns>
        public static CommandValidationResult Success()
        {
            return new CommandValidationResult { IsValid = true };
        }

        /// <summary>
        /// Creates a failed validation result with a single error message.
        /// </summary>
        /// <param name="error">The validation error message.</param>
        /// <returns>A <see cref="CommandValidationResult"/> indicating validation failure.</returns>
        public static CommandValidationResult Failure(string error)
        {
            return new CommandValidationResult
            {
                IsValid = false,
                Errors = [error]
            };
        }

        /// <summary>
        /// Creates a failed validation result with multiple error messages.
        /// </summary>
        /// <param name="errors">The validation error messages.</param>
        /// <returns>A <see cref="CommandValidationResult"/> indicating validation failure.</returns>
        public static CommandValidationResult Failure(params string[] errors)
        {
            return new CommandValidationResult
            {
                IsValid = false,
                Errors = errors?.ToList() ?? []
            };
        }

        /// <summary>
        /// Creates a failed validation result with the specified list of error messages.
        /// </summary>
        /// <param name="errors">The validation error messages.</param>
        /// <returns>A <see cref="CommandValidationResult"/> indicating validation failure.</returns>
        public static CommandValidationResult Failure(List<string> errors)
        {
            return new CommandValidationResult
            {
                IsValid = false,
                Errors = errors ?? []
            };
        }

        /// <summary>
        /// Merges another validation result into this instance.
        /// </summary>
        /// <param name="other">The other validation result to merge.</param>
        /// <returns>The current <see cref="CommandValidationResult"/> instance for method chaining.</returns>
        public CommandValidationResult Merge(CommandValidationResult other)
        {
            if (other != null)
            {
                Errors.AddRange(other.Errors);
                Warnings.AddRange(other.Warnings);
                IsValid = IsValid && other.IsValid;
            }
            return this;
        }

        /// <summary>
        /// Gets a user-friendly display message describing the validation result.
        /// </summary>
        /// <returns>A formatted message indicating whether validation passed and any errors or warnings.</returns>
        public string GetDisplayMessage()
        {
            if (IsValid)
            {
                return Warnings.Count != 0
                    ? $"Validation passed with warnings: {string.Join("; ", Warnings)}"
                    : "Validation passed.";
            }
            else
            {
                return $"Validation failed: {string.Join("; ", Errors)}";
            }
        }

        /// <summary>
        /// Returns a string that represents the current <see cref="CommandValidationResult"/>.
        /// </summary>
        /// <returns>A formatted string containing validation status, error count, and warning count.</returns>
        public override string ToString()
        {
            return $"CommandValidationResult[IsValid={IsValid}, Errors={Errors.Count}, Warnings={Warnings.Count}]";
        }
    }
}