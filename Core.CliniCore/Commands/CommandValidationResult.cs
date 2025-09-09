using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Commands
{
    /// <summary>
    /// Represents the result of command validation
    /// </summary>
    public class CommandValidationResult
    {
        /// <summary>
        /// Whether the validation passed
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// List of validation errors
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// List of validation warnings (non-blocking)
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// Adds an error to the validation result
        /// </summary>
        public CommandValidationResult AddError(string error)
        {
            Errors.Add(error);
            IsValid = false;
            return this;
        }

        /// <summary>
        /// Adds a warning to the validation result
        /// </summary>
        public CommandValidationResult AddWarning(string warning)
        {
            Warnings.Add(warning);
            return this;
        }

        /// <summary>
        /// Factory method for successful validation
        /// </summary>
        public static CommandValidationResult Success()
        {
            return new CommandValidationResult { IsValid = true };
        }

        /// <summary>
        /// Factory method for failed validation with single error
        /// </summary>
        public static CommandValidationResult Failure(string error)
        {
            return new CommandValidationResult
            {
                IsValid = false,
                Errors = new List<string> { error }
            };
        }

        /// <summary>
        /// Factory method for failed validation with multiple errors
        /// </summary>
        public static CommandValidationResult Failure(params string[] errors)
        {
            return new CommandValidationResult
            {
                IsValid = false,
                Errors = errors?.ToList() ?? new List<string>()
            };
        }

        /// <summary>
        /// Factory method for failed validation with error list
        /// </summary>
        public static CommandValidationResult Failure(List<string> errors)
        {
            return new CommandValidationResult
            {
                IsValid = false,
                Errors = errors ?? new List<string>()
            };
        }

        /// <summary>
        /// Merges another validation result into this one
        /// </summary>
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
        /// Gets a display message for the validation result
        /// </summary>
        public string GetDisplayMessage()
        {
            if (IsValid)
            {
                return Warnings.Any()
                    ? $"Validation passed with warnings: {string.Join("; ", Warnings)}"
                    : "Validation passed.";
            }
            else
            {
                return $"Validation failed: {string.Join("; ", Errors)}";
            }
        }

        public override string ToString()
        {
            return $"CommandValidationResult[IsValid={IsValid}, Errors={Errors.Count}, Warnings={Warnings.Count}]";
        }
    }
}