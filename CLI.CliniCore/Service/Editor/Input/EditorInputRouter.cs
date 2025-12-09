using System;
using System.Linq;
using CLI.CliniCore.Service.Editor.Workflows;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.ClinicalDocumentation;
using Core.CliniCore.Domain.ClinicalDocumentation.ClinicalEntries;
using Core.CliniCore.Domain.Enumerations;

namespace CLI.CliniCore.Service.Editor.Input
{
    /// <summary>
    /// Routes keyboard input to appropriate handlers in the clinical document editor.
    /// Coordinates between navigation, workflows, and direct actions.
    /// </summary>
    public class EditorInputRouter
    {
        private readonly ClinicalDocumentEditor _editor;
        private readonly ThreadSafeConsoleManager _console;
        private readonly StatusBarInputHandler _inputHandler;
        private readonly EditorRenderer _renderer;
        private readonly NavigationHandler _navigationHandler;

        // Current active workflow (null when not in a multi-step flow)
        private IEntryWorkflow? _activeWorkflow;

        // Edit mode state
        private bool _isEditing;
        private AbstractClinicalEntry? _entryBeingEdited;
        private string? _editField;

        public EditorInputRouter(
            ClinicalDocumentEditor editor,
            ThreadSafeConsoleManager console,
            StatusBarInputHandler inputHandler,
            EditorRenderer renderer)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _inputHandler = inputHandler ?? throw new ArgumentNullException(nameof(inputHandler));
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            _navigationHandler = new NavigationHandler(renderer);
        }

        /// <summary>
        /// Process a key input and return the result
        /// </summary>
        public EditorKeyResult HandleKeyInput(ConsoleKeyInfo keyInfo, EditorState state)
        {
            // If input handler is active (collecting text input)
            if (_inputHandler.IsActive)
            {
                return HandleActiveInput(keyInfo, state);
            }

            // Handle control key combinations
            if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
            {
                return HandleControlKeys(keyInfo, state);
            }

            // Handle navigation keys
            if (IsNavigationKey(keyInfo.Key))
            {
                _navigationHandler.HandleNavigation(keyInfo.Key, state);
                return new EditorKeyResult(EditorAction.Continue);
            }

            // Handle action keys
            return HandleActionKey(keyInfo, state);
        }

        private EditorKeyResult HandleActiveInput(ConsoleKeyInfo keyInfo, EditorState state)
        {
            bool continueInput = _inputHandler.ProcessKey(keyInfo);

            if (continueInput)
            {
                return new EditorKeyResult(EditorAction.Continue);
            }

            // Input completed - handle based on current mode
            var input = _inputHandler.CurrentText;
            _inputHandler.StopEditing();

            if (_activeWorkflow != null)
            {
                return HandleWorkflowInput(input, state);
            }

            if (_isEditing)
            {
                return HandleEditComplete(input, state);
            }

            return new EditorKeyResult(EditorAction.Continue);
        }

        private EditorKeyResult HandleWorkflowInput(string input, EditorState state)
        {
            _activeWorkflow!.ProcessInput(input);

            if (_activeWorkflow.IsCancelled)
            {
                var message = _activeWorkflow.ErrorMessage ?? "Operation cancelled";
                _activeWorkflow = null;
                _renderer.InvalidateStatusZone();
                return new EditorKeyResult(EditorAction.Continue, message, MessageType.Warning);
            }

            if (_activeWorkflow.IsComplete)
            {
                return ExecuteWorkflow(state);
            }

            // Workflow has more steps - continue with next prompt
            if (_activeWorkflow.ErrorMessage != null)
            {
                // Error but not cancelled - show error and retry same step
                _renderer.ShowStatusMessage(_activeWorkflow.ErrorMessage, MessageType.Error);
            }

            StartWorkflowPrompt();
            return new EditorKeyResult(EditorAction.Continue);
        }

        private EditorKeyResult ExecuteWorkflow(EditorState state)
        {
            var documentId = _editor.GetDocumentId();
            if (!documentId.HasValue)
            {
                _activeWorkflow = null;
                return new EditorKeyResult(EditorAction.Continue, "No document ID available", MessageType.Error);
            }

            var parameters = _activeWorkflow!.BuildParameters(documentId.Value);
            if (parameters == null)
            {
                _activeWorkflow = null;
                return new EditorKeyResult(EditorAction.Continue, "Failed to build command parameters", MessageType.Error);
            }

            var result = _editor.ExecuteCommand(_activeWorkflow.CommandKey, parameters);
            _activeWorkflow = null;

            if (result?.Success == true && result.Data is AbstractClinicalEntry newEntry)
            {
                // Add to local document and refresh
                try { state.Document.AddEntry(newEntry); } catch { /* Already persisted */ }
                state.RefreshFlattenedEntries();

                // Select the new entry
                var newIndex = state.GetEntryIndex(newEntry);
                state.SelectedIndex = newIndex >= 0 ? newIndex : Math.Max(0, state.FlattenedEntries.Count - 1);

                _renderer.InvalidateTreeZone();
                _renderer.InvalidateContentZone();
                _renderer.InvalidateStatusZone();

                var displayName = GetEntryDisplayName(newEntry);
                return new EditorKeyResult(EditorAction.Continue, $"Added {displayName}", MessageType.Success);
            }

            var errorMsg = result?.Message ?? "Command execution failed";
            _renderer.InvalidateStatusZone();
            return new EditorKeyResult(EditorAction.Continue, errorMsg, MessageType.Error);
        }

        private void StartWorkflowPrompt()
        {
            if (_activeWorkflow == null) return;
            _inputHandler.StartEditing(_activeWorkflow.CurrentPrompt, _activeWorkflow.DefaultValue, 200);
        }

        private EditorKeyResult HandleControlKeys(ConsoleKeyInfo keyInfo, EditorState state)
        {
            return keyInfo.Key switch
            {
                ConsoleKey.X => new EditorKeyResult(EditorAction.Exit),
                ConsoleKey.S => new EditorKeyResult(EditorAction.Save),
                ConsoleKey.Q => new EditorKeyResult(EditorAction.Exit),
                _ => new EditorKeyResult(EditorAction.Continue)
            };
        }

        private static bool IsNavigationKey(ConsoleKey key)
        {
            return key is ConsoleKey.UpArrow or ConsoleKey.DownArrow
                or ConsoleKey.Home or ConsoleKey.End
                or ConsoleKey.PageUp or ConsoleKey.PageDown;
        }

        private EditorKeyResult HandleActionKey(ConsoleKeyInfo keyInfo, EditorState state)
        {
            return keyInfo.Key switch
            {
                ConsoleKey.A => StartAddEntry(state),
                ConsoleKey.C => StartChiefComplaint(state),
                ConsoleKey.E or ConsoleKey.Enter => StartEditEntry(state),
                ConsoleKey.D => HandleDeleteEntry(state),
                ConsoleKey.V => HandleToggleView(state),
                ConsoleKey.S => new EditorKeyResult(EditorAction.Save),
                ConsoleKey.F1 => new EditorKeyResult(EditorAction.ShowHelp),
                ConsoleKey.Escape => HandleEscape(state),
                _ => new EditorKeyResult(EditorAction.Continue)
            };
        }

        private EditorKeyResult StartAddEntry(EditorState state)
        {
            // Show entry type selection
            var prompt = "Entry type - [S]ubjective [O]bjective [A]ssessment [P]lan [R]x (Esc=cancel): ";
            _inputHandler.StartEditing(prompt, "", 200);

            // Set up a temporary handler for type selection
            _activeWorkflow = new EntryTypeSelector(state, this);
            return new EditorKeyResult(EditorAction.Continue);
        }

        /// <summary>
        /// Starts a specific workflow based on entry type
        /// </summary>
        internal void StartWorkflow(IEntryWorkflow workflow)
        {
            _activeWorkflow = workflow;
            StartWorkflowPrompt();
        }

        private EditorKeyResult StartChiefComplaint(EditorState state)
        {
            _isEditing = true;
            _editField = "chief_complaint";
            var current = state.Document.ChiefComplaint ?? "";
            _inputHandler.StartEditing("Chief Complaint: ", current, 200);
            return new EditorKeyResult(EditorAction.Continue);
        }

        private EditorKeyResult StartEditEntry(EditorState state)
        {
            var entry = state.SelectedEntry;
            if (entry == null)
            {
                return new EditorKeyResult(EditorAction.Continue, "No entry selected", MessageType.Warning);
            }

            _isEditing = true;
            _entryBeingEdited = entry;

            if (entry is PrescriptionEntry rx)
            {
                _editField = "prescription_field_selection";
                var prompt = "Edit: [M]edication [D]osage [F]requency [R]oute [U]ration [I]nstructions (Esc=cancel): ";
                _inputHandler.StartEditing(prompt, "", 200);
            }
            else
            {
                _editField = "content";
                var (width, _) = _console.GetDimensions();
                _inputHandler.StartEditing("Edit content: ", entry.Content, width - 4);
            }

            return new EditorKeyResult(EditorAction.Continue);
        }

        private EditorKeyResult HandleEditComplete(string input, EditorState state)
        {
            if (_editField == "chief_complaint")
            {
                if (!string.IsNullOrWhiteSpace(input))
                {
                    state.Document.ChiefComplaint = input.Trim();
                    state.MarkDirty();
                }
                ClearEditState();
                _renderer.InvalidateStatusZone();
                return new EditorKeyResult(EditorAction.Continue, "Chief complaint updated", MessageType.Success);
            }

            if (_entryBeingEdited == null)
            {
                ClearEditState();
                return new EditorKeyResult(EditorAction.Continue, "Edit cancelled");
            }

            if (_editField == "prescription_field_selection")
            {
                return HandlePrescriptionFieldSelection(input);
            }

            // Apply the edit
            ApplyEdit(input, state);
            ClearEditState();
            _renderer.InvalidateContentZone();
            _renderer.InvalidateStatusZone();
            return new EditorKeyResult(EditorAction.Continue, "Entry updated", MessageType.Success);
        }

        private EditorKeyResult HandlePrescriptionFieldSelection(string input)
        {
            var rx = (PrescriptionEntry)_entryBeingEdited!;
            var lastChar = input.Length > 0 ? char.ToUpper(input[input.Length - 1]) : ' ';

            string prompt;
            string initialValue;

            switch (lastChar)
            {
                case 'M':
                    _editField = "medication";
                    prompt = "Edit medication name: ";
                    initialValue = rx.MedicationName;
                    break;
                case 'D':
                    _editField = "dosage";
                    prompt = "Edit dosage: ";
                    initialValue = rx.Dosage ?? "";
                    break;
                case 'F':
                    _editField = "frequency";
                    prompt = "Edit frequency: ";
                    initialValue = rx.Frequency?.ToString() ?? "";
                    break;
                case 'R':
                    _editField = "route";
                    prompt = "Edit route: ";
                    initialValue = rx.Route.ToString();
                    break;
                case 'U':
                    _editField = "duration";
                    prompt = "Edit duration: ";
                    initialValue = rx.Duration ?? "";
                    break;
                case 'I':
                    _editField = "instructions";
                    prompt = "Edit instructions: ";
                    initialValue = rx.Instructions ?? "";
                    break;
                default:
                    ClearEditState();
                    return new EditorKeyResult(EditorAction.Continue, "Edit cancelled");
            }

            var (width, _) = _console.GetDimensions();
            _inputHandler.StartEditing(prompt, initialValue, width - 4);
            return new EditorKeyResult(EditorAction.Continue);
        }

        private void ApplyEdit(string input, EditorState state)
        {
            if (_entryBeingEdited is PrescriptionEntry rx)
            {
                switch (_editField)
                {
                    case "medication":
                        rx.MedicationName = input;
                        break;
                    case "dosage":
                        rx.Dosage = string.IsNullOrWhiteSpace(input) ? null : input;
                        break;
                    case "frequency":
                        if (Enum.TryParse<DosageFrequency>(input, true, out var freq))
                            rx.Frequency = freq;
                        break;
                    case "route":
                        if (Enum.TryParse<MedicationRoute>(input, true, out var route))
                            rx.Route = route;
                        break;
                    case "duration":
                        rx.Duration = string.IsNullOrWhiteSpace(input) ? null : input;
                        break;
                    case "instructions":
                        rx.Instructions = string.IsNullOrWhiteSpace(input) ? null : input;
                        break;
                }
            }
            else if (_entryBeingEdited != null)
            {
                _entryBeingEdited.Content = input;
            }

            state.MarkDirty();
        }

        private void ClearEditState()
        {
            _isEditing = false;
            _entryBeingEdited = null;
            _editField = null;
        }

        private EditorKeyResult HandleDeleteEntry(EditorState state)
        {
            var entry = state.SelectedEntry;
            if (entry == null)
            {
                return new EditorKeyResult(EditorAction.Continue, "No entry selected", MessageType.Warning);
            }

            if (!ConfirmDelete(entry))
            {
                return new EditorKeyResult(EditorAction.Continue, "Delete cancelled");
            }

            if (!state.Document.RemoveEntry(entry))
            {
                return new EditorKeyResult(EditorAction.Continue, "Entry not found in document", MessageType.Warning);
            }

            state.RefreshFlattenedEntries();
            state.MarkDirty();

            if (state.SelectedIndex >= state.FlattenedEntries.Count)
            {
                state.SelectedIndex = Math.Max(0, state.FlattenedEntries.Count - 1);
            }

            _renderer.InvalidateTreeZone();
            _renderer.InvalidateContentZone();
            return new EditorKeyResult(EditorAction.Continue, "Entry deleted", MessageType.Success);
        }

        private bool ConfirmDelete(AbstractClinicalEntry entry)
        {
            var entryType = entry.EntryType.ToString();
            var preview = entry.GetDisplayString();
            if (preview.Length > 40)
                preview = preview.Substring(0, 37) + "...";

            _console.SetCursorPosition(0, Console.WindowHeight - 1);
            _console.SetForegroundColor(ConsoleColor.Yellow);
            _console.Write($"Delete {entryType}: \"{preview}\"? (Y/N): ");
            _console.ResetColor();

            while (true)
            {
                var key = _console.ReadKey(true);
                if (key.Key == ConsoleKey.Y) return true;
                if (key.Key == ConsoleKey.N || key.Key == ConsoleKey.Escape) return false;
            }
        }

        private EditorKeyResult HandleToggleView(EditorState state)
        {
            state.ViewMode = state.ViewMode switch
            {
                EditorState.EditorViewMode.Tree => EditorState.EditorViewMode.List,
                EditorState.EditorViewMode.List => EditorState.EditorViewMode.Details,
                EditorState.EditorViewMode.Details => EditorState.EditorViewMode.Tree,
                _ => EditorState.EditorViewMode.Tree
            };

            _renderer.InvalidateTreeZone();
            return new EditorKeyResult(EditorAction.Continue, $"Switched to {state.ViewMode} view", MessageType.Info);
        }

        private EditorKeyResult HandleEscape(EditorState state)
        {
            if (state.IsDirty)
            {
                return new EditorKeyResult(EditorAction.Continue,
                    "Document has unsaved changes. Use Ctrl+X to exit.", MessageType.Warning);
            }
            return new EditorKeyResult(EditorAction.Exit);
        }

        private static string GetEntryDisplayName(AbstractClinicalEntry entry)
        {
            return entry switch
            {
                PrescriptionEntry rx => $"prescription '{rx.MedicationName}'",
                DiagnosisEntry => "diagnosis",
                ObservationEntry => "observation",
                AssessmentEntry => "assessment",
                PlanEntry => "plan",
                _ => entry.EntryType.ToString().ToLower()
            };
        }
    }

    /// <summary>
    /// Helper workflow for entry type selection before starting the actual workflow
    /// </summary>
    internal class EntryTypeSelector : IEntryWorkflow
    {
        private readonly EditorState _state;
        private readonly EditorInputRouter _router;

        public EntryTypeSelector(EditorState state, EditorInputRouter router)
        {
            _state = state;
            _router = router;
        }

        public string CurrentPrompt => "";
        public string DefaultValue => "";
        public bool IsComplete => false;
        public bool IsCancelled { get; private set; }
        public string? ErrorMessage { get; private set; }
        public string CommandKey => "";

        public void ProcessInput(string input)
        {
            var lastChar = input.Length > 0 ? char.ToUpper(input[input.Length - 1]) : ' ';

            IEntryWorkflow? workflow = lastChar switch
            {
                'S' => new AddObservationWorkflow(),
                'O' => new AddObservationWorkflow(),
                'A' => new AddAssessmentWorkflow(),
                'P' => CreatePlanWorkflow(),
                'R' => CreatePrescriptionWorkflow(),
                _ => null
            };

            if (workflow == null)
            {
                IsCancelled = true;
                ErrorMessage = "Add operation cancelled";
                return;
            }

            // For S/O, set the category in the observation workflow
            if (workflow is AddObservationWorkflow obsWorkflow)
            {
                // The workflow handles category selection as its first step
                // Just start it - it will ask for subjective/objective type
            }

            if (workflow.IsCancelled)
            {
                IsCancelled = true;
                ErrorMessage = workflow.ErrorMessage;
                return;
            }

            _router.StartWorkflow(workflow);
        }

        private IEntryWorkflow CreatePlanWorkflow()
        {
            var diagnoses = _state.FlattenedEntries.OfType<DiagnosisEntry>().ToList();
            return new AddPlanWorkflow(diagnoses);
        }

        private IEntryWorkflow? CreatePrescriptionWorkflow()
        {
            var diagnoses = _state.FlattenedEntries.OfType<DiagnosisEntry>().ToList();
            if (diagnoses.Count == 0)
            {
                IsCancelled = true;
                ErrorMessage = "No diagnoses found. Create a diagnosis entry first before adding prescriptions.";
                return null;
            }
            return new AddPrescriptionWorkflow(diagnoses);
        }

        public void Cancel() => IsCancelled = true;
        public CommandParameters? BuildParameters(Guid documentId) => null;
    }
}
