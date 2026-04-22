using Modules.Alerts.Dto;
using Shared.Entity;

namespace Modules.Alerts.Service;

/// <summary>
/// Service interface for alert business operations
/// </summary>
public interface IAlertService
{
    /// <summary>
    /// Gets paginated alert responses (with display names) for API listing
    /// </summary>
    Task<(IReadOnlyList<AlertResponse> Items, int TotalCount)> GetAllAsync(GetAlertsRequest request);

    /// <summary>
    /// Gets all active (unresolved) alerts ordered by severity for the dashboard
    /// </summary>
    Task<IReadOnlyList<ActiveAlertResponse>> GetActiveAsync();

    /// <summary>
    /// Gets an alert by ID, throws KeyNotFoundException if not found
    /// </summary>
    Task<Alert> GetByIdAsync(Guid id);

    /// <summary>
    /// Acknowledges an active alert, throws KeyNotFoundException if not found or InvalidOperationException if not active
    /// </summary>
    Task<Alert> AcknowledgeAsync(Guid id);

    /// <summary>
    /// Resolves an active or acknowledged alert, throws KeyNotFoundException if not found or InvalidOperationException if not active/acknowledged
    /// </summary>
    Task<Alert> ResolveAsync(Guid id);
}
