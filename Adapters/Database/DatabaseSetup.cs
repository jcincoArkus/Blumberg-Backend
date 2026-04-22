using Adapters.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Adapters.Database;

/// <summary>
/// Extension methods for setting up database services
/// </summary>
public static class DatabaseSetup
{
    /// <summary>
    /// Adds PostgreSQL database context to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="connectionString">PostgreSQL connection string</param>
    /// <param name="enableSensitiveDataLogging">Enable sensitive data logging (default: false, only for development)</param>
    /// <param name="enableDetailedErrors">Enable detailed errors (default: false, only for development)</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        string connectionString,
        bool enableSensitiveDataLogging = false,
        bool enableDetailedErrors = false)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                // Enable retry on failure
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);

                // Set command timeout (30 seconds)
                npgsqlOptions.CommandTimeout(30);

                // Use migrations assembly (same as DbContext)
                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            });

            // Development-only options
            if (enableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }

            if (enableDetailedErrors)
            {
                options.EnableDetailedErrors();
            }

            // Log to console in development
            options.LogTo(Console.WriteLine, LogLevel.Information);
        });

        return services;
    }

    /// <summary>
    /// Adds PostgreSQL database context using configuration from AppConfig
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="config">Application configuration</param>
    /// <param name="isDevelopment">Whether the environment is development</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        AppConfig config,
        bool isDevelopment = false)
    {
        var connectionString = config.Database.GetConnectionString();

        return services.AddDatabase(
            connectionString,
            enableSensitiveDataLogging: isDevelopment,
            enableDetailedErrors: isDevelopment);
    }

    /// <summary>
    /// Tests the database connection
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <returns>True if connection is successful, false otherwise</returns>
    public static async Task<bool> TestDatabaseConnectionAsync(this IServiceProvider serviceProvider)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Console.WriteLine("🔄 Testing database connection...");
            var canConnect = await dbContext.Database.CanConnectAsync();

            if (canConnect)
            {
                Console.WriteLine("✅ Database connection successful");
            }
            else
            {
                Console.WriteLine("❌ Database connection failed");
            }

            return canConnect;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Database connection error: {ex.Message}");
            return false;
        }
    }
}

