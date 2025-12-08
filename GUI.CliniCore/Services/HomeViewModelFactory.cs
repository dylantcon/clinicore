using Core.CliniCore.Domain.Enumerations;
using GUI.CliniCore.ViewModels.Base;
using GUI.CliniCore.ViewModels.Home;
using Microsoft.Extensions.DependencyInjection;

namespace GUI.CliniCore.Services
{
    /// <summary>
    /// Factory implementation that creates role-specific home ViewModels
    /// Mirrors the ConsoleMenuBuilder pattern from the CLI
    /// </summary>
    public class HomeViewModelFactory : IHomeViewModelFactory
    {
        private readonly SessionManager _sessionManager;
        private readonly IServiceProvider _serviceProvider;

        public HomeViewModelFactory(SessionManager sessionManager, IServiceProvider serviceProvider)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public BaseViewModel CreateForCurrentRole()
        {
            if (!_sessionManager.IsAuthenticated)
            {
                throw new InvalidOperationException("Cannot create home ViewModel: No authenticated session");
            }

            return _sessionManager.CurrentUserRole switch
            {
                UserRole.Administrator => _serviceProvider.GetRequiredService<AdministratorHomeViewModel>(),
                UserRole.Physician => _serviceProvider.GetRequiredService<PhysicianHomeViewModel>(),
                UserRole.Patient => _serviceProvider.GetRequiredService<PatientHomeViewModel>(),
                _ => throw new InvalidOperationException($"Unknown user role: {_sessionManager.CurrentUserRole}")
            };
        }
    }
}
