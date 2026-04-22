namespace Adapters.Database.Seeders;

/// <summary>
/// Canonical seed data names and structures. Shared so SiteSeeder and EquipmentSeeder
/// </summary>
public static class SeedData
{
	/// <summary>Site names used by SiteSeeder and EquipmentSeeder.</summary>
	public static class SiteNames
	{
		public const string NorthDistributionCenter = "North Distribution Center";
		public const string WestCoastWarehouse = "West Coast Warehouse";
		public const string EastCoastHub = "East Coast Hub";
		public const string SouthRegionalDc = "South Regional DC";
	}
}

/// <summary>
/// One equipment row for seed. Align with frontend mock equipment array.
/// </summary>
public record EquipmentSeedData(
	string Name,
	string EquipmentType,
	string SiteName
);

/// <summary>
/// Equipment seed rows.
/// </summary>
public static class EquipmentSeedRows
{
	public static readonly EquipmentSeedData[] All =
	[
		new("Compressor Unit A", "Refrigeration", SeedData.SiteNames.NorthDistributionCenter),
		new("HVAC System B", "Climate Control", SeedData.SiteNames.NorthDistributionCenter),
		new("Dehumidifier C", "Climate Control", SeedData.SiteNames.NorthDistributionCenter),
		new("Freezer Bank 1", "Refrigeration", SeedData.SiteNames.WestCoastWarehouse),
		new("Chiller Unit D", "Refrigeration", SeedData.SiteNames.WestCoastWarehouse),
		new("Air Handler E", "Climate Control", SeedData.SiteNames.EastCoastHub),
		new("Cold Storage F", "Refrigeration", SeedData.SiteNames.EastCoastHub),
		new("Backup Generator", "Power", SeedData.SiteNames.EastCoastHub),
		new("Main Freezer", "Refrigeration", SeedData.SiteNames.SouthRegionalDc),
		new("Power Distribution Unit", "Power", SeedData.SiteNames.SouthRegionalDc),
	];
}

/// <summary>
/// One sensor row for seed.
/// Type: temperature | humidity | pressure | energy | co2 | o2.
/// Status: active | offline | stale | warning.
/// Min/Max: threshold range; alert when value is outside. Duration: time value must remain outside range before triggering alert (seconds).
/// </summary>
public record SensorSeedData(
	string Name,
	string Type,
	string EquipmentName,
	string Status,
	decimal Min,
	decimal Max,
	int DurationSeconds
);

/// <summary>
/// Sensor seed rows. Order is significant: ingestion simulator loads by CreatedAt, so types are
/// interleaved (temp, pressure, humidity, energy, co2, o2, ...) so partition and round-robin
/// get variety across all sensor types instead of one type dominating offline/stale/silent/healthy.
/// </summary>
public static class SensorSeedRows
{
	public static readonly SensorSeedData[] All =
	[
		// Cycle 1: one of each type first (so partition has mix in offline/stale/silent/healthy)
		new("Temp Probe 1", "temperature", "Compressor Unit A", "active", Min: 0, Max: 40, DurationSeconds: 30),
		new("Pressure Gauge 1", "pressure", "Compressor Unit A", "active", Min: 50, Max: 200, DurationSeconds: 30),
		new("Humidity Sensor 1", "humidity", "HVAC System B", "active", Min: 30, Max: 70, DurationSeconds: 30),
		new("Energy Meter 1", "energy", "HVAC System B", "active", Min: 0, Max: 500, DurationSeconds: 30),
		new("CO2 Monitor 1", "co2", "Air Handler E", "active", Min: 400, Max: 1500, DurationSeconds: 30),
		new("O2 Monitor 1", "o2", "Cold Storage F", "active", Min: 19, Max: 23, DurationSeconds: 30),
		// Cycle 2
		new("Temp Probe 2", "temperature", "HVAC System B", "active", Min: 16, Max: 28, DurationSeconds: 30),
		new("Chiller Pressure", "pressure", "Chiller Unit D", "active", Min: 80, Max: 180, DurationSeconds: 30),
		new("Supply Pressure", "pressure", "Air Handler E", "active", Min: 10, Max: 150, DurationSeconds: 30),
		new("Humidity Sensor 2", "humidity", "Dehumidifier C", "active", Min: 25, Max: 65, DurationSeconds: 30),
		new("Generator Energy", "energy", "Backup Generator", "active", Min: 0, Max: 1000, DurationSeconds: 30),
		new("CO2 Monitor 2", "co2", "Cold Storage F", "active", Min: 350, Max: 1200, DurationSeconds: 30),
		new("O2 Monitor 2", "o2", "Backup Generator", "active", Min: 19.5m, Max: 22, DurationSeconds: 30),
		// Cycle 3+
		new("Freezer Temp 1", "temperature", "Freezer Bank 1", "active", Min: -25, Max: -15, DurationSeconds: 30),
		new("Chiller Temp", "temperature", "Chiller Unit D", "active", Min: 2, Max: 8, DurationSeconds: 30),
		new("PDU Energy", "energy", "Power Distribution Unit", "active", Min: 0, Max: 800, DurationSeconds: 30),
		new("Air Handler Temp", "temperature", "Air Handler E", "active", Min: 15, Max: 30, DurationSeconds: 30),
		new("Cold Storage Temp", "temperature", "Cold Storage F", "active", Min: -5, Max: 5, DurationSeconds: 30),
		new("Freezer Temp 2", "temperature", "Freezer Bank 1", "active", Min: -25, Max: -15, DurationSeconds: 30),
		new("Main Freezer Temp", "temperature", "Main Freezer", "active", Min: -22, Max: -18, DurationSeconds: 30),
	];
}
