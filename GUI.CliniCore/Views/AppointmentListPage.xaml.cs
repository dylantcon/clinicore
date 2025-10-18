using GUI.CliniCore.ViewModels;

namespace GUI.CliniCore.Views
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
