using Shared.Entity;

namespace Adapters.Database.Seeders;

/// <summary>
/// Seeds a test organization. Other seeders (e.g. AdminSeeder) bind their data to this organization.
/// </summary>
public class OrganizationSeeder : ISeeder
{
    /// <summary>
    /// Slug of the test organization created by this seeder. Use this to look up the org in other seeders.
    /// </summary>
    public const string TestOrganizationSlug = "blumberg";

    public int Order => -1; // Run before AdminSeeder (0)

    public async Task SeedAsync(ApplicationDbContext context)
    {
        if (context.Organizations.Any(o => o.Slug == TestOrganizationSlug))
        {
            Console.WriteLine($"  → Test organization '{TestOrganizationSlug}' already exists, skipping...");
            return;
        }

        var org = new Organization
        {
            Id = Guid.Empty(),
            Name = "Blumberg",
            Slug = TestOrganizationSlug,
            CreatedAt = DateTime.UtcNow
        };

        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        Console.WriteLine($"  → Created test organization '{org.Name}' (slug: {TestOrganizationSlug})");
    }
}
