using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
using Modules.Alerts.Dto;
using Modules.Alerts.Repository;
using Shared.Entity;
using Shared.Enums;

namespace Modules.Alerts.Service;

/// <summary>
/// Service implementation for alert business operations
/// </summary>
public class AlertService(
    IAlertRepository alertRepository,
    ILogger<AlertService> logger) : IAlertService
{
    /// <inheritdoc />
    [Span]
    public virtual async Task<(IReadOnlyList<Alert> Items, int TotalCount)> GetAllAsync(GetAlertsRequest request)
    {
        logger.LogDebug("Getting alerts");
        return await alertRepository.GetPagedAsync(request);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Alert> GetByIdAsync(Guid id)
    {
        logger.LogDebug("Getting alert {Id}", id);

        var alert = await alertRepository.GetByIdAsync(id);
        if (alert == null)
        {
            logger.LogWarning("Alert not found: {Id}", id);
            throw new KeyNotFoundException($"Alert with ID {id} was not found");
        }

        return alert;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Alert> AcknowledgeAsync(Guid id)
    {
        logger.LogDebug("Acknowledging alert {Id}", id);

        var alert = await alertRepository.GetByIdAsync(id);
        if (alert == null)
        {
            logger.LogWarning("Alert not found: {Id}", id);
            throw new KeyNotFoundException($"Alert with ID {id} was not found");
        }

        if (alert.Status != AlertStatus.Active)
            throw new InvalidOperationException($"Alert {id} is not active and cannot be acknowledged");

        alert.Status = AlertStatus.Acknowledged;

        return await alertRepository.UpdateAsync(alert);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Alert> ResolveAsync(Guid id)
    {
        logger.LogDebug("Resolving alert {Id}", id);

        var alert = await alertRepository.GetByIdAsync(id);
        if (alert == null)
        {
            logger.LogWarning("Alert not found: {Id}", id);
            throw new KeyNotFoundException($"Alert with ID {id} was not found");
        }

        if (alert.Status != AlertStatus.Active && alert.Status != AlertStatus.Acknowledged)
            throw new InvalidOperationException($"Alert {id} is not active or acknowledged and cannot be resolved");

        alert.Status = AlertStatus.Resolved;
        alert.ResolvedAt = DateTime.UtcNow;

        return await alertRepository.UpdateAsync(alert);
    }
}
