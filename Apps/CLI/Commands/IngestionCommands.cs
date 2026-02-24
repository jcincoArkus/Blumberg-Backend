using System.CommandLine;
using System.Text;
using System.Text.Json;
using Adapters.Config;
using Adapters.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Entity;
using Shared.Enums;

namespace CLI.Commands;

/// <summary>
/// Ingestion simulator command for dev: periodically POSTs fake readings to the ingestion API.
/// Can load sensors from the database (by org) and generate type-appropriate fake values.
/// </summary>
public static class IngestionCommands
{
    private const string EnvBaseUrl = "INGESTION_SIMULATOR_BASE_URL";
    private const string EnvApiKey = "INGESTION_SIMULATOR_API_KEY";
    private const string EnvSensorIds = "INGESTION_SIMULATOR_SENSOR_IDS";
    private const string EnvOrgId = "INGESTION_SIMULATOR_ORG_ID";

    /// <summary>
    /// Creates the ingestion:simulate command (long-running until Ctrl+C).
    /// </summary>
    public static Command Simulate(ILoggerFactory loggerFactory, AppConfig config)
    {
        var logger = loggerFactory.CreateLogger("IngestionSimulate");
        var command = new Command("ingestion:simulate", "Periodically POST fake sensor readings to the ingestion API (dev). Load sensors from DB by org, or pass sensor IDs. Press Ctrl+C to stop.");

        var baseUrlOption = new Option<string>(
            ["--base-url", "-u"],
            getDefaultValue: () => Environment.GetEnvironmentVariable(EnvBaseUrl) ?? config.Application.Urls.FirstOrDefault() ?? "http://localhost:5000",
            "API base URL (e.g. http://localhost:5000). Overrides INGESTION_SIMULATOR_BASE_URL.");
        var apiKeyOption = new Option<string?>(
            ["--api-key", "-k"],
            () => Environment.GetEnvironmentVariable(EnvApiKey),
            "API key for X-Api-Key header. Overrides INGESTION_SIMULATOR_API_KEY.");
        var orgIdOption = new Option<string?>(
            ["--org-id", "-o"],
            () => Environment.GetEnvironmentVariable(EnvOrgId),
            "Organization ID: load sensors from DB for this org and generate type-appropriate values. Overrides INGESTION_SIMULATOR_ORG_ID.");
        var sensorIdsOption = new Option<string?>(
            ["--sensor-ids", "-s"],
            () => Environment.GetEnvironmentVariable(EnvSensorIds),
            "Comma-separated sensor GUIDs (used when --org-id is not set). Overrides INGESTION_SIMULATOR_SENSOR_IDS.");
        var intervalOption = new Option<int>(
            ["--interval", "-i"],
            getDefaultValue: () => 30,
            "Seconds between each batch (default: 30).");
        var batchSizeOption = new Option<int>(
            ["--batch-size", "-b"],
            getDefaultValue: () => 5,
            "Readings per batch (default: 5, max 5000).");
        var rejectChanceOption = new Option<double>(
            ["--reject-chance", "-r"],
            getDefaultValue: () => 0.2,
            "Chance (0.0–1.0) that one reading in each batch is invalid (invalid unit or unknown sensor). Default: 0.2. Only applies to 'healthy' sensors in variety mode.");
        var noVarietyOption = new Option<bool>(
            ["--no-variety"],
            getDefaultValue: () => false,
            "Disable health-variety mode: treat all sensors the same (no offline/stale/silent/invalid-unit partitioning).");

        command.AddOption(baseUrlOption);
        command.AddOption(apiKeyOption);
        command.AddOption(orgIdOption);
        command.AddOption(sensorIdsOption);
        command.AddOption(intervalOption);
        command.AddOption(batchSizeOption);
        command.AddOption(rejectChanceOption);
        command.AddOption(noVarietyOption);

        command.SetHandler(async (string baseUrl, string? apiKey, string? orgIdStr, string? sensorIdsStr, int interval, int batchSize, double rejectChance, bool noVariety) =>
        {
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
            var ct = cts.Token;

            baseUrl = baseUrl.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                logger.LogError("API key is required. Set INGESTION_SIMULATOR_API_KEY or pass --api-key.");
                Environment.Exit(1);
            }

            List<SensorReadingInfo> sensorInfos;

            if (!string.IsNullOrWhiteSpace(orgIdStr) && Guid.TryParse(orgIdStr, out var orgId))
            {
                // Load sensors from DB for this org (bypass tenant filter)
                sensorInfos = await LoadSensorsFromDatabaseAsync(orgId, logger, ct);
                if (sensorInfos.Count == 0)
                {
                    logger.LogError("No sensors found for organization {OrgId}. Create sensors or use --sensor-ids instead.", orgId);
                    Environment.Exit(1);
                }
                logger.LogInformation("Loaded {Count} sensor(s) from database for org {OrgId}.", sensorInfos.Count, orgId);
            }
            else
            {
                // Fallback: require explicit sensor IDs (no type/unit from DB)
                if (string.IsNullOrWhiteSpace(sensorIdsStr))
                {
                    logger.LogError("Either --org-id (or INGESTION_SIMULATOR_ORG_ID) or --sensor-ids is required.");
                    Environment.Exit(1);
                }
                sensorInfos = ParseSensorIds(sensorIdsStr!, logger);
                if (sensorInfos.Count == 0)
                {
                    logger.LogError("No valid sensor GUIDs found.");
                    Environment.Exit(1);
                }
            }

            batchSize = Math.Clamp(batchSize, 1, 5000);
            rejectChance = Math.Clamp(rejectChance, 0, 1);

            // Health-variety mode: partition sensors into offline / invalid / stale / silent / healthy (only when using org-id and enough sensors)
            SensorPartition? partition = null;
            if (!noVariety && !string.IsNullOrWhiteSpace(orgIdStr) && sensorInfos.Count >= 5)
            {
                partition = BuildPartition(sensorInfos);
                logger.LogInformation(
                    "Health variety mode: 1 offline (no data), 1 invalid unit (always rejected), {Stale} stale (~11 min), {Silent} silent (~31 min), {Healthy} healthy (every {Interval}s).",
                    partition.Stale.Count, partition.Silent.Count, partition.Healthy.Count, interval);
            }
            else
            {
                logger.LogInformation("Ingestion simulator started. Base URL: {BaseUrl}, sensors: {Count}, interval: {Interval}s, reject chance: {RejectChance:P0}. Press Ctrl+C to stop.",
                    baseUrl, sensorInfos.Count, interval, rejectChance);
            }

            var readingsPerBatch = Math.Min(batchSize, sensorInfos.Count);

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

            var random = new Random();
            var run = 0;

            while (!ct.IsCancellationRequested)
            {
                run++;
                List<SimulateReadingDto> readings;

                if (partition != null)
                {
                    readings = BuildVarietyBatch(partition, random, rejectChance, run, logger);
                }
                else
                {
                    readings = new List<SimulateReadingDto>();
                    for (var i = 0; i < readingsPerBatch; i++)
                    {
                        var info = sensorInfos[i % sensorInfos.Count];
                        var value = GeneratePlausibleValue(info.Kind, random);
                        readings.Add(new SimulateReadingDto
                        {
                            SensorId = info.SensorId,
                            Value = value,
                            TimestampUtc = DateTime.UtcNow,
                            Unit = (int)info.Unit
                        });
                    }

                    if (rejectChance > 0 && random.NextDouble() < rejectChance && readings.Count > 0)
                    {
                        var badIndex = random.Next(readings.Count);
                        if (random.Next(2) == 0)
                        {
                            readings[badIndex] = new SimulateReadingDto
                            {
                                SensorId = readings[badIndex].SensorId,
                                Value = readings[badIndex].Value,
                                TimestampUtc = readings[badIndex].TimestampUtc,
                                Unit = 99
                            };
                        }
                        else
                        {
                            readings[badIndex] = new SimulateReadingDto
                            {
                                SensorId = Guid.Empty,
                                Value = 0,
                                TimestampUtc = DateTime.UtcNow,
                                Unit = 0
                            };
                        }
                    }
                }

                var url = $"{baseUrl}/api/v1/ingestion/readings";
                var json = JsonSerializer.Serialize(readings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    var response = await http.PostAsync(url, content, ct);
                    if (response.IsSuccessStatusCode)
                    {
                        var body = await response.Content.ReadAsStringAsync(ct);
                        logger.LogInformation("Run {Run}: POST {Url} -> {Status}. {Body}",
                            run, url, (int)response.StatusCode, body.Length > 200 ? body[..200] + "..." : body);
                    }
                    else
                    {
                        var body = await response.Content.ReadAsStringAsync(ct);
                        logger.LogWarning("Run {Run}: POST {Url} -> {Status}. {Body}", run, url, (int)response.StatusCode, body);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Run {Run}: POST {Url} failed", run, url);
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(interval), ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            logger.LogInformation("Ingestion simulator stopped after {Runs} run(s).", run);
        },
            baseUrlOption,
            apiKeyOption,
            orgIdOption,
            sensorIdsOption,
            intervalOption,
            batchSizeOption,
            rejectChanceOption,
            noVarietyOption);

        return command;
    }

    /// <summary>Stable partition of sensors for health-variety mode. Index 0=offline, 1=invalid, 2-4=stale, 5-7=silent, 8+=healthy.</summary>
    private sealed class SensorPartition
    {
        public SensorReadingInfo Offline { get; init; }   // 1 sensor: never send
        public SensorReadingInfo InvalidUnit { get; init; } // 1 sensor: always send with Unit=99 (rejected)
        public IReadOnlyList<SensorReadingInfo> Stale { get; init; } = [];   // up to 3: send with timestamp -11 min
        public IReadOnlyList<SensorReadingInfo> Silent { get; init; } = []; // up to 3: send with timestamp -31 min
        public IReadOnlyList<SensorReadingInfo> Healthy { get; init; } = []; // rest: send with Now, optional reject chance
    }

    private const int StaleDelayMinutes = 11;  // Backend warning threshold 10 min -> last seen 11 min = stale
    private const int SilentDelayMinutes = 31; // Backend critical 30 min -> last seen 31 min = offline/silent

    private static SensorPartition BuildPartition(List<SensorReadingInfo> sensorInfos)
    {
        var list = sensorInfos; // stable order: 0=offline, 1=invalid, 2-4=stale, 5-7=silent, 8+=healthy
        var offline = list[0];
        var invalidUnit = list[1];
        var staleCount = Math.Min(3, Math.Max(0, list.Count - 2));
        var stale = list.Skip(2).Take(staleCount).ToList();
        var silentCount = Math.Min(3, Math.Max(0, list.Count - 2 - staleCount));
        var silent = list.Skip(2 + staleCount).Take(silentCount).ToList();
        var healthy = list.Skip(2 + staleCount + silentCount).ToList();
        return new SensorPartition
        {
            Offline = offline,
            InvalidUnit = invalidUnit,
            Stale = stale,
            Silent = silent,
            Healthy = healthy
        };
    }

    private static List<SimulateReadingDto> BuildVarietyBatch(
        SensorPartition partition,
        Random random,
        double rejectChance,
        int run,
        ILogger logger)
    {
        var now = DateTime.UtcNow;
        var staleTs = now.AddMinutes(-StaleDelayMinutes);
        var silentTs = now.AddMinutes(-SilentDelayMinutes);
        var readings = new List<SimulateReadingDto>();

        // Invalid unit: one reading with valid SensorId but Unit=99 -> always rejected, sensor shows ingestion errors and no LastSeenAt
        readings.Add(new SimulateReadingDto
        {
            SensorId = partition.InvalidUnit.SensorId,
            Value = GeneratePlausibleValue(partition.InvalidUnit.Kind, random),
            TimestampUtc = now,
            Unit = 99
        });

        // Stale: send with timestamp 11 min ago
        foreach (var info in partition.Stale)
        {
            readings.Add(new SimulateReadingDto
            {
                SensorId = info.SensorId,
                Value = GeneratePlausibleValue(info.Kind, random),
                TimestampUtc = staleTs,
                Unit = (int)info.Unit
            });
        }

        // Silent: send with timestamp 31 min ago
        foreach (var info in partition.Silent)
        {
            readings.Add(new SimulateReadingDto
            {
                SensorId = info.SensorId,
                Value = GeneratePlausibleValue(info.Kind, random),
                TimestampUtc = silentTs,
                Unit = (int)info.Unit
            });
        }

        // Healthy: send with Now; optionally corrupt one with reject chance
        foreach (var info in partition.Healthy)
        {
            readings.Add(new SimulateReadingDto
            {
                SensorId = info.SensorId,
                Value = GeneratePlausibleValue(info.Kind, random),
                TimestampUtc = now,
                Unit = (int)info.Unit
            });
        }

        if (partition.Healthy.Count > 0 && rejectChance > 0 && random.NextDouble() < rejectChance)
        {
            var healthyReadings = readings.Count - partition.Healthy.Count;
            var badIndex = healthyReadings + random.Next(partition.Healthy.Count);
            if (random.Next(2) == 0)
            {
                readings[badIndex] = new SimulateReadingDto
                {
                    SensorId = readings[badIndex].SensorId,
                    Value = readings[badIndex].Value,
                    TimestampUtc = readings[badIndex].TimestampUtc,
                    Unit = 99
                };
                logger.LogDebug("Run {Run}: injected invalid unit in healthy batch at index {Index}.", run, badIndex);
            }
            else
            {
                readings[badIndex] = new SimulateReadingDto
                {
                    SensorId = Guid.Empty,
                    Value = 0,
                    TimestampUtc = now,
                    Unit = 0
                };
                logger.LogDebug("Run {Run}: injected unknown sensor in healthy batch at index {Index}.", run, badIndex);
            }
        }

        return readings;
    }

    private sealed record SensorReadingInfo(Guid SensorId, SensorTypeKind Kind, Unit Unit);

    private static async Task<List<SensorReadingInfo>> LoadSensorsFromDatabaseAsync(Guid organizationId, ILogger logger, CancellationToken ct)
    {
        await using var context = ApplicationDbContextFactory.Create();
        var sensors = await context.Sensors
            .IgnoreQueryFilters()
            .Where(s => s.DeletedAt == null && s.OrganizationId == organizationId)
            .Include(s => s.SensorType)
            .AsNoTracking()
            .ToListAsync(ct);

        return sensors
            .Select(s => new SensorReadingInfo(s.Id, s.SensorType.Type, s.SensorType.Unit))
            .ToList();
    }

    private static List<SensorReadingInfo> ParseSensorIds(string sensorIdsStr, ILogger logger)
    {
        var list = new List<SensorReadingInfo>();
        foreach (var s in sensorIdsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (Guid.TryParse(s, out var id))
                list.Add(new SensorReadingInfo(id, SensorTypeKind.Temperature, Unit.Celsius));
            else
                logger.LogWarning("Invalid sensor ID skipped: {Value}", s);
        }
        return list;
    }

    private static decimal GeneratePlausibleValue(SensorTypeKind kind, Random random)
    {
        return kind switch
        {
            SensorTypeKind.Temperature => Math.Round((decimal)(random.NextDouble() * 8 + 18), 2),       // 18–26 °C
            SensorTypeKind.Humidity => Math.Round((decimal)(random.NextDouble() * 40 + 30), 2),       // 30–70 %
            SensorTypeKind.Co2 => Math.Round((decimal)(random.NextDouble() * 600 + 400), 2),          // 400–1000 ppm
            SensorTypeKind.O2 => Math.Round((decimal)(random.NextDouble() * 5 + 19), 2),             // 19–24 %
            SensorTypeKind.Pressure => Math.Round((decimal)(random.NextDouble() * 0.5 + 1.0), 2),     // 1.0–1.5 bar
            SensorTypeKind.Energy => Math.Round((decimal)(random.NextDouble() * 10 + 2), 2),            // 2–12 kW
            _ => Math.Round((decimal)(random.NextDouble() * 10 + 20), 2)
        };
    }

    private sealed class SimulateReadingDto
    {
        public Guid SensorId { get; set; }
        public decimal Value { get; set; }
        public DateTime TimestampUtc { get; set; }
        public int Unit { get; set; }
    }
}
