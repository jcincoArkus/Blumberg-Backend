using Modules.Sensors.Dto;
using Modules.Sensors.Repository;

namespace Modules.Sensors.Service;

/// <summary>
/// Service implementation for sensor operations
/// </summary>
public class SensorService(ISensorRepository sensorRepository, ISensorReadingRepository sensorReadingRepository) : ISensorService
{
    private readonly ISensorRepository _sensorRepository = sensorRepository;
    private readonly ISensorReadingRepository _sensorReadingRepository = sensorReadingRepository;

    /// <inheritdoc />
    public async Task<List<SensorResponse>> GetAllAsync()
    {
        var sensors = await _sensorRepository.GetAllAsync();
        return sensors.Select(MapToResponse).ToList();
    }

    /// <inheritdoc />
    public async Task<SensorResponse> GetByIdAsync(Guid id)
    {
        var sensor = await _sensorRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Sensor with ID {id} was not found");

        return MapToResponse(sensor);
    }

    /// <inheritdoc />
    public async Task<SensorResponse> CreateAsync(SensorRequest request)
    {
        var sensor = new Shared.Entity.Sensor
        {
            Serial = request.Serial,
            Status = request.Status,
            EquipmentId = request.EquipmentId,
            SensorTypeId = request.SensorTypeId,
            ThresholdId = request.ThresholdId
        };

        var createdSensor = await _sensorRepository.CreateAsync(sensor);
        return MapToResponse(createdSensor);
    }

    /// <inheritdoc />
    public async Task<SensorResponse> UpdateAsync(Guid id, SensorRequest request)
    {
        var sensor = await _sensorRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Sensor with ID {id} was not found");

        sensor.Serial = request.Serial;
        sensor.Status = request.Status;
        sensor.EquipmentId = request.EquipmentId;
        sensor.SensorTypeId = request.SensorTypeId;
        sensor.ThresholdId = request.ThresholdId;

        var updatedSensor = await _sensorRepository.UpdateAsync(sensor);
        return MapToResponse(updatedSensor);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id)
    {
        var deleted = await _sensorRepository.SoftDeleteAsync(id);
        if (!deleted)
            throw new KeyNotFoundException($"Sensor with ID {id} was not found");
    }

    /// <inheritdoc />
    public async Task<PagedResponse<SensorReadingResponse>> GetReadingsAsync(
        Guid sensorId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize)
    {
        _ = await _sensorRepository.GetByIdAsync(sensorId)
            ?? throw new KeyNotFoundException($"Sensor with ID {sensorId} was not found");

        var (items, totalCount) = await _sensorReadingRepository.GetBySensorIdAsync(sensorId, fromUtc, toUtc, page, pageSize);

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
