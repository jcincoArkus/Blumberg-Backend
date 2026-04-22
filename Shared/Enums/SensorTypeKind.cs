namespace Shared.Enums;

/// <summary>
/// Kind/category of sensor (e.g. temperature, humidity).
/// Stored as string in database (VARCHAR 50); adding new values does not require a migration.
/// </summary>
/// <remarks>Named SensorTypeKind to avoid conflict with <see cref="Shared.Entity.SensorType"/> entity.</remarks>
public enum SensorTypeKind
{
    /// <summary>Temperature sensor.</summary>
    Temperature,

    /// <summary>Humidity sensor.</summary>
    Humidity,

    /// <summary>CO2 sensor.</summary>
    Co2,

    /// <summary>O2 sensor.</summary>
    O2,

    /// <summary>Pressure sensor.</summary>
    Pressure,

    /// <summary>Energy sensor.</summary>
    Energy,

    /// <summary>Custom or other sensor type.</summary>
    Custom
}
