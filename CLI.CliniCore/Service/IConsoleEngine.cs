using System;

namespace CLI.CliniCore.Service
{
    public enum MessageType
    {
        Info,
        Success,
        Warning,
        Error,
        Debug
    }

    public class ConsoleMenu
    {
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public List<ConsoleMenuItem> Items { get; set; } = new();
        public string? HelpText { get; set; }
        public bool ShowBackOption { get; set; } = true;
        public string Prompt { get; set; } = "Enter your choice: ";
    }

    public class ConsoleMenuItem
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Action? Action { get; set; }
        public Func<ConsoleMenu>? SubMenuFactory { get; set; }
        public bool IsVisible { get; set; } = true;
        public bool IsEnabled { get; set; } = true;
        public ConsoleColor? Color { get; set; }
    }

    public interface IConsoleEngine
    {
        void Start();
        void Stop();
        void DisplayMenu(ConsoleMenu menu);
        string? GetUserInput(string prompt);
        string? GetSecureInput(string prompt);
        void DisplayMessage(string message, MessageType type = MessageType.Info);
        void Clear();
        void DisplayTable<T>(IEnumerable<T> items, params (string Header, Func<T, string> ValueGetter)[] columns);
        bool Confirm(string prompt, bool defaultValue = false);
        void DisplayHeader(string text);
        void DisplaySeparator();
        void Pause(string message = "Press any key to continue...");
        void SetColor(ConsoleColor foreground, ConsoleColor? background = null);
        void ResetColor();
        string GetBreadcrumb();
        void PushBreadcrumb(string crumb);
        void PopBreadcrumb();
    }
}
