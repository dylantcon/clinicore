using System;
using System.Text;
using Core.CliniCore.Commands;

namespace CLI.CliniCore.Service
{
    public class TTYConsoleEngine : AbstractConsoleEngine
    {
        private ConsoleCommandParser _commandParser;

        public TTYConsoleEngine(
            ConsoleMenuBuilder? menuBuilder,
            CommandInvoker commandInvoker,
            ConsoleSessionManager sessionManager,
            ConsoleCommandParser? commandParser)
            : base(sessionManager, menuBuilder, commandInvoker)
        {
            _commandParser = commandParser!; // Will be set later if null during bootstrap
        }

        public void SetCommandParser(ConsoleCommandParser commandParser)
        {
            _commandParser = commandParser ?? throw new ArgumentNullException(nameof(commandParser));
        }

        public override string? GetUserInput(string prompt)
        {
            _console.SetMode(ThreadSafeConsoleManager.ConsoleMode.Input);
            _console.Write(prompt);
            
            // Check if input is being redirected (for testing/automation)
            if (_console.IsInputRedirected)
            {
                var result = _console.ReadLine();
                _console.SetMode(ThreadSafeConsoleManager.ConsoleMode.Menu);
                return result;
            }
            
            // Interactive mode with Escape key support
            var input = new StringBuilder();
            ConsoleKeyInfo key;
            
            try
            {
                do
                {
                    key = _console.ReadKey(true);
                    
                    if (key.Key == ConsoleKey.Escape)
                    {
                        // Clear the current line and show cancellation message
                        _console.WriteLine();
                        _console.WriteLine("[Input cancelled by user]");
                        return null; // Return null to indicate cancellation
                    }
                    else if (key.Key == ConsoleKey.Backspace && input.Length > 0)
                    {
                        input.Length--;
                        _console.Write("\b \b");
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        _console.WriteLine();
                        break;
                    }
                    else if (!char.IsControl(key.KeyChar))
                    {
                        input.Append(key.KeyChar);
                        _console.Write(key.KeyChar.ToString());
                    }
                } while (true);
                
                return input.ToString();
            }
            finally
            {
                _console.SetMode(ThreadSafeConsoleManager.ConsoleMode.Menu);
            }
        }

        public override string? GetSecureInput(string prompt)
        {
            _console.SetMode(ThreadSafeConsoleManager.ConsoleMode.Input);
            _console.Write(prompt);
            
            var password = new StringBuilder();
            ConsoleKeyInfo key;
            
            try
            {
                do
                {
                    key = _console.ReadKey(true);
                    
                    if (key.Key == ConsoleKey.Escape)
                    {
                        // Clear the current line and show cancellation message
                        _console.WriteLine();
                        _console.WriteLine("[Input cancelled by user]");
                        return null; // Return null to indicate cancellation
                    }
                    else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                    {
                        password.Length--;
                        _console.Write("\b \b");
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        _console.WriteLine();
                        break;
                    }
                    else if (!char.IsControl(key.KeyChar))
                    {
                        password.Append(key.KeyChar);
                        _console.Write("*");
                    }
                } while (true);
                
                return password.ToString();
            }
            finally
            {
                _console.SetMode(ThreadSafeConsoleManager.ConsoleMode.Menu);
            }
        }

        public override void DisplaySeparator()
        {
            try
            {
                SetColor(ConsoleColor.DarkGray);
                var (width, _) = _console.GetDimensions();
                var separatorWidth = width > 0 ? width - 1 : 79;
                _console.WriteLine(new string('-', separatorWidth));
                ResetColor();
            }
            catch
            {
                // Fallback if dimensions are not available
                SetColor(ConsoleColor.DarkGray);
                _console.WriteLine(new string('-', 79));
                ResetColor();
            }
        }
    }
}