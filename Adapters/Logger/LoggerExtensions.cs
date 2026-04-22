using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Adapters.Logger;

/// <summary>
/// Extension methods for ILogger to support Zap-like syntax with key-value pairs
/// </summary>
public static class LoggerExtensions
{
    // ANSI color codes for JSON formatting (similar to Zap)
    private const string ColorReset = "\u001b[0m";
    private const string ColorYellow = "\u001b[93m"; // for JSON keys
    private const string ColorCyan = "\u001b[96m";   // for JSON values
    private const string ColorGreen = "\u001b[92m";  // for numbers
    private const string ColorMagenta = "\u001b[95m"; // for booleans

    /// <summary>
    /// Logs a debug message with key-value properties (Zap-like syntax)
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="message">The log message</param>
    /// <param name="properties">Key-value pairs (must be even number of arguments)</param>
    /// <example>
    /// logger.LogDebugWithProps("Processing data", "userId", userId, "count", count);
    /// </example>
    public static void LogDebugWithProps(this ILogger logger, string message, params object[] properties)
    {
        LogWithProperties(logger, LogLevel.Debug, message, null, properties);
    }

    /// <summary>
    /// Logs an information message with key-value properties (Zap-like syntax)
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="message">The log message</param>
    /// <param name="properties">Key-value pairs (must be even number of arguments)</param>
    /// <example>
    /// logger.LogInfoWithProps("User logged in", "email", email, "ipAddress", ip);
    /// </example>
    public static void LogInfoWithProps(this ILogger logger, string message, params object[] properties)
    {
        LogWithProperties(logger, LogLevel.Information, message, null, properties);
    }

    /// <summary>
    /// Logs a warning message with key-value properties (Zap-like syntax)
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="message">The log message</param>
    /// <param name="properties">Key-value pairs (must be even number of arguments)</param>
    /// <example>
    /// logger.LogWarnWithProps("Invalid username", "email", email);
    /// </example>
    public static void LogWarnWithProps(this ILogger logger, string message, params object[] properties)
    {
        LogWithProperties(logger, LogLevel.Warning, message, null, properties);
    }

    /// <summary>
    /// Logs an error message with key-value properties (Zap-like syntax)
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="message">The log message</param>
    /// <param name="properties">Key-value pairs (must be even number of arguments)</param>
    /// <example>
    /// logger.LogErrorWithProps("Database connection failed", "host", host, "port", port);
    /// </example>
    public static void LogErrorWithProps(this ILogger logger, string message, params object[] properties)
    {
        LogWithProperties(logger, LogLevel.Error, message, null, properties);
    }

    /// <summary>
    /// Logs an error message with exception and key-value properties (Zap-like syntax)
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="exception">The exception to log</param>
    /// <param name="message">The log message</param>
    /// <param name="properties">Key-value pairs (must be even number of arguments)</param>
    /// <example>
    /// logger.LogErrorWithProps(ex, "Failed to save sensor", "sensorId", id, "orgId", orgId);
    /// </example>
    public static void LogErrorWithProps(this ILogger logger, Exception exception, string message, params object[] properties)
    {
        LogWithProperties(logger, LogLevel.Error, message, exception, properties);
    }

    /// <summary>
    /// Logs a critical/fatal message with key-value properties (Zap-like syntax)
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="message">The log message</param>
    /// <param name="properties">Key-value pairs (must be even number of arguments)</param>
    /// <example>
    /// logger.LogCriticalWithProps("Out of memory", "availableMemory", memory);
    /// </example>
    public static void LogCriticalWithProps(this ILogger logger, string message, params object[] properties)
    {
        LogWithProperties(logger, LogLevel.Critical, message, null, properties);
    }

    /// <summary>
    /// Logs a critical/fatal message with exception and key-value properties (Zap-like syntax)
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="exception">The exception to log</param>
    /// <param name="message">The log message</param>
    /// <param name="properties">Key-value pairs (must be even number of arguments)</param>
    /// <example>
    /// logger.LogCriticalWithProps(ex, "Application crash", "component", component);
    /// </example>
    public static void LogCriticalWithProps(this ILogger logger, Exception exception, string message, params object[] properties)
    {
        LogWithProperties(logger, LogLevel.Critical, message, exception, properties);
    }

    /// <summary>
    /// Internal method that handles the actual logging with properties
    /// </summary>
    private static void LogWithProperties(ILogger logger, LogLevel level, string message, Exception? exception, params object[] properties)
    {
        if (properties.Length % 2 != 0)
        {
            throw new ArgumentException("Properties must be key-value pairs (even number of arguments)", nameof(properties));
        }

        if (properties.Length == 0)
        {
            // No properties, just log the message
            logger.Log(level, exception, message);
            return;
        }

        // Build a dictionary of properties
        var propsDict = new Dictionary<string, object?>();
        for (int i = 0; i < properties.Length; i += 2)
        {
            var key = properties[i]?.ToString() ?? $"Property{i}";
            var value = properties[i + 1];
            propsDict[key] = value;
        }

        // Serialize properties to JSON for display in console
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var propsJson = JsonSerializer.Serialize(propsDict, jsonOptions);

        // Colorize the JSON output (similar to Zap)
        var coloredJson = ColorizeJson(propsJson);

        // Use LogContext to add structured properties (for JSON output)
        var disposables = new List<IDisposable>();

        try
        {
            // Push all properties to LogContext for structured logging
            foreach (var kvp in propsDict)
            {
                disposables.Add(LogContext.PushProperty(kvp.Key, kvp.Value));
            }

            // Log the message with colored JSON representation appended
            var fullMessage = $"{message}\n{coloredJson}";
            logger.Log(level, exception, fullMessage);
        }
        finally
        {
            // Clean up LogContext
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
        }
    }

    /// <summary>
    /// Colorizes JSON output with ANSI color codes (similar to Zap logger)
    /// Keys: Yellow, String values: Cyan, Numbers: Green, Booleans: Magenta
    /// </summary>
    private static string ColorizeJson(string json)
    {
        var result = new StringBuilder();
        var lines = json.Split('\n');

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                result.AppendLine(line);
                continue;
            }

            // Check if line contains a key-value pair
            var colonIndex = line.IndexOf(':');
            if (colonIndex == -1)
            {
                // No colon, just append the line (e.g., { or })
                result.AppendLine(line);
                continue;
            }

            // Split into key and value parts
            var keyPart = line[..colonIndex];
            var valuePart = line[(colonIndex + 1)..];

            // Colorize the key (between quotes)
            var coloredKey = ColorizeQuotedString(keyPart, ColorYellow);

            // Colorize the value based on type
            var coloredValue = ColorizeValue(valuePart);

            result.Append(coloredKey);
            result.Append(':');
            result.AppendLine(coloredValue);
        }

        return result.ToString();
    }

    /// <summary>
    /// Colorizes a quoted string (e.g., "key" -> "coloredKey")
    /// </summary>
    private static string ColorizeQuotedString(string input, string color)
    {
        var firstQuote = input.IndexOf('"');
        var lastQuote = input.LastIndexOf('"');

        if (firstQuote == -1 || lastQuote == -1 || firstQuote == lastQuote)
        {
            return input;
        }

        var before = input[..(firstQuote + 1)];
        var content = input[(firstQuote + 1)..lastQuote];
        var after = input[lastQuote..];

        return $"{before}{color}{content}{ColorReset}{after}";
    }

    /// <summary>
    /// Colorizes a value based on its type
    /// </summary>
    private static string ColorizeValue(string value)
    {
        var trimmed = value.Trim();

        // String value (quoted)
        if (trimmed.StartsWith('"'))
        {
            return ColorizeQuotedString(value, ColorCyan);
        }

        // Boolean
        if (trimmed.StartsWith("true") || trimmed.StartsWith("false"))
        {
            var commaIndex = trimmed.IndexOf(',');
            if (commaIndex == -1)
            {
                return value.Replace("true", $"{ColorMagenta}true{ColorReset}")
                           .Replace("false", $"{ColorMagenta}false{ColorReset}");
            }
            else
            {
                var boolPart = trimmed[..commaIndex];
                var rest = trimmed[commaIndex..];
                var coloredBool = boolPart.Replace("true", $"{ColorMagenta}true{ColorReset}")
                                         .Replace("false", $"{ColorMagenta}false{ColorReset}");
                return value.Replace(trimmed, coloredBool + rest);
            }
        }

        // Number (not quoted, not boolean)
        if (char.IsDigit(trimmed[0]) || trimmed[0] == '-')
        {
            var commaIndex = trimmed.IndexOf(',');
            if (commaIndex == -1)
            {
                // No comma, color the whole number
                return value.Replace(trimmed, $"{ColorGreen}{trimmed}{ColorReset}");
            }
            else
            {
                // Has comma, color only the number part
                var numberPart = trimmed[..commaIndex];
                var rest = trimmed[commaIndex..];
                return value.Replace(trimmed, $"{ColorGreen}{numberPart}{ColorReset}{rest}");
            }
        }

        // Default: no coloring
        return value;
    }
}

