using Shared.Dto;

namespace Modules.Inventory.Dto;

/// <summary>Filter / pagination parameters for lots list</summary>
public class GetLotsRequest : PaginationRequest
{
    /// <summary>Filter by product</summary>
    public Guid? ProductId { get; set; }

    /// <summary>Filter by inventory site</summary>
    public Guid? SiteId { get; set; }

    /// <summary>Only return lots expiring before this date</summary>
    public DateTime? ExpiringBefore { get; set; }
}
