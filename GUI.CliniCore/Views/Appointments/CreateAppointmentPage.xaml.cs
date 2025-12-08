using GUI.CliniCore.ViewModels.Appointments;

namespace GUI.CliniCore.Views.Appointments
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
