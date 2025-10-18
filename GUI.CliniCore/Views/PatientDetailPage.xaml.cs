using GUI.CliniCore.ViewModels;

namespace GUI.CliniCore.Views
{
    public partial class PatientDetailPage : ContentPage
    {
        public PatientDetailPage(PatientDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
