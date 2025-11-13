using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SS.Notifier.Data;
using SS.Notifier.Data.Entity;
using SS.Notifier.Data.Repository;
using SS.Notifier.Data.Settings;
using SS.Notifier.Services;

namespace SS.Notifier;

public class Program
{
    public static async Task Main(string[] args)
    {
        IHost host = CreateHostBuilder(args).Build();

        // Wait for database connection and apply migrations
        using (var scope = host.Services.CreateScope())
        {
            IServiceProvider services = scope.ServiceProvider;
            ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("Connecting to database...");
                NotifierDbContext context = services.GetRequiredService<NotifierDbContext>();

                // Test connection
                await context.Database.CanConnectAsync();
                logger.LogInformation("Database connection successful");

                // Apply migrations
                logger.LogInformation("Applying database migrations...");
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while connecting to the database or applying migrations");
                throw;
            }

            string? botToken = Environment.GetEnvironmentVariable("BOT_TOKEN");
            logger.LogInformation("BOT_TOKEN={token}", botToken);

            AppSettings appSettings = services.GetRequiredService<IOptions<AppSettings>>().Value;

            logger.LogInformation("Chat num: {n}", appSettings.Chats.Count);
            foreach (KeyValuePair<string, long> kvp in appSettings.Chats)
            {
                logger.LogInformation("Chat '{name}' is '{chatId}'", kvp.Key, kvp.Value);
            }
        }


        await host.RunAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .AddUserSecrets<Program>(); // Loads user secrets
            })
            .ConfigureServices((context, services) =>
            {
                services.AddHttpClient();

                // Connection string comes from environment variable (set in docker-compose)
                var connStr = context.Configuration.GetConnectionString("Postgres")
                              ?? throw new InvalidOperationException("Missing connection string");
                services.Configure<AppSettings>(context.Configuration);
                services.AddDbContext<NotifierDbContext>(opt => opt.UseNpgsql(connStr));
                services.AddTransient<IRepository<ApartmentEntity, string>, ApartmentRepository>();
                services.AddTransient<IApartmentService, ApartmentService>();
            });
}