using GUI.CliniCore.ViewModels.Appointments;

namespace GUI.CliniCore.Views.Appointments
{
    public partial class AppointmentDetailPage : ContentPage
    {
        public AppointmentDetailPage(AppointmentDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
