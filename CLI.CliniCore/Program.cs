using CLI.CliniCore.Service;

namespace CLI.CliniCore
{
    public class Program
    {
        static void Main(string[] args)
        {
            ServiceContainer? container = null;
            try
            {
                // Create the service container with development data in DEBUG mode
                #if DEBUG
                container = ServiceContainer.Create(includeDevelopmentData: true);
                #else
                container = ServiceContainer.Create(includeDevelopmentData: false);
                #endif

                // Start the application
                var consoleEngine = container.GetConsoleEngine();
                consoleEngine.Start();
            }
            catch (Exception ex)
            {
                var console = ThreadSafeConsoleManager.Instance;
                console.SetForegroundColor(ConsoleColor.Red);
                console.WriteLine($"Fatal error: {ex.Message}");
                console.WriteLine($"Stack trace: {ex.StackTrace}");
                console.ResetColor();
                console.WriteLine("Press any key to exit...");
                console.ReadKey();
                Environment.Exit(1);
            }
            finally
            {
                // Ensure proper cleanup of resources
                container?.Dispose();
            }
        }
    }
}