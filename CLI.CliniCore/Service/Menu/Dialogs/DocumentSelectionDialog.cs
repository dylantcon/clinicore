using System;
using System.Linq;
using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Clinical;
using Core.CliniCore.Domain.ClinicalDocumentation;
using Core.CliniCore.Service;

namespace CLI.CliniCore.Service.Menu.Dialogs
{
    /// <summary>
    /// Dialog for selecting a clinical document from the available documents.
    /// Offers to create a new document if none exist.
    /// </summary>
    public class DocumentSelectionDialog
    {
        private readonly IConsoleEngine _console;
        private readonly ClinicalDocumentService _clinicalDocService;
        private readonly CommandInvoker _commandInvoker;
        private readonly CommandFactory _commandFactory;
        private readonly ConsoleSessionManager _sessionManager;
        private readonly ConsoleCommandParser _commandParser;

        public DocumentSelectionDialog(
            IConsoleEngine console,
            ClinicalDocumentService clinicalDocService,
            CommandInvoker commandInvoker,
            CommandFactory commandFactory,
            ConsoleSessionManager sessionManager,
            ConsoleCommandParser commandParser)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _clinicalDocService = clinicalDocService ?? throw new ArgumentNullException(nameof(clinicalDocService));
            _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _commandParser = commandParser ?? throw new ArgumentNullException(nameof(commandParser));
        }

        /// <summary>
        /// Shows the document selection dialog and returns the selected document ID.
        /// Returns Guid.Empty if selection was cancelled or failed.
        /// </summary>
        public Guid Show()
        {
            try
            {
                var allDocuments = _clinicalDocService.GetAllDocuments().ToList();

                if (allDocuments.Count == 0)
                {
                    return PromptCreateDocument();
                }

                return SelectFromList(allDocuments);
            }
            catch (Exception ex)
            {
                _console.DisplayMessage($"Error selecting document: {ex.Message}", MessageType.Error);
                return Guid.Empty;
            }
        }

        /// <summary>
        /// Gets the selected document by ID, or null if not found
        /// </summary>
        public ClinicalDocument? GetDocument(Guid documentId)
        {
            return _clinicalDocService.GetDocumentById(documentId);
        }

        private Guid PromptCreateDocument()
        {
            _console.DisplayMessage("No clinical documents found.", MessageType.Warning);
            _console.DisplayMessage("Would you like to create one? (Y/n): ", MessageType.Info);
            var response = _console.GetUserInput("");

            if (string.IsNullOrEmpty(response) || response.Trim().ToUpper().StartsWith("Y"))
            {
                var result = ExecuteCreateCommand();
                if (result?.Success == true && result.Data is ClinicalDocument newDoc)
                {
                    return newDoc.Id;
                }
            }

            return Guid.Empty;
        }

        private Guid SelectFromList(System.Collections.Generic.List<ClinicalDocument> documents)
        {
            _console.DisplayMessage("Select Clinical Document:", MessageType.Info);
            _console.DisplayTable(documents,
                ("Date", d => d.CreatedAt.ToString("yyyy-MM-dd")),
                ("Patient", d => d.PatientId.ToString("N")[..8] + "..."),
                ("Status", d => d.IsCompleted ? "Completed" : "In Progress"),
                ("Entries", d => d.Entries.Count.ToString()),
                ("ID", d => d.Id.ToString())
            );

            _console.DisplayMessage($"Enter selection (1-{documents.Count}): ", MessageType.Info);
            var input = _console.GetUserInput("");

            if (string.IsNullOrEmpty(input))
            {
                return Guid.Empty;
            }

            if (int.TryParse(input, out int selection) &&
                selection >= 1 && selection <= documents.Count)
            {
                return documents[selection - 1].Id;
            }

            _console.DisplayMessage("Invalid selection.", MessageType.Error);
            return Guid.Empty;
        }

        private CommandResult? ExecuteCreateCommand()
        {
            var command = _commandFactory.CreateCommand(CreateClinicalDocumentCommand.Key);
            if (command == null) return null;

            try
            {
                // Parse parameters interactively (prompts for PatientId, PhysicianId, AppointmentId, ChiefComplaint)
                var parameters = _commandParser.ParseInteractive(command);
                return _commandInvoker.Execute(command, parameters, _sessionManager.CurrentSession);
            }
            catch (UserInputCancelledException)
            {
                _console.DisplayMessage("Document creation cancelled.", MessageType.Info);
                return null;
            }
        }
    }
}
