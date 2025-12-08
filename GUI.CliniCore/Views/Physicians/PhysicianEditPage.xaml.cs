using GUI.CliniCore.ViewModels.Physicians;

namespace GUI.CliniCore.Views.Physicians
{
    public partial class PhysicianEditPage : ContentPage
    {
        public PhysicianEditPage(PhysicianEditViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
