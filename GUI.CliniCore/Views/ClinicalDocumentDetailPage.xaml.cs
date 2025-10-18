using GUI.CliniCore.ViewModels;

namespace GUI.CliniCore.Views
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
