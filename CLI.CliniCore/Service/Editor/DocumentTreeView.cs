using System;
using System.Linq;
using Core.CliniCore.ClinicalDoc;

namespace CLI.CliniCore.Service.Editor
{
    /// <summary>
    /// Renders the document tree view showing SOAP entries in hierarchical structure
    /// </summary>
    public class DocumentTreeView
    {
        private readonly ThreadSafeConsoleManager _console;

        public DocumentTreeView(ThreadSafeConsoleManager console)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
        }

        public void RenderTree(EditorState state, Region region)
        {
            if (!state.HasEntries)
            {
                RenderEmptyTree(region);
                return;
            }

            switch (state.ViewMode)
            {
                case EditorState.EditorViewMode.Tree:
                    RenderGroupedTree(state, region);
                    break;
                case EditorState.EditorViewMode.List:
                    RenderFlatList(state, region);
                    break;
                case EditorState.EditorViewMode.Details:
                    RenderDetailedView(state, region);
                    break;
            }
        }

        private void RenderEmptyTree(Region region)
        {
            _console.SetCursorPosition(region.Left, region.Top + 1);
            _console.SetForegroundColor(ConsoleColor.DarkGray);
            _console.Write("No entries yet");
            
            _console.SetCursorPosition(region.Left, region.Top + 3);
            _console.Write("Press 'A' to add");
            _console.ResetColor();
        }

        private void RenderGroupedTree(EditorState state, Region region)
        {
            var grouped = state.GetGroupedEntries();
            int currentLine = 0;
            int selectedIndex = state.SelectedIndex;
            int currentEntryIndex = 0;

            // Render SOAP groups
            currentLine += RenderSOAPGroup("S", "SUBJECTIVE", grouped.Subjective, 
                region, currentLine, selectedIndex, ref currentEntryIndex);

            currentLine += RenderSOAPGroup("O", "OBJECTIVE", grouped.Objective, 
                region, currentLine, selectedIndex, ref currentEntryIndex);

            currentLine += RenderSOAPGroup("A", "ASSESSMENT", grouped.Assessment, 
                region, currentLine, selectedIndex, ref currentEntryIndex);

            currentLine += RenderSOAPGroup("P", "PLAN", grouped.Plan, 
                region, currentLine, selectedIndex, ref currentEntryIndex);
        }

        private int RenderSOAPGroup(string letter, string title, 
            System.Collections.Generic.IList<AbstractClinicalEntry> entries,
            Region region, int startLine, int selectedIndex, ref int currentEntryIndex)
        {
            if (startLine >= region.Height) return 0;

            int linesUsed = 0;

            // Group header
            _console.SetCursorPosition(region.Left, region.Top + startLine);
            _console.SetForegroundColor(ConsoleColor.Cyan);
            _console.Write($"[{letter}] {title}");
            _console.ResetColor();
            linesUsed++;

            // Group entries
            foreach (var entry in entries.Take(Math.Max(0, region.Height - startLine - 1)))
            {
                if (startLine + linesUsed >= region.Height) break;

                bool isSelected = currentEntryIndex == selectedIndex;
                RenderTreeEntry(entry, region, startLine + linesUsed, 2, isSelected);
                currentEntryIndex++;
                linesUsed++;
            }

            // Add spacing after group
            linesUsed++;
            
            return linesUsed;
        }

        private void RenderFlatList(EditorState state, Region region)
        {
            var entries = state.FlattenedEntries;
            int selectedIndex = state.SelectedIndex;
            
            // Calculate visible range for scrolling
            int maxVisible = region.Height - 1;
            int scrollOffset = Math.Max(0, selectedIndex - maxVisible / 2);
            int endIndex = Math.Min(entries.Count, scrollOffset + maxVisible);

            for (int i = scrollOffset; i < endIndex; i++)
            {
                int displayLine = i - scrollOffset;
                bool isSelected = i == selectedIndex;
                
                RenderTreeEntry(entries[i], region, displayLine, 0, isSelected);
            }

            // Show scroll indicators if needed
            if (scrollOffset > 0)
            {
                _console.SetCursorPosition(region.Left + region.Width - 3, region.Top);
                _console.SetForegroundColor(ConsoleColor.DarkGray);
                _console.Write("↑");
                _console.ResetColor();
            }

            if (endIndex < entries.Count)
            {
                _console.SetCursorPosition(region.Left + region.Width - 3, region.Top + maxVisible - 1);
                _console.SetForegroundColor(ConsoleColor.DarkGray);
                _console.Write("↓");
                _console.ResetColor();
            }
        }

        private void RenderDetailedView(EditorState state, Region region)
        {
            var entry = state.SelectedEntry;
            if (entry == null)
            {
                RenderEmptyTree(region);
                return;
            }

            int line = 0;

            // Entry summary
            _console.SetCursorPosition(region.Left, region.Top + line++);
            _console.SetForegroundColor(ConsoleColor.Yellow);
            _console.Write(GetEntryDisplayName(entry));
            _console.ResetColor();

            _console.SetCursorPosition(region.Left, region.Top + line++);
            _console.SetForegroundColor(ConsoleColor.DarkGray);
            _console.Write($"Created: {entry.CreatedAt:MM/dd HH:mm}");
            _console.ResetColor();

            line++; // Spacing

            // Navigation info
            _console.SetCursorPosition(region.Left, region.Top + line++);
            _console.SetForegroundColor(ConsoleColor.Cyan);
            _console.Write($"Entry {state.SelectedIndex + 1} of {state.FlattenedEntries.Count}");
            _console.ResetColor();

            // Group context
            var grouped = state.GetGroupedEntries();
            _console.SetCursorPosition(region.Left, region.Top + line++);
            _console.Write($"S:{grouped.Subjective.Count} O:{grouped.Objective.Count}");
            
            _console.SetCursorPosition(region.Left, region.Top + line++);
            _console.Write($"A:{grouped.Assessment.Count} P:{grouped.Plan.Count}");
        }

        private void RenderTreeEntry(AbstractClinicalEntry entry, Region region, int line, int indent, bool isSelected)
        {
            if (line >= region.Height) return;

            _console.SetCursorPosition(region.Left + indent, region.Top + line);

            if (isSelected)
            {
                _console.SetForegroundColor(ConsoleColor.Black);
                _console.SetBackgroundColor(ConsoleColor.White);
            }
            else
            {
                _console.SetForegroundColor(GetEntryColor(entry));
            }

            var displayText = GetEntryDisplayText(entry, region.Width - indent - 1);
            _console.Write(displayText.PadRight(region.Width - indent - 1));
            
            _console.ResetColor();
        }

        private string GetEntryDisplayText(AbstractClinicalEntry entry, int maxWidth)
        {
            var prefix = GetEntryPrefix(entry);
            var name = GetEntryDisplayName(entry);
            var fullText = $"{prefix} {name}";

            // Ensure we don't go negative or exceed bounds
            maxWidth = Math.Max(10, maxWidth); // Minimum width of 10
            
            if (fullText.Length > maxWidth)
            {
                var truncateLength = Math.Max(7, maxWidth - 3); // Leave room for "..."
                return fullText.Substring(0, truncateLength) + "...";
            }

            return fullText;
        }

        private string GetEntryPrefix(AbstractClinicalEntry entry)
        {
            return entry switch
            {
                ObservationEntry => "📝",     // Subjective
                DiagnosisEntry => "🔍",       // Objective
                PrescriptionEntry => "💊",    // Objective
                AssessmentEntry => "📋",      // Assessment
                PlanEntry => "📋",           // Plan
                _ => "•"
            };
        }

        private string GetEntryDisplayName(AbstractClinicalEntry entry)
        {
            return entry switch
            {
                ObservationEntry => $"Observation: {TruncateText(entry.Content, 25)}",
                DiagnosisEntry => $"Diagnosis: {TruncateText(entry.Content, 25)}",
                PrescriptionEntry rx => $"Rx: {TruncateText(rx.MedicationName, 25)}",
                AssessmentEntry => $"Assessment: {TruncateText(entry.Content, 25)}",
                PlanEntry => $"Plan: {TruncateText(entry.Content, 25)}",
                _ => "Unknown Entry"
            };
        }

        private ConsoleColor GetEntryColor(AbstractClinicalEntry entry)
        {
            return entry switch
            {
                ObservationEntry => ConsoleColor.Green,      // Subjective
                DiagnosisEntry => ConsoleColor.Yellow,       // Objective
                PrescriptionEntry => ConsoleColor.Magenta,   // Objective
                AssessmentEntry => ConsoleColor.Cyan,        // Assessment
                PlanEntry => ConsoleColor.Blue,              // Plan
                _ => ConsoleColor.White
            };
        }

        private string TruncateText(string? text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return "...";
            
            maxLength = Math.Max(7, maxLength); // Minimum length of 7
            
            if (text.Length <= maxLength) return text;
            
            var truncateLength = Math.Max(4, maxLength - 3); // Leave room for "..."
            return text.Substring(0, truncateLength) + "...";
        }
    }
}