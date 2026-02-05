namespace Shared.ValueObjects;

/// <summary>
/// Value object representing an RGB color for role display
/// </summary>
public record RoleColor(byte R, byte G, byte B)
{
    /// <summary>
    /// Converts the color to a hex string (e.g., "#DC2626")
    /// </summary>
    public string ToHex() => $"#{R:X2}{G:X2}{B:X2}";

    /// <summary>
    /// Creates a RoleColor from a hex string (e.g., "#DC2626" or "DC2626")
    /// </summary>
    public static RoleColor FromHex(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length != 6)
            throw new ArgumentException("Hex color must be 6 characters", nameof(hex));

        var r = Convert.ToByte(hex.Substring(0, 2), 16);
        var g = Convert.ToByte(hex.Substring(2, 2), 16);
        var b = Convert.ToByte(hex.Substring(4, 2), 16);

        return new RoleColor(r, g, b);
    }

    /// <summary>
    /// Predefined colors for common roles
    /// </summary>
    public static class Predefined
    {
        public static readonly RoleColor Red = new(220, 38, 38);        // #DC2626
        public static readonly RoleColor Blue = new(37, 99, 235);       // #2563EB
        public static readonly RoleColor Green = new(22, 163, 74);      // #16A34A
        public static readonly RoleColor Yellow = new(234, 179, 8);     // #EAB308
        public static readonly RoleColor Purple = new(147, 51, 234);    // #9333EA
        public static readonly RoleColor Gray = new(107, 114, 128);     // #6B7280
        public static readonly RoleColor Orange = new(249, 115, 22);    // #F97316
        public static readonly RoleColor Pink = new(236, 72, 153);      // #EC4899
    }
}

