using GUI.CliniCore.ViewModels;

namespace GUI.CliniCore.Views
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
