using System;
using System.Linq;
using CLI.CliniCore.Service.Editor;
using CLI.CliniCore.Service.Menu.Dialogs;
using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Authentication;
using Core.CliniCore.Commands.Clinical;
using Core.CliniCore.Commands.Profile;
using Core.CliniCore.Commands.Reports;
using Core.CliniCore.Commands.Scheduling;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.ClinicalDocumentation;
using Core.CliniCore.Service;

namespace CLI.CliniCore.Service.Menu
{
    /// <summary>
    /// Handles command execution and custom menu actions.
    /// </summary>
    public class MenuExecutor
    {
        private readonly CommandInvoker _commandInvoker;
        private readonly CommandFactory _commandFactory;
        private readonly ConsoleSessionManager _sessionManager;
        private readonly ConsoleCommandParser _commandParser;
        private readonly IConsoleEngine _console;
        private readonly ClinicalDocumentService _clinicalDocService;
        private readonly DocumentSelectionDialog _documentSelectionDialog;

        public MenuExecutor(
            CommandInvoker commandInvoker,
            CommandFactory commandFactory,
            ConsoleSessionManager sessionManager,
            ConsoleCommandParser commandParser,
            IConsoleEngine console,
            ClinicalDocumentService clinicalDocService)
        {
            _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _commandParser = commandParser ?? throw new ArgumentNullException(nameof(commandParser));
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _clinicalDocService = clinicalDocService ?? throw new ArgumentNullException(nameof(clinicalDocService));
            _documentSelectionDialog = new DocumentSelectionDialog(console, clinicalDocService, commandInvoker, commandFactory, sessionManager, commandParser);
        }

        /// <summary>
        /// Executes a command by its key, parsing parameters interactively.
        /// </summary>
        public CommandResult? ExecuteCommand(string commandName)
        {
            CommandResult? result = null;
            try
            {
                _sessionManager.UpdateActivity();
                var command = _commandFactory.CreateCommand(commandName);
                if (command == null)
                {
                    _console.DisplayMessage($"Command '{commandName}' not found.", MessageType.Error);
                    _console.Pause();
                    return null;
                }

                CommandParameters parameters;
                try
                {
                    parameters = _commandParser.ParseInteractive(command);
                }
                catch (UserInputCancelledException)
                {
                    _console.DisplayMessage("Operation cancelled by user.", MessageType.Info);
                    _console.Pause();
                    return null;
                }

                result = _commandInvoker.Execute(command, parameters, _sessionManager.CurrentSession);

                if (result.Success)
                {
                    _console.DisplayMessage(result.Message ?? "Command executed successfully.", MessageType.Success);

                    // Handle special session management cases
                    if (commandName == LogoutCommand.Key)
                    {
                        _sessionManager.EndSession();
                    }
                    else if (commandName == LoginCommand.Key && result.Data is SessionContext session)
                    {
                        _sessionManager.StartSession(session);
                    }
                }
                else
                {
                    _console.DisplayMessage($"Command failed: {result.Message}", MessageType.Error);
                    if (result.ValidationErrors.Any())
                    {
                        foreach (var error in result.ValidationErrors)
                        {
                            _console.DisplayMessage($"  - {error}", MessageType.Error);
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _console.DisplayMessage($"Authorization failed: {ex.Message}", MessageType.Error);
            }
            catch (Exception ex)
            {
                _console.DisplayMessage($"Error executing command: {ex.Message}", MessageType.Error);
            }

            _console.Pause();
            return result;
        }

        /// <summary>
        /// Views the current user's own profile.
        /// </summary>
        public void ViewOwnProfile()
        {
            if (_sessionManager.CurrentUserId.HasValue)
            {
                var parameters = new CommandParameters();
                parameters[ViewProfileCommand.Parameters.ProfileId] = _sessionManager.CurrentUserId.Value;

                var command = _commandFactory.CreateCommand(ViewProfileCommand.Key);
                if (command == null)
                {
                    _console.DisplayMessage("ViewProfile command not found.", MessageType.Error);
                    _console.Pause();
                    return;
                }
                var result = _commandInvoker.Execute(command, parameters, _sessionManager.CurrentSession);

                if (result.Success)
                {
                    _console.DisplayMessage(result.Message ?? "Profile retrieved successfully.", MessageType.Success);
                }
                else
                {
                    _console.DisplayMessage($"Failed to retrieve profile: {result.Message}", MessageType.Error);
                }
                _console.Pause();
            }
        }

        /// <summary>
        /// Sets the current physician's availability.
        /// </summary>
        public void SetOwnAvailability()
        {
            if (_sessionManager.CurrentUserId.HasValue)
            {
                var command = _commandFactory.CreateCommand(SetPhysicianAvailabilityCommand.Key);
                if (command == null)
                {
                    _console.DisplayMessage("SetPhysicianAvailability command not found.", MessageType.Error);
                    _console.Pause();
                    return;
                }
                CommandParameters parameters;
                try
                {
                    parameters = _commandParser.ParseInteractive(command);
                }
                catch (UserInputCancelledException)
                {
                    _console.DisplayMessage("Operation cancelled by user.", MessageType.Info);
                    _console.Pause();
                    return;
                }
                parameters[SetPhysicianAvailabilityCommand.Parameters.PhysicianId] = _sessionManager.CurrentUserId.Value;

                var result = _commandInvoker.Execute(command, parameters, _sessionManager.CurrentSession);

                if (result.Success)
                {
                    _console.DisplayMessage("Availability updated successfully.", MessageType.Success);
                }
                else
                {
                    _console.DisplayMessage($"Failed to update availability: {result.Message}", MessageType.Error);
                }
                _console.Pause();
            }
        }

        /// <summary>
        /// Generates the current physician's performance report.
        /// </summary>
        public void GenerateOwnPhysicianReport()
        {
            if (_sessionManager.CurrentUserId.HasValue)
            {
                var parameters = new CommandParameters();
                parameters[GeneratePhysicianReportCommand.Parameters.PhysicianId] = _sessionManager.CurrentUserId.Value;

                var command = _commandFactory.CreateCommand(GeneratePhysicianReportCommand.Key);
                if (command == null)
                {
                    _console.DisplayMessage("GeneratePhysicianReport command not found.", MessageType.Error);
                    _console.Pause();
                    return;
                }
                var result = _commandInvoker.Execute(command, parameters, _sessionManager.CurrentSession);

                if (result.Success)
                {
                    _console.DisplayMessage(result.Message ?? "Report generated successfully.", MessageType.Success);
                }
                else
                {
                    _console.DisplayMessage($"Failed to generate report: {result.Message}", MessageType.Error);
                }
                _console.Pause();
            }
        }

        /// <summary>
        /// Launches the clinical document editor for a selected document.
        /// </summary>
        public void LaunchDocumentEditor()
        {
            try
            {
                var documentId = _documentSelectionDialog.Show();
                if (documentId == Guid.Empty)
                {
                    _console.DisplayMessage("No document selected for editing.", MessageType.Warning);
                    _console.Pause();
                    return;
                }

                var document = _clinicalDocService.GetDocumentById(documentId);

                if (document == null)
                {
                    _console.DisplayMessage($"Document with ID {documentId} not found.", MessageType.Error);
                    _console.Pause();
                    return;
                }

                var editor = new ClinicalDocumentEditor(_sessionManager, _commandInvoker, _commandFactory, _clinicalDocService);
                editor.EditDocument(document);
            }
            catch (Exception ex)
            {
                _console.DisplayMessage($"Failed to launch document editor: {ex.Message}", MessageType.Error);
                _console.Pause();
            }
        }

        /// <summary>
        /// Updates a clinical document's chief complaint.
        /// </summary>
        public void ExecuteUpdateClinicalDocument()
        {
            try
            {
                var documentId = _documentSelectionDialog.Show();
                if (documentId == Guid.Empty)
                {
                    _console.Pause();
                    return;
                }

                var document = _clinicalDocService.GetDocumentById(documentId);
                if (document == null)
                {
                    _console.DisplayMessage("Document not found.", MessageType.Error);
                    _console.Pause();
                    return;
                }

                if (document.IsCompleted)
                {
                    _console.DisplayMessage("Cannot modify a completed clinical document.", MessageType.Error);
                    _console.Pause();
                    return;
                }

                _console.DisplayMessage("\nCurrent Chief Complaint:", MessageType.Info);
                _console.DisplayMessage($"  {document.ChiefComplaint ?? "(not set)"}", MessageType.Debug);
                _console.DisplayMessage("");

                _console.DisplayMessage("Enter new Chief Complaint (or press Enter to keep current): ", MessageType.Info);
                var newChiefComplaint = _console.GetUserInput("");

                if (string.IsNullOrWhiteSpace(newChiefComplaint))
                {
                    _console.DisplayMessage("No changes made.", MessageType.Info);
                    _console.Pause();
                    return;
                }

                var command = _commandFactory.CreateCommand(UpdateClinicalDocumentCommand.Key);
                if (command == null)
                {
                    _console.DisplayMessage("Update command not available.", MessageType.Error);
                    _console.Pause();
                    return;
                }

                var parameters = new CommandParameters()
                    .SetParameter(UpdateClinicalDocumentCommand.Parameters.DocumentId, documentId)
                    .SetParameter(UpdateClinicalDocumentCommand.Parameters.ChiefComplaint, newChiefComplaint.Trim());

                var result = _commandInvoker.Execute(command, parameters, _sessionManager.CurrentSession);

                if (result.Success)
                {
                    _console.DisplayMessage("Chief complaint updated successfully!", MessageType.Success);
                }
                else
                {
                    _console.DisplayMessage($"Failed to update document: {result.Message}", MessageType.Error);
                }

                _console.Pause();
            }
            catch (Exception ex)
            {
                _console.DisplayMessage($"Error updating document: {ex.Message}", MessageType.Error);
                _console.Pause();
            }
        }

        /// <summary>
        /// Finalizes a clinical document, making it immutable.
        /// </summary>
        public void ExecuteFinalizeClinicalDocument()
        {
            try
            {
                var documentId = _documentSelectionDialog.Show();
                if (documentId == Guid.Empty)
                {
                    _console.Pause();
                    return;
                }

                var document = _clinicalDocService.GetDocumentById(documentId);
                if (document == null)
                {
                    _console.DisplayMessage("Document not found.", MessageType.Error);
                    _console.Pause();
                    return;
                }

                if (document.IsCompleted)
                {
                    _console.DisplayMessage("Document is already finalized.", MessageType.Warning);
                    _console.Pause();
                    return;
                }

                _console.DisplayMessage("\nDocument Completion Check:", MessageType.Info);
                var validationErrors = document.GetValidationErrors();
                if (validationErrors.Any())
                {
                    _console.DisplayMessage("Document cannot be finalized. Missing required entries:", MessageType.Warning);
                    foreach (var error in validationErrors)
                    {
                        _console.DisplayMessage($"  - {error}", MessageType.Error);
                    }
                    _console.Pause();
                    return;
                }

                _console.DisplayMessage("\nDocument is ready to be finalized.", MessageType.Success);
                _console.DisplayMessage("Once finalized, the document cannot be modified.", MessageType.Warning);
                _console.DisplayMessage("Finalize this document? (y/n): ", MessageType.Info);
                var confirm = _console.GetUserInput("");

                if (confirm?.ToLower() != "y")
                {
                    _console.DisplayMessage("Finalization cancelled.", MessageType.Info);
                    _console.Pause();
                    return;
                }

                var command = _commandFactory.CreateCommand(UpdateClinicalDocumentCommand.Key);
                if (command == null)
                {
                    _console.DisplayMessage("Update command not available.", MessageType.Error);
                    _console.Pause();
                    return;
                }

                var parameters = new CommandParameters()
                    .SetParameter(UpdateClinicalDocumentCommand.Parameters.DocumentId, documentId)
                    .SetParameter(UpdateClinicalDocumentCommand.Parameters.Complete, true);

                var result = _commandInvoker.Execute(command, parameters, _sessionManager.CurrentSession);

                if (result.Success)
                {
                    _console.DisplayMessage("Clinical document finalized successfully!", MessageType.Success);
                }
                else
                {
                    _console.DisplayMessage($"Failed to finalize document: {result.Message}", MessageType.Error);
                }

                _console.Pause();
            }
            catch (Exception ex)
            {
                _console.DisplayMessage($"Error finalizing document: {ex.Message}", MessageType.Error);
                _console.Pause();
            }
        }
    }
}
