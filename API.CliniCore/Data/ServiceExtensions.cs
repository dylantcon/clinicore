using API.CliniCore.Data.Repositories;
using Core.CliniCore.Bootstrap;
using Core.CliniCore.Commands;
using Core.CliniCore.Domain.Authentication;
using Core.CliniCore.Repositories;
using Core.CliniCore.Scheduling.Management;
using Core.CliniCore.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace API.CliniCore.Data
{
    /// <summary>
    /// Extension methods for registering CliniCore services with Entity Framework Core.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds CliniCore services with SQLite/EF Core persistence.
        /// This replaces the in-memory repositories with EF Core implementations.
        /// </summary>
        public static IServiceCollection AddCliniCoreWithEfCore(
            this IServiceCollection services,
            string connectionString)
        {
            // Register DbContext
            services.AddDbContext<CliniCoreDbContext>(options =>
                options.UseSqlite(connectionString));

            // Repository Layer (Scoped - one per request, matches DbContext lifecycle)
            services.AddScoped<IPatientRepository, EfPatientRepository>();
            services.AddScoped<IPhysicianRepository, EfPhysicianRepository>();
            services.AddScoped<IAdministratorRepository, EfAdministratorRepository>();
            services.AddScoped<IAppointmentRepository, EfAppointmentRepository>();
            services.AddScoped<IClinicalDocumentRepository, EfClinicalDocumentRepository>();
            services.AddScoped<ICredentialRepository, EfCredentialRepository>();

            // Core Authentication Service (Scoped - depends on scoped repositories)
            services.AddScoped<IAuthenticationService, BasicAuthenticationService>();
            services.AddSingleton<RoleBasedAuthorizationService>();

            // Service Layer (Scoped to match repositories)
            services.AddScoped<ProfileService>(sp => new ProfileService(
                sp.GetRequiredService<IPatientRepository>(),
                sp.GetRequiredService<IPhysicianRepository>(),
                sp.GetRequiredService<IAdministratorRepository>(),
                sp.GetRequiredService<ICredentialRepository>(),
                sp.GetRequiredService<IAppointmentRepository>(),
                sp.GetRequiredService<IClinicalDocumentRepository>()
            ));
            services.AddScoped<SchedulerService>();
            services.AddScoped<ClinicalDocumentService>();

            // Scheduling helpers
            services.AddScoped<ScheduleConflictDetector>();

            // Command Infrastructure
            services.AddScoped<CommandInvoker>();
            services.AddScoped<CommandFactory>(serviceProvider =>
                new CommandFactory(
                    serviceProvider.GetRequiredService<IAuthenticationService>(),
                    serviceProvider.GetRequiredService<SchedulerService>(),
                    serviceProvider.GetRequiredService<ProfileService>(),
                    serviceProvider.GetRequiredService<ClinicalDocumentService>()
                ));

            return services;
        }

        /// <summary>
        /// Ensures the database is created and seeds initial data if empty.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when database is locked by another process.</exception>
        public static void InitializeDatabase(this IServiceProvider serviceProvider, bool seedData = true)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CliniCoreDbContext>();

            try
            {
                // Ensure database is created
                context.Database.EnsureCreated();

                // Seed development data if requested (seeder is smart - only seeds what's missing)
                if (seedData)
                {
                    DevelopmentDataSeeder.SeedDevelopmentData(scope.ServiceProvider, createSampleData: true);
                }
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 5) // SQLITE_BUSY
            {
                throw new InvalidOperationException(
                    "Database is locked by another process. " +
                    "Please close other instances of the API or applications using clinicore.db and try again.", ex);
            }
        }
    }
}
