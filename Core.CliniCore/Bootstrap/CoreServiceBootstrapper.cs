using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Repositories;
using Core.CliniCore.Repositories.InMemory;
using Core.CliniCore.Scheduling.Management;
using Core.CliniCore.Service;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Core.CliniCore.Bootstrap
{
    /// <summary>
    /// Provides dependency injection registration for all core CliniCore services.
    /// This bootstrapper ensures consistent service configuration across different client applications (CLI, GUI, etc.)
    /// </summary>
    public static class CoreServiceBootstrapper
    {
        /// <summary>
        /// Registers all core CliniCore services with the dependency injection container.
        /// IMPORTANT: You must also register repositories separately using either:
        /// - AddInMemoryRepositories() for standalone/testing
        /// - AddRemoteRepositories(apiBaseUrl) for API-backed persistence
        /// Call this method from your client application's startup/configuration.
        /// </summary>
        /// <param name="services">The service collection to register services with</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddCliniCoreServices(this IServiceCollection services)
        {
            // NOTE: Repository registration (including ICredentialRepository) is handled separately via:
            // - services.AddInMemoryRepositories() for standalone/testing
            // - services.AddRemoteRepositories(url) for API-backed persistence
            // Repositories MUST be registered BEFORE calling this method.

            // Core Authentication Service (depends on repositories)
            services.AddSingleton<IAuthenticationService, BasicAuthenticationService>();
            services.AddSingleton<RoleBasedAuthorizationService>();

            // Service Layer (uses repositories via DI)
            services.AddSingleton<ProfileService>();
            services.AddSingleton<SchedulerService>();
            services.AddSingleton<ClinicalDocumentService>();

            // Scheduling helpers
            services.AddSingleton<ScheduleConflictDetector>();

            // Command Infrastructure
            services.AddSingleton<CommandInvoker>();

            // CommandFactory needs special handling due to its dependencies
            services.AddSingleton<CommandFactory>(serviceProvider =>
                new CommandFactory(
                    serviceProvider.GetRequiredService<IAuthenticationService>(),
                    serviceProvider.GetRequiredService<SchedulerService>(),
                    serviceProvider.GetRequiredService<ProfileService>(),
                    serviceProvider.GetRequiredService<ClinicalDocumentService>()
                ));

            return services;
        }

        /// <summary>
        /// Registers all core CliniCore services WITH InMemory repositories.
        /// Use this for backwards compatibility or standalone operation.
        /// </summary>
        public static IServiceCollection AddCliniCoreServicesWithInMemory(this IServiceCollection services)
        {
            services.AddInMemoryRepositories();
            services.AddCliniCoreServices();
            return services;
        }

        /// <summary>
        /// Registers core services with custom implementations.
        /// Useful for testing or when you need to override default implementations.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="customAuthService">Custom authentication service implementation</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddCliniCoreServicesWithCustomAuth(
            this IServiceCollection services,
            IAuthenticationService customAuthService)
        {
            // Register the custom auth service
            services.AddSingleton<IAuthenticationService>(customAuthService);
            services.AddSingleton<RoleBasedAuthorizationService>();

            // Repository Layer (In-Memory implementations)
            services.AddSingleton<IPatientRepository, InMemoryPatientRepository>();
            services.AddSingleton<IPhysicianRepository, InMemoryPhysicianRepository>();
            services.AddSingleton<IAdministratorRepository, InMemoryAdministratorRepository>();
            services.AddSingleton<IAppointmentRepository, InMemoryAppointmentRepository>();
            services.AddSingleton<IClinicalDocumentRepository, InMemoryClinicalDocumentRepository>();
            services.AddSingleton<ICredentialRepository, InMemoryCredentialRepository>();

            // Service Layer
            services.AddSingleton<ProfileService>();
            services.AddSingleton<SchedulerService>();
            services.AddSingleton<ClinicalDocumentService>();

            services.AddSingleton<ScheduleConflictDetector>();
            services.AddSingleton<CommandInvoker>();

            services.AddSingleton<CommandFactory>(serviceProvider =>
                new CommandFactory(
                    serviceProvider.GetRequiredService<IAuthenticationService>(),
                    serviceProvider.GetRequiredService<SchedulerService>(),
                    serviceProvider.GetRequiredService<ProfileService>(),
                    serviceProvider.GetRequiredService<ClinicalDocumentService>()
                ));

            return services;
        }

        #region API Server Management

        private static Process? _apiProcess;
        private static readonly object _apiLock = new();

        /// <summary>
        /// Ensures the API server is available. If localProvider is true and the API
        /// is not running, attempts to start it as an independent background process.
        /// </summary>
        /// <param name="apiBaseUrl">The base URL of the API (e.g., "http://localhost:5000")</param>
        /// <param name="localProvider">If true, will attempt to start API locally if not running</param>
        /// <param name="timeoutSeconds">How long to wait for API to become healthy</param>
        /// <returns>True if API is available, false otherwise</returns>
        public static async Task<bool> EnsureApiAvailableAsync(string apiBaseUrl, bool localProvider, int timeoutSeconds = 30)
        {
            // Check if API is already running
            if (await IsApiHealthyAsync(apiBaseUrl).ConfigureAwait(false))
            {
                Console.WriteLine($"API server available at {apiBaseUrl}");
                return true;
            }

            // If not a local provider, we can't start it
            if (!localProvider)
            {
                Console.WriteLine($"Remote API at {apiBaseUrl} is not available");
                return false;
            }

            // Try to start the local API server
            if (!TryStartLocalApiServer(apiBaseUrl))
            {
                return false;
            }

            // Wait for API to become healthy
            return await WaitForApiHealthyAsync(apiBaseUrl, timeoutSeconds).ConfigureAwait(false);
        }

        /// <summary>
        /// Checks if the API is responding to health check requests.
        /// Uses dedicated /health endpoint - lightweight, no database queries.
        /// Catches expected network exceptions by type to avoid debugger breaks.
        /// </summary>
        public static async Task<bool> IsApiHealthyAsync(string apiBaseUrl)
        {
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
                var response = await client.GetAsync($"{apiBaseUrl.TrimEnd('/')}/health").ConfigureAwait(false);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                // Expected when server is not running - connection refused, host unreachable, etc.
                return false;
            }
            catch (TaskCanceledException)
            {
                // Expected on timeout
                return false;
            }
            catch (OperationCanceledException)
            {
                // Expected on cancellation/timeout
                return false;
            }
        }

        /// <summary>
        /// Attempts to start the API server as an independent background process.
        /// </summary>
        private static bool TryStartLocalApiServer(string apiBaseUrl)
        {
            lock (_apiLock)
            {
                if (_apiProcess != null && !_apiProcess.HasExited)
                {
                    Console.WriteLine("API server process already started");
                    return true;
                }

                var apiProjectPath = FindApiProjectPath();
                if (apiProjectPath == null)
                {
                    Console.WriteLine("Could not locate API.CliniCore project");
                    return false;
                }

                try
                {
                    Console.WriteLine($"Starting API server from {apiProjectPath}...");

                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"run --project \"{apiProjectPath}\" --urls \"{apiBaseUrl}\""
                    };

                    // Platform-specific window behavior
                    if (OperatingSystem.IsWindows())
                    {
                        // Windows: open visible console window (close window = kill process)
                        startInfo.UseShellExecute = true;
                        startInfo.CreateNoWindow = false;
                    }
                    else
                    {
                        // Linux/macOS: run headless (no display required)
                        startInfo.UseShellExecute = false;
                        startInfo.CreateNoWindow = true;
                        startInfo.RedirectStandardOutput = true;
                        startInfo.RedirectStandardError = true;
                    }

                    _apiProcess = Process.Start(startInfo);
                    if (_apiProcess == null)
                    {
                        Console.WriteLine("Failed to start API server process");
                        return false;
                    }

                    Console.WriteLine($"API server process started (PID: {_apiProcess.Id})");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error starting API server: {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// Waits for the API to become healthy within the specified timeout.
        /// </summary>
        private static async Task<bool> WaitForApiHealthyAsync(string apiBaseUrl, int timeoutSeconds)
        {
            Console.WriteLine($"Waiting for API to become healthy (timeout: {timeoutSeconds}s)...");
            var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);

            while (DateTime.UtcNow < deadline)
            {
                if (await IsApiHealthyAsync(apiBaseUrl).ConfigureAwait(false))
                {
                    Console.WriteLine("API server is healthy and ready");
                    return true;
                }
                await Task.Delay(500).ConfigureAwait(false);
            }

            Console.WriteLine("Timeout waiting for API server");
            return false;
        }

        /// <summary>
        /// Locates the API.CliniCore project by searching up the directory tree.
        /// </summary>
        private static string? FindApiProjectPath()
        {
            // Start from executable location and walk up
            var searchDir = new DirectoryInfo(AppContext.BaseDirectory);

            while (searchDir != null)
            {
                var apiPath = Path.Combine(searchDir.FullName, "API.CliniCore", "API.CliniCore.csproj");
                if (File.Exists(apiPath))
                    return apiPath;

                searchDir = searchDir.Parent;
            }

            // Fallback: try relative to working directory
            var fallback = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "API.CliniCore", "API.CliniCore.csproj"));
            return File.Exists(fallback) ? fallback : null;
        }

        #endregion
    }
}
