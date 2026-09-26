using System.ComponentModel.DataAnnotations;

namespace Modules.Inventory.Dto;

/// <summary>Create / update an intake shipment header</summary>
public class IntakeShipmentRequest
{
    /// <summary>Purchase-order reference (unique)</summary>
    /// <example>PO-2025-001</example>
    [Required]
    [StringLength(100)]
    public string PoReference { get; set; } = string.Empty;

    /// <summary>Supplier ID</summary>
    [Required]
    public Guid SupplierId { get; set; }

    /// <summary>Vehicle description or plate</summary>
    [StringLength(200)]
    public string? Vehicle { get; set; }

    /// <summary>Driver name</summary>
    [StringLength(200)]
    public string? Driver { get; set; }

    /// <summary>Receiving site ID</summary>
    [Required]
    public Guid SiteId { get; set; }

    /// <summary>Zone where goods were unloaded</summary>
    [Required]
    [StringLength(100)]
    public string ReceivingZone { get; set; } = string.Empty;

    /// <summary>Cold-chain temperature on arrival (°C)</summary>
    public decimal? ColdChainTempC { get; set; }

    /// <summary>Arrival timestamp (UTC)</summary>
    [Required]
    public DateTime ArrivedAt { get; set; }

    /// <summary>Staff member who received the shipment</summary>
    [Required]
    [StringLength(200)]
    public string ReceivedBy { get; set; } = string.Empty;

    /// <summary>Status: "draft" | "received" | "cancelled"</summary>
    /// <example>draft</example>
    [Required]
    [RegularExpression("^(draft|received|cancelled)$",
        ErrorMessage = "Status must be 'draft', 'received', or 'cancelled'")]
    public string Status { get; set; } = "draft";
}
