using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Text;
using System.Text.Json;
using Adapters.Config;
using Adapters.Database;
using Adapters.Database.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Ingestion.Repository;
using Modules.Ingestion.Service;
using Shared.Entity;
using Shared.Enums;

namespace CLI.Commands;

/// <summary>
/// Ingestion simulator command for dev: periodically POSTs fake readings to the ingestion API.
/// Can load sensors from the database (by org) and generate type-appropriate fake values.
///
/// Why HTTP (POST to /api/v1/ingestion/readings) instead of direct DB insert?
/// The ingestion API runs the full pipeline: validation (unit, sensor, org), accept/reject tracking,
/// BuildNewAlertsAsync (threshold + duration logic), and sensor breach state (FirstOutOfRangeAt).
/// Direct insert would only add SensorReading rows and would not create alerts, ingestion runs, or
/// breach state—so we use the real API to exercise the same logic production uses.
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
        var command = new Command("ingestion:simulate", "Periodically POST fake sensor readings to the ingestion API (dev). With no flags, uses seed org and auto-creates an API key. Or pass --org-id and --api-key. Load sensors from DB by org, or pass --sensor-ids. Press Ctrl+C to stop.");

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
            getDefaultValue: () => 300,
            "Seconds between each batch.");
        var batchSizeOption = new Option<int>(
            ["--batch-size", "-b"],
            getDefaultValue: () => 5,
            "Readings per batch (max 5000).");
        var rejectChanceOption = new Option<double>(
            ["--reject-chance", "-r"],
            getDefaultValue: () => 0.2,
            "Chance (0.0–1.0) that one reading in each batch is invalid (invalid unit or unknown sensor). Only applies to 'healthy' sensors in variety mode.");
        var noVarietyOption = new Option<bool>(
            ["--no-variety"],
            getDefaultValue: () => false,
            "Disable health-variety mode: treat all sensors the same (no offline/stale/silent/invalid-unit partitioning).");
        var alertEveryOption = new Option<int>(
            ["--alert-every", "-a"],
            getDefaultValue: () => 2,
            "Every N batches, send one valid reading above or below a sensor threshold to trigger an alert (0 = disabled). Only when using --org-id (sensors with thresholds from DB).");

        command.AddOption(baseUrlOption);
        command.AddOption(apiKeyOption);
        command.AddOption(orgIdOption);
        command.AddOption(sensorIdsOption);
        command.AddOption(intervalOption);
        command.AddOption(batchSizeOption);
        command.AddOption(rejectChanceOption);
        command.AddOption(noVarietyOption);
        command.AddOption(alertEveryOption);

        command.SetHandler(async (InvocationContext invocationContext) =>
        {
            var pr = invocationContext.ParseResult;
            var rawBaseUrl = GetOptionValue(
                pr,
                baseUrlOption,
                Environment.GetEnvironmentVariable(EnvBaseUrl) ?? config.Application.Urls.FirstOrDefault() ?? "http://localhost:5000");
            var baseUrl = NormalizeBaseUrlForHttpTarget(rawBaseUrl, logger).TrimEnd('/');
            var apiKey = GetOptionValue(pr, apiKeyOption);
            var orgIdStr = GetOptionValue(pr, orgIdOption);
            var sensorIdsStr = GetOptionValue(pr, sensorIdsOption);
            var interval = GetOptionValue(pr, intervalOption, 30);
            var batchSize = GetOptionValue(pr, batchSizeOption, 5);
            var rejectChance = GetOptionValue(pr, rejectChanceOption, 0.2);
            var noVariety = GetOptionValue(pr, noVarietyOption, false);
            var alertEvery = GetOptionValue(pr, alertEveryOption, 0);

            try
            {
                using var cts = new CancellationTokenSource();
                Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
                var ct = cts.Token;
                Guid orgId;
                if (string.IsNullOrWhiteSpace(apiKey) && string.IsNullOrWhiteSpace(orgIdStr))
                {
                    var (resolvedKey, resolvedOrgId) = await TryResolveOrgAndApiKeyFromSeedAsync(loggerFactory, ct);
                    if (resolvedKey == null || resolvedOrgId == null)
                    {
                        logger.LogError(
                            "No API key or org provided and seed data not found. Run database seed (e.g. dotnet run --project Apps/API seed), or pass --api-key and --org-id.");
                        Environment.Exit(1);
                    }
                    apiKey = resolvedKey;
                    orgId = resolvedOrgId.Value;
                    orgIdStr = orgId.ToString();
                    logger.LogInformation("Using seed org {OrgId}; created API key for this run (key not shown).", orgId);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(orgIdStr))
                    {
                        logger.LogError("When using ingestion:simulate with an API key or org, both --api-key and --org-id are required.");
                        Environment.Exit(1);
                    }
                    if (!Guid.TryParse(orgIdStr, out orgId))
                    {
                        logger.LogError("Invalid --org-id. Use a valid GUID.");
                        Environment.Exit(1);
                    }
                }

                List<SensorReadingInfo> sensorInfos;

                if (!string.IsNullOrWhiteSpace(orgIdStr))
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

            if (alertEvery > 0 && sensorInfos.All(s => s.ThresholdMin == null || s.ThresholdMax == null))
            {
                logger.LogWarning("--alert-every {N} ignored: no sensors with thresholds (use --org-id to load sensors from DB).", alertEvery);
                alertEvery = 0;
            }
            else if (alertEvery > 0)
            {
                var withThreshold = sensorInfos.Count(s => s.ThresholdMin != null && s.ThresholdMax != null);
                logger.LogInformation("Alert trigger: every {N} batch(es) one reading will be sent above/below threshold ({WithThreshold} sensors with thresholds).", alertEvery, withThreshold);
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
                    if (alertEvery > 0 && run % alertEvery == 0)
                        InjectAlertReading(readings, partition.Healthy, run, alertEvery, logger);
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

                    if (alertEvery > 0 && run % alertEvery == 0)
                        InjectAlertReading(readings, sensorInfos, run, alertEvery, logger);

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
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ingestion simulator failed");
                Environment.Exit(1);
            }
        });

        return command;
    }

    private static string? GetOptionValue(ParseResult pr, Option<string?> opt)
    {
        if (pr.FindResultFor(opt) is OptionResult or && or.Tokens.Count > 0)
            return or.Tokens[0].Value;
        return null;
    }

    private static string GetOptionValue(ParseResult pr, Option<string> opt, string defaultValue)
    {
        if (pr.FindResultFor(opt) is OptionResult or && or.Tokens.Count > 0)
            return or.Tokens[0].Value;
        return defaultValue;
    }

    private static int GetOptionValue(ParseResult pr, Option<int> opt, int defaultValue)
    {
        if (pr.FindResultFor(opt) is OptionResult or && or.Tokens.Count > 0 && int.TryParse(or.Tokens[0].Value, out var v))
            return v;
        return defaultValue;
    }

    private static double GetOptionValue(ParseResult pr, Option<double> opt, double defaultValue)
    {
        if (pr.FindResultFor(opt) is OptionResult or && or.Tokens.Count > 0 && double.TryParse(or.Tokens[0].Value, out var v))
            return v;
        return defaultValue;
    }

    private static bool GetOptionValue(ParseResult pr, Option<bool> opt, bool defaultValue)
    {
        if (pr.FindResultFor(opt) is OptionResult or && or.Tokens.Count > 0 && bool.TryParse(or.Tokens[0].Value, out var v))
            return v;
        return defaultValue;
    }

    /// <summary>
    /// Replaces one reading in the batch with an out-of-threshold value so the ingestion service creates an alert.
    /// Cycles through Critical (above max), Warning (below min, large margin), Info (below min, small margin).
    /// </summary>
    private static void InjectAlertReading(
        List<SimulateReadingDto> readings,
        IReadOnlyList<SensorReadingInfo> sensorInfosWithThreshold,
        int run,
        int alertEvery,
        ILogger logger)
    {
        var alertable = sensorInfosWithThreshold
            .Where(s => s.ThresholdMin != null && s.ThresholdMax != null)
            .ToList();
        if (alertable.Count == 0)
            return;

        var cycle = (run / alertEvery) % 3; // 0 = Critical, 1 = Warning, 2 = Info
        var index = (run / alertEvery) % alertable.Count;
        var info = alertable[index];
        var min = info.ThresholdMin!.Value;
        var max = info.ThresholdMax!.Value;
        var rangeSpan = Math.Max(max - min, 0.001m);

        decimal value;
        string variant;
        if (cycle == 0)
        {
            value = max + 1;
            variant = "above max (critical)";
        }
        else if (cycle == 1)
        {
            value = min - Math.Max(1, rangeSpan * 0.5m);
            variant = "below min (warning)";
        }
        else
        {
            value = min - Math.Max(rangeSpan * 0.05m, 0.001m);
            variant = "slightly below min (info)";
        }

        var readingIndex = readings.FindIndex(r => r.SensorId == info.SensorId);
        if (readingIndex < 0)
        {
            readings.Add(new SimulateReadingDto
            {
                SensorId = info.SensorId,
                Value = value,
                TimestampUtc = DateTime.UtcNow,
                Unit = (int)info.Unit
            });
            logger.LogDebug("Run {Run}: added out-of-threshold reading for sensor {SensorId} (value {Value}, {Variant}).",
                run, info.SensorId, value, variant);
        }
        else
        {
            readings[readingIndex] = new SimulateReadingDto
            {
                SensorId = info.SensorId,
                Value = value,
                TimestampUtc = readings[readingIndex].TimestampUtc,
                Unit = (int)info.Unit
            };
            logger.LogDebug("Run {Run}: replaced reading at index {Index} with out-of-threshold value for sensor {SensorId} (value {Value}, {Variant}).",
                run, readingIndex, info.SensorId, value, variant);
        }
    }

    /// <summary>Stable partition of sensors for health-variety mode. Index 0=offline, 1=invalid, 2-4=stale, 5-7=silent, 8+=healthy.</summary>
    private sealed class SensorPartition
    {
        // Initialized via object initializer in BuildPartition; default! avoids CS8618 (required init-only properties)
        public SensorReadingInfo Offline { get; init; } = default!;       // 1 sensor: never send
        public SensorReadingInfo InvalidUnit { get; init; } = default!;   // 1 sensor: always send with Unit=99 (rejected)
        public IReadOnlyList<SensorReadingInfo> Stale { get; init; } = [];   // up to 3: send with timestamp -11 min
        public IReadOnlyList<SensorReadingInfo> Silent { get; init; } = []; // up to 3: send with timestamp -31 min
        public IReadOnlyList<SensorReadingInfo> Healthy { get; init; } = []; // rest: send with Now, optional reject chance
    }

    private const int StaleDelayMinutes = 11;  // Backend Stale > 2×5min=10min -> last seen 11 min = stale
    private const int SilentDelayMinutes = 26; // Backend Offline > 5×5min=25min -> last seen 26 min = offline

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

    private sealed record SensorReadingInfo(Guid SensorId, SensorTypeKind Kind, Unit Unit, decimal? ThresholdMin, decimal? ThresholdMax);

    /// <summary>
    /// When seed data exists (test org from OrganizationSeeder), creates a new API key for that org
    /// and returns (rawKey, orgId) so the simulator can run without --api-key or --org-id.
    /// </summary>
    private static async Task<(string? RawKey, Guid? OrganizationId)> TryResolveOrgAndApiKeyFromSeedAsync(
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        await using var context = ApplicationDbContextFactory.Create();
        var org = await context.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                o => o.Slug == OrganizationSeeder.TestOrganizationSlug && o.DeletedAt == null,
                ct);
        if (org == null)
            return (null, null);

        var keyRepoLogger = loggerFactory.CreateLogger<ApiKeyRepository>();
        var keyServiceLogger = loggerFactory.CreateLogger<ApiKeyService>();
        var apiKeyRepository = new ApiKeyRepository(context, keyRepoLogger);
        var apiKeyService = new ApiKeyService(apiKeyRepository, keyServiceLogger);

        var (_, rawKey) = await apiKeyService.CreateKeyAsync(org.Id, "CLI ingestion:simulate (auto)", ct);
        return (rawKey, org.Id);
    }

    private static async Task<List<SensorReadingInfo>> LoadSensorsFromDatabaseAsync(Guid organizationId, ILogger logger, CancellationToken ct)
    {
        await using var context = ApplicationDbContextFactory.Create();
        var sensors = await context.Sensors
            .IgnoreQueryFilters()
            .Where(s => s.DeletedAt == null && s.OrganizationId == organizationId)
            .Include(s => s.SensorType)
            .Include(s => s.Threshold)
            .OrderBy(s => s.CreatedAt)
            .ThenBy(s => s.Id)
            .AsNoTracking()
            .ToListAsync(ct);

        return sensors
            .Select(s => new SensorReadingInfo(
                s.Id,
                s.SensorType.Type,
                s.SensorType.Unit,
                s.Threshold?.Min,
                s.Threshold?.Max))
            .ToList();
    }

    private static List<SensorReadingInfo> ParseSensorIds(string sensorIdsStr, ILogger logger)
    {
        var list = new List<SensorReadingInfo>();
        foreach (var s in sensorIdsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (Guid.TryParse(s, out var id))
                list.Add(new SensorReadingInfo(id, SensorTypeKind.Temperature, Unit.Celsius, null, null));
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

    /// <summary>
    /// Normalizes a base URL used by HttpClient.
    /// Converts bind/wildcard hosts (0.0.0.0, ::, +, *) to localhost, since they are valid bind addresses
    /// for servers but invalid as client request targets.
    /// </summary>
    private static string NormalizeBaseUrlForHttpTarget(string baseUrl, ILogger logger)
    {
        var trimmed = (baseUrl ?? "").Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            return "http://localhost:5000";

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            return trimmed;

        var host = uri.Host;
        var isWildcardHost =
            host == "0.0.0.0" ||
            host == "::" ||
            host == "[::]" ||
            host == "+" ||
            host == "*";

        if (!isWildcardHost)
            return trimmed;

        var builder = new UriBuilder(uri) { Host = "localhost" };
        var normalized = builder.Uri.ToString().TrimEnd('/');
        logger.LogWarning(
            "Base URL '{BaseUrl}' uses bind/wildcard host '{Host}', which is not a valid HTTP target. Using '{Normalized}' instead.",
            trimmed, host, normalized);
        return normalized;
    }

    private sealed class SimulateReadingDto
    {
        public Guid SensorId { get; set; }
        public decimal Value { get; set; }
        public DateTime TimestampUtc { get; set; }
        public int Unit { get; set; }
    }
}
