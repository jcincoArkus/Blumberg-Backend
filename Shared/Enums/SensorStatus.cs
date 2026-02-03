namespace Shared.Enums;

/// <summary>
/// Operational status of a sensor.
/// Stored as string in database (VARCHAR 50); adding new values does not require a migration.
/// </summary>
public enum SensorStatus
{
    /// <summary>Sensor is active and reporting.</summary>
    Active,

    /// <summary>Sensor is inactive (e.g. disabled).</summary>
    Inactive,

    /// <summary>Sensor is under maintenance.</summary>
    Maintenance,

    /// <summary>Status is unknown.</summary>
    Unknown
}
