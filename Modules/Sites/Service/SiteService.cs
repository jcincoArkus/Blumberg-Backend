using Modules.Sites.Dto;
using Modules.Sites.Repository;
using Shared.Entity;

namespace Modules.Sites.Service;

/// <summary>
/// Service implementation for site operations
/// </summary>
public class SiteService(ISiteRepository siteRepository) : ISiteService
{
    private readonly ISiteRepository _siteRepository = siteRepository;

    /// <inheritdoc />
    public async Task<List<SiteResponse>> GetAllAsync()
    {
        var sites = await _siteRepository.GetAllAsync();
        return sites.Select(MapToResponse).ToList();
    }

    /// <inheritdoc />
    public async Task<SiteResponse> GetByIdAsync(Guid id)
    {
        var site = await _siteRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Site with ID {id} was not found");

        return MapToResponse(site);
    }

    /// <inheritdoc />
    public async Task<SiteResponse> CreateAsync(SiteRequest request)
    {
        var site = new Site
        {
            Name = request.Name,
            Address = request.Address,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country
        };

        var createdSite = await _siteRepository.CreateAsync(site);
        return MapToResponse(createdSite);
    }

    /// <inheritdoc />
    public async Task<SiteResponse> UpdateAsync(Guid id, SiteRequest request)
    {
        var site = await _siteRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Site with ID {id} was not found");

        site.Name = request.Name;
        site.Address = request.Address;
        site.City = request.City;
        site.State = request.State;
        site.PostalCode = request.PostalCode;
        site.Country = request.Country;

        var updatedSite = await _siteRepository.UpdateAsync(site);
        return MapToResponse(updatedSite);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id)
    {
        var deleted = await _siteRepository.SoftDeleteAsync(id);
        if (!deleted)
            throw new KeyNotFoundException($"Site with ID {id} was not found");
    }

    /// <summary>
    /// Maps a Site entity to a SiteResponse DTO
    /// </summary>
    private static SiteResponse MapToResponse(Site site)
    {
        return new SiteResponse
        {
            Id = site.Id,
            Name = site.Name,
            Address = site.Address,
            City = site.City,
            State = site.State,
            PostalCode = site.PostalCode,
            Country = site.Country,
            OrganizationId = site.OrganizationId,
            OrganizationName = site.Organization?.Name ?? string.Empty,
            CreatedAt = site.CreatedAt,
            UpdatedAt = site.UpdatedAt
        };
    }
}