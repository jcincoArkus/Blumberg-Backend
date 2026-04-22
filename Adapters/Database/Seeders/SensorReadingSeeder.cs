using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Entity;
using Shared.Enums;

namespace Adapters.Database.Seeders;

/// <summary>
/// Seeds sample sensor readings for the last 7 days (4 readings per day per sensor).
/// </summary>
public class SensorReadingSeeder(ILoggerFactory loggerFactory) : ISeeder
{
	private readonly ILogger _logger = loggerFactory.CreateLogger<SensorReadingSeeder>();

	/// <summary>Readings per day per sensor (e.g. 00:00, 06:00, 12:00, 18:00 UTC).</summary>
	private const int ReadingsPerDay = 4;

	/// <summary>Number of days of history.</summary>
	private const int DaysBack = 7;

	public int Order => 4;

	public async Task SeedAsync(ApplicationDbContext context)
	{
		var org = await context.Organizations
			.FirstOrDefaultAsync(o => o.Slug == OrganizationSeeder.TestOrganizationSlug && o.DeletedAt == null);

		if (org == null)
		{
			throw new InvalidOperationException(
				$"Test organization '{OrganizationSeeder.TestOrganizationSlug}' not found. Ensure OrganizationSeeder runs before SensorReadingSeeder (Order).");
		}

		if (context.SensorReadings.IgnoreQueryFilters().Any(r => r.OrganizationId == org.Id && r.DeletedAt == null))
		{
			_logger.LogInformation("Sensor readings for organization '{Slug}' already exist, skipping", OrganizationSeeder.TestOrganizationSlug);
			return;
		}

		var sensors = await context.Sensors
			.IgnoreQueryFilters()
			.Where(s => s.OrganizationId == org.Id && s.DeletedAt == null)
			.Include(s => s.SensorType)
			.ToListAsync();

		if (sensors.Count == 0)
		{
			throw new InvalidOperationException(
				"No sensors found for test organization. Ensure SensorSeeder runs before SensorReadingSeeder (Order).");
		}

		var now = DateTime.UtcNow;
		var readings = new List<SensorReading>();
		var endDate = DateOnly.FromDateTime(now);
		var startDate = endDate.AddDays(-DaysBack);

		foreach (var sensor in sensors)
		{
			var unit = sensor.SensorType?.Unit ?? Unit.Custom;
			var (min, max) = PlausibleRangeForUnit(unit);

			for (var d = 0; d <= DaysBack; d++)
			{
				var date = startDate.AddDays(d);
				for (var i = 0; i < ReadingsPerDay; i++)
				{
					var hour = i * (24 / ReadingsPerDay);
					var timestampUtc = date.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(hour)), DateTimeKind.Utc);
					if (timestampUtc > now)
						continue;

					var value = PlausibleValue(sensor.Id, date, i, min, max);
					readings.Add(new SensorReading
					{
						Id = Guid.NewGuid(),
						OrganizationId = org.Id,
						SensorId = sensor.Id,
						Value = value,
						TimestampUtc = timestampUtc,
						Unit = unit,
						IngestionRunId = null,
						CreatedAt = now,
					});
				}
			}
		}

		context.SensorReadings.AddRange(readings);
		await context.SaveChangesAsync();
		_logger.LogInformation("Seeded {Count} sensor readings (last {Days} days, {Sensors} sensors)", readings.Count, DaysBack, sensors.Count);
	}

	private static (decimal min, decimal max) PlausibleRangeForUnit(Unit unit)
	{
		return unit switch
		{
			Unit.Celsius => (-25m, 35m),
			Unit.Fahrenheit => (-15m, 95m),
			Unit.Percent => (0m, 100m),
			Unit.Psi => (0m, 200m),
			Unit.Bar => (0m, 15m),
			Unit.Kw => (0m, 50m),
			Unit.Kwh => (0m, 500m),
			Unit.Ppm => (0m, 2000m),
			_ => (0m, 100m),
		};
	}

	/// <summary>Deterministic value from sensor id + date + index so seed data is reproducible.</summary>
	private static decimal PlausibleValue(Guid sensorId, DateOnly date, int index, decimal min, decimal max)
	{
		var span = max - min;
		var seed = sensorId.GetHashCode() ^ (date.DayNumber * 31) + index;
		var hash = Math.Abs(seed % 1000);
		var t = (decimal)hash / 1000m;
		return Math.Round(min + (t * span), 2);
	}
}
