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
        private readonly StatusBarInputHandler _inputHandler;
        private readonly ZonedRenderer _zonedRenderer;
        private bool _layoutInvalid = true;
        
        // Layout regions
        private EditorLayout _layout = new();
        private int _statusBarHeight = 1; // Dynamic status bar height
        
        // Zone identifiers
        private const string ZONE_BORDERS = "borders";
        private const string ZONE_TREE = "tree";
        private const string ZONE_CONTENT = "content";
        private const string ZONE_STATUS = "status";

        public EditorRenderer(ThreadSafeConsoleManager console)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _treeView = new DocumentTreeView(_console);
            _inputHandler = new StatusBarInputHandler(_console);
            _zonedRenderer = new ZonedRenderer(_console);
            
            InitializeZones();
            CalculateLayout();
        }
        
        public StatusBarInputHandler InputHandler => _inputHandler;
        
        /// <summary>
        /// Shows a help overlay as a temporary zone
        /// </summary>
        public void ShowHelpOverlay()
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
                "Press any key to continue..."
            };

            int startY = Math.Max(2, (_layout.TotalHeight - helpText.Length) / 2);
            int startX = Math.Max(2, (_layout.TotalWidth - 50) / 2);
            
            // Create temporary overlay zone
            var overlayRegion = new Region(startX, startY, 50, helpText.Length);
            
            _console.SetForegroundColor(ConsoleColor.White);
            _console.SetBackgroundColor(ConsoleColor.DarkBlue);

            for (int i = 0; i < helpText.Length; i++)
            {
                _console.SetCursorPosition(startX, startY + i);
                _console.Write(helpText[i].PadRight(48)); // -2 for margins
            }

            _console.ResetColor();
        }
        
        /// <summary>
        /// Shows a status message in the status zone
        /// </summary>
        public void ShowStatusMessage(string message, MessageType type)
        {
            var color = type switch
            {
                MessageType.Success => ConsoleColor.Green,
                MessageType.Warning => ConsoleColor.Yellow,
                MessageType.Error => ConsoleColor.Red,
                _ => ConsoleColor.White
            };

            _console.SetCursorPosition(_layout.StatusRegion.Left, _layout.StatusRegion.Top);
            _console.SetForegroundColor(color);
            _console.Write(message.PadRight(_layout.StatusRegion.Width));
            _console.ResetColor();
        }
        
        /// <summary>
        /// Shows a prompt message in the status bar - NON-BLOCKING
        /// </summary>
        public void ShowPromptMessage(string prompt)
        {
            _console.SetCursorPosition(_layout.StatusRegion.Left, _layout.StatusRegion.Top);
            _console.SetForegroundColor(ConsoleColor.Yellow);
            _console.Write((prompt + " (Y/n): ").PadRight(_layout.StatusRegion.Width));
            _console.ResetColor();
        }

        public void RenderEditor(EditorState state)
        {
            // Check for status bar height changes that cascade to layout
            CheckAndHandleStatusBarHeightChange();
            
            bool needsFullRedraw = false;
            
            // Update layout if invalid
            if (_layoutInvalid)
            {
                // CRITICAL: Full screen clear needed for layout changes (like resize)
                _console.Clear();
                _console.SetCursorPosition(0, 0);
                _console.ResetColor();
                needsFullRedraw = true;
                
                UpdateZoneLayouts();
                _layoutInvalid = false;
            }

            // Use zone-based rendering for normal updates, full redraw for layout changes
            if (needsFullRedraw)
            {
                // Force all zones to redraw after full clear
                _zonedRenderer.InvalidateAll();
            }
            
            _zonedRenderer.RenderDirtyZones(state);
            
            // Position cursor appropriately
            PositionCursorForCurrentMode();
        }

        public void InvalidateLayout()
        {
            _layoutInvalid = true;
            // Invalidate all zones since layout affects everything
            _zonedRenderer.InvalidateAll();
        }
        
        /// <summary>
        /// Invalidates only the status zone for live text updates
        /// </summary>
        public void InvalidateStatusZone()
        {
            _zonedRenderer.InvalidateZone(ZONE_STATUS);
        }
        
        /// <summary>
        /// Invalidates only the tree zone for view mode changes
        /// </summary>
        public void InvalidateTreeZone()
        {
            _zonedRenderer.InvalidateZone(ZONE_TREE);
        }
        
        /// <summary>
        /// Invalidates only the content zone for entry updates
        /// </summary>
        public void InvalidateContentZone()
        {
            _zonedRenderer.InvalidateZone(ZONE_CONTENT);
        }

        public bool LayoutInvalid => _layoutInvalid || _zonedRenderer.IsZoneDirty(ZONE_BORDERS) || 
                                     _zonedRenderer.IsZoneDirty(ZONE_TREE) || 
                                     _zonedRenderer.IsZoneDirty(ZONE_CONTENT) || 
                                     _zonedRenderer.IsZoneDirty(ZONE_STATUS);




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

            // Adjust regions based on status bar height
            var contentHeight = height - 3 - _statusBarHeight; // -3 for borders and header
            var statusTop = height - _statusBarHeight;
            
            _layout = new EditorLayout
            {
                TotalWidth = width,
                TotalHeight = height,
                TreeRegion = new Region(1, 2, treeWidth - 1, contentHeight),
                ContentRegion = new Region(treeWidth + 2, 2, contentWidth, contentHeight),  
                StatusRegion = new Region(1, statusTop, width - 2, _statusBarHeight),
                TreeWidth = treeWidth,
                ContentWidth = contentWidth
            };
        }

        private void DrawBordersInternal()
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

            // Bottom border - adjust for dynamic status bar height
            var bottomBorderY = height - _statusBarHeight - 1;
            _console.SetCursorPosition(0, bottomBorderY);
            _console.Write("├" + new string('─', _layout.TreeWidth - 1) + "┼" + new string('─', width - _layout.TreeWidth - 2) + "┤");

            // Draw status bar borders
            for (int y = bottomBorderY + 1; y < height - 1; y++)
            {
                _console.SetCursorPosition(0, y);
                _console.Write("│");
                _console.SetCursorPosition(width - 1, y);
                _console.Write("│");
            }
            
            _console.SetCursorPosition(0, height - 1);
            _console.Write("└" + new string('─', width - 2) + "┘");

            _console.ResetColor();
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
                    if (!string.IsNullOrEmpty(rx.Frequency))
                        sb.AppendLine($"Frequency: {rx.Frequency}");
                    if (!string.IsNullOrEmpty(rx.Route))
                        sb.AppendLine($"Route: {rx.Route}");
                    if (!string.IsNullOrEmpty(rx.Duration))
                        sb.AppendLine($"Duration: {rx.Duration}");
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

        
        private void InitializeZones()
        {
            // Register all rendering zones with their callbacks
            _zonedRenderer.RegisterZone(ZONE_BORDERS, new Region(0, 0, 80, 24), RenderBordersZone, 10);
            _zonedRenderer.RegisterZone(ZONE_TREE, new Region(1, 2, 26, 20), RenderTreeZone, 20);
            _zonedRenderer.RegisterZone(ZONE_CONTENT, new Region(28, 2, 50, 20), RenderContentZone, 20);
            _zonedRenderer.RegisterZone(ZONE_STATUS, new Region(1, 23, 78, 1), RenderStatusZone, 30);
            
            // Set up dependencies - content and tree depend on borders for layout
            _zonedRenderer.AddDependency(ZONE_TREE, ZONE_BORDERS);
            _zonedRenderer.AddDependency(ZONE_CONTENT, ZONE_BORDERS);
            _zonedRenderer.AddDependency(ZONE_STATUS, ZONE_BORDERS);
        }
        
        private void CheckAndHandleStatusBarHeightChange()
        {
            if (_inputHandler.IsActive)
            {
                var requiredHeight = _inputHandler.RequiredHeight;
                if (requiredHeight != _statusBarHeight)
                {
                    HandleStatusBarHeightChange(requiredHeight);
                }
            }
            else if (_statusBarHeight != 1)
            {
                HandleStatusBarHeightChange(1);
            }
        }
        
        private void HandleStatusBarHeightChange(int newHeight)
        {
            var oldHeight = _statusBarHeight;
            _statusBarHeight = Math.Min(newHeight, 10); // Max 10 lines
            
            if (oldHeight != _statusBarHeight)
            {
                _layoutInvalid = true;
                
                // Clear the affected area first (from old content bottom to screen bottom)
                var clearRegion = new Region(0, _layout.ContentRegion.Top + _layout.ContentRegion.Height - Math.Abs(_statusBarHeight - oldHeight), 
                                           _layout.TotalWidth, Math.Abs(_statusBarHeight - oldHeight) + _statusBarHeight + 1);
                ClearRegion(clearRegion);
                
                // Invalidate zones that will move
                _zonedRenderer.InvalidateZone(ZONE_BORDERS);
                _zonedRenderer.InvalidateZone(ZONE_TREE);
                _zonedRenderer.InvalidateZone(ZONE_CONTENT);
                _zonedRenderer.InvalidateZone(ZONE_STATUS);
            }
        }
        
        private void UpdateZoneLayouts()
        {
            CalculateLayout();
            
            // Update all zone boundaries based on new layout
            var borderRegion = new Region(0, 0, _layout.TotalWidth, _layout.TotalHeight);
            _zonedRenderer.UpdateZoneBounds(ZONE_BORDERS, borderRegion);
            _zonedRenderer.UpdateZoneBounds(ZONE_TREE, _layout.TreeRegion);
            _zonedRenderer.UpdateZoneBounds(ZONE_CONTENT, _layout.ContentRegion);
            _zonedRenderer.UpdateZoneBounds(ZONE_STATUS, _layout.StatusRegion);
        }
        
        private void PositionCursorForCurrentMode()
        {
            if (!_inputHandler.IsActive)
            {
                try
                {
                    // Hide cursor at bottom-right when not editing
                    _console.SetCursorPosition(_layout.TotalWidth - 1, _layout.TotalHeight - 1);
                }
                catch
                {
                    // Ignore cursor positioning errors
                }
            }
            // If input handler is active, it manages its own cursor positioning
        }
        
        // Zone render callbacks
        private void RenderBordersZone(RenderZone zone, object? data)
        {
            DrawBordersInternal();
        }
        
        private void RenderTreeZone(RenderZone zone, object? data)
        {
            if (data is EditorState state)
            {
                ClearRegion(zone.Bounds);
                DrawTreePane(state);
            }
        }
        
        private void RenderContentZone(RenderZone zone, object? data)
        {
            if (data is EditorState state)
            {
                ClearRegion(zone.Bounds);
                DrawContentPane(state);
            }
        }
        
        private void RenderStatusZone(RenderZone zone, object? data)
        {
            if (data is EditorState state)
            {
                ClearRegion(zone.Bounds);
                if (_inputHandler.IsActive)
                {
                    _inputHandler.Render(_layout.StatusRegion);
                }
                else
                {
                    DrawStatusBar(state);
                }
            }
        }
        
        private void ClearRegion(Region region)
        {
            for (int y = 0; y < region.Height; y++)
            {
                _console.SetCursorPosition(region.Left, region.Top + y);
                _console.Write(new string(' ', region.Width));
            }
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