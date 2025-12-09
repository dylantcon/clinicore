using System;
using Core.CliniCore.Commands;

namespace CLI.CliniCore.Service.Editor.Workflows
{
    /// <summary>
    /// Interface for multi-step entry creation workflows in the clinical document editor.
    /// Each workflow handles collecting user input for a specific entry type (observation, diagnosis, etc.)
    /// </summary>
    public interface IEntryWorkflow
    {
        /// <summary>
        /// Gets the prompt to display to the user for the current step
        /// </summary>
        string CurrentPrompt { get; }

        /// <summary>
        /// Gets the default/initial value for the current step's input
        /// </summary>
        string DefaultValue { get; }

        /// <summary>
        /// Gets whether the workflow has completed successfully
        /// </summary>
        bool IsComplete { get; }

        /// <summary>
        /// Gets whether the workflow was cancelled
        /// </summary>
        bool IsCancelled { get; }

        /// <summary>
        /// Gets any error message from the last step
        /// </summary>
        string? ErrorMessage { get; }

        /// <summary>
        /// Gets the command key to execute when workflow completes
        /// </summary>
        string CommandKey { get; }

        /// <summary>
        /// Process user input for the current step and advance to the next step
        /// </summary>
        /// <param name="input">The user's input</param>
        void ProcessInput(string input);

        /// <summary>
        /// Cancel the workflow
        /// </summary>
        void Cancel();

        /// <summary>
        /// Build the command parameters from collected data
        /// </summary>
        /// <param name="documentId">The document ID to add the entry to</param>
        /// <returns>Parameters for the add command, or null if incomplete</returns>
        CommandParameters? BuildParameters(Guid documentId);
    }
}
