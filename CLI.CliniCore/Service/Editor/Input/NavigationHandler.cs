using System;

namespace CLI.CliniCore.Service.Editor.Input
{
    /// <summary>
    /// Handles navigation key input for the clinical document editor.
    /// Manages cursor movement, selection, and scrolling through entries.
    /// </summary>
    public class NavigationHandler
    {
        private readonly EditorRenderer _renderer;

        public NavigationHandler(EditorRenderer renderer)
        {
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        /// <summary>
        /// Handle navigation key and return whether a navigation occurred
        /// </summary>
        public bool HandleNavigation(ConsoleKey key, EditorState state)
        {
            if (!state.HasEntries) return false;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    return MoveUp(state);
                case ConsoleKey.DownArrow:
                    return MoveDown(state);
                case ConsoleKey.Home:
                    return MoveToFirst(state);
                case ConsoleKey.End:
                    return MoveToLast(state);
                case ConsoleKey.PageUp:
                    return PageUp(state);
                case ConsoleKey.PageDown:
                    return PageDown(state);
                default:
                    return false;
            }
        }

        private bool MoveUp(EditorState state)
        {
            if (state.SelectedIndex > 0)
            {
                state.MoveSelectionUp();
                InvalidateAfterNavigation();
                return true;
            }
            return false;
        }

        private bool MoveDown(EditorState state)
        {
            if (state.SelectedIndex < state.FlattenedEntries.Count - 1)
            {
                state.MoveSelectionDown();
                InvalidateAfterNavigation();
                return true;
            }
            return false;
        }

        private bool MoveToFirst(EditorState state)
        {
            if (state.SelectedIndex != 0)
            {
                state.MoveToFirst();
                InvalidateAfterNavigation();
                return true;
            }
            return false;
        }

        private bool MoveToLast(EditorState state)
        {
            var lastIndex = state.FlattenedEntries.Count - 1;
            if (state.SelectedIndex != lastIndex)
            {
                state.MoveToLast();
                InvalidateAfterNavigation();
                return true;
            }
            return false;
        }

        private bool PageUp(EditorState state)
        {
            var newIndex = Math.Max(0, state.SelectedIndex - 10);
            if (newIndex != state.SelectedIndex)
            {
                state.SelectedIndex = newIndex;
                InvalidateAfterNavigation();
                return true;
            }
            return false;
        }

        private bool PageDown(EditorState state)
        {
            var newIndex = Math.Min(state.FlattenedEntries.Count - 1, state.SelectedIndex + 10);
            if (newIndex != state.SelectedIndex)
            {
                state.SelectedIndex = newIndex;
                InvalidateAfterNavigation();
                return true;
            }
            return false;
        }

        private void InvalidateAfterNavigation()
        {
            _renderer.InvalidateTreeZone();
            _renderer.InvalidateContentZone();
        }
    }
}
