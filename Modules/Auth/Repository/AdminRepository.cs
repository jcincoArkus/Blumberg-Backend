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
}

