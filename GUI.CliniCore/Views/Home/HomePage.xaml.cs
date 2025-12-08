using GUI.CliniCore.Services;
using GUI.CliniCore.Views.Shared;

namespace GUI.CliniCore.Views.Home
{
    public partial class HomePage : ContentPage
    {
        private readonly IHomeViewModelFactory _viewModelFactory;

        public HomePage(IHomeViewModelFactory viewModelFactory)
        {
            System.Diagnostics.Debug.WriteLine("HomePage constructor called");
            InitializeComponent();
            _viewModelFactory = viewModelFactory;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            System.Diagnostics.Debug.WriteLine("HomePage OnAppearing called");

            // Only initialize if BindingContext hasn't been set yet
            if (BindingContext == null)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("Creating ViewModel for current role...");

                    // Create ViewModel when navigated to (after login), not in constructor
                    var viewModel = _viewModelFactory.CreateForCurrentRole();
                    BindingContext = viewModel;

                    System.Diagnostics.Debug.WriteLine($"ViewModel created: {viewModel?.GetType().Name}");

                    // Get the template selector and apply the appropriate template
                    var selector = (RoleBasedContentTemplateSelector)Resources["RoleSelector"];
                    var template = selector.SelectTemplate(viewModel, this);

                    System.Diagnostics.Debug.WriteLine($"Template selected: {template != null}");

                    if (template != null)
                    {
                        // Create the content from the template
                        var content = template.CreateContent();
                        if (content is View view)
                        {
                            view.BindingContext = viewModel;
                            RoleContentView.Content = view;
                            System.Diagnostics.Debug.WriteLine("RoleContentView.Content set successfully");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in HomePage.OnAppearing: {ex}");
                }
            }
        }
    }
}
