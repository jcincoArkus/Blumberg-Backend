using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Equipment.Repository;

/// <summary>
/// Repository implementation for Equipment entity operations
/// </summary>
public class EquipmentRepository(ApplicationDbContext context, ILogger<EquipmentRepository> logger) : IEquipmentRepository
{
    /// <inheritdoc />
    [Span]
    public virtual async Task<(IReadOnlyList<Shared.Entity.Equipment> Items, int TotalCount)> GetPagedAsync(PaginationRequest request)
    {
        logger.LogDebug("Querying equipment page {Page}, pageSize {PageSize}, search '{Search}'", request.Page, request.PageSize, request.Search);

        IQueryable<Shared.Entity.Equipment> query = context.Equipment
            .Where(e => e.DeletedAt == null)
            .Include(e => e.Organization)
            .Include(e => e.Site);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(e => e.Name.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(e => e.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} equipment from database (total: {TotalCount})", items.Count, totalCount);

        return (items, totalCount);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Shared.Entity.Equipment?> GetByIdAsync(Guid id)
    {
        logger.LogDebug("Querying equipment by ID: {Id}", id);

        var equipment = await context.Equipment
            .Include(e => e.Organization)
            .Include(e => e.Site)
            .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null);

        if (equipment != null)
            logger.LogInformation("Found equipment {Id}", id);
        else
            logger.LogDebug("Equipment not found with ID: {Id}", id);

        return equipment;
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<Shared.Entity.Equipment> CreateAsync(Shared.Entity.Equipment equipment)
    {
        logger.LogDebug("Creating equipment in database: {Name}", equipment.Name);

        equipment.Id = Guid.NewGuid();
        equipment.CreatedAt = DateTime.UtcNow;
        equipment.UpdatedAt = null;
        equipment.DeletedAt = null;

        context.Equipment.Add(equipment);
        await context.SaveChangesAsync();

        logger.LogInformation("Equipment created in database with ID: {Id}", equipment.Id);

        return equipment;
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<Shared.Entity.Equipment> UpdateAsync(Shared.Entity.Equipment equipment)
    {
        logger.LogDebug("Updating equipment in database: {Id}", equipment.Id);

        equipment.UpdatedAt = DateTime.UtcNow;

        context.Equipment.Update(equipment);
        await context.SaveChangesAsync();

        logger.LogInformation("Equipment {Id} updated in database", equipment.Id);

        return equipment;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<bool> SoftDeleteAsync(Guid id)
    {
        logger.LogDebug("Soft deleting equipment in database: {Id}", id);

        var equipment = await GetByIdAsync(id);
        if (equipment == null)
        {
            logger.LogWarning("Equipment not found for deletion: {Id}", id);
            return false;
        }

        equipment.DeletedAt = DateTime.UtcNow;
        equipment.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        logger.LogInformation("Equipment {Id} soft deleted in database", id);

        return true;
    }
}
