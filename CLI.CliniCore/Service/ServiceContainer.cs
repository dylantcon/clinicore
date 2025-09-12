using System;
using CLI.CliniCore.Service;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Scheduling.Management;

namespace CLI.CliniCore.Service
{
    /// <summary>
    /// Simple dependency injection container to resolve circular dependencies
    /// and provide clean service lifecycle management.
    /// </summary>
    public sealed class ServiceContainer : IDisposable
    {
        private readonly IAuthenticationService _authService;
        private readonly ScheduleManager _scheduleManager;
        private readonly CommandFactory _commandFactory;
        private readonly CommandInvoker _commandInvoker;
        private readonly ConsoleSessionManager _sessionManager;
        private readonly ConsoleCommandParser _commandParser;
        private readonly ConsoleMenuBuilder _menuBuilder;
        private readonly TTYConsoleEngine _consoleEngine;
        private bool _disposed = false;

        private ServiceContainer(
            IAuthenticationService authService,
            ScheduleManager scheduleManager,
            CommandFactory commandFactory,
            CommandInvoker commandInvoker,
            ConsoleSessionManager sessionManager,
            ConsoleCommandParser commandParser,
            ConsoleMenuBuilder menuBuilder,
            TTYConsoleEngine consoleEngine)
        {
            _authService = authService;
            _scheduleManager = scheduleManager;
            _commandFactory = commandFactory;
            _commandInvoker = commandInvoker;
            _sessionManager = sessionManager;
            _commandParser = commandParser;
            _menuBuilder = menuBuilder;
            _consoleEngine = consoleEngine;
        }

        public static ServiceContainer Create()
        {
            // Initialize core services
            var authService = new BasicAuthenticationService();
            var scheduleManager = ScheduleManager.Instance;
            var commandFactory = new CommandFactory(authService, scheduleManager);
            var commandInvoker = new CommandInvoker();
            var sessionManager = new ConsoleSessionManager();

            // Create console engine with placeholder dependencies
            var consoleEngine = new TTYConsoleEngine(
                null, // Will be resolved below
                commandInvoker,
                sessionManager,
                null  // Will be resolved below
            );

            // Create command parser with console engine
            var commandParser = new ConsoleCommandParser(consoleEngine);

            // Create menu builder with all dependencies
            var menuBuilder = new ConsoleMenuBuilder(
                commandInvoker,
                commandFactory,
                sessionManager,
                commandParser,
                consoleEngine
            );

            // Resolve circular dependencies
            consoleEngine.SetMenuBuilder(menuBuilder);
            consoleEngine.SetCommandParser(commandParser);

            return new ServiceContainer(
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
                _disposed = true;
            }
        }
    }
}