using Modules.Alerts.Dto;
using Shared.Entity;

namespace Modules.Alerts.Service;

/// <summary>
/// Service interface for alert business operations
/// </summary>
public interface IAlertService
{
    /// <summary>
    /// Gets paginated alerts with optional filters
    /// </summary>
    Task<(IReadOnlyList<Alert> Items, int TotalCount)> GetAllAsync(GetAlertsRequest request);

    /// <summary>
    /// Gets an alert by ID, throws KeyNotFoundException if not found
    /// </summary>
    Task<Alert> GetByIdAsync(Guid id);

    /// <summary>
    /// Resolves an active alert, throws KeyNotFoundException if not found or InvalidOperationException if not active
    /// </summary>
    Task<Alert> ResolveAsync(Guid id);
}
