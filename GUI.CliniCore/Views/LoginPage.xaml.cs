using GUI.CliniCore.ViewModels;

namespace GUI.CliniCore.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;

#if DEBUG
            // Show development credentials in debug builds
            DevCredentialsBorder.IsVisible = true;
#endif
        }
    }
}
