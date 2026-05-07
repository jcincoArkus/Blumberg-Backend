using Modules.Inventory.Dto;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Inventory.Service;

public interface ISiteZoneService
{
    Task<(IReadOnlyList<SiteZone> Items, int TotalCount)> GetAllAsync(PaginationRequest request, Guid? siteId);
    Task<SiteZone> GetByIdAsync(Guid id);
    Task<SiteZone> CreateAsync(SiteZoneRequest request);
    Task<SiteZone> UpdateAsync(Guid id, SiteZoneRequest request);
    Task DeleteAsync(Guid id);
}
