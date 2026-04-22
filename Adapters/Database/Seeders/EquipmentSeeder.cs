using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Entity;

namespace Adapters.Database.Seeders;

public class EquipmentSeeder(ILoggerFactory loggerFactory) : ISeeder
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<EquipmentSeeder>();

    public int Order => 2;

    public async Task SeedAsync(ApplicationDbContext context)
    {
        var org = await context.Organizations
            .FirstOrDefaultAsync(o => o.Slug == OrganizationSeeder.TestOrganizationSlug && o.DeletedAt == null);

        if (org == null)
        {
            throw new InvalidOperationException(
                $"Test organization '{OrganizationSeeder.TestOrganizationSlug}' not found. Ensure OrganizationSeeder runs before EquipmentSeeder (Order).");
        }

        var sites = await context.Sites
            .IgnoreQueryFilters()
            .Where(s => s.OrganizationId == org.Id && s.DeletedAt == null)
            .ToListAsync();

        var siteByName = sites.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
        if (siteByName.Count == 0)
        {
            throw new InvalidOperationException(
                "No sites found for test organization. Ensure SiteSeeder runs before EquipmentSeeder (Order).");
        }

        if (context.Equipment.IgnoreQueryFilters().Any(e => e.OrganizationId == org.Id && e.DeletedAt == null))
        {
            _logger.LogInformation("Equipment for organization '{Slug}' already exist, skipping", OrganizationSeeder.TestOrganizationSlug);
            return;
        }

        var now = DateTime.UtcNow;
        var equipment = new List<Equipment>();
        foreach (var row in EquipmentSeedRows.All)
        {
            if (!siteByName.TryGetValue(row.SiteName, out var site))
            {
                throw new InvalidOperationException(
                    $"Site '{row.SiteName}' not found. Ensure SiteSeeder uses SeedData.SiteNames.");
            }

            equipment.Add(new Equipment
            {
                Id = Guid.NewGuid(),
                OrganizationId = org.Id,
                SiteId = site.Id,
                Name = row.Name,
                EquipmentType = row.EquipmentType,
                CreatedAt = now,
            });
        }

        context.Equipment.AddRange(equipment);
        await context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} equipment across {SiteCount} sites", equipment.Count, sites.Count);
    }
}