using System;
using System.Linq;
using System.Text;

namespace CLI.CliniCore.Service.Editor
{
    /// <summary>
    /// Handles rendering of the split-pane editor interface with
    /// document tree on left and content view on right.
    /// </summary>
    public class EditorRenderer
    {
        private readonly ThreadSafeConsoleManager _console;
        private readonly DocumentTreeView _treeView;
        private bool _layoutInvalid = true;
        
        // Layout regions
        private EditorLayout _layout = new();

        public EditorRenderer(ThreadSafeConsoleManager console)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _treeView = new DocumentTreeView(_console);
            CalculateLayout();
        }

        public void RenderEditor(EditorState state)
        {
            if (_layoutInvalid)
            {
                CalculateLayout();
                _layoutInvalid = false;
            }

            // Clear screen and ensure we start at absolute top-left
            _console.Clear();
            
            // Force cursor to absolute top-left and ensure clean state
            _console.SetCursorPosition(0, 0);
            _console.ResetColor();
            
            // Draw borders first to establish the layout
            DrawBorders();
            
            // Clear only the content areas inside the borders
            ClearContentAreas();
            
            // Draw content in the regions
            DrawTreePane(state);
            DrawContentPane(state);
            DrawStatusBar(state);
            
            // Hide cursor to prevent flickering
            try
            {
                _console.SetCursorPosition(_layout.TotalWidth - 1, _layout.TotalHeight - 1);
            }
            catch
            {
                // Ignore cursor positioning errors
            }
        }

        public void InvalidateLayout()
        {
            _layoutInvalid = true;
        }

        public bool LayoutInvalid => _layoutInvalid;

        public void ShowHelpOverlay(int width, int height)
        {
            var helpText = new[]
            {
                "=== CLINICAL DOCUMENT EDITOR HELP ===",
                "",
                "Navigation:",
                "  ↑/↓         - Move selection up/down",
                "  Home/End    - First/last entry",
                "  Page Up/Dn  - Scroll by page",
                "",
                "Commands:",
                "  A           - Add new SOAP entry",
                "  E           - Edit selected entry",
                "  D           - Delete selected entry",
                "  V           - Toggle view mode",
                "  S           - Save document",
                "  ?           - Show this help",
                "  Ctrl+X      - Exit editor",
                "  Escape      - Cancel current operation",
                "",
                "View Modes:",
                "  Tree        - Grouped by SOAP categories",
                "  List        - Flat list of all entries",
                "  Details     - Detailed view of selection",
                "",
                "Press any key to continue..."
            };

            int startY = Math.Max(0, (height - helpText.Length) / 2);
            int startX = Math.Max(0, (width - 50) / 2);

            _console.SetForegroundColor(ConsoleColor.White);
            _console.SetBackgroundColor(ConsoleColor.DarkBlue);

            for (int i = 0; i < helpText.Length; i++)
            {
                Console.SetCursorPosition(startX, startY + i);
                _console.Write(helpText[i].PadRight(50));
            }

            _console.ResetColor();
        }

        public void ShowStatusMessage(string message, MessageType type, int width, int height)
        {
            _console.SetCursorPosition(2, height - 1);
            
            var color = type switch
            {
                MessageType.Success => ConsoleColor.Green,
                MessageType.Warning => ConsoleColor.Yellow,
                MessageType.Error => ConsoleColor.Red,
                _ => ConsoleColor.White
            };

            _console.SetForegroundColor(color);
            _console.Write(message.PadRight(width - 4));
            _console.ResetColor();
        }

        public void ShowPrompt(string prompt)
        {
            _console.SetCursorPosition(2, _layout.StatusRegion.Top);
            _console.SetForegroundColor(ConsoleColor.Yellow);
            _console.Write(prompt);
            _console.ResetColor();
        }

        private void CalculateLayout()
        {
            var (width, height) = _console.GetDimensionsForceRefresh();
            
            // Ensure minimum dimensions
            width = Math.Max(width, 80);
            height = Math.Max(height, 24);

            var treeWidth = width / 3; // Left 1/3 for tree
            var contentWidth = width - treeWidth - 3; // Right 2/3 minus borders

            // Ensure minimum widths to prevent rendering issues
            treeWidth = Math.Max(20, treeWidth);
            contentWidth = Math.Max(30, contentWidth);

            _layout = new EditorLayout
            {
                TotalWidth = width,
                TotalHeight = height,
                TreeRegion = new Region(1, 2, treeWidth - 1, height - 4),
                ContentRegion = new Region(treeWidth + 2, 2, contentWidth, height - 4),  
                StatusRegion = new Region(1, height - 1, width - 2, 1),
                TreeWidth = treeWidth,
                ContentWidth = contentWidth
            };
        }

        private void DrawBorders()
        {
            var width = _layout.TotalWidth;
            var height = _layout.TotalHeight;

            _console.SetForegroundColor(ConsoleColor.DarkGray);

            // Top border with safe calculations
            var leftBorderWidth = Math.Max(0, _layout.TreeWidth - 1);
            var rightBorderWidth = Math.Max(0, width - _layout.TreeWidth - 2);
            
            _console.SetCursorPosition(0, 0);
            _console.Write("┌" + new string('─', leftBorderWidth) + "┬" + new string('─', rightBorderWidth) + "┐");


            // Header labels
            _console.SetCursorPosition(2, 1);
            _console.SetForegroundColor(ConsoleColor.Cyan);
            _console.Write("Document Structure");
            
            _console.SetCursorPosition(_layout.TreeWidth + 3, 1);
            _console.Write("Entry Content");

            // Middle borders
            for (int y = 1; y < height - 2; y++)
            {
                _console.SetCursorPosition(0, y);
                _console.SetForegroundColor(ConsoleColor.DarkGray);
                _console.Write("│");
                
                _console.SetCursorPosition(_layout.TreeWidth, y);
                _console.Write("│");
                
                _console.SetCursorPosition(width - 1, y);
                _console.Write("│");
            }

            // Bottom border  
            _console.SetCursorPosition(0, height - 2);
            _console.Write("├" + new string('─', _layout.TreeWidth - 1) + "┼" + new string('─', width - _layout.TreeWidth - 2) + "┤");

            _console.SetCursorPosition(0, height - 1);
            _console.Write("└" + new string('─', width - 2) + "┘");

            _console.ResetColor();
        }

        private void ClearContentAreas()
        {
            // Clear tree pane area with debug
            var treeRegion = _layout.TreeRegion;
            for (int y = 0; y < treeRegion.Height; y++)
            {
                var actualY = treeRegion.Top + y;
                _console.SetCursorPosition(treeRegion.Left, actualY);
                // Clear tree pane area
                _console.Write(new string(' ', treeRegion.Width));
            }
            
            // Clear content pane area
            var contentRegion = _layout.ContentRegion;
            for (int y = 0; y < contentRegion.Height; y++)
            {
                _console.SetCursorPosition(contentRegion.Left, contentRegion.Top + y);
                _console.Write(new string(' ', contentRegion.Width));
            }
        }

        private void DrawTreePane(EditorState state)
        {
            _treeView.RenderTree(state, _layout.TreeRegion);
        }

        private void DrawContentPane(EditorState state)
        {
            var region = _layout.ContentRegion;
            var entry = state.SelectedEntry;

            if (entry == null)
            {
                _console.SetCursorPosition(region.Left, region.Top + 1);
                _console.SetForegroundColor(ConsoleColor.DarkGray);
                _console.Write("No entry selected");
                _console.ResetColor();
                return;
            }

            var content = FormatEntryContent(entry, region.Width);
            var lines = content.Split('\n');

            for (int i = 0; i < Math.Min(lines.Length, region.Height); i++)
            {
                _console.SetCursorPosition(region.Left, region.Top + i);
                var line = lines[i] ?? "";
                
                // Ensure we have a valid width, leaving space for right border
                var safeWidth = Math.Max(10, region.Width - 1);
                
                if (line.Length > safeWidth)
                {
                    var truncateLength = Math.Max(7, safeWidth - 3);
                    line = line.Substring(0, truncateLength) + "...";
                }
                _console.Write(line);
            }
        }

        private void DrawStatusBar(EditorState state)
        {
            var region = _layout.StatusRegion;
            var status = BuildStatusText(state);

            _console.SetCursorPosition(region.Left, region.Top);
            _console.SetForegroundColor(ConsoleColor.White);
            _console.Write(status.PadRight(region.Width));
            _console.ResetColor();
        }

        private string FormatEntryContent(Core.CliniCore.ClinicalDoc.AbstractClinicalEntry entry, int maxWidth)
        {
            var sb = new StringBuilder();
            
            // Entry type and basic info
            sb.AppendLine($"Type: {entry.GetType().Name}");
            sb.AppendLine($"Created: {entry.CreatedAt:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"ID: {entry.Id:N}");
            sb.AppendLine();

            // Entry content - all entries have Content property from base class
            sb.AppendLine("CONTENT:");
            sb.AppendLine(WrapText(entry.Content, maxWidth - 2));

            // Additional type-specific info if available
            switch (entry)
            {
                case Core.CliniCore.ClinicalDoc.PrescriptionEntry rx:
                    sb.AppendLine();
                    sb.AppendLine("PRESCRIPTION DETAILS:");
                    sb.AppendLine($"Medication: {rx.MedicationName}");
                    if (!string.IsNullOrEmpty(rx.Dosage))
                        sb.AppendLine($"Dosage: {rx.Dosage}");
                    if (!string.IsNullOrEmpty(rx.Instructions))
                        sb.AppendLine($"Instructions: {WrapText(rx.Instructions, maxWidth - 2)}");
                    break;
            }

            return sb.ToString();
        }

        private string WrapText(string text, int width)
        {
            if (string.IsNullOrEmpty(text)) return "";
            
            width = Math.Max(10, width); // Minimum width to prevent issues

            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var lines = new StringBuilder();
            var currentLine = new StringBuilder();

            foreach (var word in words)
            {
                if (currentLine.Length + word.Length + 1 > width)
                {
                    if (currentLine.Length > 0)
                    {
                        lines.AppendLine(currentLine.ToString());
                        currentLine.Clear();
                    }
                }

                if (currentLine.Length > 0) currentLine.Append(' ');
                currentLine.Append(word);
            }

            if (currentLine.Length > 0)
            {
                lines.Append(currentLine.ToString());
            }

            return lines.ToString();
        }

        private string BuildStatusText(EditorState state)
        {
            var sb = new StringBuilder();
            
            sb.Append($"Entries: {state.FlattenedEntries.Count}");
            
            if (state.HasEntries)
            {
                sb.Append($" | Selected: {state.SelectedIndex + 1}/{state.FlattenedEntries.Count}");
            }
            
            sb.Append($" | View: {state.ViewMode}");
            
            if (state.IsDirty)
            {
                sb.Append(" | [MODIFIED]");
            }

            sb.Append(" | A-Add E-Edit D-Delete V-View S-Save Ctrl+X-Exit");

            return sb.ToString();
        }
    }

    /// <summary>
    /// Defines the layout regions for the editor interface
    /// </summary>
    public record EditorLayout
    {
        public int TotalWidth { get; init; }
        public int TotalHeight { get; init; }
        public Region TreeRegion { get; init; } = new Region(0, 0, 0, 0);
        public Region ContentRegion { get; init; } = new Region(0, 0, 0, 0);
        public Region StatusRegion { get; init; } = new Region(0, 0, 0, 0);
        public int TreeWidth { get; init; }
        public int ContentWidth { get; init; }
    }

    /// <summary>
    /// Represents a rectangular region on the console
    /// </summary>
    public record Region(int Left, int Top, int Width, int Height);
}