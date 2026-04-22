using Shared.Entity;
using Shared.Enums;

namespace Modules.Alerts.Repository;

/// <summary>
/// Repository for predefined recommended actions keyed by sensor type and severity.
/// </summary>
public interface IRecommendedActionRepository
{
    /// <summary>
    /// Gets active recommended actions for the given sensor type and severity, ordered by display order.
    /// </summary>
    Task<IReadOnlyList<RecommendedAction>> GetActiveBySensorTypeAndSeverityAsync(Guid sensorTypeId, AlertSeverity severity);
}
