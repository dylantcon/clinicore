using GUI.CliniCore.ViewModels.ClinicalDocuments;

namespace GUI.CliniCore.Views.ClinicalDocuments
{
    public partial class ClinicalDocumentListPage : ContentPage
    {
        public ClinicalDocumentListPage(ClinicalDocumentListViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
