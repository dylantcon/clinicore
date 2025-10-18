using GUI.CliniCore.ViewModels;

namespace GUI.CliniCore.Views
{
    public partial class AppointmentEditPage : ContentPage
    {
        public AppointmentEditPage(AppointmentEditViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
