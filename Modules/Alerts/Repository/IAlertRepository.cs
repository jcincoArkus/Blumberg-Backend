using Modules.Alerts.Dto;
using Shared.Entity;

namespace Modules.Alerts.Repository;

/// <summary>
/// Repository interface for alert operations
/// </summary>
public interface IAlertRepository
{
    /// <summary>
    /// Gets paginated alert responses (with equipment name and sensor serial) for API listing
    /// </summary>
    Task<(IReadOnlyList<AlertResponse> Items, int TotalCount)> GetPagedResponsesAsync(GetAlertsRequest request);

    /// <summary>
    /// Gets paginated alerts with optional filters (entity form for internal use)
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
    /// Gets an unresolved alert (Active or Acknowledged) for a sensor, or null if none exists.
    /// Used to enforce: do not create a new alert until the previous one is Resolved.
    /// </summary>
    Task<Alert?> GetUnresolvedBySensorIdAsync(Guid sensorId);

    /// <summary>
    /// Gets all active (unresolved) alerts ordered by severity for the dashboard
    /// </summary>
    Task<IReadOnlyList<ActiveAlertResponse>> GetActiveAsync();

    /// <summary>
    /// Creates a new alert (and its initial Triggered event) and saves. Preferred way to create a single alert.
    /// </summary>
    Task<Alert> CreateAsync(Alert entity);

    /// <summary>
    /// Adds alerts and their initial Triggered events to the context without saving.
    /// Use when another unit of work (e.g. IngestionRunRepository) will call SaveChanges in the same transaction.
    /// </summary>
    void AddRangeWithTriggeredEvents(IEnumerable<Alert> entities);

    /// <summary>
    /// Updates an existing alert
    /// </summary>
    Task<Alert> UpdateAsync(Alert entity);

    /// <summary>
    /// Appends a lifecycle event for an alert (e.g. Acknowledged, Resolved)
    /// </summary>
    Task AddEventAsync(AlertEvent entity);
}
