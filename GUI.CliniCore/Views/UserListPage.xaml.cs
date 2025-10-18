using GUI.CliniCore.ViewModels;

namespace GUI.CliniCore.Views
{
    public partial class UserListPage : ContentPage
    {
        public UserListPage(UserListViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
