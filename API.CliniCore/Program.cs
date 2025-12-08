using API.CliniCore.Data;

namespace API.CliniCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Get connection string (defaults to SQLite file in current directory)
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=clinicore.db";

            // Add CliniCore services with EF Core/SQLite persistence
            builder.Services.AddCliniCoreWithEfCore(connectionString);

            // Add controllers
            builder.Services.AddControllers();

            // Configure Swagger/OpenAPI for API documentation
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "CliniCore API",
                    Version = "v1",
                    Description = "REST API for CliniCore medical clinic management system. " +
                                  "Data is persisted to SQLite database (clinicore.db)."
                });
            });

            // Configure CORS for GUI client
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Initialize database and seed data
            app.Services.InitializeDatabase(seedData: app.Environment.IsDevelopment());

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "CliniCore API v1");
                    options.RoutePrefix = "swagger";
                });
            }

            app.UseCors();
            app.UseAuthorization();
            app.MapControllers();

            Console.WriteLine("CliniCore API started with SQLite persistence");
            Console.WriteLine($"Database: {connectionString}");
            Console.WriteLine("Swagger UI: http://localhost:5000/swagger");

            app.Run();
        }
    }
}
