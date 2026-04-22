using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Entity;

namespace Adapters.Database.Seeders;

public class SiteSeeder(ILoggerFactory loggerFactory) : ISeeder
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<SiteSeeder>();

    public int Order => 1;

    public async Task SeedAsync(ApplicationDbContext context)
    {
        var org = await context.Organizations
            .FirstOrDefaultAsync(o => o.Slug == OrganizationSeeder.TestOrganizationSlug && o.DeletedAt == null);

        if (org == null)
        {
            throw new InvalidOperationException(
                $"Test organization '{OrganizationSeeder.TestOrganizationSlug}' not found. Ensure OrganizationSeeder runs before SiteSeeder (Order).");
        }

        if (context.Sites.IgnoreQueryFilters().Any(s => s.OrganizationId == org.Id && s.DeletedAt == null))
        {
            _logger.LogInformation("Sites for organization '{Slug}' already exist, skipping", OrganizationSeeder.TestOrganizationSlug);
            return;
        }

        var now = DateTime.UtcNow;
        var sites = new List<Site>
        {
            new()
            {
                Id = Guid.NewGuid(),
                OrganizationId = org.Id,
                Name = SeedData.SiteNames.NorthDistributionCenter,
                Address = "123 Main St",
                City = "Chicago",
                State = "IL",
                PostalCode = "60601",
                Country = "USA",
                CreatedAt = now,
            },
            new()
            {
                Id = Guid.NewGuid(),
                OrganizationId = org.Id,
                Name = SeedData.SiteNames.WestCoastWarehouse,
                Address = "456 Elm St",
                City = "Los Angeles",
                State = "CA",
                PostalCode = "90001",
                Country = "USA",
                CreatedAt = now,
            },
            new()
            {
                Id = Guid.NewGuid(),
                OrganizationId = org.Id,
                Name = SeedData.SiteNames.EastCoastHub,
                Address = "789 Oak St",
                City = "New York",
                State = "NY",
                PostalCode = "10001",
                Country = "USA",
                CreatedAt = now,
            },
            new()
            {
                Id = Guid.NewGuid(),
                OrganizationId = org.Id,
                Name = SeedData.SiteNames.SouthRegionalDc,
                Address = "101 Pine St",
                City = "Houston",
                State = "TX",
                PostalCode = "77001",
                Country = "USA",
                CreatedAt = now,
            },
        };

        context.Sites.AddRange(sites);
        await context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} sites for organization '{OrgName}'", sites.Count, org.Name);
    }
}