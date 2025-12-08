using GUI.CliniCore.ViewModels.Patients;

namespace GUI.CliniCore.Views.Patients
{
    public partial class PatientListPage : ContentPage
    {
        public PatientListPage(PatientListViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
