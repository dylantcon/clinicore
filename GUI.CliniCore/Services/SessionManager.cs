using Core.CliniCore.Domain.Authentication.Representation;

namespace GUI.CliniCore.Services
{
    /// <summary>
    /// Manages the current user session across the GUI application
    /// Provides access to SessionContext for authentication and authorization
    /// </summary>
    public class SessionManager
    {
        private SessionContext? _currentSession;

        /// <summary>
        /// Event raised when the session changes (login, logout, session expired)
        /// </summary>
        public event EventHandler? SessionChanged;

        /// <summary>
        /// Gets or sets the current user session
        /// </summary>
        public SessionContext? CurrentSession
        {
            get => _currentSession;
            set
            {
                if (_currentSession != value)
                {
                    _currentSession = value;
                    SessionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Indicates whether a user is currently authenticated
        /// </summary>
        public bool IsAuthenticated => CurrentSession != null && !CurrentSession.IsExpired();

        /// <summary>
        /// Gets the current username, or null if not authenticated
        /// </summary>
        public string? CurrentUsername => CurrentSession?.Username;

        /// <summary>
        /// Gets the current user's role, or null if not authenticated
        /// </summary>
        public Core.CliniCore.Domain.Enumerations.UserRole? CurrentUserRole => CurrentSession?.UserRole;

        /// <summary>
        /// Clears the current session (logout)
        /// </summary>
        public void ClearSession()
        {
            CurrentSession = null;
        }
    }
}
