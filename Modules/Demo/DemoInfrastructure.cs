using Adapters.Database;
using Shared.Abstractions;
using Shared.Entity;
using Shared.Notifications;

namespace Modules.Demo;

/// <summary>
/// Fixed tenant for background demo work (no HttpContext). Null means "no tenant" (same as design-time).
/// </summary>
internal sealed class DemoTenantContext(Guid? organizationId) : ITenantContext
{
    public Guid? CurrentOrganizationId { get; } = organizationId;
}

/// <summary>
/// Creates short-lived DbContexts for demo work. Every unit of work gets its own context (disposed by the caller).
/// </summary>
internal static class DemoDb
{
    public static ApplicationDbContext Create(Guid? organizationId)
        => new(ApplicationDbContextFactory.GetOptions(), new DemoTenantContext(organizationId));
}

/// <summary>
/// Demo live loop uses the real ingestion pipeline but must never send e-mails.
/// </summary>
internal sealed class NoOpAlertTriggeredNotifier : IAlertTriggeredNotifier
{
    public Task NotifyTriggeredAsync(Alert alert, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

/// <summary>
/// Deterministic hashing / pseudo-random helpers so generated demo data is reproducible
/// (same timestamp + sensor always yields the same value, which keeps top-ups idempotent and consistent).
/// </summary>
internal static class DemoHash
{
    public static ulong Mix(ulong x)
    {
        unchecked
        {
            x += 0x9E3779B97F4A7C15UL;
            x = (x ^ (x >> 30)) * 0xBF58476D1CE4E5B9UL;
            x = (x ^ (x >> 27)) * 0x94D049BB133111EBUL;
            return x ^ (x >> 31);
        }
    }

    public static ulong Of(string s)
    {
        unchecked
        {
            var h = 14695981039346656037UL;
            foreach (var c in s)
            {
                h ^= c;
                h *= 1099511628211UL;
            }
            return Mix(h);
        }
    }

    public static ulong Combine(ulong a, long b) => Mix(a ^ Mix(unchecked((ulong)b)));

    public static ulong Combine(ulong a, long b, long c) => Combine(Combine(a, b), c);

    /// <summary>Uniform double in [0, 1).</summary>
    public static double Unit(ulong h) => (h >> 11) * (1.0 / (1UL << 53));

    /// <summary>Uniform double in [-1, 1).</summary>
    public static double Signed(ulong h) => Unit(h) * 2.0 - 1.0;
}

/// <summary>Small deterministic PRNG (SplitMix64).</summary>
internal sealed class DemoRng(ulong seed)
{
    private ulong _state = seed;

    public ulong NextULong()
    {
        _state = unchecked(_state + 0x9E3779B97F4A7C15UL);
        return DemoHash.Mix(_state);
    }

    public double NextDouble() => DemoHash.Unit(NextULong());

    public double Range(double min, double max) => min + (max - min) * NextDouble();

    /// <summary>Integer in [min, maxExclusive).</summary>
    public int Next(int min, int maxExclusive) => min + (int)(NextDouble() * (maxExclusive - min));

    public bool Chance(double p) => NextDouble() < p;

    public T Pick<T>(IReadOnlyList<T> items) => items[Next(0, items.Count)];
}
