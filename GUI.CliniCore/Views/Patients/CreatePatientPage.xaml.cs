using GUI.CliniCore.ViewModels.Patients;

namespace GUI.CliniCore.Views.Patients
{
    public partial class CreatePatientPage : ContentPage
    {
        public CreatePatientPage(CreatePatientViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
