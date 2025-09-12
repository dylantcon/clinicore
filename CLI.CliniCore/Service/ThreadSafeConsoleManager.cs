using System;
using System.Threading;

namespace CLI.CliniCore.Service
{
    /// <summary>
    /// Thread-safe singleton for managing console operations.
    /// Prevents race conditions between main UI thread and background threads (like resize listener).
    /// </summary>
    public sealed class ThreadSafeConsoleManager : IDisposable
    {
        private static readonly object _instanceLock = new();
        private static ThreadSafeConsoleManager? _instance;
        
        private readonly ReaderWriterLockSlim _consoleLock = new();
        private readonly object _dimensionCacheLock = new();
        
        // Cached dimensions to minimize lock contention
        private int _cachedWidth = 80;
        private int _cachedHeight = 25;
        private DateTime _lastDimensionUpdate = DateTime.MinValue;
        private readonly TimeSpan _cacheTimeout = TimeSpan.FromMilliseconds(50);
        
        // Console mode management
        private ConsoleMode _currentMode = ConsoleMode.Menu;
        private bool _disposed = false;

        private ThreadSafeConsoleManager()
        {
            RefreshDimensions();
        }

        public static ThreadSafeConsoleManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        _instance ??= new ThreadSafeConsoleManager();
                    }
                }
                return _instance;
            }
        }

        public enum ConsoleMode
        {
            Menu,       // Normal menu navigation
            Editor,     // Text editor has exclusive control
            Input       // Waiting for user input
        }

        public ConsoleMode CurrentMode
        {
            get
            {
                _consoleLock.EnterReadLock();
                try
                {
                    return _currentMode;
                }
                finally
                {
                    _consoleLock.ExitReadLock();
                }
            }
        }

        public void SetMode(ConsoleMode mode)
        {
            _consoleLock.EnterWriteLock();
            try
            {
                _currentMode = mode;
            }
            finally
            {
                _consoleLock.ExitWriteLock();
            }
        }

        public (int Width, int Height) GetDimensions()
        {
            lock (_dimensionCacheLock)
            {
                var now = DateTime.UtcNow;
                if (now - _lastDimensionUpdate > _cacheTimeout)
                {
                    RefreshDimensions();
                    _lastDimensionUpdate = now;
                }
                return (_cachedWidth, _cachedHeight);
            }
        }

        public (int Width, int Height) GetDimensionsForceRefresh()
        {
            lock (_dimensionCacheLock)
            {
                RefreshDimensions();
                _lastDimensionUpdate = DateTime.UtcNow;
                return (_cachedWidth, _cachedHeight);
            }
        }

        private void RefreshDimensions()
        {
            _consoleLock.EnterReadLock();
            try
            {
                try
                {
                    _cachedWidth = Console.WindowWidth;
                    _cachedHeight = Console.WindowHeight;
                }
                catch
                {
                    // Fallback dimensions if console not available
                    _cachedWidth = 80;
                    _cachedHeight = 25;
                }
            }
            finally
            {
                _consoleLock.ExitReadLock();
            }
        }

        public void Clear()
        {
            _consoleLock.EnterWriteLock();
            try
            {
                Console.Clear();
            }
            finally
            {
                _consoleLock.ExitWriteLock();
            }
        }

        public void Write(string text)
        {
            _consoleLock.EnterWriteLock();
            try
            {
                Console.Write(text);
            }
            finally
            {
                _consoleLock.ExitWriteLock();
            }
        }

        public void WriteLine(string text = "")
        {
            _consoleLock.EnterWriteLock();
            try
            {
                Console.WriteLine(text);
            }
            finally
            {
                _consoleLock.ExitWriteLock();
            }
        }

        public ConsoleKeyInfo ReadKey(bool intercept = false)
        {
            _consoleLock.EnterWriteLock();
            try
            {
                return Console.ReadKey(intercept);
            }
            finally
            {
                _consoleLock.ExitWriteLock();
            }
        }

        public string? ReadLine()
        {
            _consoleLock.EnterWriteLock();
            try
            {
                return Console.ReadLine();
            }
            finally
            {
                _consoleLock.ExitWriteLock();
            }
        }

        public void SetForegroundColor(ConsoleColor color)
        {
            _consoleLock.EnterWriteLock();
            try
            {
                Console.ForegroundColor = color;
            }
            finally
            {
                _consoleLock.ExitWriteLock();
            }
        }

        public void SetBackgroundColor(ConsoleColor color)
        {
            _consoleLock.EnterWriteLock();
            try
            {
                Console.BackgroundColor = color;
            }
            finally
            {
                _consoleLock.ExitWriteLock();
            }
        }

        public void ResetColor()
        {
            _consoleLock.EnterWriteLock();
            try
            {
                Console.ResetColor();
            }
            finally
            {
                _consoleLock.ExitWriteLock();
            }
        }

        public void SetCursorPosition(int left, int top)
        {
            _consoleLock.EnterWriteLock();
            try
            {
                // Get current console dimensions directly to avoid stale cache issues
                int width, height;
                try
                {
                    width = Console.WindowWidth;
                    height = Console.WindowHeight;
                }
                catch
                {
                    // Fallback to cached dimensions if console not available
                    width = _cachedWidth;
                    height = _cachedHeight;
                }
                
                left = Math.Max(0, Math.Min(left, width - 1));
                top = Math.Max(0, Math.Min(top, height - 1));
                Console.SetCursorPosition(left, top);
            }
            finally
            {
                _consoleLock.ExitWriteLock();
            }
        }

        public (int Left, int Top) GetCursorPosition()
        {
            _consoleLock.EnterReadLock();
            try
            {
                return (Console.CursorLeft, Console.CursorTop);
            }
            finally
            {
                _consoleLock.ExitReadLock();
            }
        }

        public bool IsInputRedirected
        {
            get
            {
                _consoleLock.EnterReadLock();
                try
                {
                    return Console.IsInputRedirected;
                }
                finally
                {
                    _consoleLock.ExitReadLock();
                }
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _consoleLock?.Dispose();
                _disposed = true;
            }
        }
    }
}