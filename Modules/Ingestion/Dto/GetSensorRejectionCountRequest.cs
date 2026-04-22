namespace Modules.Ingestion.Dto;

/// <summary>
/// Query parameters for GET /api/v1/ingestion/sensors/{sensorId}/rejection-count
/// </summary>
public class GetSensorRejectionCountRequest
{
    /// <summary>Start of time range (run CreatedAt), UTC inclusive</summary>
    /// <example>2025-02-22T00:00:00Z</example>
    public DateTime? From { get; set; }

    /// <summary>End of time range (run CreatedAt), UTC inclusive</summary>
    /// <example>2025-02-23T23:59:59Z</example>
    public DateTime? To { get; set; }
}
