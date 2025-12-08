using GUI.CliniCore.ViewModels.ClinicalDocuments;

namespace GUI.CliniCore.Views.ClinicalDocuments
{
    public partial class CreateClinicalDocumentPage : ContentPage
    {
        public CreateClinicalDocumentPage(CreateClinicalDocumentViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
