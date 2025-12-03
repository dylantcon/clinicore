using System;
using CLI.CliniCore.Service;
using Core.CliniCore.Bootstrap;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Service;
using Core.CliniCore.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CLI.CliniCore.Service
{
    /// <summary>
    /// Dependency injection container that leverages Microsoft.Extensions.DependencyInjection
    /// and the CoreServiceBootstrapper for consistent service configuration.
    /// </summary>
    public sealed class ServiceContainer : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IAuthenticationService _authService;
        private readonly SchedulerService _scheduleManager;
        private readonly CommandFactory _commandFactory;
        private readonly CommandInvoker _commandInvoker;
        private readonly ConsoleSessionManager _sessionManager;
        private readonly ConsoleCommandParser _commandParser;
        private readonly ConsoleMenuBuilder _menuBuilder;
        private readonly TTYConsoleEngine _consoleEngine;
        private bool _disposed = false;

        private ServiceContainer(
            ServiceProvider serviceProvider,
            IAuthenticationService authService,
            SchedulerService scheduleManager,
            CommandFactory commandFactory,
            CommandInvoker commandInvoker,
            ConsoleSessionManager sessionManager,
            ConsoleCommandParser commandParser,
            ConsoleMenuBuilder menuBuilder,
            TTYConsoleEngine consoleEngine)
        {
            _serviceProvider = serviceProvider;
            _authService = authService;
            _scheduleManager = scheduleManager;
            _commandFactory = commandFactory;
            _commandInvoker = commandInvoker;
            _sessionManager = sessionManager;
            _commandParser = commandParser;
            _menuBuilder = menuBuilder;
            _consoleEngine = consoleEngine;
        }

        public static ServiceContainer Create(bool includeDevelopmentData = false)
        {
            // Configure services using the bootstrapper
            var services = new ServiceCollection();

            // Add core CliniCore services from the bootstrapper
            services.AddCliniCoreServices();

            // Add CLI-specific services
            services.AddSingleton<ConsoleSessionManager>();

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Initialize development data if requested (after building provider)
            if (includeDevelopmentData)
            {
                CoreServiceBootstrapper.InitializeDevelopmentData(serviceProvider, createSampleData: true);
            }

            // Get core services from DI
            var authService = serviceProvider.GetRequiredService<IAuthenticationService>();
            var scheduleManager = serviceProvider.GetRequiredService<SchedulerService>();
            var profileService = serviceProvider.GetRequiredService<ProfileService>();
            var clinicalDocService = serviceProvider.GetRequiredService<ClinicalDocumentService>();
            var commandFactory = serviceProvider.GetRequiredService<CommandFactory>();
            var commandInvoker = serviceProvider.GetRequiredService<CommandInvoker>();
            var sessionManager = serviceProvider.GetRequiredService<ConsoleSessionManager>();

            // Create console engine with placeholder dependencies
            var consoleEngine = new TTYConsoleEngine(
                null, // Will be resolved below
                commandInvoker,
                sessionManager,
                null  // Will be resolved below
            );

            // Create command parser with console engine, profile service, scheduler service, and clinical doc service
            var commandParser = new ConsoleCommandParser(consoleEngine, profileService, scheduleManager, clinicalDocService);

            // Create menu builder with all dependencies
            var menuBuilder = new ConsoleMenuBuilder(
                commandInvoker,
                commandFactory,
                sessionManager,
                commandParser,
                consoleEngine,
                clinicalDocService
            );

            // Resolve circular dependencies
            consoleEngine.SetMenuBuilder(menuBuilder);
            consoleEngine.SetCommandParser(commandParser);

            return new ServiceContainer(
                serviceProvider,
                authService,
                scheduleManager,
                commandFactory,
                commandInvoker,
                sessionManager,
                commandParser,
                menuBuilder,
                consoleEngine
            );
        }

        public TTYConsoleEngine GetConsoleEngine() => _consoleEngine;
        
        public IAuthenticationService GetAuthenticationService() => _authService;
        
        public void Dispose()
        {
            if (!_disposed)
            {
                _consoleEngine?.Dispose();
                _serviceProvider?.Dispose();
                _disposed = true;
            }
        }
    }
}