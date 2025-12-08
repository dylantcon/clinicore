using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Repositories;
using GUI.CliniCore.Services;

namespace GUI.CliniCore.ViewModels.Base
{
    /// <summary>
    /// Base class for all ViewModels providing INotifyPropertyChanged implementation,
    /// common validation/busy state management, and RBAC helper methods
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // Validation error collection (bound to UI)
        private ObservableCollection<string> _validationErrors;
        public ObservableCollection<string> ValidationErrors
        {
            get => _validationErrors;
            set
            {
                if (_validationErrors != value)
                {
                    // Unsubscribe from old collection
                    if (_validationErrors != null)
                        _validationErrors.CollectionChanged -= OnValidationErrorsChanged;

                    _validationErrors = value;

                    // Subscribe to new collection
                    if (_validationErrors != null)
                        _validationErrors.CollectionChanged += OnValidationErrorsChanged;

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasValidationErrors));
                }
            }
        }

        // Validation warnings collection (bound to UI)
        private ObservableCollection<string> _validationWarnings;
        public ObservableCollection<string> ValidationWarnings
        {
            get => _validationWarnings;
            set
            {
                if (_validationWarnings != value)
                {
                    // Unsubscribe from old collection
                    if (_validationWarnings != null)
                        _validationWarnings.CollectionChanged -= OnValidationWarningsChanged;

                    _validationWarnings = value;

                    // Subscribe to new collection
                    if (_validationWarnings != null)
                        _validationWarnings.CollectionChanged += OnValidationWarningsChanged;

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasValidationWarnings));
                }
            }
        }

        // Convenience properties for UI binding
        public bool HasValidationErrors => ValidationErrors?.Count > 0;
        public bool HasValidationWarnings => ValidationWarnings?.Count > 0;

        /// <summary>
        /// Constructor - initializes validation collections with change tracking
        /// </summary>
        protected BaseViewModel()
        {
            _validationErrors = new ObservableCollection<string>();
            _validationErrors.CollectionChanged += OnValidationErrorsChanged;

            _validationWarnings = new ObservableCollection<string>();
            _validationWarnings.CollectionChanged += OnValidationWarningsChanged;
        }

        private void OnValidationErrorsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasValidationErrors));
        }

        private void OnValidationWarningsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasValidationWarnings));
        }

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
        protected static bool HasPermission(SessionManager sessionManager, Permission permission)
        {
            return sessionManager?.CurrentSession?.HasPermission(permission) ?? false;
        }

        /// <summary>
        /// Helper method to check if current user has any of the specified permissions
        /// </summary>
        protected static bool HasAnyPermission(SessionManager sessionManager, params Permission[] permissions)
        {
            if (sessionManager?.CurrentSession == null) return false;
            return permissions.Any(p => sessionManager.CurrentSession.HasPermission(p));
        }

        /// <summary>
        /// Helper method to check if current user has all of the specified permissions
        /// </summary>
        protected static bool HasAllPermissions(SessionManager sessionManager, params Permission[] permissions)
        {
            if (sessionManager?.CurrentSession == null) return false;
            return permissions.All(p => sessionManager.CurrentSession.HasPermission(p));
        }

        /// <summary>
        /// Gets the current user's role
        /// </summary>
        protected static UserRole? GetCurrentRole(SessionManager sessionManager)
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

        /// <summary>
        /// Sets a single validation error, clearing any previous errors
        /// </summary>
        protected void SetValidationError(string message)
        {
            ValidationErrors.Clear();
            ValidationErrors.Add(message);
        }

        /// <summary>
        /// Adds a validation error without clearing previous errors
        /// </summary>
        protected void AddValidationError(string message)
        {
            ValidationErrors.Add(message);
        }

        /// <summary>
        /// Sets a single validation warning, clearing any previous warnings
        /// </summary>
        protected void SetValidationWarning(string message)
        {
            ValidationWarnings.Clear();
            ValidationWarnings.Add(message);
        }

        /// <summary>
        /// Extracts a user-friendly error message from an exception.
        /// Handles RepositoryOperationException specially to show context.
        /// </summary>
        protected static string GetExceptionMessage(Exception ex)
        {
            // Check for RepositoryOperationException (direct or nested)
            if (ex is RepositoryOperationException repoEx)
                return repoEx.ToString();

            if (ex.InnerException is RepositoryOperationException innerRepoEx)
                return innerRepoEx.ToString();

            // Default to message
            return ex.Message;
        }

        /// <summary>
        /// Sets a validation error from an exception, extracting full context from RepositoryOperationException
        /// </summary>
        protected void SetValidationError(string prefix, Exception ex)
        {
            SetValidationError($"{prefix}: {GetExceptionMessage(ex)}");
        }
    }
}
