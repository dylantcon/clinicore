using GUI.CliniCore.ViewModels.Patients;

namespace GUI.CliniCore.Views.Patients
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
