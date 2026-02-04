using Adapters.Database;
using Microsoft.EntityFrameworkCore;
using Shared.Entity;

namespace Modules.Equipment.Repository;

/// <summary>
/// Repository implementation for Equipment entity operations
/// </summary>
public class EquipmentRepository(ApplicationDbContext context) : IEquipmentRepository
{
    private readonly ApplicationDbContext _context = context;

    /// <inheritdoc />
    public async Task<List<Shared.Entity.Equipment>> GetAllAsync()
    {
        return await _context.Equipment
            .Where(e => e.DeletedAt == null)
            .Include(e => e.Organization)
            .Include(e => e.Site)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Shared.Entity.Equipment?> GetByIdAsync(Guid id)
    {
        return await _context.Equipment
            .Include(e => e.Organization)
            .Include(e => e.Site)
            .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null);
    }

    /// <inheritdoc />
    public async Task<Shared.Entity.Equipment> CreateAsync(Shared.Entity.Equipment equipment)
    {
        equipment.Id = Guid.NewGuid();
        equipment.CreatedAt = DateTime.UtcNow;
        equipment.UpdatedAt = null;
        equipment.DeletedAt = null;

        _context.Equipment.Add(equipment);
        await _context.SaveChangesAsync();

        return equipment;
    }

    /// <inheritdoc />
    public async Task<Shared.Entity.Equipment> UpdateAsync(Shared.Entity.Equipment equipment)
    {
        equipment.UpdatedAt = DateTime.UtcNow;

        _context.Equipment.Update(equipment);
        await _context.SaveChangesAsync();

        return equipment;
    }

    /// <inheritdoc />
    public async Task<bool> SoftDeleteAsync(Guid id)
    {
        var equipment = await GetByIdAsync(id);
        if (equipment == null)
            return false;

        equipment.DeletedAt = DateTime.UtcNow;
        equipment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
