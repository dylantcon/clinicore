using GUI.CliniCore.ViewModels;

namespace GUI.CliniCore.Views
{
    public partial class ClinicalDocumentEditPage : ContentPage
    {
        public ClinicalDocumentEditPage(ClinicalDocumentEditViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
