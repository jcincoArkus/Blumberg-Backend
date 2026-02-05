using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
using Modules.Sensors.Dto;
using Modules.Sensors.Repository;

namespace Modules.Sensors.Service;

/// <summary>
/// Service implementation for sensor operations
/// </summary>
public class SensorService(ISensorRepository sensorRepository, ISensorReadingRepository sensorReadingRepository, ILogger<SensorService> logger) : ISensorService
{
    /// <inheritdoc />
    [Span]
    public virtual async Task<List<SensorResponse>> GetAllAsync()
    {
        logger.LogDebug("Getting all sensors from repository");

        var sensors = await sensorRepository.GetAllAsync();

        logger.LogInformation("Retrieved {Count} sensors", sensors.Count);

        return sensors.Select(MapToResponse).ToList();
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<SensorResponse> GetByIdAsync(Guid id)
    {
        logger.LogDebug("Getting sensor by ID: {Id}", id);

        var sensor = await sensorRepository.GetByIdAsync(id);

        if (sensor == null)
        {
            logger.LogWarning("Sensor not found with ID: {Id}", id);
            throw new KeyNotFoundException($"Sensor with ID {id} was not found");
        }

        logger.LogInformation("Retrieved sensor {Id}", id);

        return MapToResponse(sensor);
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<SensorResponse> CreateAsync(SensorRequest request)
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

        return MapToResponse(createdSensor);
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<SensorResponse> UpdateAsync(Guid id, SensorRequest request)
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

        return MapToResponse(updatedSensor);
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
    public virtual async Task<PagedResponse<SensorReadingResponse>> GetReadingsAsync(
        Guid sensorId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize)
    {
        logger.LogDebug("Getting readings for sensor {SensorId}, page {Page}, pageSize {PageSize}", sensorId, page, pageSize);

        var sensor = await sensorRepository.GetByIdAsync(sensorId);

        if (sensor == null)
        {
            logger.LogWarning("Sensor not found with ID: {SensorId}", sensorId);
            throw new KeyNotFoundException($"Sensor with ID {sensorId} was not found");
        }

        var (items, totalCount) = await sensorReadingRepository.GetBySensorIdAsync(sensorId, fromUtc, toUtc, page, pageSize);

        logger.LogInformation("Retrieved {Count} readings for sensor {SensorId}", items.Count, sensorId);

        return new PagedResponse<SensorReadingResponse>
        {
            Items = items.Select(MapReadingToResponse).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Maps a SensorReading entity to a SensorReadingResponse DTO
    /// </summary>
    private static SensorReadingResponse MapReadingToResponse(Shared.Entity.SensorReading reading)
    {
        return new SensorReadingResponse
        {
            Id = reading.Id,
            SensorId = reading.SensorId,
            Value = reading.Value,
            TimestampUtc = reading.TimestampUtc,
            Unit = reading.Unit,
            OrganizationId = reading.OrganizationId,
            IngestionRunId = reading.IngestionRunId,
            CreatedAt = reading.CreatedAt
        };
    }

    /// <summary>
    /// Maps a Sensor entity to a SensorResponse DTO
    /// </summary>
    private static SensorResponse MapToResponse(Shared.Entity.Sensor sensor)
    {
        return new SensorResponse
        {
            Id = sensor.Id,
            Serial = sensor.Serial,
            Status = sensor.Status,
            OrganizationId = sensor.OrganizationId,
            OrganizationName = sensor.Organization?.Name ?? string.Empty,
            EquipmentId = sensor.EquipmentId,
            EquipmentName = sensor.Equipment?.Name ?? string.Empty,
            SensorTypeId = sensor.SensorTypeId,
            SensorTypeName = sensor.SensorType?.Type.ToString() ?? string.Empty,
            ThresholdId = sensor.ThresholdId,
            CreatedAt = sensor.CreatedAt,
            UpdatedAt = sensor.UpdatedAt
        };
    }
}
