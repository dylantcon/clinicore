using GUI.CliniCore.ViewModels.Stub;

namespace GUI.CliniCore.Views.Stub
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
