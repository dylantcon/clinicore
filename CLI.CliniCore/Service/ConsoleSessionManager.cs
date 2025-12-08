using System;
using Core.CliniCore.Domain.Authentication.Representation;

namespace CLI.CliniCore.Service
{
    public class ConsoleSessionManager
    {
        private SessionContext? _currentSession;
        private DateTime? _lastActivityTime;
        private readonly TimeSpan _sessionTimeout = TimeSpan.FromMinutes(30);

        public SessionContext? CurrentSession => _currentSession;
        
        public bool IsAuthenticated => _currentSession != null && !IsSessionExpired();
        
        public string CurrentUsername => _currentSession?.Username ?? "Guest";
        
        public string CurrentUserRole => _currentSession?.UserRole.ToString() ?? "None";

        public Guid? CurrentUserId => _currentSession?.UserId;

        public void StartSession(SessionContext session)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            _currentSession = session;
            _lastActivityTime = DateTime.Now;
        }

        public void EndSession()
        {
            _currentSession = null;
            _lastActivityTime = null;
        }

        public void UpdateActivity()
        {
            if (_currentSession != null)
            {
                _lastActivityTime = DateTime.Now;
                _currentSession.UpdateActivity();
            }
        }

        public bool IsSessionExpired()
        {
            if (_currentSession == null || _lastActivityTime == null)
                return true;

            return DateTime.Now - _lastActivityTime.Value > _sessionTimeout;
        }

        public void ValidateSession()
        {
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("No active session. Please login first.");
            }

            if (IsSessionExpired())
            {
                EndSession();
                throw new InvalidOperationException("Session has expired. Please login again.");
            }

            UpdateActivity();
        }

        public bool HasPermission(Core.CliniCore.Domain.Enumerations.Permission permission)
        {
            if (_currentSession == null)
                return false;

            return _currentSession.HasPermission(permission);
        }

        public string GetSessionInfo()
        {
            if (!IsAuthenticated)
                return "Not logged in";

            var duration = DateTime.Now - (_lastActivityTime ?? DateTime.Now);
            return $"User: {CurrentUsername} | Role: {CurrentUserRole} | Session: {duration:hh\\:mm\\:ss}";
        }

        public void RequireAuthentication()
        {
            if (!IsAuthenticated)
            {
                throw new UnauthorizedAccessException("Authentication required. Please login first.");
            }
            ValidateSession();
        }

        public void RequireRole(Core.CliniCore.Domain.Enumerations.UserRole role)
        {
            RequireAuthentication();
            
            if (_currentSession?.UserRole != role)
            {
                throw new UnauthorizedAccessException($"This operation requires {role} role.");
            }
        }

        public void RequirePermission(Core.CliniCore.Domain.Enumerations.Permission permission)
        {
            RequireAuthentication();
            
            if (!HasPermission(permission))
            {
                throw new UnauthorizedAccessException($"You don't have permission to perform this operation: {permission}");
            }
        }
    }
}
