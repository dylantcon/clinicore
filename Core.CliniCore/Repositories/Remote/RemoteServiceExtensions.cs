using Microsoft.Extensions.DependencyInjection;

namespace Core.CliniCore.Repositories.Remote
{
    /// <summary>
    /// Extension methods for registering Remote repositories with dependency injection.
    /// Use this when the application should communicate with the API for persistence.
    /// </summary>
    public static class RemoteServiceExtensions
    {
        /// <summary>
        /// Registers all Remote repository implementations with the service collection.
        /// Configures HttpClient to point to the specified API base URL.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="apiBaseUrl">The base URL of the CliniCore API (e.g., "http://localhost:5000")</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddRemoteRepositories(this IServiceCollection services, string apiBaseUrl)
        {
            // Register HttpClient with base address
            services.AddHttpClient<IPatientRepository, RemotePatientRepository>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            services.AddHttpClient<IPhysicianRepository, RemotePhysicianRepository>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            services.AddHttpClient<IAppointmentRepository, RemoteAppointmentRepository>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            services.AddHttpClient<IClinicalDocumentRepository, RemoteClinicalDocumentRepository>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            services.AddHttpClient<IAdministratorRepository, RemoteAdministratorRepository>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            services.AddHttpClient<ICredentialRepository, RemoteCredentialRepository>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            return services;
        }

        /// <summary>
        /// Registers Remote repositories with a shared HttpClient instance.
        /// Use this overload when you want to manage the HttpClient lifecycle yourself.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="httpClientFactory">Factory function that creates configured HttpClient instances</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddRemoteRepositories(this IServiceCollection services, Func<HttpClient> httpClientFactory)
        {
            services.AddScoped<IPatientRepository>(sp => new RemotePatientRepository(httpClientFactory()));
            services.AddScoped<IPhysicianRepository>(sp => new RemotePhysicianRepository(httpClientFactory()));
            services.AddScoped<IAppointmentRepository>(sp => new RemoteAppointmentRepository(httpClientFactory()));
            services.AddScoped<IClinicalDocumentRepository>(sp => new RemoteClinicalDocumentRepository(httpClientFactory()));
            services.AddScoped<IAdministratorRepository>(sp => new RemoteAdministratorRepository(httpClientFactory()));
            services.AddScoped<ICredentialRepository>(sp => new RemoteCredentialRepository(httpClientFactory()));

            return services;
        }
    }
}
