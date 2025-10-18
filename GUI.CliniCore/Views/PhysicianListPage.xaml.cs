using GUI.CliniCore.ViewModels;

namespace GUI.CliniCore.Views
{
    public partial class PhysicianListPage : ContentPage
    {
        public PhysicianListPage(PhysicianListViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
