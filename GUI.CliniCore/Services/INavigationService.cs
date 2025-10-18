using GUI.CliniCore.ViewModels;

namespace GUI.CliniCore.Services
{
    /// <summary>
    /// Navigation service abstraction for page navigation
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Navigates to a page by route name
        /// </summary>
        Task NavigateToAsync(string route);

        /// <summary>
        /// Navigates back to the previous page
        /// </summary>
        Task GoBackAsync();

        /// <summary>
        /// Navigates to the login page
        /// </summary>
        Task NavigateToLoginAsync();

        /// <summary>
        /// Navigates to the home page
        /// </summary>
        Task NavigateToHomeAsync();
    }
}
