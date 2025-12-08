using GUI.CliniCore.ViewModels.Users;

namespace GUI.CliniCore.Views.Users
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
