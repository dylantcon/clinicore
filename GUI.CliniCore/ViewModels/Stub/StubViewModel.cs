using GUI.CliniCore.Commands;
using GUI.CliniCore.Resources.Fonts;
using GUI.CliniCore.Services;
using GUI.CliniCore.ViewModels.Base;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels.Stub
{
    /// <summary>
    /// ViewModel for feature stub pages
    /// Shows user-friendly message for unimplemented features
    /// </summary>
    [QueryProperty(nameof(StubType), "type")]
    public partial class StubViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        private string _stubType = string.Empty;
        public string StubType
        {
            get => _stubType;
            set
            {
                if (SetProperty(ref _stubType, value))
                {
                    ConfigureStub(value);
                }
            }
        }

        private string _featureName = string.Empty;
        public string FeatureName
        {
            get => _featureName;
            set => SetProperty(ref _featureName, value);
        }

        private string _icon = MaterialIcons.Info;
        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private string _reason = string.Empty;
        public string Reason
        {
            get => _reason;
            set => SetProperty(ref _reason, value);
        }

        public MauiCommand BackCommand { get; }

        public StubViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            BackCommand = new AsyncRelayCommand(async () => await _navigationService.NavigateToHomeAsync());
        }

        private void ConfigureStub(string type)
        {
            switch (type?.ToLower())
            {
                case "reports":
                    ConfigureReportsStub();
                    break;
                case "admin":
                    ConfigureSystemAdminStub();
                    break;
                default:
                    Title = "Feature Not Available";
                    FeatureName = "Coming Soon";
                    Icon = MaterialIcons.Info;
                    Description = "This feature is currently under development.";
                    Reason = "Implementation is planned for a future release.";
                    break;
            }
        }

        private void ConfigureReportsStub()
        {
            Title = "Reports";
            FeatureName = "Reports & Analytics";
            Icon = MaterialIcons.Assignment;
            Description = "This feature generates statistical reports and analytics for patients, physicians, appointments, and facility operations. It provides insights into system usage, performance metrics, and clinical outcomes.";
            Reason = "Report generation has been intentionally left as a stub for this assignment. While the command architecture supports it, implementing comprehensive report generation with charts and data visualization would significantly expand the scope beyond the assignment requirements.";
        }

        public void ConfigureSystemAdminStub()
        {
            Title = "System Administration";
            FeatureName = "System Administration";
            Icon = MaterialIcons.Settings;
            Description = "This feature supports multi-facility management, including creating facilities, updating facility settings, viewing audit logs, and performing system maintenance tasks.";
            Reason = "Multi-tenancy and facility management have been intentionally left as a stub for this assignment. The system currently operates in single-facility mode, which is sufficient to demonstrate the core EMR functionality without the complexity of managing multiple organizations.";
        }
    }
}
