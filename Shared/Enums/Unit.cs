namespace Shared.Enums;

/// <summary>
/// Unit of measurement for sensor readings.
/// Stored as string in database (VARCHAR 50); adding new values does not require a migration.
/// </summary>
public enum Unit
{
    /// <summary>Degrees Celsius.</summary>
    Celsius,

    /// <summary>Degrees Fahrenheit.</summary>
    Fahrenheit,

    /// <summary>Percentage.</summary>
    Percent,

    /// <summary>Parts per million.</summary>
    Ppm,

    /// <summary>Pounds per square inch.</summary>
    Psi,

    /// <summary>Bar (pressure).</summary>
    Bar,

    /// <summary>Kilowatts.</summary>
    Kw,

    /// <summary>Kilowatt-hours.</summary>
    Kwh,

    /// <summary>Custom or other unit.</summary>
    Custom
}
