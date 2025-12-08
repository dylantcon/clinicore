using GUI.CliniCore.ViewModels.ClinicalDocuments;

namespace GUI.CliniCore.Views.ClinicalDocuments
{
    public partial class ClinicalDocumentEditPage : ContentPage
    {
        public ClinicalDocumentEditPage(ClinicalDocumentEditViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        /// <summary>
        /// Unfocuses any focused element when tapping on the background.
        /// Uses platform-agnostic approach without expensive visual tree traversal.
        /// </summary>
        private void OnBackgroundTapped(object? sender, TappedEventArgs e)
        {
            // Simple approach: unfocus by focusing the page itself
            // This avoids expensive recursive visual tree traversal
            if (this.IsFocused == false)
            {
                this.Focus();
            }
        }
    }
}
