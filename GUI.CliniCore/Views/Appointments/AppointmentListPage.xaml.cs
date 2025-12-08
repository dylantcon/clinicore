using GUI.CliniCore.ViewModels.Appointments;

namespace GUI.CliniCore.Views.Appointments
{
    public partial class AppointmentListPage : ContentPage
    {
        public AppointmentListPage(AppointmentListViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
