using Casbin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Adapters.Database;
using Adapters.Permissions.Repositories;

namespace Adapters.Permissions;

/// <summary>
/// Setup for Casbin authorization
/// </summary>
public static class PermissionsSetup
{
    /// <summary>
    /// Add Casbin authorization services
    /// </summary>
    public static IServiceCollection AddCasbinAuthorization(this IServiceCollection services)
    {
        // Register Casbin enforcer with file adapter
        services.AddSingleton<IEnforcer>(serviceProvider =>
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("Casbin.Permissions");

            try
            {
                // Get the model and policy file paths
                var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "casbin_model.conf");
                var policyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "casbin_policy.csv");

                if (!File.Exists(modelPath))
                {
                    logger.LogWarning("Casbin model file not found at {ModelPath}, creating default model", modelPath);
                    CreateDefaultModelFile(modelPath);
                }

                logger.LogInformation("Loading Casbin model from {ModelPath}", modelPath);

                // Create enforcer with model and policy file
                var enforcer = new Enforcer(modelPath, policyPath);

                logger.LogInformation("Casbin enforcer initialized successfully");

                return enforcer;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error initializing Casbin enforcer");
                throw;
            }
        });

        // Register authorization service
        services.AddScoped<ICasbinAuthorizationService, CasbinAuthorizationService>();

        // Register repositories
        services.AddScoped<IRbacRepository, RbacRepository>();
        services.AddScoped<IRoleMetadataRepository, RoleMetadataRepository>();

        // Register bootstrap service
        services.AddScoped<PermissionBootstrap>();

        // Register initializer
        services.AddSingleton<PermissionInitializer>();

        return services;
    }

    /// <summary>
    /// Initializes the permission system on application startup.
    /// This method:
    /// 1. Applies all pending database migrations
    /// 2. Creates Casbin tables if they don't exist
    /// 3. Syncs policies from CSV to database
    /// 4. Loads role metadata from JSON
    ///
    /// Call this in Program.cs after building the app:
    /// await app.InitializePermissionSystemAsync();
    /// </summary>
    public static async Task InitializePermissionSystemAsync(this IServiceProvider serviceProvider)
    {
        var initializer = serviceProvider.GetRequiredService<PermissionInitializer>();
        await initializer.InitializeAsync();
    }

    /// <summary>
    /// Sync Casbin policies from CSV files to database
    /// This should be called once during application startup to sync policies to database
    /// DEPRECATED: Use InitializePermissionSystemAsync instead
    /// </summary>
    [Obsolete("Use InitializePermissionSystemAsync instead")]
    public static async Task SyncCasbinPoliciesToDatabaseAsync(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("Casbin.Permissions");

        try
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            logger.LogInformation("Syncing Casbin policies to database");

            var enforcer = serviceProvider.GetRequiredService<IEnforcer>();
            var policyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "casbin_policy.csv");

            if (!File.Exists(policyPath))
            {
                logger.LogWarning("Policy CSV file not found at {PolicyPath}", policyPath);
                return;
            }

            // Clear existing policies in database
            var existingRules = context.Set<Shared.Entity.CasbinRule>().ToList();
            context.Set<Shared.Entity.CasbinRule>().RemoveRange(existingRules);

            // Load policies from CSV and save to database
            var lines = await File.ReadAllLinesAsync(policyPath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                    continue;

                var parts = line.Split(',').Select(p => p.Trim()).ToArray();
                if (parts.Length >= 4 && parts[0] == "p")
                {
                    // Policy line: p, role, resource, action
                    var rule = new Shared.Entity.CasbinRule
                    {
                        PType = parts[0],
                        V0 = parts[1], // role
                        V1 = parts[2], // resource
                        V2 = parts[3]  // action
                    };
                    context.Set<Shared.Entity.CasbinRule>().Add(rule);
                }
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Casbin policies synced to database successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error syncing Casbin policies to database");
            throw;
        }
    }

    private static void CreateDefaultModelFile(string path)
    {
        var modelContent = @"# Casbin RBAC Model Configuration
[request_definition]
r = sub, obj, act

[policy_definition]
p = sub, obj, act

[role_definition]
g = _, _

[policy_effect]
e = some(where (p.eft == allow))

[matchers]
m = g(r.sub, p.sub) && r.obj == p.obj && r.act == p.act
";
        File.WriteAllText(path, modelContent);
    }
}

