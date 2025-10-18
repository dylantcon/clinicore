using GUI.CliniCore.ViewModels;

namespace GUI.CliniCore.Views
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
