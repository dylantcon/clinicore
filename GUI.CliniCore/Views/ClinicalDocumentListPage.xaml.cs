using GUI.CliniCore.ViewModels;

namespace GUI.CliniCore.Views
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
