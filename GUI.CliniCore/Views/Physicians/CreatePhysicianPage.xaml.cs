using GUI.CliniCore.ViewModels.Physicians;

namespace GUI.CliniCore.Views.Physicians
{
    public partial class CreatePhysicianPage : ContentPage
    {
        public CreatePhysicianPage(CreatePhysicianViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
