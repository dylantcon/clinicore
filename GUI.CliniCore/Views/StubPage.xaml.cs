using GUI.CliniCore.ViewModels;

namespace GUI.CliniCore.Views
{
    public partial class StubPage : ContentPage
    {
        public StubPage(StubViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
