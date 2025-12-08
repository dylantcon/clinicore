using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.Enumerations;

namespace Core.CliniCore.Service
{
    /// <summary>
    /// Service for handling role-based authorization checks
    /// </summary>
    public class RoleBasedAuthorizationService
    {
        private readonly Dictionary<UserRole, HashSet<Permission>> _rolePermissions;

        public RoleBasedAuthorizationService()
        {
            _rolePermissions = [];
            InitializeRolePermissions();
        }

        /// <summary>
        /// Initializes the permission mappings for each role
        /// </summary>
        private void InitializeRolePermissions()
        {
            // Patient permissions
            _rolePermissions[UserRole.Patient] =
            [
                Permission.ViewOwnProfile,
                Permission.EditOwnProfile,
                Permission.ViewOwnAppointments,
                Permission.ScheduleOwnAppointment,
                Permission.ViewOwnClinicalDocuments
            ];

            // Physician permissions (includes all patient permissions plus more)
            _rolePermissions[UserRole.Physician] =
            [
                // Own permissions (like a patient)
                Permission.ViewOwnProfile,
                Permission.EditOwnProfile,
                Permission.ViewOwnAppointments,
                Permission.ViewOwnClinicalDocuments,

                // Physician-specific permissions
                Permission.ViewAllPatients,
                Permission.CreatePatientProfile,
                Permission.ViewPatientProfile,
                Permission.CreateClinicalDocument,
                Permission.UpdateClinicalDocument,
                Permission.ViewAllAppointments,
                Permission.ScheduleAnyAppointment,
                Permission.EditOwnAvailability
            ];

            // Administrator permissions (all permissions)
            _rolePermissions[UserRole.Administrator] = [.. Enum.GetValues<Permission>()];
        }

        /// <summary>
        /// Checks if a session has a specific permission
        /// </summary>
        public bool Authorize(SessionContext? session, Permission permission)
        {
            if (session == null)
                return false;

            if (session.IsExpired())
                return false;

            return HasPermission(session.UserRole, permission);
        }

        /// <summary>
        /// Checks if a session has all of the specified permissions
        /// </summary>
        public bool AuthorizeAll(SessionContext? session, params Permission[] permissions)
        {
            if (session == null || permissions == null || permissions.Length == 0)
                return false;

            return permissions.All(p => Authorize(session, p));
        }

        /// <summary>
        /// Checks if a session has any of the specified permissions
        /// </summary>
        public bool AuthorizeAny(SessionContext? session, params Permission[] permissions)
        {
            if (session == null || permissions == null || permissions.Length == 0)
                return false;

            return permissions.Any(p => Authorize(session, p));
        }

        /// <summary>
        /// Checks if a role has a specific permission
        /// </summary>
        public bool HasPermission(UserRole role, Permission permission)
        {
            if (_rolePermissions.TryGetValue(role, out var permissions))
            {
                return permissions.Contains(permission);
            }
            return false;
        }

        /// <summary>
        /// Gets all permissions for a specific role
        /// </summary>
        public IEnumerable<Permission> GetRolePermissions(UserRole role)
        {
            if (_rolePermissions.TryGetValue(role, out var permissions))
            {
                return [.. permissions];
            }
            return [];
        }

        /// <summary>
        /// Checks if a user can access another user's data
        /// </summary>
        public static bool CanAccessUserData(SessionContext? session, Guid targetUserId)
        {
            if (session == null)
                return false;

            // Users can always access their own data
            if (session.UserId == targetUserId)
                return true;

            // Physicians can access patient data
            if (session.UserRole == UserRole.Physician)
                return true;

            // Administrators can access all data
            if (session.UserRole == UserRole.Administrator)
                return true;

            return false;
        }

        /// <summary>
        /// Checks if a user can modify another user's data
        /// </summary>
        public bool CanModifyUserData(SessionContext? session, Guid targetUserId)
        {
            if (session == null)
                return false;

            // Users can modify their own data (if they have edit permission)
            if (session.UserId == targetUserId)
                return Authorize(session, Permission.EditOwnProfile);

            // Physicians can modify patient clinical data but not profile data
            // This would need more granular checks in production

            // Administrators can modify all data
            if (session.UserRole == UserRole.Administrator)
                return true;

            return false;
        }

        /// <summary>
        /// Validates that a session is valid and not expired
        /// </summary>
        public static bool ValidateSession(SessionContext? session)
        {
            if (session == null)
                return false;

            if (session.IsExpired())
                return false;

            // Could add more validation here (IP checks, device fingerprinting, etc.)
            return true;
        }

        /// <summary>
        /// Creates an authorization exception with context
        /// </summary>
        public static UnauthorizedAccessException CreateAuthorizationException(
            SessionContext? session,
            Permission requiredPermission,
            string? additionalMessage = null)
        {
            if (session == null)
            {
                return new UnauthorizedAccessException(
                    "Authentication required. Please log in to continue.");
            }

            var message = $"User '{session.Username}' with role '{session.UserRole}' " +
                         $"does not have permission '{requiredPermission}'.";

            if (!string.IsNullOrWhiteSpace(additionalMessage))
            {
                message += $" {additionalMessage}";
            }

            return new UnauthorizedAccessException(message);
        }
    }
}