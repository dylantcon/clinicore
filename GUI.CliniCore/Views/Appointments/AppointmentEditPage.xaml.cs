using GUI.CliniCore.ViewModels.Appointments;

namespace GUI.CliniCore.Views.Appointments
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
