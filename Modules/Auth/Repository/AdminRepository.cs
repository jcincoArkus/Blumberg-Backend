using Adapters.Database;
using Microsoft.EntityFrameworkCore;
using Shared.Entity;

namespace Modules.Auth.Repository;

/// <summary>
/// Repository implementation for Admin entity operations
/// </summary>
public class AdminRepository(ApplicationDbContext context) : IAdminRepository
{
    private readonly ApplicationDbContext _context = context;

    /// <inheritdoc />
    public async Task<Admin?> GetByIdAsync(Guid id)
    {
        return await _context.Admins
            .FirstOrDefaultAsync(a => a.Id == id && a.DeletedAt == null);
    }

    /// <inheritdoc />
    public async Task<Admin?> GetByEmailAsync(string email)
    {
        return await _context.Admins
            .FirstOrDefaultAsync(a => a.Email == email && a.DeletedAt == null);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string email)
    {
        return await _context.Admins
            .AnyAsync(a => a.Email == email && a.DeletedAt == null);
    }

    /// <inheritdoc />
    public async Task<List<Admin>> GetAllAsync()
    {
        return await _context.Admins
            .Where(a => a.DeletedAt == null)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Admin> CreateAsync(Admin admin)
    {
        _context.Admins.Add(admin);
        await _context.SaveChangesAsync();
        return admin;
    }

    /// <inheritdoc />
    public async Task<Admin> UpdateAsync(Admin admin)
    {
        _context.Admins.Update(admin);
        await _context.SaveChangesAsync();
        return admin;
    }

    /// <inheritdoc />
    public async Task<Admin> DeleteAsync(Admin admin)
    {
        // Soft delete: set DeletedAt timestamp
        admin.DeletedAt = DateTime.UtcNow;
        admin.UpdatedAt = DateTime.UtcNow;
        
        _context.Admins.Update(admin);
        await _context.SaveChangesAsync();
        return admin;
    }
}

