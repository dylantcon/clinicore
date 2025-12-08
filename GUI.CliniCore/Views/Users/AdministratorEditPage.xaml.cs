using GUI.CliniCore.ViewModels.Users;

namespace GUI.CliniCore.Views.Users
{
    public partial class AdministratorEditPage : ContentPage
    {
        public AdministratorEditPage(AdministratorEditViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
