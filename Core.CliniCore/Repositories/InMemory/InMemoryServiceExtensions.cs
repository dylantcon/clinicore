using Microsoft.Extensions.DependencyInjection;

namespace Core.CliniCore.Repositories.InMemory
{
    /// <summary>
    /// Extension methods for registering InMemory repositories with dependency injection.
    /// Use this for testing or standalone operation without an API backend.
    /// </summary>
    public static class InMemoryServiceExtensions
    {
        /// <summary>
        /// Registers all InMemory repository implementations with the service collection.
        /// Data is stored in memory and will not persist between application restarts.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddInMemoryRepositories(this IServiceCollection services)
        {
            // Register as singletons so data persists for the lifetime of the application
            services.AddSingleton<IPatientRepository, InMemoryPatientRepository>();
            services.AddSingleton<IPhysicianRepository, InMemoryPhysicianRepository>();
            services.AddSingleton<IAppointmentRepository, InMemoryAppointmentRepository>();
            services.AddSingleton<IClinicalDocumentRepository, InMemoryClinicalDocumentRepository>();
            services.AddSingleton<IAdministratorRepository, InMemoryAdministratorRepository>();
            services.AddSingleton<ICredentialRepository, InMemoryCredentialRepository>();

            return services;
        }
    }
}
