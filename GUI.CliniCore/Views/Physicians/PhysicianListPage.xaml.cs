using GUI.CliniCore.ViewModels.Physicians;

namespace GUI.CliniCore.Views.Physicians
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
