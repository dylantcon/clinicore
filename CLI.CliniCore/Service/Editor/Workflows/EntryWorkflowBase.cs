using System;
using System.Collections.Generic;
using Core.CliniCore.Commands;

namespace CLI.CliniCore.Service.Editor.Workflows
{
    /// <summary>
    /// Base class for multi-step entry creation workflows.
    /// Provides common functionality for step tracking, data collection, and enum parsing.
    /// </summary>
    public abstract class EntryWorkflowBase : IEntryWorkflow
    {
        protected readonly Dictionary<string, string> _collectedData = new();
        protected int _currentStep;

        public bool IsComplete { get; protected set; }
        public bool IsCancelled { get; protected set; }
        public string? ErrorMessage { get; protected set; }

        public abstract string CurrentPrompt { get; }
        public abstract string DefaultValue { get; }
        public abstract string CommandKey { get; }

        /// <summary>
        /// Process user input for the current step
        /// </summary>
        public abstract void ProcessInput(string input);

        /// <summary>
        /// Build command parameters from collected data
        /// </summary>
        public abstract CommandParameters? BuildParameters(Guid documentId);

        /// <summary>
        /// Cancel the workflow
        /// </summary>
        public void Cancel()
        {
            IsCancelled = true;
        }

        /// <summary>
        /// Store a collected value
        /// </summary>
        protected void SetData(string key, string value)
        {
            _collectedData[key] = value;
        }

        /// <summary>
        /// Get a collected value, or default if not set
        /// </summary>
        protected string GetData(string key, string defaultValue = "")
        {
            return _collectedData.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// Check if a key has been collected
        /// </summary>
        protected bool HasData(string key)
        {
            return _collectedData.ContainsKey(key) && !string.IsNullOrWhiteSpace(_collectedData[key]);
        }

        /// <summary>
        /// Move to the next step
        /// </summary>
        protected void NextStep()
        {
            _currentStep++;
            ErrorMessage = null;
        }

        /// <summary>
        /// Complete the workflow successfully
        /// </summary>
        protected void Complete()
        {
            IsComplete = true;
            ErrorMessage = null;
        }

        /// <summary>
        /// Set an error without cancelling (allows retry)
        /// </summary>
        protected void SetError(string message)
        {
            ErrorMessage = message;
        }

        /// <summary>
        /// Cancel with an error message
        /// </summary>
        protected void CancelWithError(string message)
        {
            ErrorMessage = message;
            IsCancelled = true;
        }

        /// <summary>
        /// Get the last character from input (for single-character selections)
        /// </summary>
        protected static char GetLastChar(string input)
        {
            return input.Length > 0 ? char.ToUpper(input[input.Length - 1]) : ' ';
        }

        /// <summary>
        /// Try to parse an enum from input, checking abbreviation, display name, and raw name
        /// </summary>
        protected static bool TryParseEnum<T>(string input, Func<T, string> getAbbreviation, Func<T, string> getDisplayName, T[] allValues, out T result) where T : struct, Enum
        {
            result = default;
            var trimmed = input?.Trim() ?? "";

            foreach (var value in allValues)
            {
                if (value.ToString().Equals(trimmed, StringComparison.OrdinalIgnoreCase) ||
                    getDisplayName(value).Equals(trimmed, StringComparison.OrdinalIgnoreCase) ||
                    getAbbreviation(value).Equals(trimmed, StringComparison.OrdinalIgnoreCase))
                {
                    result = value;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Simple enum parse by name only
        /// </summary>
        protected static bool TryParseEnumByName<T>(string input, out T result) where T : struct, Enum
        {
            return Enum.TryParse(input?.Trim() ?? "", true, out result);
        }
    }
}
