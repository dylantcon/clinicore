using GUI.CliniCore.ViewModels;

namespace GUI.CliniCore.Views
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
