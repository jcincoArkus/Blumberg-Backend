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
/// </summary>
public record SensorSeedData(
	string Name,
	string Type,
	string EquipmentName,
	string Status
);

/// <summary>
/// Sensor seed rows.
/// </summary>
public static class SensorSeedRows
{
	public static readonly SensorSeedData[] All =
	[
		new("Temp Probe 1", "temperature", "Compressor Unit A", "active"),
		new("Pressure Gauge 1", "pressure", "Compressor Unit A", "active"),
		new("Humidity Sensor 1", "humidity", "HVAC System B", "active"),
		new("Temp Probe 2", "temperature", "HVAC System B", "active"),
		new("Energy Meter 1", "energy", "HVAC System B", "active"),
		new("Humidity Sensor 2", "humidity", "Dehumidifier C", "warning"),
		new("Freezer Temp 1", "temperature", "Freezer Bank 1", "offline"),
		new("Freezer Temp 2", "temperature", "Freezer Bank 1", "offline"),
		new("Chiller Temp", "temperature", "Chiller Unit D", "active"),
		new("Chiller Pressure", "pressure", "Chiller Unit D", "active"),
		new("Air Handler Temp", "temperature", "Air Handler E", "active"),
		new("Cold Storage Temp", "temperature", "Cold Storage F", "active"),
		new("Generator Energy", "energy", "Backup Generator", "stale"),
		new("Main Freezer Temp", "temperature", "Main Freezer", "warning"),
		new("PDU Energy", "energy", "Power Distribution Unit", "active"),
	];
}
