using Modules.Admins.Dto;
using Modules.Auth.Repository;
using Shared.Entity;

namespace Modules.Admins.Service;

/// <summary>
/// Service implementation for admin operations
/// </summary>
public class AdminService(IAdminRepository adminRepository) : IAdminService
{
    private readonly IAdminRepository _adminRepository = adminRepository;

    /// <inheritdoc />
    public async Task<List<AdminResponse>> GetAllAsync()
    {
        var admins = await _adminRepository.GetAllAsync();
        
        return admins.Select(admin => new AdminResponse
        {
            Id = admin.Id,
            Email = admin.Email,
            FirstName = admin.FirstName,
            LastName = admin.LastName,
            CreatedAt = admin.CreatedAt,
            UpdatedAt = admin.UpdatedAt
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<AdminResponse> GetByIdAsync(Guid id)
    {
        var admin = await _adminRepository.GetByIdAsync(id);
        
        if (admin == null)
        {
            throw new KeyNotFoundException($"Admin with ID {id} was not found.");
        }
        
        return new AdminResponse
        {
            Id = admin.Id,
            Email = admin.Email,
            FirstName = admin.FirstName,
            LastName = admin.LastName,
            CreatedAt = admin.CreatedAt,
            UpdatedAt = admin.UpdatedAt
        };
    }

    /// <inheritdoc />
    public async Task<AdminResponse> CreateAsync(AdminRequest request)
    {
        // Check if email already exists
        var emailExists = await _adminRepository.ExistsAsync(request.Email);
        if (emailExists)
        {
            throw new InvalidOperationException($"Admin with email {request.Email} already exists.");
        }

        // Create new admin entity
        var newAdmin = new Admin
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow
        };

        // Save to database
        var createdAdmin = await _adminRepository.CreateAsync(newAdmin);

        // Map to response DTO
        return new AdminResponse
        {
            Id = createdAdmin.Id,
            Email = createdAdmin.Email,
            FirstName = createdAdmin.FirstName,
            LastName = createdAdmin.LastName,
            CreatedAt = createdAdmin.CreatedAt,
            UpdatedAt = createdAdmin.UpdatedAt
        };
    }

    /// <inheritdoc />
    public async Task<AdminResponse> UpdateAsync(Guid id, AdminRequest request)
    {
        var admin = await _adminRepository.GetByIdAsync(id);
        if (admin == null)
        {
            throw new KeyNotFoundException($"Admin with ID {id} was not found.");
        }

        // Update admin entity
        admin.Email = request.Email;
        admin.FirstName = request.FirstName;
        admin.LastName = request.LastName;
        admin.UpdatedAt = DateTime.UtcNow;

        // Save to database
        var updatedAdmin = await _adminRepository.UpdateAsync(admin);

        // Map to response DTO
        return new AdminResponse
        {
            Id = updatedAdmin.Id,
            Email = updatedAdmin.Email,
            FirstName = updatedAdmin.FirstName,
            LastName = updatedAdmin.LastName,
            CreatedAt = updatedAdmin.CreatedAt,
            UpdatedAt = updatedAdmin.UpdatedAt
        };
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id)
    {
        var admin = await _adminRepository.GetByIdAsync(id);
        if (admin == null)
        {
            throw new KeyNotFoundException($"Admin with ID {id} was not found.");
        }

        // Soft delete admin entity
        await _adminRepository.DeleteAsync(admin);
    }

}
