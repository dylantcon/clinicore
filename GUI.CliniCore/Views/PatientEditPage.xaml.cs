using GUI.CliniCore.ViewModels;

namespace GUI.CliniCore.Views
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
