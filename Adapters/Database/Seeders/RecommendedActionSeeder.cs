using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Entity;
using Shared.Enums;

namespace Adapters.Database.Seeders;

/// <summary>
/// Seeds predefined recommended actions for Temperature, Humidity, and Pressure sensor types
/// with at least High severity (Critical and Warning). Idempotent: skips when a matching
/// action (same sensor type, severity, title) already exists. Actions are active by default.
/// Runs after SensorTypeSeeder so sensor type IDs exist. For Dev/Staging (run via dev seed).
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
    ];

    public async Task SeedAsync(ApplicationDbContext context)
    {
        var sensorTypes = await context.SensorTypes
            .Where(st => st.DeletedAt == null && (st.Type == SensorTypeKind.Temperature || st.Type == SensorTypeKind.Humidity || st.Type == SensorTypeKind.Pressure))
            .ToDictionaryAsync(st => st.Type, st => st.Id);

        if (sensorTypes.Count == 0)
        {
            _logger.LogInformation("No Temperature, Humidity, or Pressure sensor types found; skipping recommended actions seed");
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
            _logger.LogInformation("Seeded {Count} recommended actions for Temperature, Humidity, Pressure", added);
        }
        else
        {
            _logger.LogInformation("Recommended actions for Temperature, Humidity, Pressure already exist, skipping");
        }
    }
}
