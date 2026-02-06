using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
using Modules.Sensors.Dto;
using Modules.Sensors.Repository;
using Shared.Dto;

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
}
