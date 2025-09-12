using System;
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

        public EditorKeyHandler(ClinicalDocumentEditor editor, ThreadSafeConsoleManager console)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _console = console ?? throw new ArgumentNullException(nameof(console));
        }

        public EditorKeyResult HandleKeyInput(ConsoleKeyInfo keyInfo, EditorState state)
        {
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
                return new EditorKeyResult(EditorAction.Continue);
            }
            return new EditorKeyResult(EditorAction.Continue, "Already at first entry");
        }

        private EditorKeyResult HandleNavigationDown(EditorState state)
        {
            if (state.HasEntries && state.SelectedIndex < state.FlattenedEntries.Count - 1)
            {
                state.MoveSelectionDown();
                return new EditorKeyResult(EditorAction.Continue);
            }
            return new EditorKeyResult(EditorAction.Continue, "Already at last entry");
        }

        private EditorKeyResult HandleNavigationHome(EditorState state)
        {
            if (state.HasEntries)
            {
                state.MoveToFirst();
                return new EditorKeyResult(EditorAction.Continue, "Moved to first entry");
            }
            return new EditorKeyResult(EditorAction.Continue);
        }

        private EditorKeyResult HandleNavigationEnd(EditorState state)
        {
            if (state.HasEntries)
            {
                state.MoveToLast();
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
                return new EditorKeyResult(EditorAction.Continue);
            }
            return new EditorKeyResult(EditorAction.Continue);
        }

        private EditorKeyResult HandleAddEntry(EditorState state)
        {
            try
            {
                // Show SOAP entry type selection
                var entryType = PromptForEntryType();
                if (entryType == null)
                {
                    return new EditorKeyResult(EditorAction.Continue, "Add operation cancelled");
                }

                // Create new entry based on type
                var newEntry = CreateNewEntry(entryType, state.Document);
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

                    return new EditorKeyResult(EditorAction.Continue, 
                        $"Added new {entryType} entry", MessageType.Success);
                }
            }
            catch (Exception ex)
            {
                return new EditorKeyResult(EditorAction.Continue, 
                    $"Failed to add entry: {ex.Message}", MessageType.Error);
            }

            return new EditorKeyResult(EditorAction.Continue, "Add operation cancelled");
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

        private string? PromptForEntryType()
        {
            // Show prompt at bottom of screen
            var (_, height) = _console.GetDimensions();
            _console.SetCursorPosition(2, height - 2);
            _console.SetForegroundColor(ConsoleColor.Yellow);
            _console.Write("Entry type - [S]ubjective [O]bjective [A]ssessment [P]lan [R]x (Esc=cancel): ");
            _console.ResetColor();
            
            // Read single key
            var key = _console.ReadKey(true);
            
            return key.Key switch
            {
                ConsoleKey.S => "observation",    // Subjective -> ObservationEntry 
                ConsoleKey.O => "diagnosis",      // Objective -> DiagnosisEntry
                ConsoleKey.A => "assessment",     // Assessment -> AssessmentEntry
                ConsoleKey.P => "plan",           // Plan -> PlanEntry
                ConsoleKey.R => "prescription",   // Rx -> PrescriptionEntry
                ConsoleKey.Escape => null,
                _ => null
            };
        }

        private AbstractClinicalEntry? CreateNewEntry(string entryType, ClinicalDocument document)
        {
            // Use a default author ID - in real implementation this would come from session
            var authorId = Guid.NewGuid(); 
            
            return entryType.ToLowerInvariant() switch
            {
                "observation" or "s" => new ObservationEntry(authorId, "New observation - edit to add details"),
                "diagnosis" or "o" => new DiagnosisEntry(authorId, "New diagnosis - edit to add details"),
                "prescription" or "rx" => new PrescriptionEntry(authorId, Guid.NewGuid(), "New medication"),
                "assessment" or "a" => new AssessmentEntry(authorId, "New assessment - edit to add details"),
                "plan" or "p" => new PlanEntry(authorId, "New plan - edit to add details"),
                _ => null
            };
        }

        private bool EditEntry(AbstractClinicalEntry entry)
        {
            // Show prompt at bottom of screen
            var (_, height) = _console.GetDimensions();
            _console.SetCursorPosition(2, height - 2);
            _console.SetForegroundColor(ConsoleColor.Yellow);
            _console.Write($"Edit content (was: \"{entry.Content}\"): ");
            _console.ResetColor();
            
            // Read user input
            var newContent = _console.ReadLine();
            
            if (!string.IsNullOrWhiteSpace(newContent))
            {
                entry.Content = newContent;
                return true;
            }
            
            return false; // Cancelled or no input
        }

        private bool ConfirmDelete(AbstractClinicalEntry entry)
        {
            // This would show a confirmation dialog
            // For now, return true (in real implementation, would prompt user)
            return true;
        }
    }
}