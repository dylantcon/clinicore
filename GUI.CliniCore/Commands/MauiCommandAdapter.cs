using System;
// Type aliases to distinguish between the two ICommand interfaces
using CoreCommand = Core.CliniCore.Commands.ICommand;
using MauiCommand = System.Windows.Input.ICommand;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication;
using GUI.CliniCore.ViewModels;

namespace GUI.CliniCore.Commands
{
    /// <summary>
    /// Adapts Core.CliniCore.Commands.ICommand (GoF Command Pattern) to System.Windows.Input.ICommand (MAUI Command Pattern)
    /// This adapter allows rich enterprise commands with validation, authorization, and undo capabilities
    /// to be used seamlessly in MAUI's declarative XAML binding system.
    /// </summary>
    public class MauiCommandAdapter : MauiCommand
    {
        private readonly CoreCommand _coreCommand;
        private readonly Func<CommandParameters> _parameterBuilder;
        private readonly Func<SessionContext?> _sessionProvider;
        private readonly Action<CommandResult> _resultHandler;
        private readonly BaseViewModel _viewModel;

        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Creates an adapter that wraps a Core command for use with MAUI binding
        /// </summary>
        /// <param name="coreCommand">The Core.CliniCore command to wrap</param>
        /// <param name="parameterBuilder">Function to build CommandParameters from ViewModel state</param>
        /// <param name="sessionProvider">Function to get current SessionContext</param>
        /// <param name="resultHandler">Action to handle CommandResult (update ViewModel, navigate, etc.)</param>
        /// <param name="viewModel">The ViewModel that owns this command (for validation updates)</param>
        public MauiCommandAdapter(
            CoreCommand coreCommand,
            Func<CommandParameters> parameterBuilder,
            Func<SessionContext?> sessionProvider,
            Action<CommandResult> resultHandler,
            BaseViewModel viewModel)
        {
            _coreCommand = coreCommand ?? throw new ArgumentNullException(nameof(coreCommand));
            _parameterBuilder = parameterBuilder ?? throw new ArgumentNullException(nameof(parameterBuilder));
            _sessionProvider = sessionProvider ?? throw new ArgumentNullException(nameof(sessionProvider));
            _resultHandler = resultHandler ?? throw new ArgumentNullException(nameof(resultHandler));
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }

        /// <summary>
        /// Implements MAUI's CanExecute - validates the command and updates UI with validation messages
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            try
            {
                var commandParams = _parameterBuilder();
                var session = _sessionProvider();

                // Call the core command's validation
                var validationResult = _coreCommand.Validate(commandParams, session);

                System.Diagnostics.Debug.WriteLine($"[CanExecute] Command={_coreCommand.GetType().Name}, IsValid={validationResult.IsValid}");
                if (!validationResult.IsValid && validationResult.Errors != null)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"[CanExecute] Error: {error}");
                    }
                }

                // Update validation messages in UI so user sees WHY button is disabled
                // Must run on main thread since we're updating observable collections
                if (MainThread.IsMainThread)
                {
                    UpdateValidationMessages(validationResult);
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() => UpdateValidationMessages(validationResult));
                }

                return validationResult.IsValid;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CanExecute] EXCEPTION: {ex.Message}\n{ex.StackTrace}");
                // Handle parameter building errors
                return false;
            }
        }

        /// <summary>
        /// Implements MAUI's Execute - executes the command and handles the result
        /// </summary>
        public void Execute(object? parameter)
        {
            System.Diagnostics.Debug.WriteLine("MauiCommandAdapter.Execute called!");
            try
            {
                // Clear previous validation messages
                _viewModel.ClearValidation();

                var commandParams = _parameterBuilder();
                var session = _sessionProvider();

                System.Diagnostics.Debug.WriteLine($"Executing command: {_coreCommand.GetType().Name}");

                // Execute the core command (returns rich CommandResult)
                var result = _coreCommand.Execute(commandParams, session);

                System.Diagnostics.Debug.WriteLine($"Command result: Success={result.Success}, Message={result.Message}");

                // If execution failed, show errors in UI
                if (!result.Success)
                {
                    _viewModel.ValidationErrors.Clear();
                    _viewModel.ValidationErrors.Add(result.Message);

                    // Add any additional error details
                    if (result.ValidationErrors != null && result.ValidationErrors.Any())
                    {
                        foreach (var error in result.ValidationErrors)
                        {
                            _viewModel.ValidationErrors.Add(error);
                        }
                    }
                }

                // Call the result handler (ViewModel-specific logic)
                _resultHandler(result);

                // Re-raise CanExecuteChanged to re-enable button after execution
                RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Command execution exception: {ex}");
                // Handle unexpected errors
                _viewModel.ValidationErrors.Clear();
                _viewModel.ValidationErrors.Add($"Unexpected error: {ex.Message}");

                var failureResult = CommandResult.Fail($"Command execution failed: {ex.Message}", ex);
                _resultHandler(failureResult);

                // Re-raise CanExecuteChanged to re-enable button after exception
                RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Raises the CanExecuteChanged event to notify MAUI that command availability may have changed
        /// Call this when ViewModel properties change that affect validation
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            // MAUI requires CanExecuteChanged to be raised on the main thread
            if (MainThread.IsMainThread)
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));
            }
        }

        /// <summary>
        /// Updates the ViewModel's validation collections based on the validation result
        /// </summary>
        private void UpdateValidationMessages(CommandValidationResult validationResult)
        {
            // Clear previous messages
            _viewModel.ValidationErrors.Clear();
            _viewModel.ValidationWarnings.Clear();

            // Add validation errors
            if (validationResult.Errors != null)
            {
                foreach (var error in validationResult.Errors)
                {
                    _viewModel.ValidationErrors.Add(error);
                }
            }

            // Add validation warnings
            if (validationResult.Warnings != null)
            {
                foreach (var warning in validationResult.Warnings)
                {
                    _viewModel.ValidationWarnings.Add(warning);
                }
            }
        }
    }
}
