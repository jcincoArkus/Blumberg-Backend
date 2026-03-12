using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Entity;
using Shared.Enums;

namespace Adapters.Database.Seeders;

/// <summary>
/// Seeds reference data: sensor types (Temperature, Humidity, Pressure, Energy, Co2, O2) with their units.
/// Runs before SensorSeeder so sensors can reference them. No tenant scope.
/// </summary>
public class SensorTypeSeeder(ILoggerFactory loggerFactory) : ISeeder
{
	private readonly ILogger _logger = loggerFactory.CreateLogger<SensorTypeSeeder>();

	/// <summary>
	/// Run before Organization so reference data exists for all seeders.
	/// </summary>
	public int Order => -2;

	public async Task SeedAsync(ApplicationDbContext context)
	{
		var existing = await context.SensorTypes.Where(st => st.DeletedAt == null).ToListAsync();
		var existingKinds = existing.Select(st => st.Type).ToHashSet();

		var toCreate = new (SensorTypeKind Kind, Unit Unit)[]
		{
			(SensorTypeKind.Temperature, Unit.Celsius),
			(SensorTypeKind.Humidity, Unit.Percent),
			(SensorTypeKind.Pressure, Unit.Psi),
			(SensorTypeKind.Energy, Unit.Kw),
			(SensorTypeKind.Co2, Unit.Ppm),
			(SensorTypeKind.O2, Unit.Percent),
		};

		var now = DateTime.UtcNow;
		var added = 0;
		foreach (var (kind, unit) in toCreate)
		{
			if (existingKinds.Contains(kind))
				continue;
			context.SensorTypes.Add(new SensorType
			{
				Id = Guid.NewGuid(),
				Type = kind,
				Unit = unit,
				CreatedAt = now,
			});
			existingKinds.Add(kind);
			added++;
		}

		if (added > 0)
		{
			await context.SaveChangesAsync();
			_logger.LogInformation("Seeded {Count} sensor types", added);
		}
		else
		{
			_logger.LogInformation("Sensor types already exist, skipping");
		}
	}
}
