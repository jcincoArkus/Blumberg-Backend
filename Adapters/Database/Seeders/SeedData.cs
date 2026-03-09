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
/// Type: temperature | humidity | energy | pressure.
/// Status: active | offline | stale | warning.
/// Min/Max: threshold range; alert when value is outside. Duration: time value must remain outside range before triggering alert (minutes).
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
/// Sensor seed rows.
/// </summary>
public static class SensorSeedRows
{
	public static readonly SensorSeedData[] All =
	[
		new("Temp Probe 1", "temperature", "Compressor Unit A", "active", Min: 0, Max: 40, DurationSeconds: 30),
		new("Pressure Gauge 1", "pressure", "Compressor Unit A", "active", Min: 50, Max: 200, DurationSeconds: 30),
		new("Humidity Sensor 1", "humidity", "HVAC System B", "active", Min: 30, Max: 70, DurationSeconds: 30),
		new("Temp Probe 2", "temperature", "HVAC System B", "active", Min: 16, Max: 28, DurationSeconds: 30),
		new("Energy Meter 1", "energy", "HVAC System B", "active", Min: 0, Max: 500, DurationSeconds: 30),
		new("Humidity Sensor 2", "humidity", "Dehumidifier C", "active", Min: 25, Max: 65, DurationSeconds: 30),
		new("Freezer Temp 1", "temperature", "Freezer Bank 1", "active", Min: -25, Max: -15, DurationSeconds: 30),
		new("Freezer Temp 2", "temperature", "Freezer Bank 1", "active", Min: -25, Max: -15, DurationSeconds: 30),
		new("Chiller Temp", "temperature", "Chiller Unit D", "active", Min: 2, Max: 8, DurationSeconds: 30),
		new("Chiller Pressure", "pressure", "Chiller Unit D", "active", Min: 80, Max: 180, DurationSeconds: 30),
		new("Air Handler Temp", "temperature", "Air Handler E", "active", Min: 15, Max: 30, DurationSeconds: 30),
		new("Cold Storage Temp", "temperature", "Cold Storage F", "active", Min: -5, Max: 5, DurationSeconds: 30),
		new("Generator Energy", "energy", "Backup Generator", "active", Min: 0, Max: 1000, DurationSeconds: 30),
		new("Main Freezer Temp", "temperature", "Main Freezer", "active", Min: -22, Max: -18, DurationSeconds: 30),
		new("PDU Energy", "energy", "Power Distribution Unit", "active", Min: 0, Max: 800, DurationSeconds: 30),
	];
}
