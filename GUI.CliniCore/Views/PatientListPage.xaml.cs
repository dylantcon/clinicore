using GUI.CliniCore.ViewModels;

namespace GUI.CliniCore.Views
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
