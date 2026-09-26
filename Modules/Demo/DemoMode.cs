namespace Modules.Demo;

/// <summary>
/// Demo-data mode switch. Enabled only when the environment variable <c>DEMO_MODE</c> is
/// <c>true</c>/<c>1</c>/<c>yes</c>. When disabled (default) nothing in <see cref="Modules.Demo"/> runs.
/// </summary>
public static class DemoMode
{
    /// <summary>Environment variable that enables demo mode.</summary>
    public const string EnvVar = "DEMO_MODE";

    /// <summary>Optional override (seconds) for the live loop interval. Default 60.</summary>
    public const string LiveIntervalEnvVar = "DEMO_LIVE_INTERVAL_SECONDS";

    /// <summary>True when DEMO_MODE is set to a truthy value.</summary>
    public static bool IsEnabled
    {
        get
        {
            var raw = Environment.GetEnvironmentVariable(EnvVar)?.Trim();
            return raw != null && (raw.Equals("true", StringComparison.OrdinalIgnoreCase)
                || raw == "1"
                || raw.Equals("yes", StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>Live loop interval (defaults to 60 s, clamped to 15 s – 15 min).</summary>
    public static TimeSpan LiveInterval
    {
        get
        {
            var raw = Environment.GetEnvironmentVariable(LiveIntervalEnvVar);
            var seconds = int.TryParse(raw, out var s) ? s : 60;
            return TimeSpan.FromSeconds(Math.Clamp(seconds, 15, 900));
        }
    }
}
