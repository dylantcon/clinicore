using GUI.CliniCore.Views;

namespace GUI.CliniCore
{
    public partial class App : Application
    {
        public App(AppShell appShell)
        {
            InitializeComponent();

            MainPage = appShell;

            // Navigate to login after shell is set
            MainPage.Dispatcher.Dispatch(async () =>
            {
                await appShell.GoToAsync($"//{nameof(LoginPage)}");
            });
        }
    }
}
