using System;
using System.Text;
using System.Collections.Generic;

namespace CLI.CliniCore.Service.Editor
{
    /// <summary>
    /// Handles multi-line text input integrated into the status bar area
    /// </summary>
    public class StatusBarInputHandler
    {
        private readonly ThreadSafeConsoleManager _console;
        private StringBuilder _currentText;
        private int _cursorPosition;
        private List<string> _wrappedLines;
        private int _currentLineIndex;
        private int _currentColumnIndex;
        private bool _isActive;
        private string _prompt;
        private int _maxWidth;

        public StatusBarInputHandler(ThreadSafeConsoleManager console)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _currentText = new StringBuilder();
            _wrappedLines = new List<string>();
            _prompt = "";
        }

        public bool IsActive => _isActive;
        public int RequiredHeight => _wrappedLines.Count + 1; // +1 for the prompt line
        public string CurrentText => _currentText.ToString();

        /// <summary>
        /// Start editing with optional initial text
        /// </summary>
        public void StartEditing(string prompt, string initialText = "", int maxWidth = 0)
        {
            _prompt = prompt;
            _currentText = new StringBuilder(initialText ?? "");
            _cursorPosition = _currentText.Length;
            _isActive = true;
            _maxWidth = maxWidth > 0 ? maxWidth : 80;
            
            UpdateWrappedLines();
            UpdateCursorIndices();
        }

        /// <summary>
        /// Process a key input and return true if input continues, false if done
        /// </summary>
        public bool ProcessKey(ConsoleKeyInfo keyInfo)
        {
            if (!_isActive) return false;

            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter:
                    if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift))
                    {
                        // Shift+Enter adds a newline
                        InsertCharacter('\n');
                        return true;
                    }
                    else
                    {
                        // Enter alone submits
                        _isActive = false;
                        return false;
                    }

                case ConsoleKey.Escape:
                    // Cancel editing
                    _currentText.Clear();
                    _isActive = false;
                    return false;

                case ConsoleKey.Backspace:
                    if (_cursorPosition > 0)
                    {
                        _currentText.Remove(_cursorPosition - 1, 1);
                        _cursorPosition--;
                        UpdateWrappedLines();
                        UpdateCursorIndices();
                    }
                    return true;

                case ConsoleKey.Delete:
                    if (_cursorPosition < _currentText.Length)
                    {
                        _currentText.Remove(_cursorPosition, 1);
                        UpdateWrappedLines();
                        UpdateCursorIndices();
                    }
                    return true;

                case ConsoleKey.LeftArrow:
                    if (_cursorPosition > 0)
                    {
                        _cursorPosition--;
                        UpdateCursorIndices();
                    }
                    return true;

                case ConsoleKey.RightArrow:
                    if (_cursorPosition < _currentText.Length)
                    {
                        _cursorPosition++;
                        UpdateCursorIndices();
                    }
                    return true;

                case ConsoleKey.Home:
                    _cursorPosition = 0;
                    UpdateCursorIndices();
                    return true;

                case ConsoleKey.End:
                    _cursorPosition = _currentText.Length;
                    UpdateCursorIndices();
                    return true;

                default:
                    if (!char.IsControl(keyInfo.KeyChar))
                    {
                        InsertCharacter(keyInfo.KeyChar);
                    }
                    return true;
            }
        }

        /// <summary>
        /// Render the input area within the given region
        /// </summary>
        public void Render(Region region)
        {
            if (!_isActive) return;

            // Clear the region first
            for (int y = 0; y < region.Height; y++)
            {
                _console.SetCursorPosition(region.Left, region.Top + y);
                _console.Write(new string(' ', region.Width));
            }

            // Draw prompt on first line
            _console.SetCursorPosition(region.Left, region.Top);
            _console.SetForegroundColor(ConsoleColor.Yellow);
            _console.Write(_prompt);
            _console.ResetColor();

            // Draw wrapped text lines
            int lineY = 0;
            foreach (var line in _wrappedLines)
            {
                if (lineY >= region.Height - 1) break; // -1 for prompt line
                
                _console.SetCursorPosition(region.Left, region.Top + lineY + 1);
                _console.Write(line);
                lineY++;
            }

            // Position cursor
            if (_currentLineIndex < _wrappedLines.Count && _currentLineIndex < region.Height - 1)
            {
                _console.SetCursorPosition(
                    region.Left + _currentColumnIndex, 
                    region.Top + _currentLineIndex + 1); // +1 for prompt line
            }
        }

        /// <summary>
        /// Stop editing and return the final text
        /// </summary>
        public string StopEditing()
        {
            _isActive = false;
            var result = _currentText.ToString();
            _currentText.Clear();
            _wrappedLines.Clear();
            return result;
        }

        private void InsertCharacter(char ch)
        {
            _currentText.Insert(_cursorPosition, ch);
            _cursorPosition++;
            UpdateWrappedLines();
            UpdateCursorIndices();
        }

        private void UpdateWrappedLines()
        {
            _wrappedLines.Clear();
            
            if (_currentText.Length == 0)
            {
                _wrappedLines.Add("");
                return;
            }

            var text = _currentText.ToString();
            var lines = text.Split('\n');
            
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    _wrappedLines.Add("");
                }
                else
                {
                    // Wrap long lines
                    var remaining = line;
                    while (remaining.Length > 0)
                    {
                        var chunkLength = Math.Min(remaining.Length, _maxWidth - 2); // -2 for margins
                        _wrappedLines.Add(remaining.Substring(0, chunkLength));
                        remaining = remaining.Substring(chunkLength);
                    }
                }
            }
        }

        private void UpdateCursorIndices()
        {
            // Calculate which wrapped line and column the cursor is on
            int charCount = 0;
            _currentLineIndex = 0;
            _currentColumnIndex = 0;

            foreach (var line in _wrappedLines)
            {
                if (charCount + line.Length >= _cursorPosition)
                {
                    _currentColumnIndex = _cursorPosition - charCount;
                    return;
                }
                
                charCount += line.Length;
                
                // Account for newline characters in original text
                if (charCount < _currentText.Length && _currentText[charCount] == '\n')
                {
                    charCount++; // Skip the newline
                }
                
                _currentLineIndex++;
            }

            // Cursor is at the end
            if (_wrappedLines.Count > 0)
            {
                _currentLineIndex = _wrappedLines.Count - 1;
                _currentColumnIndex = _wrappedLines[_currentLineIndex].Length;
            }
        }
    }
}