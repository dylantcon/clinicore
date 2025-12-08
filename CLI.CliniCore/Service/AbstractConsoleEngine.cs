using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Core.CliniCore.Bootstrap;
using Core.CliniCore.Commands;

namespace CLI.CliniCore.Service
{
    public abstract class AbstractConsoleEngine : IConsoleEngine, IDisposable
    {
        protected readonly Stack<string> _breadcrumbs = new();
        protected readonly ConsoleSessionManager _sessionManager;
        protected ConsoleMenuBuilder _menuBuilder;
        protected readonly CommandInvoker _commandInvoker;
        protected readonly ThreadSafeConsoleManager _console;
        protected readonly CancellationTokenSource _cancellationTokenSource;
        protected bool _isRunning;
        private bool _disposed = false;

        protected AbstractConsoleEngine(
            ConsoleSessionManager sessionManager,
            ConsoleMenuBuilder? menuBuilder,
            CommandInvoker commandInvoker)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _menuBuilder = menuBuilder!; // Will be set later if null during bootstrap
            _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
            _console = ThreadSafeConsoleManager.Instance;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void SetMenuBuilder(ConsoleMenuBuilder menuBuilder)
        {
            _menuBuilder = menuBuilder ?? throw new ArgumentNullException(nameof(menuBuilder));
        }

        public virtual void Start()
        {
            _isRunning = true;
            _console.SetMode(ThreadSafeConsoleManager.ConsoleMode.Menu);
            Clear();
            DisplayWelcome();
            
            while (_isRunning && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var menu = _menuBuilder.BuildMainMenu();
                    DisplayMenu(menu);
                }
                catch (OperationCanceledException)
                {
                    // Clean shutdown requested
                    break;
                }
                catch (Exception ex)
                {
                    DisplayMessage($"An error occurred: {ex.Message}", MessageType.Error);
                    Pause();
                }
            }
        }

        public virtual void Stop()
        {
            _isRunning = false;
            _cancellationTokenSource.Cancel();
            DisplayMessage("Shutting down CliniCore...", MessageType.Info);
        }

        public virtual void DisplayMenu(ConsoleMenu menu)
        {
            Clear();
            DisplayHeader(menu.Title);
            
            if (!string.IsNullOrEmpty(menu.Subtitle))
            {
                DisplayMessage(menu.Subtitle, MessageType.Info);
                DisplaySeparator();
            }

            if (_breadcrumbs.Any())
            {
                DisplayMessage($"Navigation: {GetBreadcrumb()}", MessageType.Debug);
                DisplaySeparator();
            }

            foreach (var item in menu.Items.Where(i => i.IsVisible))
            {
                var color = item.IsEnabled ? item.Color ?? ConsoleColor.White : ConsoleColor.DarkGray;
                SetColor(color);
                
                var label = $"[{item.Key}] {item.Label}";
                if (!string.IsNullOrEmpty(item.Description))
                {
                    label += $" - {item.Description}";
                }
                
                _console.WriteLine(label);
                ResetColor();
            }

            if (menu.ShowBackOption && _breadcrumbs.Any())
            {
                DisplayMessage("[B] Back - Return to previous menu", MessageType.Info);
            }

            if (!string.IsNullOrEmpty(menu.HelpText))
            {
                DisplaySeparator();
                DisplayMessage(menu.HelpText, MessageType.Debug);
            }

            DisplaySeparator();
            var choice = GetUserInput(menu.Prompt)?.ToUpperInvariant();
            
            if (string.IsNullOrEmpty(choice))
            {
                return;
            }

            if (choice == "B" && menu.ShowBackOption && _breadcrumbs.Any())
            {
                PopBreadcrumb();
                return;
            }

            var selectedItem = menu.Items.FirstOrDefault(i => 
                i.Key.Equals(choice, StringComparison.OrdinalIgnoreCase) && 
                i.IsVisible && 
                i.IsEnabled);

            if (selectedItem == null)
            {
                DisplayMessage("Invalid selection. Please try again.", MessageType.Warning);
                Pause();
                return;
            }

            if (selectedItem.Key.Equals("X", StringComparison.OrdinalIgnoreCase))
            {
                if (Confirm("Are you sure you want to exit?"))
                {
                    Stop();
                }
                return;
            }

            if (selectedItem.SubMenuFactory != null)
            {
                PushBreadcrumb(selectedItem.Label);
                var subMenu = selectedItem.SubMenuFactory();
                DisplayMenu(subMenu);
                PopBreadcrumb();
            }
            else if (selectedItem.Action != null)
            {
                try
                {
                    selectedItem.Action();
                }
                catch (Exception ex)
                {
                    DisplayMessage($"Error executing action: {ex.Message}", MessageType.Error);
                    Pause();
                }
            }
        }

        public abstract string? GetUserInput(string prompt);
        public abstract string? GetSecureInput(string prompt);

        public virtual void DisplayMessage(string message, MessageType type = MessageType.Info)
        {
            var (foreground, background) = GetMessageColors(type);
            SetColor(foreground, background);
            _console.WriteLine(message);
            ResetColor();
        }

        public virtual void Clear()
        {
            _console.Clear();
        }

        public virtual void DisplayTable<T>(IEnumerable<T> items, params (string Header, Func<T, string> ValueGetter)[] columns)
        {
            var itemList = items.ToList();
            if (!itemList.Any())
            {
                DisplayMessage("No items to display.", MessageType.Info);
                return;
            }

            var columnWidths = new int[columns.Length];
            
            for (int i = 0; i < columns.Length; i++)
            {
                columnWidths[i] = columns[i].Header.Length;
                foreach (var item in itemList)
                {
                    var value = columns[i].ValueGetter(item) ?? string.Empty;
                    columnWidths[i] = Math.Max(columnWidths[i], value.Length);
                }
            }

            var separator = new StringBuilder("+");
            var headerRow = new StringBuilder("|");
            
            for (int i = 0; i < columns.Length; i++)
            {
                separator.Append(new string('-', columnWidths[i] + 2));
                separator.Append('+');
                headerRow.Append(' ');
                headerRow.Append(columns[i].Header.PadRight(columnWidths[i]));
                headerRow.Append(" |");
            }

            _console.WriteLine(separator.ToString());
            _console.WriteLine(headerRow.ToString());
            _console.WriteLine(separator.ToString());

            foreach (var item in itemList)
            {
                var row = new StringBuilder("|");
                for (int i = 0; i < columns.Length; i++)
                {
                    row.Append(' ');
                    var value = columns[i].ValueGetter(item) ?? string.Empty;
                    row.Append(value.PadRight(columnWidths[i]));
                    row.Append(" |");
                }
                _console.WriteLine(row.ToString());
            }
            
            _console.WriteLine(separator.ToString());
        }

        public virtual bool Confirm(string prompt, bool defaultValue = false)
        {
            var defaultText = defaultValue ? "[Y/n]" : "[y/N]";
            var input = GetUserInput($"{prompt} {defaultText}: ")?.ToLowerInvariant();
            
            if (string.IsNullOrEmpty(input))
            {
                return defaultValue;
            }
            
            return input.StartsWith("y");
        }

        public virtual void DisplayHeader(string text)
        {
            var border = new string('=', text.Length + 4);
            SetColor(ConsoleColor.Cyan);
            _console.WriteLine(border);
            _console.WriteLine($"| {text} |");
            _console.WriteLine(border);
            ResetColor();
        }

        public virtual void DisplaySeparator()
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
                // Fallback separator
                SetColor(ConsoleColor.DarkGray);
                _console.WriteLine(new string('-', 79));
                ResetColor();
            }
        }

        public virtual void Pause(string message = "Press any key to continue...")
        {
            DisplayMessage(message, MessageType.Debug);
            _console.ReadKey(true);
        }

        public virtual void SetColor(ConsoleColor foreground, ConsoleColor? background = null)
        {
            _console.SetForegroundColor(foreground);
            if (background.HasValue)
            {
                _console.SetBackgroundColor(background.Value);
            }
        }

        public virtual void ResetColor()
        {
            _console.ResetColor();
        }

        public virtual string GetBreadcrumb()
        {
            if (!_breadcrumbs.Any())
                return "Home";
            
            return "Home > " + string.Join(" > ", _breadcrumbs.Reverse());
        }

        public virtual void PushBreadcrumb(string crumb)
        {
            _breadcrumbs.Push(crumb);
        }

        public virtual void PopBreadcrumb()
        {
            if (_breadcrumbs.Any())
            {
                _breadcrumbs.Pop();
            }
        }

        protected virtual void DisplayWelcome()
        {
            SetColor(ConsoleColor.Green);
            _console.WriteLine(@"
╔═════════════════════════════════════════════════════════════════╗
║                                                                 ║
║                ___ _ _       _   ___                            ║
║               / __\ (_)_ __ (_) / __\___  _ __ ___              ║
║              / /  | | | '_ \| |/ /  / _ \| '__/ _ \             ║
║             / /___| | | | | | / /__| (_) | | |  __/             ║
║             \____/|_|_|_| |_|_\____/\___/|_|  \___|             ║
║                                                                 ║
║              EMR & Practice Management System v1.0              ║
║                                                                 ║
╚═════════════════════════════════════════════════════════════════╝
");
            ResetColor();
            DisplayMessage("Welcome to CliniCore - Your comprehensive medical practice solution", MessageType.Success);

            if (DevelopmentData.IsDebugMode)
            {
                DisplaySeparator();
                DisplayMessage("Development Credentials:", MessageType.Info);
                SetColor(ConsoleColor.Cyan);
                _console.WriteLine($"  Admin:     {SampleCredentials.AdminUsername} / {SampleCredentials.AdminPassword}");
                _console.WriteLine($"  Physician: {SampleCredentials.PhysicianUsername} / {SampleCredentials.PhysicianPassword}");
                _console.WriteLine($"  Patient:   {SampleCredentials.Patient1Username} / {SampleCredentials.Patient1Password}");
                ResetColor();
            }

            DisplaySeparator();
            Pause("\nPress any key to begin . . .");
        }

        protected virtual (ConsoleColor foreground, ConsoleColor? background) GetMessageColors(MessageType type)
        {
            return type switch
            {
                MessageType.Success => (ConsoleColor.Green, null),
                MessageType.Warning => (ConsoleColor.Yellow, null),
                MessageType.Error => (ConsoleColor.Red, null),
                MessageType.Debug => (ConsoleColor.DarkGray, null),
                _ => (ConsoleColor.White, null)
            };
        }

        public virtual void Dispose()
        {
            if (!_disposed)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _disposed = true;
            }
        }
    }
}