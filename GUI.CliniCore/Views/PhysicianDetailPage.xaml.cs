using GUI.CliniCore.ViewModels;

namespace GUI.CliniCore.Views
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
