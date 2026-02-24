using System.Collections.Frozen;
using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
using Modules.Sensors.Dto;
using Modules.Sensors.Repository;
using Shared.Dto;
using Shared.Enums;

namespace Modules.Sensors.Service;

/// <summary>
/// Service implementation for sensor operations
/// </summary>
public class SensorService(ISensorRepository sensorRepository, ISensorReadingRepository sensorReadingRepository, ILogger<SensorService> logger) : ISensorService
{
    /// <inheritdoc />
    [Span]
    public virtual async Task<(IReadOnlyList<Shared.Entity.Sensor> Items, int TotalCount)> GetAllAsync(PaginationRequest request)
    {
        logger.LogDebug("Getting sensors page {Page}, pageSize {PageSize}", request.Page, request.PageSize);

        var result = await sensorRepository.GetPagedAsync(request);

        logger.LogInformation("Retrieved {Count} sensors (total: {TotalCount})", result.Items.Count, result.TotalCount);

        return result;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Shared.Entity.Sensor> GetByIdAsync(Guid id)
    {
        logger.LogDebug("Getting sensor by ID: {Id}", id);

        var sensor = await sensorRepository.GetByIdAsync(id);

        if (sensor == null)
        {
            logger.LogWarning("Sensor not found with ID: {Id}", id);
            throw new KeyNotFoundException($"Sensor with ID {id} was not found");
        }

        logger.LogInformation("Retrieved sensor {Id}", id);

        return sensor;
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<Shared.Entity.Sensor> CreateAsync(SensorRequest request)
    {
        logger.LogDebug("Creating new sensor: {Serial}", request.Serial);

        var sensor = new Shared.Entity.Sensor
        {
            Serial = request.Serial,
            Status = request.Status,
            EquipmentId = request.EquipmentId,
            SensorTypeId = request.SensorTypeId,
            ThresholdId = request.ThresholdId
        };

        var createdSensor = await sensorRepository.CreateAsync(sensor);

        logger.LogInformation("Sensor created successfully with ID: {Id}", createdSensor.Id);

        return createdSensor;
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<Shared.Entity.Sensor> UpdateAsync(Guid id, SensorRequest request)
    {
        logger.LogDebug("Updating sensor {Id}", id);

        var sensor = await sensorRepository.GetByIdAsync(id);

        if (sensor == null)
        {
            logger.LogWarning("Sensor not found with ID: {Id}", id);
            throw new KeyNotFoundException($"Sensor with ID {id} was not found");
        }

        sensor.Serial = request.Serial;
        sensor.Status = request.Status;
        sensor.EquipmentId = request.EquipmentId;
        sensor.SensorTypeId = request.SensorTypeId;
        sensor.ThresholdId = request.ThresholdId;

        var updatedSensor = await sensorRepository.UpdateAsync(sensor);

        logger.LogInformation("Sensor {Id} updated successfully", id);

        return updatedSensor;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task DeleteAsync(Guid id)
    {
        logger.LogDebug("Deleting sensor {Id}", id);

        var deleted = await sensorRepository.SoftDeleteAsync(id);

        if (!deleted)
        {
            logger.LogWarning("Sensor not found with ID: {Id}", id);
            throw new KeyNotFoundException($"Sensor with ID {id} was not found");
        }

        logger.LogInformation("Sensor {Id} deleted successfully", id);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<(IReadOnlyList<Shared.Entity.SensorReading> Items, int TotalCount)> GetReadingsAsync(
        Guid sensorId,
        GetSensorReadingsRequest request)
    {
        logger.LogDebug("Getting readings for sensor {SensorId}, page {Page}, pageSize {PageSize}", sensorId, request.Page, request.PageSize);

        var sensor = await sensorRepository.GetByIdAsync(sensorId);

        if (sensor == null)
        {
            logger.LogWarning("Sensor not found with ID: {SensorId}", sensorId);
            throw new KeyNotFoundException($"Sensor with ID {sensorId} was not found");
        }

        var result = await sensorReadingRepository.GetBySensorIdAsync(sensorId, request);

        logger.LogInformation("Retrieved {Count} readings for sensor {SensorId}", result.Items.Count, sensorId);

        return result;
    }

    // Global expected reporting interval (5 min). Freshness: Fresh ≤ 1×, Stale > 2×, Offline > 5×.
    private const int ExpectedIntervalSeconds = 300;
    private static readonly int StaleThresholdSeconds = 2 * ExpectedIntervalSeconds;   // 2× expected
    private static readonly int OfflineThresholdSeconds = 5 * ExpectedIntervalSeconds; // 5× expected (configurable)
    private const int ReliabilityWindowHours = 24;
    private const int ExpectedReadingsPerHour = 12;    // 300s interval = 12 readings/hour

    /// <inheritdoc />
    [Span]
    public virtual async Task<(IReadOnlyList<SensorHealthListResult> Items, int TotalCount)> GetHealthListAsync(GetSensorHealthRequest request)
    {
        var sensors = await sensorRepository.GetForHealthListAsync(request);
        if (sensors.Count == 0)
            return ([], 0);

        var sensorIds = sensors.Select(s => s.Id).ToList();
        var latestReadings = await sensorReadingRepository.GetLatestBySensorIdsAsync(sensorIds);
        var readingBySensor = latestReadings.ToFrozenDictionary(r => r.SensorId);

        var windowEnd = DateTime.UtcNow;
        var windowStart = windowEnd.AddHours(-ReliabilityWindowHours);
        var countsBySensor = await sensorReadingRepository.GetReadingCountsBySensorIdsInWindowAsync(sensorIds, windowStart, windowEnd);
        var expectedPerSensor = ReliabilityWindowHours * ExpectedReadingsPerHour;

        var list = new List<SensorHealthListResult>();
        foreach (var sensor in sensors)
        {
            readingBySensor.TryGetValue(sensor.Id, out var reading);
            var lastSeenAt = sensor.LastSeenAt ?? reading?.TimestampUtc;
            countsBySensor.TryGetValue(sensor.Id, out var received);
            var reliability = expectedPerSensor > 0
                ? Math.Round(Math.Min(100.0, (received / (double)expectedPerSensor) * 100.0), 1)
                : 100.0;
            var healthStatus = ComputeHealthStatus(sensor, reading, reliability);
            if (request.HealthStatus.HasValue && healthStatus != request.HealthStatus.Value)
                continue;

            list.Add(new SensorHealthListResult
            {
                Id = sensor.Id,
                Name = sensor.Serial,
                SensorType = sensor.SensorType?.Type.ToString() ?? string.Empty,
                HealthStatus = healthStatus,
                LastSeenAt = lastSeenAt,
                ReliabilityScore = reliability,
                SiteId = sensor.Equipment?.SiteId ?? Guid.Empty,
                SiteName = sensor.Equipment?.Site?.Name ?? string.Empty,
                EquipmentId = sensor.EquipmentId,
                EquipmentName = sensor.Equipment?.Name ?? string.Empty,
                LastValue = reading != null ? (double)reading.Value : null,
                Unit = reading != null ? reading.Unit.ToString() : (sensor.SensorType != null ? sensor.SensorType.Unit.ToString() : string.Empty),
                IngestionSource = reading?.IngestionRun?.Source
            });
        }

        var totalCount = list.Count;
        var skip = (request.Page - 1) * request.PageSize;
        var items = list.Skip(skip).Take(request.PageSize).ToList();

        logger.LogInformation("Health list: {Count} items (total: {TotalCount})", items.Count, totalCount);
        return (items, totalCount);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<SensorHealthDetailResult> GetHealthDetailAsync(Guid id)
    {
        var sensor = await sensorRepository.GetByIdAsync(id);
        if (sensor == null)
            throw new KeyNotFoundException($"Sensor with ID {id} was not found");

        var latestList = await sensorReadingRepository.GetLatestBySensorIdsAsync([id]);
        var reading = latestList.FirstOrDefault();

        var now = DateTime.UtcNow;
        var lastSeenAt = sensor.LastSeenAt ?? reading?.TimestampUtc;
        var freshnessSeconds = lastSeenAt.HasValue ? (now - lastSeenAt.Value).TotalSeconds : (double?)null;

        var windowEnd = now;
        var windowStart = now.AddHours(-ReliabilityWindowHours);
        var receivedPoints = await sensorReadingRepository.CountBySensorIdInWindowAsync(id, windowStart, windowEnd);
        var expectedPoints = ReliabilityWindowHours * ExpectedReadingsPerHour;
        var reliabilityScore = expectedPoints > 0
            ? Math.Min(100.0, (receivedPoints / (double)expectedPoints) * 100.0)
            : 100.0;
        var healthStatus = ComputeHealthStatus(sensor, reading, reliabilityScore);

        return new SensorHealthDetailResult
        {
            SensorId = sensor.Id,
            Name = sensor.Serial,
            HealthStatus = healthStatus,
            LastSeenAt = lastSeenAt,
            ReliabilityScore = Math.Round(reliabilityScore, 1),
            LastValue = reading != null ? (double)reading.Value : null,
            Unit = reading != null ? reading.Unit.ToString() : (sensor.SensorType != null ? sensor.SensorType.Unit.ToString() : string.Empty),
            FreshnessSeconds = freshnessSeconds.HasValue ? Math.Round(freshnessSeconds.Value, 1) : null,
            RecentReadingsCount = receivedPoints,
            ExpectedPoints = expectedPoints,
            ReceivedPoints = receivedPoints
        };
    }

    /// <summary>
    /// Computes health from freshness (age vs expected interval) and reliability (received vs expected in 24h).
    /// Fresh: age ≤ 1× expected; Stale: age &gt; 2×; Offline: no reading or age &gt; 5×.
    /// When Fresh: Healthy if reliability &gt; 90%, Warning if 70–90%, Critical if &lt; 70%.
    /// </summary>
    private static SensorHealthStatus ComputeHealthStatus(
        Shared.Entity.Sensor sensor,
        Shared.Entity.SensorReading? latestReading,
        double reliabilityScore)
    {
        if (sensor.Status == SensorStatus.Inactive)
            return SensorHealthStatus.Offline;

        if (latestReading == null)
            return SensorHealthStatus.Offline;

        var ageSeconds = (DateTime.UtcNow - latestReading.TimestampUtc).TotalSeconds;
        if (ageSeconds > OfflineThresholdSeconds)
            return SensorHealthStatus.Offline;
        if (ageSeconds > StaleThresholdSeconds)
            return SensorHealthStatus.Stale;

        // Fresh: derive from reliability
        if (reliabilityScore > 90.0)
            return SensorHealthStatus.Healthy;
        if (reliabilityScore >= 70.0)
            return SensorHealthStatus.Warning;
        return SensorHealthStatus.Critical;
    }

}
