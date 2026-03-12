using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Entity;
using Shared.Enums;

namespace Adapters.Database.Seeders;

public class SensorSeeder(ILoggerFactory loggerFactory) : ISeeder
{
	private readonly ILogger _logger = loggerFactory.CreateLogger<SensorSeeder>();

	public int Order => 3;

	public async Task SeedAsync(ApplicationDbContext context)
	{
		var org = await context.Organizations
			.FirstOrDefaultAsync(o => o.Slug == OrganizationSeeder.TestOrganizationSlug && o.DeletedAt == null);

		if (org == null)
		{
			throw new InvalidOperationException(
				$"Test organization '{OrganizationSeeder.TestOrganizationSlug}' not found. Ensure OrganizationSeeder runs before SensorSeeder (Order).");
		}

		var equipment = await context.Equipment
			.IgnoreQueryFilters()
			.Where(e => e.OrganizationId == org.Id && e.DeletedAt == null)
			.ToListAsync();

		var equipmentByName = equipment.ToDictionary(e => e.Name, StringComparer.OrdinalIgnoreCase);
		if (equipmentByName.Count == 0)
		{
			throw new InvalidOperationException(
				"No equipment found for test organization. Ensure EquipmentSeeder runs before SensorSeeder (Order).");
		}

		if (context.Sensors.IgnoreQueryFilters().Any(s => s.OrganizationId == org.Id && s.DeletedAt == null))
		{
			_logger.LogInformation("Sensors for organization '{Slug}' already exist, skipping", OrganizationSeeder.TestOrganizationSlug);
			return;
		}

		var sensorTypesByKind = await context.SensorTypes
			.Where(st => st.DeletedAt == null)
			.ToDictionaryAsync(st => st.Type);

		var now = DateTime.UtcNow;
		var sensors = new List<Sensor>();
		foreach (var row in SensorSeedRows.All)
		{
			if (!equipmentByName.TryGetValue(row.EquipmentName, out var equip))
			{
				throw new InvalidOperationException(
					$"Equipment '{row.EquipmentName}' not found. Ensure EquipmentSeeder uses names that match SensorSeedRows.");
			}

			var kind = ParseSensorTypeKind(row.Type);
			if (!sensorTypesByKind.TryGetValue(kind, out var sensorType))
			{
				throw new InvalidOperationException(
					$"SensorType for '{row.Type}' (Kind={kind}) not found. Ensure SensorTypeSeeder runs before SensorSeeder (Order -2).");
			}

			var threshold = new Threshold
			{
				Id = Guid.NewGuid(),
				Min = row.Min,
				Max = row.Max,
				Duration = TimeSpan.FromSeconds(row.DurationSeconds),
				CreatedAt = now,
			};
			context.Thresholds.Add(threshold);

			var status = ParseSensorStatus(row.Status);
			sensors.Add(new Sensor
			{
				Id = Guid.NewGuid(),
				OrganizationId = org.Id,
				EquipmentId = equip.Id,
				SensorTypeId = sensorType.Id,
				ThresholdId = threshold.Id,
				Serial = row.Name,
				Status = status,
				CreatedAt = now,
			});
		}

		context.Sensors.AddRange(sensors);
		await context.SaveChangesAsync();
		_logger.LogInformation("Seeded {Count} sensors across {EquipmentCount} equipment", sensors.Count, equipment.Count);
	}

	private static SensorTypeKind ParseSensorTypeKind(string type)
	{
		return type.ToLowerInvariant() switch
		{
			"temperature" => SensorTypeKind.Temperature,
			"humidity" => SensorTypeKind.Humidity,
			"pressure" => SensorTypeKind.Pressure,
			"energy" => SensorTypeKind.Energy,
			"co2" => SensorTypeKind.Co2,
			"o2" => SensorTypeKind.O2,
			_ => SensorTypeKind.Custom,
		};
	}

	private static SensorStatus ParseSensorStatus(string status)
	{
		return status.ToLowerInvariant() switch
		{
			"active" => SensorStatus.Active,
			"offline" => SensorStatus.Inactive,
			"stale" => SensorStatus.Unknown,
			"warning" => SensorStatus.Maintenance,
			_ => SensorStatus.Unknown,
		};
	}
}
