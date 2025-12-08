using GUI.CliniCore.ViewModels.Authentication;

namespace GUI.CliniCore.Views.Authentication
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
