using System;
using System.Collections.Concurrent;
using System.Linq;
using Core.CliniCore.ClinicalDoc;

namespace CLI.CliniCore.Service.Editor
{
    /// <summary>
    /// Handles keyboard input processing for the clinical document editor
    /// </summary>
    public class EditorKeyHandler
    {
        private readonly ClinicalDocumentEditor _editor;
        private readonly ThreadSafeConsoleManager _console;
        private readonly StatusBarInputHandler _inputHandler;
        private readonly EditorRenderer _renderer;
        private bool _isEditingText = false;
        private AbstractClinicalEntry? _entryBeingEdited;
        private string? _editMode; // "content", "prescription", etc.
        
        // Add entry state
        private bool _isAddingEntry = false;
        private string _addEntryStep = ""; // "type", "content", "prescription_name", "prescription_dosage", etc.
        private ConcurrentDictionary<string, string> _addEntryData = new();

        public EditorKeyHandler(ClinicalDocumentEditor editor, ThreadSafeConsoleManager console, StatusBarInputHandler inputHandler, EditorRenderer renderer)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _inputHandler = inputHandler ?? throw new ArgumentNullException(nameof(inputHandler));
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        public EditorKeyResult HandleKeyInput(ConsoleKeyInfo keyInfo, EditorState state)
        {
            // If we're in text editing mode, handle input differently
            if (_isEditingText && _inputHandler.IsActive)
            {
                bool continueEditing = _inputHandler.ProcessKey(keyInfo);
                
                if (!continueEditing)
                {
                    // Editing complete - save the changes
                    var newText = _inputHandler.CurrentText;
                    
                    // CRITICAL: Stop the input handler first to clear edit mode
                    _inputHandler.StopEditing();
                    
                    if (!string.IsNullOrEmpty(newText) && _entryBeingEdited != null)
                    {
                        ApplyEditToEntry(_entryBeingEdited, newText, _editMode);
                        state.MarkDirty();
                        
                        // CRITICAL: Invalidate content zone to show updated text
                        _renderer.InvalidateContentZone();
                        _renderer.InvalidateStatusZone(); // Clear edit dialog
                        
                        _isEditingText = false;
                        _entryBeingEdited = null;
                        _editMode = null;
                        return new EditorKeyResult(EditorAction.Continue, "Entry updated successfully", MessageType.Success);
                    }
                    else
                    {
                        // Edit cancelled - clear everything
                        _renderer.InvalidateStatusZone(); // Clear edit dialog
                        _isEditingText = false;
                        _entryBeingEdited = null;
                        _editMode = null;
                        return new EditorKeyResult(EditorAction.Continue, "Edit cancelled");
                    }
                }
                
                return new EditorKeyResult(EditorAction.Continue);
            }
            
            // If we're in add entry mode, handle input differently
            if (_isAddingEntry && _inputHandler.IsActive)
            {
                bool continueAdding = _inputHandler.ProcessKey(keyInfo);
                
                if (!continueAdding)
                {
                    return HandleAddEntryStepComplete(state);
                }
                
                return new EditorKeyResult(EditorAction.Continue);
            }
            
            // Handle special key combinations first
            if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
            {
                return HandleControlKeys(keyInfo, state);
            }

            // Handle regular keys
            return keyInfo.Key switch
            {
                // Navigation
                ConsoleKey.UpArrow => HandleNavigationUp(state),
                ConsoleKey.DownArrow => HandleNavigationDown(state),
                ConsoleKey.Home => HandleNavigationHome(state),
                ConsoleKey.End => HandleNavigationEnd(state),
                ConsoleKey.PageUp => HandlePageUp(state),
                ConsoleKey.PageDown => HandlePageDown(state),

                // Actions
                ConsoleKey.A => HandleAddEntry(state),
                ConsoleKey.E => HandleEditEntry(state),
                ConsoleKey.D => HandleDeleteEntry(state),
                ConsoleKey.V => HandleToggleView(state),
                ConsoleKey.S => HandleSaveDocument(state),
                ConsoleKey.Enter => HandleEditEntry(state), // Alternative to 'E'

                // Help and exit
                ConsoleKey.F1 => HandleShowHelp(),
                ConsoleKey.Escape => HandleEscape(state),

                // Ignore other keys
                _ => new EditorKeyResult(EditorAction.Continue)
            };
        }

        private EditorKeyResult HandleControlKeys(ConsoleKeyInfo keyInfo, EditorState state)
        {
            return keyInfo.Key switch
            {
                ConsoleKey.X => new EditorKeyResult(EditorAction.Exit),
                ConsoleKey.S => HandleSaveDocument(state),
                ConsoleKey.Q => new EditorKeyResult(EditorAction.Exit),
                _ => new EditorKeyResult(EditorAction.Continue)
            };
        }

        private EditorKeyResult HandleNavigationUp(EditorState state)
        {
            if (state.HasEntries && state.SelectedIndex > 0)
            {
                state.MoveSelectionUp();
                // Invalidate zones to update visual selection and content
                _renderer.InvalidateTreeZone();
                _renderer.InvalidateContentZone();
                return new EditorKeyResult(EditorAction.Continue);
            }
            return new EditorKeyResult(EditorAction.Continue, "Already at first entry");
        }

        private EditorKeyResult HandleNavigationDown(EditorState state)
        {
            if (state.HasEntries && state.SelectedIndex < state.FlattenedEntries.Count - 1)
            {
                state.MoveSelectionDown();
                // Invalidate zones to update visual selection and content
                _renderer.InvalidateTreeZone();
                _renderer.InvalidateContentZone();
                return new EditorKeyResult(EditorAction.Continue);
            }
            return new EditorKeyResult(EditorAction.Continue, "Already at last entry");
        }

        private EditorKeyResult HandleNavigationHome(EditorState state)
        {
            if (state.HasEntries)
            {
                state.MoveToFirst();
                // Invalidate zones to update visual selection and content
                _renderer.InvalidateTreeZone();
                _renderer.InvalidateContentZone();
                return new EditorKeyResult(EditorAction.Continue, "Moved to first entry");
            }
            return new EditorKeyResult(EditorAction.Continue);
        }

        private EditorKeyResult HandleNavigationEnd(EditorState state)
        {
            if (state.HasEntries)
            {
                state.MoveToLast();
                // Invalidate zones to update visual selection and content
                _renderer.InvalidateTreeZone();
                _renderer.InvalidateContentZone();
                return new EditorKeyResult(EditorAction.Continue, "Moved to last entry");
            }
            return new EditorKeyResult(EditorAction.Continue);
        }

        private EditorKeyResult HandlePageUp(EditorState state)
        {
            if (state.HasEntries)
            {
                var newIndex = Math.Max(0, state.SelectedIndex - 10);
                state.SelectedIndex = newIndex;
                // Invalidate zones to update visual selection and content
                _renderer.InvalidateTreeZone();
                _renderer.InvalidateContentZone();
                return new EditorKeyResult(EditorAction.Continue);
            }
            return new EditorKeyResult(EditorAction.Continue);
        }

        private EditorKeyResult HandlePageDown(EditorState state)
        {
            if (state.HasEntries)
            {
                var newIndex = Math.Min(state.FlattenedEntries.Count - 1, state.SelectedIndex + 10);
                state.SelectedIndex = newIndex;
                // Invalidate zones to update visual selection and content
                _renderer.InvalidateTreeZone();
                _renderer.InvalidateContentZone();
                return new EditorKeyResult(EditorAction.Continue);
            }
            return new EditorKeyResult(EditorAction.Continue);
        }

        private EditorKeyResult HandleAddEntry(EditorState state)
        {
            // Start the add entry flow with type selection
            _isAddingEntry = true;
            _addEntryStep = "type";
            _addEntryData.Clear();
            
            StartAddEntryTypeSelection();
            return new EditorKeyResult(EditorAction.Continue);
        }

        private EditorKeyResult HandleEditEntry(EditorState state)
        {
            var selectedEntry = state.SelectedEntry;
            if (selectedEntry == null)
            {
                return new EditorKeyResult(EditorAction.Continue, 
                    "No entry selected", MessageType.Warning);
            }

            try
            {
                if (EditEntry(selectedEntry))
                {
                    state.MarkDirty();
                    return new EditorKeyResult(EditorAction.Continue, 
                        "Entry updated successfully", MessageType.Success);
                }
            }
            catch (Exception ex)
            {
                return new EditorKeyResult(EditorAction.Continue, 
                    $"Failed to edit entry: {ex.Message}", MessageType.Error);
            }

            return new EditorKeyResult(EditorAction.Continue, "Edit operation cancelled");
        }

        private EditorKeyResult HandleDeleteEntry(EditorState state)
        {
            var selectedEntry = state.SelectedEntry;
            if (selectedEntry == null)
            {
                return new EditorKeyResult(EditorAction.Continue, 
                    "No entry selected", MessageType.Warning);
            }

            try
            {
                // Confirm deletion
                if (ConfirmDelete(selectedEntry))
                {
                    // Note: ClinicalDocument doesn't have RemoveEntry method
                    // This would need to be implemented or we use a command
                    state.RefreshFlattenedEntries();
                    state.MarkDirty();

                    // Adjust selection if needed
                    if (state.SelectedIndex >= state.FlattenedEntries.Count)
                    {
                        state.SelectedIndex = Math.Max(0, state.FlattenedEntries.Count - 1);
                    }

                    return new EditorKeyResult(EditorAction.Continue, 
                        "Entry deleted successfully", MessageType.Success);
                }
            }
            catch (Exception ex)
            {
                return new EditorKeyResult(EditorAction.Continue, 
                    $"Failed to delete entry: {ex.Message}", MessageType.Error);
            }

            return new EditorKeyResult(EditorAction.Continue, "Delete operation cancelled");
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
            
            // Invalidate tree zone since view mode affects tree rendering
            _renderer.InvalidateTreeZone();

            return new EditorKeyResult(EditorAction.Continue, 
                $"Switched to {state.ViewMode} view", MessageType.Info);
        }

        private EditorKeyResult HandleSaveDocument(EditorState state)
        {
            return new EditorKeyResult(EditorAction.Save);
        }

        private EditorKeyResult HandleShowHelp()
        {
            return new EditorKeyResult(EditorAction.ShowHelp);
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


        

        private bool EditEntry(AbstractClinicalEntry entry)
        {
            // Start editing using the status bar input handler
            _entryBeingEdited = entry;
            _isEditingText = true;

            // For prescription entries, show field selection menu
            if (entry is PrescriptionEntry rx)
            {
                var prompt = "Edit: [M]edication [D]osage [F]requency [R]oute [U]ration [I]nstructions (Esc=cancel): ";
                _editMode = "prescription_field_selection";
                _inputHandler.StartEditing(prompt, "", 200);
            }
            else
            {
                // For other entry types, edit content directly
                _editMode = "content";
                string prompt = "Edit content (Enter to submit, Shift+Enter for newline, Esc to cancel): ";
                string initialText = entry.Content;
                var (width, _) = _console.GetDimensions();
                _inputHandler.StartEditing(prompt, initialText, width - 4);
            }

            return true; // The actual update happens in HandleKeyInput when editing completes
        }
        
        private void ApplyEditToEntry(AbstractClinicalEntry entry, string newText, string? editMode)
        {
            if (entry is PrescriptionEntry rx && editMode == "prescription_field_selection")
            {
                // Handle prescription field selection - start editing the selected field
                var lastChar = newText.Length > 0 ? char.ToUpper(newText.Last()) : ' ';
                string prompt;
                string initialText;

                switch (lastChar)
                {
                    case 'M': // Medication
                        _editMode = "prescription_medication";
                        prompt = "Edit medication name: ";
                        initialText = rx.MedicationName;
                        break;
                    case 'D': // Dosage
                        _editMode = "prescription_dosage_edit";
                        prompt = "Edit dosage: ";
                        initialText = rx.Dosage ?? "";
                        break;
                    case 'F': // Frequency
                        _editMode = "prescription_frequency_edit";
                        prompt = "Edit frequency: ";
                        initialText = rx.Frequency ?? "";
                        break;
                    case 'R': // Route
                        _editMode = "prescription_route_edit";
                        prompt = "Edit route: ";
                        initialText = rx.Route ?? "Oral";
                        break;
                    case 'U': // Duration
                        _editMode = "prescription_duration_edit";
                        prompt = "Edit duration: ";
                        initialText = rx.Duration ?? "";
                        break;
                    case 'I': // Instructions
                        _editMode = "prescription_instructions_edit";
                        prompt = "Edit instructions: ";
                        initialText = rx.Instructions ?? "";
                        break;
                    default:
                        // Invalid selection - clear edit state
                        _isEditingText = false;
                        _entryBeingEdited = null;
                        _editMode = null;
                        return;
                }

                // Start editing the selected field
                var (width, _) = _console.GetDimensions();
                _inputHandler.StartEditing(prompt, initialText, width - 4);
                return; // Don't apply yet, wait for actual field input
            }

            // Apply the edit based on mode
            if (entry is PrescriptionEntry prescription)
            {
                switch (editMode)
                {
                    case "prescription_medication":
                        prescription.MedicationName = newText;
                        break;
                    case "prescription_dosage_edit":
                        prescription.Dosage = string.IsNullOrWhiteSpace(newText) ? null : newText;
                        break;
                    case "prescription_frequency_edit":
                        prescription.Frequency = string.IsNullOrWhiteSpace(newText) ? null : newText;
                        break;
                    case "prescription_route_edit":
                        prescription.Route = string.IsNullOrWhiteSpace(newText) ? "Oral" : newText;
                        break;
                    case "prescription_duration_edit":
                        prescription.Duration = string.IsNullOrWhiteSpace(newText) ? null : newText;
                        break;
                    case "prescription_instructions_edit":
                        prescription.Instructions = string.IsNullOrWhiteSpace(newText) ? null : newText;
                        break;
                }
            }
            else
            {
                // For non-prescription entries, just update content
                entry.Content = newText;
            }
        }
        
        /// <summary>
        /// Starts the add entry type selection in status bar
        /// </summary>
        private void StartAddEntryTypeSelection()
        {
            var prompt = "Entry type - [S]ubjective [O]bjective [A]ssessment [P]lan [R]x (Esc=cancel): ";
            _inputHandler.StartEditing(prompt, "", 200); // Wide enough for the prompt
        }
        
        /// <summary>
        /// Handles completion of each add entry step
        /// </summary>
        private EditorKeyResult HandleAddEntryStepComplete(EditorState state)
        {
            var input = _inputHandler.CurrentText;
            _inputHandler.StopEditing();
            
            switch (_addEntryStep)
            {
                case "type":
                    return HandleEntryTypeSelection(input, state);
                    
                case "objective_subtype":
                    return HandleObjectiveSubtypeSelection(input, state);
                    
                case "content":
                    return HandleEntryContentInput(input, state);
                    
                case "prescription_diagnosis_selection":
                    return HandlePrescriptionDiagnosisSelection(input, state);
                    
                case "prescription_name":
                    return HandlePrescriptionNameInput(input, state);
                    
                case "prescription_dosage":
                    return HandlePrescriptionDosageInput(input, state);

                case "prescription_frequency":
                    return HandlePrescriptionFrequencyInput(input, state);

                case "prescription_route":
                    return HandlePrescriptionRouteInput(input, state);

                case "prescription_duration":
                    return HandlePrescriptionDurationInput(input, state);

                case "prescription_instructions":
                    return HandlePrescriptionInstructionsInput(input, state);

                default:
                    return CancelAddEntry("Unknown add entry step");
            }
        }
        
        /// <summary>
        /// Handles entry type selection (S/O/A/P/R)
        /// </summary>
        private EditorKeyResult HandleEntryTypeSelection(string input, EditorState state)
        {
            // Note: For type selection, we're looking for single character input
            // The input handler might return the full prompt + character, so check the last character
            var lastChar = input.Length > 0 ? input.Last() : ' ';
            
            switch (char.ToUpper(lastChar))
            {
                case 'S':
                    _addEntryData["type"] = "observation";
                    return StartContentEntry("observation");
                    
                case 'O':
                    // Objective can be diagnosis OR prescription - need sub-selection
                    _addEntryStep = "objective_subtype";
                    var prompt = "Objective type - [D]iagnosis or [P]rescription: ";
                    _inputHandler.StartEditing(prompt, "", 150);
                    return new EditorKeyResult(EditorAction.Continue);
                    
                case 'A':
                    _addEntryData["type"] = "assessment";
                    return StartContentEntry("assessment");
                    
                case 'P':
                    _addEntryData["type"] = "plan";
                    return StartContentEntry("plan");
                    
                case 'R':
                    _addEntryData["type"] = "prescription";
                    return StartPrescriptionEntry(state);
                    
                default:
                    return CancelAddEntry("Add operation cancelled");
            }
        }
        
        /// <summary>
        /// Handles objective subtype selection (D/P)
        /// </summary>
        private EditorKeyResult HandleObjectiveSubtypeSelection(string input, EditorState state)
        {
            var lastChar = input.Length > 0 ? input.Last() : ' ';
            
            switch (char.ToUpper(lastChar))
            {
                case 'D':
                    _addEntryData["type"] = "diagnosis";
                    return StartContentEntry("diagnosis");
                    
                case 'P':
                    _addEntryData["type"] = "prescription";
                    return StartPrescriptionEntry(state);
                    
                default:
                    return CancelAddEntry("Invalid objective type selection");
            }
        }
        
        /// <summary>
        /// Starts content entry for non-prescription types
        /// </summary>
        private EditorKeyResult StartContentEntry(string entryType)
        {
            _addEntryStep = "content";
            var prompt = $"Enter {entryType} content: ";
            _inputHandler.StartEditing(prompt, "", 200);
            return new EditorKeyResult(EditorAction.Continue);
        }
        
        /// <summary>
        /// Starts prescription entry flow - but first check for existing diagnoses
        /// </summary>
        private EditorKeyResult StartPrescriptionEntry(EditorState state)
        {
            // Check if there are any existing diagnoses to link to
            var diagnoses = state.FlattenedEntries.OfType<DiagnosisEntry>().ToList();
            
            if (diagnoses.Count == 0)
            {
                return CancelAddEntry("No diagnoses found. Create a diagnosis entry first before adding prescriptions.");
            }
            
            // Store available diagnoses for selection
            _addEntryData["available_diagnoses"] = string.Join("|", diagnoses.Select(d => $"{d.Id}:{d.Content}"));
            
            _addEntryStep = "prescription_diagnosis_selection";
            var prompt = BuildDiagnosisSelectionPrompt(diagnoses);
            _inputHandler.StartEditing(prompt, "", 200);
            return new EditorKeyResult(EditorAction.Continue);
        }
        
        /// <summary>
        /// Handles content input for non-prescription entries
        /// </summary>
        private EditorKeyResult HandleEntryContentInput(string input, EditorState state)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return CancelAddEntry("Content cannot be empty");
            }
            
            _addEntryData["content"] = input;
            return CreateAndAddEntry(state);
        }
        
        /// <summary>
        /// Handles prescription medication name input
        /// </summary>
        private EditorKeyResult HandlePrescriptionNameInput(string input, EditorState state)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return CancelAddEntry("Medication name cannot be empty");
            }
            
            _addEntryData["medication_name"] = input;
            _addEntryStep = "prescription_dosage";
            
            var prompt = "Dosage: ";
            _inputHandler.StartEditing(prompt, "", 100);
            return new EditorKeyResult(EditorAction.Continue);
        }
        
        /// <summary>
        /// Handles prescription dosage input
        /// </summary>
        private EditorKeyResult HandlePrescriptionDosageInput(string input, EditorState state)
        {
            _addEntryData["dosage"] = input ?? "";
            _addEntryStep = "prescription_frequency";

            var prompt = "Frequency (e.g., 'once daily', 'twice daily', '3 times daily') *: ";
            _inputHandler.StartEditing(prompt, "", 200);
            return new EditorKeyResult(EditorAction.Continue);
        }

        /// <summary>
        /// Handles prescription frequency input (required)
        /// </summary>
        private EditorKeyResult HandlePrescriptionFrequencyInput(string input, EditorState state)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return CancelAddEntry("Frequency is required for prescriptions");
            }

            _addEntryData["frequency"] = input.Trim();
            _addEntryStep = "prescription_route";

            var prompt = "Route (e.g., 'Oral', 'IV', 'Topical') [default: Oral]: ";
            _inputHandler.StartEditing(prompt, "Oral", 100);
            return new EditorKeyResult(EditorAction.Continue);
        }

        /// <summary>
        /// Handles prescription route input (optional, defaults to Oral)
        /// </summary>
        private EditorKeyResult HandlePrescriptionRouteInput(string input, EditorState state)
        {
            _addEntryData["route"] = string.IsNullOrWhiteSpace(input) ? "Oral" : input.Trim();
            _addEntryStep = "prescription_duration";

            var prompt = "Duration (e.g., '7 days', '2 weeks') [optional]: ";
            _inputHandler.StartEditing(prompt, "", 100);
            return new EditorKeyResult(EditorAction.Continue);
        }

        /// <summary>
        /// Handles prescription duration input (optional)
        /// </summary>
        private EditorKeyResult HandlePrescriptionDurationInput(string input, EditorState state)
        {
            if (!string.IsNullOrWhiteSpace(input))
            {
                _addEntryData["duration"] = input.Trim();
            }

            _addEntryStep = "prescription_instructions";

            var prompt = "Instructions (e.g., 'Take with food', 'Take at bedtime') [optional]: ";
            _inputHandler.StartEditing(prompt, "", 200);
            return new EditorKeyResult(EditorAction.Continue);
        }
        
        /// <summary>
        /// Handles prescription instructions input - final step
        /// </summary>
        private EditorKeyResult HandlePrescriptionInstructionsInput(string input, EditorState state)
        {
            _addEntryData["instructions"] = input ?? "";
            return CreateAndAddEntry(state);
        }
        
        /// <summary>
        /// Creates the entry from collected data and adds it to the document
        /// </summary>
        private EditorKeyResult CreateAndAddEntry(EditorState state)
        {
            try
            {
                var entryType = _addEntryData["type"];
                var authorId = Guid.NewGuid();
                
                AbstractClinicalEntry? newEntry = entryType switch
                {
                    "observation" => new ObservationEntry(authorId, _addEntryData["content"]),
                    "diagnosis" => new DiagnosisEntry(authorId, _addEntryData["content"]),
                    "assessment" => new AssessmentEntry(authorId, _addEntryData["content"]),
                    "plan" => new PlanEntry(authorId, _addEntryData["content"]),
                    "prescription" => CreatePrescriptionFromData(authorId),
                    _ => null
                };
                
                if (newEntry != null)
                {
                    state.Document.AddEntry(newEntry);
                    state.RefreshFlattenedEntries();
                    state.MarkDirty();
                    
                    // Select the new entry
                    var newIndex = state.GetEntryIndex(newEntry);
                    if (newIndex >= 0)
                    {
                        state.SelectedIndex = newIndex;
                    }
                    else
                    {
                        // If we can't find the entry, just select the last one
                        state.SelectedIndex = Math.Max(0, state.FlattenedEntries.Count - 1);
                    }
                    
                    // Clear status bar and invalidate zones
                    ClearAddEntryState();
                    _renderer.InvalidateTreeZone();
                    _renderer.InvalidateContentZone();
                    _renderer.InvalidateStatusZone();
                    
                    var displayName = newEntry is PrescriptionEntry rx ? $"prescription '{rx.MedicationName}'" : entryType;
                    return new EditorKeyResult(EditorAction.Continue, 
                        $"Added new {displayName} entry", MessageType.Success);
                }
                else
                {
                    return CancelAddEntry("Failed to create entry - entry was null");
                }
            }
            catch (Exception ex)
            {
                return CancelAddEntry($"Failed to add entry: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Builds the diagnosis selection prompt
        /// </summary>
        private string BuildDiagnosisSelectionPrompt(List<DiagnosisEntry> diagnoses)
        {
            var prompt = "Select diagnosis: ";
            for (int i = 0; i < Math.Min(diagnoses.Count, 9); i++)
            {
                prompt += $"[{i + 1}]{diagnoses[i].Content.Substring(0, Math.Min(15, diagnoses[i].Content.Length))}... ";
            }
            prompt += "(1-9): ";
            return prompt;
        }
        
        /// <summary>
        /// Handles diagnosis selection for prescription
        /// </summary>
        private EditorKeyResult HandlePrescriptionDiagnosisSelection(string input, EditorState state)
        {
            var diagnoses = state.FlattenedEntries.OfType<DiagnosisEntry>().ToList();
            
            // Parse selection (1-9)
            var lastChar = input.Length > 0 ? input.Last() : ' ';
            if (char.IsDigit(lastChar))
            {
                var selection = int.Parse(lastChar.ToString()) - 1; // Convert to 0-based index
                if (selection >= 0 && selection < diagnoses.Count)
                {
                    _addEntryData["selected_diagnosis_id"] = diagnoses[selection].Id.ToString();
                    
                    // Now get medication name
                    _addEntryStep = "prescription_name";
                    var prompt = "Medication name: ";
                    _inputHandler.StartEditing(prompt, "", 100);
                    return new EditorKeyResult(EditorAction.Continue);
                }
            }
            
            return CancelAddEntry("Invalid diagnosis selection");
        }
        
        /// <summary>
        /// Creates a prescription entry from collected data with valid diagnosis link
        /// </summary>
        private PrescriptionEntry CreatePrescriptionFromData(Guid authorId)
        {
            // Get required fields
            if (!_addEntryData.TryGetValue("medication_name", out var medicationName) ||
                string.IsNullOrWhiteSpace(medicationName))
            {
                throw new InvalidOperationException("Medication name is required for prescription entry");
            }

            if (!_addEntryData.TryGetValue("selected_diagnosis_id", out var diagnosisIdStr) ||
                !Guid.TryParse(diagnosisIdStr, out var diagnosisId))
            {
                throw new InvalidOperationException("Valid diagnosis selection is required for prescription entry");
            }

            if (!_addEntryData.TryGetValue("frequency", out var frequency) ||
                string.IsNullOrWhiteSpace(frequency))
            {
                throw new InvalidOperationException("Frequency is required for prescription entry");
            }

            // Create prescription with proper diagnosis link
            var rx = new PrescriptionEntry(authorId, diagnosisId, medicationName.Trim());

            // Set required fields
            rx.Frequency = frequency.Trim();

            // Set optional fields with defaults
            if (_addEntryData.TryGetValue("dosage", out var dosage) && !string.IsNullOrWhiteSpace(dosage))
                rx.Dosage = dosage.Trim();

            if (_addEntryData.TryGetValue("route", out var route) && !string.IsNullOrWhiteSpace(route))
                rx.Route = route.Trim();
            else
                rx.Route = "Oral"; // Default

            if (_addEntryData.TryGetValue("duration", out var duration) && !string.IsNullOrWhiteSpace(duration))
                rx.Duration = duration.Trim();

            if (_addEntryData.TryGetValue("instructions", out var instructions) && !string.IsNullOrWhiteSpace(instructions))
                rx.Instructions = instructions.Trim();

            return rx;
        }
        
        /// <summary>
        /// Cancels the add entry operation and cleans up state
        /// </summary>
        private EditorKeyResult CancelAddEntry(string message)
        {
            ClearAddEntryState();
            _renderer.InvalidateStatusZone();
            return new EditorKeyResult(EditorAction.Continue, message);
        }
        
        /// <summary>
        /// Clears add entry state variables
        /// </summary>
        private void ClearAddEntryState()
        {
            _isAddingEntry = false;
            _addEntryStep = "";
            _addEntryData.Clear();
        }

        private bool ConfirmDelete(AbstractClinicalEntry entry)
        {
            // This would show a confirmation dialog
            // For now, return true (in real implementation, would prompt user)
            return true;
        }
    }
}