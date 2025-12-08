using System;
using CoreCommand = Core.CliniCore.Commands.ICommand;
using MauiCommand = System.Windows.Input.ICommand;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication.Representation;

namespace GUI.CliniCore.Commands
{
    /// <summary>
    /// Adapts Core.CliniCore.Commands.ICommand (GoF Command Pattern) to System.Windows.Input.ICommand (MAUI Command Pattern).
    ///
    /// This is a PURE ADAPTER - it only bridges the two interfaces:
    /// - CanExecute: Delegates to ICommand.Validate()
    /// - Execute: Delegates to CommandInvoker.Execute()
    ///
    /// All result handling (errors, validation messages, navigation) is the ViewModel's responsibility
    /// via the resultHandler callback. The adapter does NOT manipulate ViewModel state directly.
    /// </summary>
    public partial class MauiCommandAdapter(
        CommandInvoker commandInvoker,
        CoreCommand coreCommand,
        Func<CommandParameters> parameterBuilder,
        Func<SessionContext?> sessionProvider,
        Action<CommandResult> resultHandler) : MauiCommand
    {
        private readonly CommandInvoker _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
        private readonly CoreCommand _coreCommand = coreCommand ?? throw new ArgumentNullException(nameof(coreCommand));
        private readonly Func<CommandParameters> _parameterBuilder = parameterBuilder ?? throw new ArgumentNullException(nameof(parameterBuilder));
        private readonly Func<SessionContext?> _sessionProvider = sessionProvider ?? throw new ArgumentNullException(nameof(sessionProvider));
        private readonly Action<CommandResult> _resultHandler = resultHandler ?? throw new ArgumentNullException(nameof(resultHandler));

        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Delegates to Core command's Validate method.
        /// Returns true if command can execute, false otherwise.
        /// This is a QUERY - no side effects.
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            var validationResult = _coreCommand.Validate(_parameterBuilder(), _sessionProvider());
            return validationResult.IsValid;
        }

        /// <summary>
        /// Delegates to CommandInvoker for execution with history tracking.
        /// Passes the CommandResult to the resultHandler for ViewModel-specific handling.
        /// </summary>
        public void Execute(object? parameter)
        {
            var result = _commandInvoker.Execute(_coreCommand, _parameterBuilder(), _sessionProvider());
            _resultHandler(result);
            RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Raises CanExecuteChanged on the main thread (MAUI requirement).
        /// Call this when ViewModel properties affecting validation change.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            if (MainThread.IsMainThread)
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            else
                MainThread.BeginInvokeOnMainThread(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));
        }
    }
}
