using GUI.CliniCore.ViewModels;

namespace GUI.CliniCore.Views
{
    public partial class CreateAppointmentPage : ContentPage
    {
        public CreateAppointmentPage(CreateAppointmentViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
