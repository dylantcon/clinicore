using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Users;

namespace Core.CliniCore.Domain.Authentication.Representation
{
    /// <summary>
    /// Maintains context about the current authenticated session
    /// </summary>
    public class SessionContext
    {
        private SessionContext(IUserProfile authenticatedUser)
        {
            AuthenticatedUser = authenticatedUser ?? throw new ArgumentNullException(nameof(authenticatedUser));
            SessionId = Guid.NewGuid();
            LoginTime = DateTime.Now;
            LastActivityTime = DateTime.Now;
        }

        /// <summary>
        /// The currently authenticated user profile
        /// </summary>
        public IUserProfile AuthenticatedUser { get; }

        /// <summary>
        /// Unique identifier for this session
        /// </summary>
        public Guid SessionId { get; }

        /// <summary>
        /// When the user logged in
        /// </summary>
        public DateTime LoginTime { get; }

        /// <summary>
        /// Last time any activity occurred in this session
        /// </summary>
        public DateTime LastActivityTime { get; private set; }

        /// <summary>
        /// Convenience property to get the user's role
        /// </summary>
        public UserRole UserRole => AuthenticatedUser.Role;

        /// <summary>
        /// Convenience property to get the username
        /// </summary>
        public string Username => AuthenticatedUser.Username;

        /// <summary>
        /// Convenience property to get the user's ID
        /// </summary>
        public Guid UserId => AuthenticatedUser.Id;

        /// <summary>
        /// How long the session has been active
        /// </summary>
        public TimeSpan SessionDuration => DateTime.Now - LoginTime;

        /// <summary>
        /// How long since last activity
        /// </summary>
        public TimeSpan IdleTime => DateTime.Now - LastActivityTime;

        /// <summary>
        /// Whether the session has been idle too long (default 30 minutes)
        /// </summary>
        public bool IsExpired(TimeSpan? maxIdleTime = null)
        {
            var timeout = maxIdleTime ?? TimeSpan.FromMinutes(30);
            return IdleTime > timeout;
        }

        /// <summary>
        /// Updates the last activity time to now
        /// </summary>
        public void UpdateActivity()
        {
            LastActivityTime = DateTime.Now;
        }

        /// <summary>
        /// Checks if the authenticated user has a specific permission
        /// </summary>
        public bool HasPermission(Permission permission)
        {
            // Map role-based permissions
            // This is simplified for Assignment 1 - production would use more complex authorization
            return UserRole switch
            {
                UserRole.Patient => IsPatientPermission(permission),
                UserRole.Physician => IsPhysicianPermission(permission),
                UserRole.Administrator => true, // Admins have all permissions
                _ => false
            };
        }

        private bool IsPatientPermission(Permission permission)
        {
            return permission switch
            {
                Permission.ViewOwnProfile => true,
                Permission.EditOwnProfile => true,
                Permission.ViewOwnAppointments => true,
                Permission.ScheduleOwnAppointment => true,
                Permission.ViewOwnClinicalDocuments => true,
                _ => false
            };
        }

        private bool IsPhysicianPermission(Permission permission)
        {
            return permission switch
            {
                // Physician permissions
                Permission.ViewAllPatients => true,
                Permission.CreatePatientProfile => true,
                Permission.ViewPatientProfile => true,
                Permission.CreateClinicalDocument => true,
                Permission.ViewAllAppointments => true,
                Permission.ScheduleAnyAppointment => true,
                Permission.EditOwnAvailability => true,
                // Also has patient permissions for their own data
                Permission.ViewOwnProfile => true,
                Permission.EditOwnProfile => true,
                Permission.ViewOwnAppointments => true,
                _ => false
            };
        }

        /// <summary>
        /// Factory method to create a new session for an authenticated user
        /// </summary>
        public static SessionContext CreateSession(IUserProfile authenticatedUser)
        {
            return new SessionContext(authenticatedUser);
        }

        /// <summary>
        /// Gets a display string for the current session
        /// </summary>
        public override string ToString()
        {
            return $"Session {SessionId:N} - User: {Username} ({UserRole}) - Active: {SessionDuration:hh\\:mm\\:ss}";
        }
    }
}