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

        command.AddOption(baseUrlOption);
        command.AddOption(apiKeyOption);
        command.AddOption(orgIdOption);
        command.AddOption(sensorIdsOption);
        command.AddOption(intervalOption);
        command.AddOption(batchSizeOption);

        command.SetHandler(async (string baseUrl, string? apiKey, string? orgIdStr, string? sensorIdsStr, int interval, int batchSize) =>
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
            var readingsPerBatch = Math.Min(batchSize, sensorInfos.Count);

            logger.LogInformation("Ingestion simulator started. Base URL: {BaseUrl}, sensors: {Count}, interval: {Interval}s. Press Ctrl+C to stop.",
                baseUrl, sensorInfos.Count, interval);

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

            var random = new Random();
            var run = 0;

            while (!ct.IsCancellationRequested)
            {
                run++;
                var readings = new List<SimulateReadingDto>();
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
            batchSizeOption);

        return command;
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
