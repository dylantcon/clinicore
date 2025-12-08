using GUI.CliniCore.ViewModels.Physicians;

namespace GUI.CliniCore.Views.Physicians
{
    public partial class PhysicianDetailPage : ContentPage
    {
        public PhysicianDetailPage(PhysicianDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
