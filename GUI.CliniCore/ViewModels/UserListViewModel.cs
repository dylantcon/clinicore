using System.Collections.ObjectModel;
using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Query;
using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Enumerations;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels
{
    /// <summary>
    /// ViewModel for User List page
    /// Displays all users (administrators, physicians, patients) with role-based navigation
    /// </summary>
    public partial class UserListViewModel : BaseViewModel
    {
        private readonly CommandFactory _commandFactory;
        private readonly INavigationService _navigationService;
        private readonly SessionManager _sessionManager;

        private ObservableCollection<UserDisplayModel> _users = [];
        public ObservableCollection<UserDisplayModel> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    // Trigger search when text changes
                    FilterUsers();
                }
            }
        }

        private UserDisplayModel? _selectedUser;
        public UserDisplayModel? SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (SetProperty(ref _selectedUser, value) && value != null)
                {
                    // Navigate to detail based on role
                    ViewUserCommand.Execute(value);
                }
            }
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        // Store all users for filtering
        private List<UserDisplayModel> _allUsers = [];

        public MauiCommand LoadUsersCommand { get; }
        public MauiCommand ViewUserCommand { get; }
        public MauiCommand CreateAdminCommand { get; }
        public MauiCommand RefreshCommand { get; }
        public MauiCommand BackCommand { get; }

        public UserListViewModel(
            CommandFactory commandFactory,
            INavigationService navigationService,
            SessionManager sessionManager)
        {
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));

            Title = "User Management";

            // Create list command using the Key constant
            var listCoreCommand = _commandFactory.CreateCommand(ListAllUsersCommand.Key);
            LoadUsersCommand = new MauiCommandAdapter(
                listCoreCommand!,
                parameterBuilder: () => new CommandParameters(),
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleListResult,
                viewModel: this
            );

            // Navigate to detail based on role
            ViewUserCommand = new RelayCommand<UserDisplayModel>(
                execute: async (user) => await NavigateToDetailAsync(user!)
            );

            // Navigate to create administrator
            CreateAdminCommand = new AsyncRelayCommand(NavigateToCreateAdminAsync);

            // Refresh command
            RefreshCommand = new RelayCommand(() =>
            {
                IsRefreshing = true;
                LoadUsersCommand.Execute(null);
                IsRefreshing = false;
            });

            // Back command - navigate explicitly to home page
            BackCommand = new AsyncRelayCommand(async () => await _navigationService.NavigateToHomeAsync());

            // Load users on initialization
            LoadUsersCommand.Execute(null);
        }

        private void HandleListResult(CommandResult result)
        {
            if (result.Success && result.Data is IEnumerable<IUserProfile> users)
            {
                _allUsers.Clear();
                foreach (var user in users)
                {
                    _allUsers.Add(new UserDisplayModel(user));
                }

                FilterUsers();
                ClearValidation();
            }
            else
            {
                // Errors are already populated by the adapter
                _allUsers.Clear();
                Users.Clear();
            }
        }

        private void FilterUsers()
        {
            Users.Clear();

            var filteredUsers = _allUsers.AsEnumerable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filteredUsers = filteredUsers.Where(u =>
                    u.Name.ToLower().Contains(searchLower) ||
                    u.Username.ToLower().Contains(searchLower));
            }

            // Add filtered users to observable collection
            foreach (var user in filteredUsers.OrderBy(u => u.Role).ThenBy(u => u.Name))
            {
                Users.Add(user);
            }
        }

        private async Task NavigateToDetailAsync(UserDisplayModel user)
        {
            switch (user.Role)
            {
                case UserRole.Administrator:
                    // For now, navigate to edit page (we'll create detail page later if needed)
                    await _navigationService.NavigateToAsync($"AdministratorEditPage?userId={user.Id}");
                    break;
                case UserRole.Physician:
                    await _navigationService.NavigateToAsync($"PhysicianDetailPage?physicianId={user.Id}");
                    break;
                case UserRole.Patient:
                    await _navigationService.NavigateToAsync($"PatientDetailPage?patientId={user.Id}");
                    break;
            }
        }

        private async Task NavigateToCreateAdminAsync()
        {
            await _navigationService.NavigateToAsync("AdministratorEditPage");
        }
    }

    /// <summary>
    /// Display model wrapper for user profiles to enable easier binding in XAML
    /// </summary>
    public class UserDisplayModel
    {
        private readonly IUserProfile _userProfile;

        public UserDisplayModel(IUserProfile userProfile)
        {
            _userProfile = userProfile ?? throw new ArgumentNullException(nameof(userProfile));
        }

        public Guid Id => _userProfile.Id;
        public string Username => _userProfile.Username;
        public UserRole Role => _userProfile.Role;
        public DateTime CreatedAt => _userProfile.CreatedAt;

        public string Name
        {
            get
            {
                // Get name based on profile type
                return _userProfile switch
                {
                    AdministratorProfile admin => admin.Name,
                    PhysicianProfile physician => $"Dr. {physician.Name}",
                    PatientProfile patient => patient.Name,
                    _ => "Unknown"
                };
            }
        }

        public string RoleDisplayName => Role.ToString();

        public string RoleBadgeColor => Role switch
        {
            UserRole.Administrator => "#F44336", // Red
            UserRole.Physician => "#2196F3",     // Blue
            UserRole.Patient => "#4CAF50",       // Green
            _ => "#757575"                        // Gray
        };

        public string AdditionalInfo
        {
            get
            {
                return _userProfile switch
                {
                    PhysicianProfile physician => $"License: {physician.LicenseNumber}",
                    PatientProfile patient => patient.BirthDate != default
                        ? $"DOB: {patient.BirthDate:yyyy-MM-dd}"
                        : "No DOB recorded",
                    AdministratorProfile admin => admin.Department,
                    _ => ""
                };
            }
        }
    }
}
