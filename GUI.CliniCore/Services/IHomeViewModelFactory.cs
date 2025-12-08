using GUI.CliniCore.ViewModels.Base;

namespace GUI.CliniCore.Services
{
    /// <summary>
    /// Factory for creating the appropriate home ViewModel based on the current user's role
    /// </summary>
    public interface IHomeViewModelFactory
    {
        /// <summary>
        /// Creates the appropriate home ViewModel for the currently logged-in user
        /// </summary>
        BaseViewModel CreateForCurrentRole();
    }
}
