using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Entity;
using Shared.Enums;

namespace Modules.Alerts.Repository;

/// <summary>
/// Repository implementation for recommended actions (global config, no tenant).
/// </summary>
public class RecommendedActionRepository(ApplicationDbContext context, ILogger<RecommendedActionRepository> logger) : IRecommendedActionRepository
{
    /// <inheritdoc />
    [Span]
    public virtual async Task<IReadOnlyList<RecommendedAction>> GetActiveBySensorTypeAndSeverityAsync(Guid sensorTypeId, AlertSeverity severity)
    {
        var list = await context.RecommendedActions
            .Where(e => e.SensorTypeId == sensorTypeId && e.Severity == severity && e.IsActive)
            .OrderBy(e => e.DisplayOrder)
            .ToListAsync();
        logger.LogDebug("Retrieved {Count} recommended actions for sensor type {SensorTypeId} severity {Severity}", list.Count, sensorTypeId, severity);
        return list;
    }
}
