using GUI.CliniCore.ViewModels.ClinicalDocuments;

namespace GUI.CliniCore.Views.ClinicalDocuments
{
    public partial class ClinicalDocumentDetailPage : ContentPage
    {
        public ClinicalDocumentDetailPage(ClinicalDocumentDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
