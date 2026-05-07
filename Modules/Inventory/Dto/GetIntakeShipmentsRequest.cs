using Shared.Dto;

namespace Modules.Inventory.Dto;

/// <summary>Filter / pagination parameters for intake shipments list</summary>
public class GetIntakeShipmentsRequest : PaginationRequest
{
    /// <summary>Filter by supplier</summary>
    public Guid? SupplierId { get; set; }

    /// <summary>Filter by receiving site</summary>
    public Guid? SiteId { get; set; }

    /// <summary>Filter by status ("draft" | "received" | "cancelled")</summary>
    public string? Status { get; set; }
}
