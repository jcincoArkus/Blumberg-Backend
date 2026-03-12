using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Entity;
using Shared.Enums;

namespace Adapters.Database.Seeders;

/// <summary>
/// Seeds predefined recommended actions for all sensor types (Temperature, Humidity, Pressure, Energy, Co2, O2)
/// at Critical and Warning severity. Idempotent: skips when a matching action (same sensor type, severity, title)
/// already exists. Runs after SensorTypeSeeder so sensor type IDs exist.
/// </summary>
public class RecommendedActionSeeder(ILoggerFactory loggerFactory) : ISeeder
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<RecommendedActionSeeder>();

    /// <summary>Run after SensorTypeSeeder (Order -2) so sensor types exist. Order 5 keeps it unique and after all org/site/equipment/sensor seeders.</summary>
    public int Order => 5;

    private sealed record ActionRow(SensorTypeKind SensorKind, AlertSeverity Severity, string Title, string Description, int DisplayOrder);

    private static readonly ActionRow[] Rows =
    [
        // Temperature – Critical
        new(SensorTypeKind.Temperature, AlertSeverity.Critical, "Check equipment immediately", "Inspect the unit for failures, refrigerant leaks, or blocked airflow. Contact maintenance if needed.", 1),
        new(SensorTypeKind.Temperature, AlertSeverity.Critical, "Verify setpoints and alarms", "Confirm thermostat and BMS setpoints and that alarms are enabled.", 2),
        // Temperature – Warning
        new(SensorTypeKind.Temperature, AlertSeverity.Warning, "Monitor trend and adjust setpoints", "Review recent readings and adjust setpoints if acceptable for the process.", 1),
        new(SensorTypeKind.Temperature, AlertSeverity.Warning, "Inspect filters and coils", "Check filters and coils for dirt or blockage that could affect temperature.", 2),
        // Humidity – Critical
        new(SensorTypeKind.Humidity, AlertSeverity.Critical, "Check dehumidifier or HVAC", "Verify dehumidifier/HVAC is running and that drains are not blocked.", 1),
        new(SensorTypeKind.Humidity, AlertSeverity.Critical, "Inspect for water intrusion", "Look for leaks, condensation, or standing water in the area.", 2),
        // Humidity – Warning
        new(SensorTypeKind.Humidity, AlertSeverity.Warning, "Review humidity setpoints", "Confirm setpoints and consider temporary adjustment if within safe range.", 1),
        new(SensorTypeKind.Humidity, AlertSeverity.Warning, "Check ventilation", "Ensure adequate ventilation and that dampers or fans are operating.", 2),
        // Pressure – Critical
        new(SensorTypeKind.Pressure, AlertSeverity.Critical, "Verify pressure relief and valves", "Ensure relief valves and regulators are set correctly and not stuck.", 1),
        new(SensorTypeKind.Pressure, AlertSeverity.Critical, "Check for leaks or blockages", "Inspect lines and fittings for leaks; check for blockages in the circuit.", 2),
        // Pressure – Warning
        new(SensorTypeKind.Pressure, AlertSeverity.Warning, "Monitor trend and compare to normal", "Compare to typical operating range and adjust setpoints if appropriate.", 1),
        new(SensorTypeKind.Pressure, AlertSeverity.Warning, "Inspect filters and strainers", "Clean or replace filters and strainers if pressure drop is suspected.", 2),
        // Energy – Critical
        new(SensorTypeKind.Energy, AlertSeverity.Critical, "Check for overload or fault", "Verify breakers, fuses, and load; isolate fault if present. Contact facilities if needed.", 1),
        new(SensorTypeKind.Energy, AlertSeverity.Critical, "Verify meters and CTs", "Confirm metering and current transformers are connected and reporting correctly.", 2),
        // Energy – Warning
        new(SensorTypeKind.Energy, AlertSeverity.Warning, "Review load and schedule", "Compare to typical usage and adjust schedules or setpoints if appropriate.", 1),
        new(SensorTypeKind.Energy, AlertSeverity.Warning, "Inspect connections and panels", "Check for loose connections, heating, or tripped breakers.", 2),
        // Co2 – Critical
        new(SensorTypeKind.Co2, AlertSeverity.Critical, "Increase ventilation immediately", "Open dampers, run fans, or evacuate if levels are unsafe. Check HVAC and fresh-air supply.", 1),
        new(SensorTypeKind.Co2, AlertSeverity.Critical, "Verify sensor and calibrate", "Confirm sensor placement and calibration; rule out sensor fault.", 2),
        // Co2 – Warning
        new(SensorTypeKind.Co2, AlertSeverity.Warning, "Review ventilation setpoints", "Adjust demand-controlled ventilation or schedule to improve air quality.", 1),
        new(SensorTypeKind.Co2, AlertSeverity.Warning, "Check filters and outdoor air", "Ensure filters are clean and outdoor air intakes are not blocked.", 2),
        // O2 – Critical
        new(SensorTypeKind.O2, AlertSeverity.Critical, "Evacuate if below safe level", "Follow confined-space or safety procedures; ensure ventilation and recheck before re-entry.", 1),
        new(SensorTypeKind.O2, AlertSeverity.Critical, "Verify sensor and ventilation", "Confirm sensor is working and that fresh air or O2 supply is adequate.", 2),
        // O2 – Warning
        new(SensorTypeKind.O2, AlertSeverity.Warning, "Increase ventilation", "Improve fresh-air supply or run ventilation equipment per procedures.", 1),
        new(SensorTypeKind.O2, AlertSeverity.Warning, "Check sensor location and calibration", "Ensure sensor is in representative location and within calibration.", 2),
    ];

    public async Task SeedAsync(ApplicationDbContext context)
    {
        var sensorTypes = await context.SensorTypes
            .Where(st => st.DeletedAt == null)
            .ToDictionaryAsync(st => st.Type, st => st.Id);

        if (sensorTypes.Count == 0)
        {
            _logger.LogInformation("No sensor types found; skipping recommended actions seed");
            return;
        }

        var existingKeys = await context.RecommendedActions
            .Where(r => r.DeletedAt == null)
            .Select(r => new { r.SensorTypeId, r.Severity, r.Title })
            .ToListAsync();
        var existingSet = existingKeys.Select(k => (k.SensorTypeId, k.Severity, k.Title)).ToHashSet();

        var now = DateTime.UtcNow;
        var added = 0;
        foreach (var row in Rows)
        {
            if (!sensorTypes.TryGetValue(row.SensorKind, out var sensorTypeId))
                continue;
            var key = (sensorTypeId, row.Severity, row.Title);
            if (existingSet.Contains(key))
                continue;

            context.RecommendedActions.Add(new RecommendedAction
            {
                Id = Guid.NewGuid(),
                SensorTypeId = sensorTypeId,
                Severity = row.Severity,
                Title = row.Title,
                Description = row.Description,
                DisplayOrder = row.DisplayOrder,
                IsActive = true,
                CreatedAt = now,
            });
            existingSet.Add(key);
            added++;
        }

        if (added > 0)
        {
            await context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} recommended actions", added);
        }
        else
        {
            _logger.LogInformation("Recommended actions already exist, skipping");
        }
    }
}
