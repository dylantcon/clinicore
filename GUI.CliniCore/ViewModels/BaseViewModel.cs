using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using GUI.CliniCore.Services;

namespace GUI.CliniCore.ViewModels
{
    /// <summary>
    /// Base class for all ViewModels providing INotifyPropertyChanged implementation,
    /// common validation/busy state management, and RBAC helper methods
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // Validation error collection (bound to UI)
        private ObservableCollection<string> _validationErrors = new();
        public ObservableCollection<string> ValidationErrors
        {
            get => _validationErrors;
            set => SetProperty(ref _validationErrors, value);
        }

        // Validation warnings collection (bound to UI)
        private ObservableCollection<string> _validationWarnings = new();
        public ObservableCollection<string> ValidationWarnings
        {
            get => _validationWarnings;
            set => SetProperty(ref _validationWarnings, value);
        }

        // Convenience properties for UI binding
        public bool HasValidationErrors => ValidationErrors.Count > 0;
        public bool HasValidationWarnings => ValidationWarnings.Count > 0;

        // Busy state for async operations
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        // Page/view title
        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        /// <summary>
        /// Helper method to check if current user has a specific permission
        /// </summary>
        protected bool HasPermission(SessionManager sessionManager, Permission permission)
        {
            return sessionManager?.CurrentSession?.HasPermission(permission) ?? false;
        }

        /// <summary>
        /// Helper method to check if current user has any of the specified permissions
        /// </summary>
        protected bool HasAnyPermission(SessionManager sessionManager, params Permission[] permissions)
        {
            if (sessionManager?.CurrentSession == null) return false;
            return permissions.Any(p => sessionManager.CurrentSession.HasPermission(p));
        }

        /// <summary>
        /// Helper method to check if current user has all of the specified permissions
        /// </summary>
        protected bool HasAllPermissions(SessionManager sessionManager, params Permission[] permissions)
        {
            if (sessionManager?.CurrentSession == null) return false;
            return permissions.All(p => sessionManager.CurrentSession.HasPermission(p));
        }

        /// <summary>
        /// Gets the current user's role
        /// </summary>
        protected UserRole? GetCurrentRole(SessionManager sessionManager)
        {
            return sessionManager?.CurrentUserRole;
        }

        /// <summary>
        /// Raises PropertyChanged event for the specified property name
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Sets property value and raises PropertyChanged if value changed
        /// </summary>
        /// <returns>True if value changed, false otherwise</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);

            // Raise dependent property notifications for validation collections
            if (propertyName == nameof(ValidationErrors))
            {
                OnPropertyChanged(nameof(HasValidationErrors));
            }
            else if (propertyName == nameof(ValidationWarnings))
            {
                OnPropertyChanged(nameof(HasValidationWarnings));
            }

            return true;
        }

        /// <summary>
        /// Clears all validation messages
        /// </summary>
        public void ClearValidation()
        {
            ValidationErrors.Clear();
            ValidationWarnings.Clear();
            OnPropertyChanged(nameof(HasValidationErrors));
            OnPropertyChanged(nameof(HasValidationWarnings));
        }
    }
}
