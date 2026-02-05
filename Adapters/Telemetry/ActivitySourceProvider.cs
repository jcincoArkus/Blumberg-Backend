using System.Diagnostics;

namespace Adapters.Telemetry;

/// <summary>
/// Provides a shared ActivitySource for manual instrumentation
/// </summary>
/// <remarks>
/// Use this to create custom spans for important operations like:
/// - Database queries
/// - External API calls
/// - Business logic operations
/// - Cache operations
/// </remarks>
/// <example>
/// <code>
/// using var activity = ActivitySourceProvider.StartActivity("MyOperation");
/// activity?.SetTag("custom.tag", "value");
/// try
/// {
///     // Your code here
///     activity?.SetStatus(ActivityStatusCode.Ok);
/// }
/// catch (Exception ex)
/// {
///     activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
///     activity?.RecordException(ex);
///     throw;
/// }
/// </code>
/// </example>
public static class ActivitySourceProvider
{
    /// <summary>
    /// The name of the ActivitySource
    /// </summary>
    public const string SourceName = "Blumberg";

    /// <summary>
    /// The version of the ActivitySource
    /// </summary>
    public const string SourceVersion = "1.0.0";

    /// <summary>
    /// The shared ActivitySource instance
    /// </summary>
    private static readonly ActivitySource Source = new(SourceName, SourceVersion);

    /// <summary>
    /// Starts a new activity (span) with the given name
    /// </summary>
    /// <param name="name">The name of the activity</param>
    /// <param name="kind">The kind of activity (default: Internal)</param>
    /// <returns>The started activity, or null if not enabled</returns>
    public static Activity? StartActivity(
        string name,
        ActivityKind kind = ActivityKind.Internal)
    {
        return Source.StartActivity(name, kind);
    }

    /// <summary>
    /// Starts a new activity (span) with the given name and parent context
    /// </summary>
    /// <param name="name">The name of the activity</param>
    /// <param name="kind">The kind of activity</param>
    /// <param name="parentContext">The parent activity context</param>
    /// <returns>The started activity, or null if not enabled</returns>
    public static Activity? StartActivity(
        string name,
        ActivityKind kind,
        ActivityContext parentContext)
    {
        return Source.StartActivity(name, kind, parentContext);
    }

    /// <summary>
    /// Starts a new activity (span) with tags
    /// </summary>
    /// <param name="name">The name of the activity</param>
    /// <param name="kind">The kind of activity</param>
    /// <param name="tags">Tags to add to the activity</param>
    /// <returns>The started activity, or null if not enabled</returns>
    public static Activity? StartActivity(
        string name,
        ActivityKind kind,
        IEnumerable<KeyValuePair<string, object?>> tags)
    {
        var activity = Source.StartActivity(name, kind);
        if (activity != null)
        {
            foreach (var tag in tags)
            {
                activity.SetTag(tag.Key, tag.Value);
            }
        }
        return activity;
    }
}

/// <summary>
/// Extension methods for Activity to add common telemetry patterns
/// </summary>
public static class ActivityExtensions
{
    /// <summary>
    /// Records an exception in the activity
    /// </summary>
    /// <param name="activity">The activity</param>
    /// <param name="exception">The exception to record</param>
    public static void RecordException(this Activity? activity, Exception exception)
    {
        if (activity == null) return;

        activity.SetTag("exception.type", exception.GetType().FullName);
        activity.SetTag("exception.message", exception.Message);
        activity.SetTag("exception.stacktrace", exception.StackTrace);
    }

    /// <summary>
    /// Sets the status of the activity
    /// </summary>
    /// <param name="activity">The activity</param>
    /// <param name="status">The status code</param>
    /// <param name="description">Optional description</param>
    public static void SetStatus(this Activity? activity, ActivityStatusCode status, string? description = null)
    {
        if (activity == null) return;

        activity.SetStatus(status);
        if (!string.IsNullOrEmpty(description))
        {
            activity.SetTag("otel.status_description", description);
        }
    }

    /// <summary>
    /// Adds database-related tags to the activity
    /// </summary>
    /// <param name="activity">The activity</param>
    /// <param name="operation">The database operation (e.g., SELECT, INSERT)</param>
    /// <param name="table">The table name</param>
    public static void AddDatabaseTags(this Activity? activity, string operation, string table)
    {
        if (activity == null) return;

        activity.SetTag("db.operation", operation);
        activity.SetTag("db.table", table);
        activity.SetTag("db.system", "postgresql");
    }

    /// <summary>
    /// Adds HTTP client tags to the activity
    /// </summary>
    /// <param name="activity">The activity</param>
    /// <param name="method">HTTP method</param>
    /// <param name="url">Request URL</param>
    public static void AddHttpClientTags(this Activity? activity, string method, string url)
    {
        if (activity == null) return;

        activity.SetTag("http.method", method);
        activity.SetTag("http.url", url);
    }
}

