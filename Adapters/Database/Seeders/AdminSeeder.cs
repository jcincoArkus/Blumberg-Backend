using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Entity;

namespace Adapters.Database.Seeders;

/// <summary>
/// Admin seed data structure
/// </summary>
public record AdminSeedData(
    string Email,
    string Password,
    string FirstName,
    string LastName
);

/// <summary>
/// Seeds admin users, bound to the test organization from OrganizationSeeder.
/// </summary>
public class AdminSeeder(ILoggerFactory loggerFactory) : ISeeder
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<AdminSeeder>();

    public int Order => 0;

    /// <summary>
    /// List of admins to seed.
    /// To add a new admin, create a PR adding a new entry here.
    /// </summary>
    private static readonly AdminSeedData[] Admins =
    [
        // Default admin for development
        new("admin@blumberg.com", "Admin123.", "Admin", "User"),
        new("jibarra@blumberg.com", "Admin123.", "Juan", "Ibarra"),
        new("jlopez@blumberg.com", "Admin123.", "Jose", "Lopez"),
        new("fgonzalez@blumberg.com", "Admin123.", "Fernanda", "Gonzalez"),
        new("jlopez@arkusnexus.com", "Admin123.", "Eduardo", "Lopez"),

        // Add new admins below (one per line, create a PR to add)
        // new("email@example.com", "Password123!", "FirstName", "LastName"),
    ];

    public async Task SeedAsync(ApplicationDbContext context)
    {
        var org = await context.Organizations
            .FirstOrDefaultAsync(o => o.Slug == OrganizationSeeder.TestOrganizationSlug && o.DeletedAt == null);

        if (org == null)
        {
            throw new InvalidOperationException(
                $"Test organization '{OrganizationSeeder.TestOrganizationSlug}' not found. Ensure OrganizationSeeder runs before AdminSeeder (Order).");
        }

        var created = 0;
        foreach (var data in Admins)
        {
            if (context.Admins.IgnoreQueryFilters().Any(a => a.Email == data.Email))
            {
                _logger.LogDebug("Admin '{Email}' already exists, skipping", data.Email);
                continue;
            }

            var admin = new Admin
            {
                Id = Guid.NewGuid(),
                Email = data.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(data.Password),
                FirstName = data.FirstName,
                LastName = data.LastName,
                OrganizationId = org.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Admins.Add(admin);
            created++;
        }

        await context.SaveChangesAsync();
        _logger.LogInformation("Created {Count} admin(s) (bound to organization '{OrgName}')", created, org.Name);
    }
}

