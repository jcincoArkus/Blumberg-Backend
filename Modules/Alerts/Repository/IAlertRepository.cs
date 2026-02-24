using Modules.Alerts.Dto;
using Shared.Entity;

namespace Modules.Alerts.Repository;

/// <summary>
/// Repository interface for alert operations
/// </summary>
public interface IAlertRepository
{
    /// <summary>
    /// Gets paginated alerts with optional filters
    /// </summary>
    Task<(IReadOnlyList<Alert> Items, int TotalCount)> GetPagedAsync(GetAlertsRequest request);

    /// <summary>
    /// Gets an alert by ID
    /// </summary>
    Task<Alert?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets the active alert for a sensor, or null if none exists
    /// </summary>
    Task<Alert?> GetActiveBySensorIdAsync(Guid sensorId);

    /// <summary>
    /// Creates a new alert
    /// </summary>
    Task<Alert> CreateAsync(Alert entity);

    /// <summary>
    /// Updates an existing alert
    /// </summary>
    Task<Alert> UpdateAsync(Alert entity);
}
