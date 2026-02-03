namespace Shared.Enums;

/// <summary>
/// Health status of a sensor (e.g. healthy, warning, offline).
/// Stored as string in database (VARCHAR 50); adding new values does not require a migration.
/// </summary>
public enum SensorHealthStatus
{
    /// <summary>Sensor is healthy and responding.</summary>
    Healthy,

    /// <summary>Sensor has warnings (e.g. drift, intermittent).</summary>
    Warning,

    /// <summary>Sensor is in critical state.</summary>
    Critical,

    /// <summary>Sensor data is stale (no recent readings).</summary>
    Stale,

    /// <summary>Sensor is offline or unreachable.</summary>
    Offline
}
