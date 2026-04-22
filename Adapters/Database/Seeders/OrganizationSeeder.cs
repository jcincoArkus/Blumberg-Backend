using Microsoft.Extensions.Logging;
using Shared.Entity;

namespace Adapters.Database.Seeders;

/// <summary>
/// Seeds a test organization. Other seeders (e.g. AdminSeeder) bind their data to this organization.
/// </summary>
public class OrganizationSeeder(ILoggerFactory loggerFactory) : ISeeder
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<OrganizationSeeder>();

    /// <summary>
    /// Slug of the test organization created by this seeder. Use this to look up the org in other seeders.
    /// </summary>
    public const string TestOrganizationSlug = "blumberg";

    public int Order => -1; // Run before AdminSeeder (0)

    public async Task SeedAsync(ApplicationDbContext context)
    {
        if (context.Organizations.Any(o => o.Slug == TestOrganizationSlug))
        {
            _logger.LogInformation("Test organization '{Slug}' already exists, skipping", TestOrganizationSlug);
            return;
        }

        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Blumberg",
            Slug = TestOrganizationSlug,
            CreatedAt = DateTime.UtcNow
        };

        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        _logger.LogInformation("Created test organization '{Name}' (slug: {Slug})", org.Name, TestOrganizationSlug);
    }
}
