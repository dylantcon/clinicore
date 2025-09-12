using System;
using System.Threading;
using System.Threading.Tasks;
using Core.CliniCore.ClinicalDoc;
using Core.CliniCore.Commands;

namespace CLI.CliniCore.Service.Editor
{
    /// <summary>
    /// Comprehensive text editor for clinical documents providing split-pane interface
    /// with tree navigation and real-time editing capabilities.
    /// </summary>
    public class ClinicalDocumentEditor : AbstractConsoleEngine
    {
        private readonly EditorRenderer _renderer;
        private readonly EditorKeyHandler _keyHandler;
        private readonly new CommandInvoker _commandInvoker;
        private readonly ConsoleCommandParser _commandParser;
        
        private EditorState? _editorState;
        private Task? _resizeListenerTask;
        private CancellationTokenSource? _resizeListenerCancellation;
        private bool _editorActive = false;

        public ClinicalDocumentEditor(
            ConsoleSessionManager sessionManager,
            CommandInvoker commandInvoker,
            ConsoleCommandParser commandParser)
            : base(sessionManager, null, commandInvoker)
        {
            _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
            _commandParser = commandParser ?? throw new ArgumentNullException(nameof(commandParser));
            _renderer = new EditorRenderer(_console);
            _keyHandler = new EditorKeyHandler(this, _console);
        }

        /// <summary>
        /// Launches the editor for the specified clinical document
        /// </summary>
        public void EditDocument(ClinicalDocument document)
        {
            if (document == null)
            {
                DisplayMessage("No document provided for editing.", MessageType.Error);
                return;
            }

            try
            {
                _editorState = new EditorState(document);
                _console.SetMode(ThreadSafeConsoleManager.ConsoleMode.Editor);
                _editorActive = true;

                StartResizeListener();
                DisplayWelcomeMessage();
                EnterEditorLoop();
            }
            catch (Exception ex)
            {
                DisplayMessage($"Failed to open document editor: {ex.Message}", MessageType.Error);
            }
            finally
            {
                ExitEditor();
            }
        }

        /// <summary>
        /// Main editor input/rendering loop
        /// </summary>
        private void EnterEditorLoop()
        {
            bool shouldExit = false;
            bool forceRedraw = true;

            while (_editorActive && !shouldExit && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    // Render the current editor state (always on first run or after layout change)
                    if (forceRedraw)
                    {
                        _renderer.RenderEditor(_editorState!);
                        forceRedraw = false;
                    }

                    // Check if we need to redraw due to resize
                    if (_renderer.LayoutInvalid)
                    {
                        _renderer.RenderEditor(_editorState!);
                    }

                    // Handle keyboard input with timeout to allow periodic redraws
                    ConsoleKeyInfo key;
                    if (TryReadKeyWithTimeout(out key, 250)) // 250ms timeout
                    {
                        var result = _keyHandler.HandleKeyInput(key, _editorState!);

                    switch (result.Action)
                    {
                        case EditorAction.Continue:
                            // Continue normal operation
                            break;

                        case EditorAction.Exit:
                            if (_editorState!.IsDirty)
                            {
                                if (PromptSaveChanges())
                                {
                                    SaveDocument();
                                }
                            }
                            shouldExit = true;
                            break;

                        case EditorAction.Save:
                            SaveDocument();
                            break;

                        case EditorAction.Refresh:
                            _editorState!.RefreshFlattenedEntries();
                            break;

                        case EditorAction.ShowHelp:
                            ShowHelpOverlay();
                            break;
                    }

                    if (!string.IsNullOrEmpty(result.Message))
                    {
                        DisplayStatusMessage(result.Message, result.MessageType);
                    }

                    forceRedraw = true; // Redraw after any action
                    }
                    else
                    {
                        // No key pressed, add small delay to prevent CPU spinning
                        Thread.Sleep(50);
                        continue;
                    }
                }
                catch (OperationCanceledException)
                {
                    // Clean shutdown requested
                    break;
                }
                catch (Exception ex)
                {
                    DisplayMessage($"Editor error: {ex.Message}", MessageType.Error);
                    Pause("Press any key to continue...");
                }
            }
        }

        private bool TryReadKeyWithTimeout(out ConsoleKeyInfo key, int timeoutMs)
        {
            key = default;
            
            // Check if key is available without blocking
            if (Console.KeyAvailable)
            {
                key = _console.ReadKey(true);
                return true;
            }
            
            // If no key available, return false to allow main loop to continue
            return false;
        }

        private void StartResizeListener()
        {
            _resizeListenerCancellation = new CancellationTokenSource();
            _resizeListenerTask = Task.Run(async () =>
            {
                var (lastWidth, lastHeight) = _console.GetDimensions();
                
                while (!_resizeListenerCancellation.Token.IsCancellationRequested)
                {
                    try
                    {
                        var (width, height) = _console.GetDimensions();
                        
                        if ((width != lastWidth || height != lastHeight) && _editorActive)
                        {
                            lastWidth = width;
                            lastHeight = height;
                            
                            // Terminal was resized - trigger re-render on next main loop iteration
                            _renderer.InvalidateLayout();
                        }
                        
                        await Task.Delay(100, _resizeListenerCancellation.Token); // Slower polling to avoid performance issues
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception)
                    {
                        // Ignore resize detection errors
                    }
                }
            }, _resizeListenerCancellation.Token);
        }

        private void ExitEditor()
        {
            _editorActive = false;
            
            // Stop resize listener
            _resizeListenerCancellation?.Cancel();
            _resizeListenerTask?.Wait(1000);
            _resizeListenerCancellation?.Dispose();
            
            // Reset console mode
            _console.SetMode(ThreadSafeConsoleManager.ConsoleMode.Menu);
            Clear();
        }

        private void SaveDocument()
        {
            if (_editorState?.Document == null) return;

            try
            {
                // Use existing UpdateClinicalDocumentCommand for persistence
                var parameters = new CommandParameters();
                parameters.SetParameter("document_id", _editorState.Document.Id);
                parameters.SetParameter("status", "completed"); // Simple status for now

                // For now, just mark as clean since we don't have direct command execution here
                // In a full implementation, this would use the command factory to get the command
                _editorState.MarkClean();
                DisplayStatusMessage("Document saved (placeholder)", MessageType.Success);
            }
            catch (Exception ex)
            {
                DisplayStatusMessage($"Save error: {ex.Message}", MessageType.Error);
            }
        }

        private bool PromptSaveChanges()
        {
            _renderer.ShowPrompt("Document has unsaved changes. Save before exiting? (Y/n): ");
            var key = _console.ReadKey(true);
            _console.WriteLine();
            
            return key.Key == ConsoleKey.Enter || 
                   key.KeyChar == 'y' || 
                   key.KeyChar == 'Y';
        }

        private void DisplayWelcomeMessage()
        {
            _console.Clear();
            DisplayHeader("Clinical Document Editor");
            DisplayMessage($"Editing document for Patient ID: {_editorState!.Document.PatientId}", MessageType.Info);
            DisplayMessage("Press '?' for help, Ctrl+X to exit", MessageType.Debug);
            DisplaySeparator();
        }

        private void ShowHelpOverlay()
        {
            var (width, height) = _console.GetDimensions();
            _renderer.ShowHelpOverlay(width, height);
            
            _console.ReadKey(true); // Wait for any key
        }

        private void DisplayStatusMessage(string message, MessageType type)
        {
            var (width, height) = _console.GetDimensions();
            _renderer.ShowStatusMessage(message, type, width, height);
        }

        // Make required abstract methods available to key handler
        public override string? GetUserInput(string prompt)
        {
            return _console.IsInputRedirected ? _console.ReadLine() : null;
        }

        public override string? GetSecureInput(string prompt)
        {
            return null; // Not needed in editor context
        }

        public new void Dispose()
        {
            ExitEditor();
            base.Dispose();
        }
    }

    /// <summary>
    /// Result of processing a keyboard input in the editor
    /// </summary>
    public record EditorKeyResult(
        EditorAction Action,
        string? Message = null,
        MessageType MessageType = MessageType.Info
    );

    /// <summary>
    /// Actions that can be taken by the editor
    /// </summary>
    public enum EditorAction
    {
        Continue,   // Keep editing
        Exit,       // Close editor
        Save,       // Save document
        Refresh,    // Refresh document state
        ShowHelp    // Display help overlay
    }
}