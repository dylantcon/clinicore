using GUI.CliniCore.Views;

namespace GUI.CliniCore.Services
{
    /// <summary>
    /// MAUI Shell-based navigation service implementation
    /// Tracks navigation history to support Shell's back button
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly Stack<string> _navigationHistory = new();

        public Task NavigateToAsync(string route)
        {
            if (string.IsNullOrWhiteSpace(route))
                throw new ArgumentNullException(nameof(route));

            // Track navigation history for back button support
            // Extract base route without query parameters
            var baseRoute = route.Split('?')[0];
            _navigationHistory.Push(baseRoute);

            return Shell.Current.GoToAsync(route);
        }

        public async Task GoBackAsync()
        {
            // Pop current page from history
            if (_navigationHistory.Count > 0)
            {
                _navigationHistory.Pop();
            }

            // Navigate to previous page if available
            if (_navigationHistory.Count > 0)
            {
                var previousRoute = _navigationHistory.Pop(); // Pop again since NavigateToAsync will push it back
                await NavigateToAsync(previousRoute);
            }
            else
            {
                // No history - go to home as fallback
                await NavigateToHomeAsync();
            }
        }

        public Task NavigateToLoginAsync()
        {
            // Clear history when going to login
            _navigationHistory.Clear();
            return NavigateToAsync($"//{nameof(LoginPage)}");
        }

        public Task NavigateToHomeAsync()
        {
            // Use absolute navigation with single slash for registered route
            return NavigateToAsync($"/{nameof(HomePage)}");
        }
    }
}
