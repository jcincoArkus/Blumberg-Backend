namespace Modules.Ingestion.Dto;

/// <summary>
/// Response for GET /api/v1/ingestion/sensors/{sensorId}/rejection-count
/// </summary>
public class SensorRejectionCountResponse
{
    /// <summary>Sensor ID the count is for</summary>
    public Guid SensorId { get; set; }

    /// <summary>Number of rejected readings for this sensor in the requested time range</summary>
    /// <example>3</example>
    public int Count { get; set; }
}
