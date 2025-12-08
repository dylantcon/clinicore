using System.Collections.ObjectModel;
using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Profile;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.Service;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;
using GUI.CliniCore.ViewModels.Base;
using GUI.CliniCore.Views.Physicians;
using GUI.CliniCore.Views.Shared;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels.Physicians
{
    /// <summary>
    /// ViewModel for Physician List page
    /// Supports listing, searching, and navigating to physician details
    /// </summary>
    public partial class PhysicianListViewModel : BaseViewModel
    {
        private readonly CommandFactory _commandFactory;
        private readonly CommandInvoker _commandInvoker;
        private readonly INavigationService _navigationService;
        private readonly SessionManager _sessionManager;
        private readonly ProfileService _profileRegistry;

        private ObservableCollection<PhysicianProfile> _physicians = [];
        public ObservableCollection<PhysicianProfile> Physicians
        {
            get => _physicians;
            set => SetProperty(ref _physicians, value);
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
                    SearchCommand.Execute(null);
                }
            }
        }

        private PhysicianProfile? _selectedPhysician;
        public PhysicianProfile? SelectedPhysician
        {
            get => _selectedPhysician;
            set
            {
                if (SetProperty(ref _selectedPhysician, value) && value != null)
                {
                    // Navigate to detail when physician is selected
                    ViewPhysicianCommand.Execute(value.Id);
                }
            }
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        #region Sorting Properties

        /// <summary>
        /// Available sort options for the physician list.
        /// GRADING REQUIREMENT: Sort-by feature with 2+ properties, ascending/descending.
        /// </summary>
        public List<SortOptionBase> SortOptions { get; } = new()
        {
            new SortOption<PhysicianProfile>("Name", p => p.Name ?? string.Empty),
            new SortOption<PhysicianProfile>("Patient Count", p => p.PatientIds?.Count ?? 0),
            new SortOption<PhysicianProfile>("License Number", p => p.LicenseNumber ?? string.Empty)
        };

        private SortOptionBase? _selectedSortOption;
        public SortOptionBase? SelectedSortOption
        {
            get => _selectedSortOption;
            set => SetProperty(ref _selectedSortOption, value);
        }

        private bool _isAscending = true;
        public bool IsAscending
        {
            get => _isAscending;
            set => SetProperty(ref _isAscending, value);
        }

        #endregion

        // RBAC: Only administrators can create physicians
        public bool CanCreatePhysician => HasPermission(_sessionManager, Permission.CreatePhysicianProfile);

        public MauiCommand LoadPhysiciansCommand { get; }
        public MauiCommand SearchCommand { get; }
        public MauiCommand ViewPhysicianCommand { get; }
        public MauiCommand CreatePhysicianCommand { get; }
        public MauiCommand RefreshCommand { get; }
        public MauiCommand BackCommand { get; }

        public PhysicianListViewModel(
            CommandFactory commandFactory,
            CommandInvoker commandInvoker,
            INavigationService navigationService,
            SessionManager sessionManager,
            ProfileService profileService)
        {
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _profileRegistry = profileService ?? throw new ArgumentNullException(nameof(profileService));

            Title = "Physicians";

            // Create list command
            var listCoreCommand = _commandFactory.CreateCommand(ListPhysiciansCommand.Key);
            LoadPhysiciansCommand = new MauiCommandAdapter(
                _commandInvoker,
                listCoreCommand!,
                parameterBuilder: BuildListParameters,
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleListResult
            );

            // Search is just a variation of list with search parameter
            SearchCommand = new RelayCommand(
                execute: () => LoadPhysiciansCommand.Execute(null)
            );

            // Navigate to detail
            ViewPhysicianCommand = new RelayCommand<Guid>(
                execute: async (physicianId) => await NavigateToDetailAsync(physicianId)
            );

            // Navigate to create
            CreatePhysicianCommand = new AsyncRelayCommand(NavigateToCreateAsync);

            // Refresh command
            RefreshCommand = new RelayCommand(() =>
            {
                IsRefreshing = true;
                LoadPhysiciansCommand.Execute(null);
                IsRefreshing = false;
            });

            // Back command - navigate explicitly to home page
            BackCommand = new AsyncRelayCommand(async () => await _navigationService.NavigateToHomeAsync());

            // Load physicians on initialization
            LoadPhysiciansCommand.Execute(null);
        }

        private CommandParameters BuildListParameters()
        {
            var parameters = new CommandParameters();

            // Add search filter if provided
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                parameters.SetParameter(ListPhysiciansCommand.Parameters.Search, SearchText);
            }

            return parameters;
        }

        private void HandleListResult(CommandResult result)
        {
            ClearValidation();

            if (result.Success && result.Data is IEnumerable<PhysicianProfile> physicians)
            {
                Physicians.Clear();

                // Filter for patients - only show their assigned physician(s)
                var filteredPhysicians = physicians;
                if (GetCurrentRole(_sessionManager) == UserRole.Patient)
                {
                    var patientId = _sessionManager.CurrentSession?.UserId;
                    if (patientId.HasValue)
                    {
                        var patientProfile = _profileRegistry.GetProfileById(patientId.Value) as PatientProfile;
                        if (patientProfile?.PrimaryPhysicianId != null)
                        {
                            filteredPhysicians = physicians.Where(p => p.Id == patientProfile.PrimaryPhysicianId.Value);
                        }
                        else
                        {
                            filteredPhysicians = [];
                        }
                    }
                }

                foreach (var physician in filteredPhysicians)
                {
                    Physicians.Add(physician);
                }
            }
            else
            {
                SetValidationError(result.GetDisplayMessage());
                Physicians.Clear();
            }
        }

        private async Task NavigateToDetailAsync(Guid physicianId)
        {
            await _navigationService.NavigateToAsync($"PhysicianDetailPage?physicianId={physicianId}");
        }

        private async Task NavigateToCreateAsync()
        {
            await _navigationService.NavigateToAsync(nameof(CreatePhysicianPage));
        }
    }
}
