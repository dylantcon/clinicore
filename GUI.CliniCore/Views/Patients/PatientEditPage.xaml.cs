using GUI.CliniCore.ViewModels.Patients;

namespace GUI.CliniCore.Views.Patients
{
    public partial class PatientEditPage : ContentPage
    {
        public PatientEditPage(PatientEditViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
