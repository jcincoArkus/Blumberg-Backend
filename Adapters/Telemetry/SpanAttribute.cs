using System.Diagnostics;

namespace Adapters.Telemetry;

/// <summary>
/// Attribute to automatically create OpenTelemetry spans for methods.
/// Similar to @Span() decorator in NestJS.
/// </summary>
/// <example>
/// <code>
/// [Span]
/// public async Task&lt;Site&gt; GetByIdAsync(Guid id) { ... }
/// 
/// [Span("CustomSpanName")]
/// public async Task ProcessAsync() { ... }
/// 
/// [Span(Kind = ActivityKind.Client)]
/// public async Task&lt;Data&gt; CallExternalApiAsync() { ... }
/// 
/// [Span(IncludeArguments = true)]
/// public async Task CreateAsync(CreateRequest request) { ... }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class SpanAttribute : Attribute
{
    /// <summary>
    /// Custom name for the span. If not provided, uses "ClassName.MethodName"
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The kind of activity (Internal, Server, Client, Producer, Consumer)
    /// Default: Internal
    /// </summary>
    public ActivityKind Kind { get; set; } = ActivityKind.Internal;

    /// <summary>
    /// Whether to include method arguments as span tags
    /// Default: false (for security/privacy)
    /// </summary>
    public bool IncludeArguments { get; set; } = false;

    /// <summary>
    /// Whether to include the return value as a span tag
    /// Default: false (for security/privacy)
    /// </summary>
    public bool IncludeReturnValue { get; set; } = false;

    /// <summary>
    /// Whether to record exceptions in the span
    /// Default: true
    /// </summary>
    public bool RecordException { get; set; } = true;

    /// <summary>
    /// Custom tags to add to the span (format: "key1=value1,key2=value2")
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Creates a span attribute with default settings
    /// </summary>
    public SpanAttribute()
    {
    }

    /// <summary>
    /// Creates a span attribute with a custom name
    /// </summary>
    /// <param name="name">Custom span name</param>
    public SpanAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Parses the Tags string into a dictionary
    /// </summary>
    /// <returns>Dictionary of tag key-value pairs</returns>
    public Dictionary<string, string> GetTags()
    {
        var tags = new Dictionary<string, string>();
        
        if (string.IsNullOrWhiteSpace(Tags))
            return tags;

        var pairs = Tags.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                tags[parts[0].Trim()] = parts[1].Trim();
            }
        }

        return tags;
    }
}

