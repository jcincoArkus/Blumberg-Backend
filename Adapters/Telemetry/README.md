# Adapters.Telemetry

OpenTelemetry distributed tracing adapter for Blumberg API.

## Features

- ✅ **Distributed Tracing** - Track requests across services with OpenTelemetry
- ✅ **[Span] Decorator** - Automatic span creation with method attributes (like NestJS @Span())
- ✅ **Automatic Instrumentation** - HTTP, ASP.NET Core, Entity Framework Core
- ✅ **Manual Instrumentation** - Create custom spans for business operations
- ✅ **Multiple Exporters** - Console, OTLP (Jaeger, Datadog, etc.)
- ✅ **Serilog Integration** - Logs enriched with TraceId and SpanId
- ✅ **Environment Configuration** - Configure via environment variables

---

## Configuration

### Environment Variables

Configure OpenTelemetry using environment variables in `.env`:

```bash
# Enable/disable telemetry
OTEL_ENABLED=true

# Service identification
OTEL_SERVICE_NAME=Blumberg.API
OTEL_SERVICE_VERSION=1.0.0

# Exporter configuration
OTEL_EXPORTER=console                      # Options: console, otlp, datadog, console,otlp

# For OTLP exporter (Jaeger, custom endpoints)
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
OTEL_EXPORTER_OTLP_PROTOCOL=grpc          # Options: grpc, http/protobuf
OTEL_EXPORTER_OTLP_HEADERS=               # Format: key1=value1,key2=value2

# For Datadog exporter (simplified)
OTEL_DATADOG_API_KEY=your_api_key_here
OTEL_DATADOG_SITE=datadoghq.com           # Options: datadoghq.com, datadoghq.eu, us3.datadoghq.com, us5.datadoghq.com

# Instrumentation toggles
OTEL_INSTRUMENTATION_EF_ENABLED=true
OTEL_INSTRUMENTATION_HTTP_ENABLED=true
OTEL_INSTRUMENTATION_ASPNETCORE_ENABLED=true
```

### Exporters

#### Console Exporter (Development)

```bash
OTEL_EXPORTER=console
```

Outputs traces to console. Useful for development and debugging.

#### OTLP Exporter (Production)

```bash
OTEL_EXPORTER=otlp
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
OTEL_EXPORTER_OTLP_PROTOCOL=grpc
```

Exports traces to OTLP-compatible backends:

- **Jaeger**: `http://localhost:4317` (gRPC) or `http://localhost:4318` (HTTP)
- **Datadog**: See Datadog configuration below
- **Grafana Tempo**: `http://tempo:4317`
- **Honeycomb**: `https://api.honeycomb.io:443`

#### Multiple Exporters

```bash
OTEL_EXPORTER=console,otlp
```

### Running with Jaeger (Local Development)

```bash
# Start Jaeger
docker-compose -f docker/jaeger-compose.yml up -d

# Configure application
OTEL_ENABLED=true
OTEL_EXPORTER=otlp
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317

# View traces at http://localhost:16686
```

### Running with Datadog

#### Option 1: Simplified Configuration (Recommended) ⭐

The easiest way to send traces to Datadog:

```bash
OTEL_ENABLED=true
OTEL_EXPORTER=datadog
OTEL_DATADOG_API_KEY=your_api_key_here
OTEL_DATADOG_SITE=datadoghq.com  # or datadoghq.eu for EU
```

**Available sites:**
- `datadoghq.com` - US (default)
- `datadoghq.eu` - EU
- `us3.datadoghq.com` - US3
- `us5.datadoghq.com` - US5
- `ddog-gov.com` - Government

**That's it!** The exporter will automatically:
- Configure the correct Datadog OTLP endpoint for your region
- Set up HTTP/Protobuf protocol (required by Datadog)
- Add the API key header (`dd-api-key`)

View traces in Datadog APM: `https://app.datadoghq.com/apm/traces`

#### Option 2: Datadog Agent (Local Development)

Use the provided Docker Compose file:

```bash
# 1. Copy and configure environment file
cp docker/.env.datadog.example docker/.env.datadog
# Edit docker/.env.datadog and add your DD_API_KEY

# 2. Start Datadog Agent
docker-compose -f docker/datadog-compose.yml --env-file docker/.env.datadog up -d

# 3. Configure application
OTEL_ENABLED=true
OTEL_EXPORTER=otlp
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
OTEL_EXPORTER_OTLP_PROTOCOL=grpc
```

#### Option 3: Advanced OTLP Configuration

For custom OTLP endpoints or advanced configuration:

```bash
# Direct to Datadog (agentless)
OTEL_ENABLED=true
OTEL_EXPORTER=otlp
OTEL_EXPORTER_OTLP_ENDPOINT=https://api.datadoghq.com:443
OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf
OTEL_EXPORTER_OTLP_HEADERS=dd-api-key=YOUR_API_KEY

# Or via Datadog Agent
OTEL_ENABLED=true
OTEL_EXPORTER=otlp
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
OTEL_EXPORTER_OTLP_PROTOCOL=grpc
```

---

## Usage

### Method 1: [Span] Decorator (Recommended) 🎯

The easiest way to add tracing to your methods using the `[Span]` attribute.

#### Step 1: Register your service with span instrumentation

```csharp
// In your module setup (e.g., Modules/Sites/SitesModule.cs)
using Adapters.Telemetry;

public static class SitesModule
{
    public static IServiceCollection AddSitesModule(this IServiceCollection services)
    {
        // Instead of: services.AddScoped<ISiteService, SiteService>();
        // Use:
        services.AddScopedWithSpan<ISiteService, SiteService>();
        services.AddScopedWithSpan<ISiteRepository, SiteRepository>();

        return services;
    }
}
```

**Available registration methods:**
- `AddScopedWithSpan<TInterface, TImplementation>()` - Scoped lifetime (recommended for services)
- `AddTransientWithSpan<TInterface, TImplementation>()` - Transient lifetime
- `AddSingletonWithSpan<TInterface, TImplementation>()` - Singleton lifetime

#### Step 2: Add [Span] attribute to your methods

```csharp
using Adapters.Telemetry;
using System.Diagnostics;

public class SiteService : ISiteService
{
    // ✅ Basic usage - automatically creates span "SiteService.GetByIdAsync"
    [Span]
    public virtual async Task<Site> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    // ✅ Custom span name
    [Span("Sites.Create")]
    public virtual async Task<Site> CreateAsync(CreateSiteRequest request)
    {
        return await _repository.CreateAsync(request);
    }

    // ✅ Include method arguments as tags (useful for debugging)
    [Span(IncludeArguments = true)]
    public virtual async Task<Site> UpdateAsync(Guid id, UpdateSiteRequest request)
    {
        return await _repository.UpdateAsync(id, request);
    }

    // ✅ External API call - use Client kind
    [Span(Kind = ActivityKind.Client)]


#### Accessing Current Span Context

You can access the current span to add custom tags, events, or errors:

```csharp
using System.Diagnostics;

public class PaymentService : IPaymentService
{
    [Span(Tags = "service=payment")]
    public virtual async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        // Access current span
        var currentSpan = Activity.Current;

        // Add custom tags
        currentSpan?.SetTag("payment.amount", request.Amount);
        currentSpan?.SetTag("payment.currency", request.Currency);
        currentSpan?.SetTag("payment.method", request.Method);

        try
        {
            var result = await _paymentGateway.ChargeAsync(request);

            // Mark as successful
            currentSpan?.SetStatus(ActivityStatusCode.Ok);
            currentSpan?.SetTag("payment.transaction_id", result.TransactionId);

            return result;
        }
        catch (PaymentException ex)
        {
            // Record error
            currentSpan?.RecordException(ex);
            currentSpan?.SetStatus(ActivityStatusCode.Error, ex.Message);
            currentSpan?.SetTag("payment.error_code", ex.ErrorCode);

            throw;
        }
    }
}
```

**Useful Activity.Current methods:**

```csharp
var span = Activity.Current;

// Add tags
span?.SetTag("key", "value");
span?.SetTag("number", 123);
span?.SetTag("boolean", true);

// Set status
span?.SetStatus(ActivityStatusCode.Ok);
span?.SetStatus(ActivityStatusCode.Error, "Error message");

// Add events (breadcrumbs)
span?.AddEvent(new ActivityEvent("event_name"));
span?.AddEvent(new ActivityEvent("event_name", tags: new ActivityTagsCollection
{
    { "key1", "value1" },
    { "key2", 123 }
}));

// Record exceptions (using extension method)
span?.RecordException(exception);

// Database tags (using extension method)
span?.AddDatabaseTags("SELECT", "table_name");

// HTTP tags (using extension method)
span?.AddHttpClientTags("GET", "https://api.example.com/endpoint");
```

#### Advanced Examples

**Example 1: Service with Business Logic**

```csharp
public class SensorReadingService : ISensorReadingService
{
    [Span("SensorReading.Process")]
    public virtual async Task ProcessReadingAsync(SensorReading reading)
    {
        var currentSpan = Activity.Current;

        // Add business context
        currentSpan?.SetTag("sensor.id", reading.SensorId);
        currentSpan?.SetTag("reading.value", reading.Value);

        // Validate
        await ValidateReadingAsync(reading);

        // Store
        await _repository.CreateAsync(reading);

        // Check thresholds
        if (reading.Value > reading.Threshold)
        {
            currentSpan?.SetTag("alert.triggered", true);
            await _alertService.TriggerAlertAsync(reading.SensorId, reading.Value);
        }

        currentSpan?.SetStatus(ActivityStatusCode.Ok);
    }

    [Span(IncludeArguments = true)]
    protected virtual async Task ValidateReadingAsync(SensorReading reading)
    {
        if (reading.Value < 0)
            throw new ValidationException("Reading value cannot be negative");
    }
}
```

**Example 2: Repository with Database Operations**

```csharp
public class SiteRepository : ISiteRepository
{
    [Span(Tags = "db.operation=select,db.table=sites")]
    public virtual async Task<List<Site>> GetAllAsync()
    {
        var currentSpan = Activity.Current;
        currentSpan?.AddDatabaseTags("SELECT", "sites");

        var sites = await _context.Sites
            .Where(s => s.DeletedAt == null)
            .ToListAsync();

        currentSpan?.SetTag("db.result_count", sites.Count);
        currentSpan?.SetStatus(ActivityStatusCode.Ok);

        return sites;
    }

    [Span(IncludeArguments = true, Tags = "db.operation=insert,db.table=sites")]
    public virtual async Task<Site> CreateAsync(Site site)
    {
        var currentSpan = Activity.Current;
        currentSpan?.AddDatabaseTags("INSERT", "sites");

        site.Id = Guid.NewGuid();
        site.CreatedAt = DateTime.UtcNow;

        _context.Sites.Add(site);
        await _context.SaveChangesAsync();

        currentSpan?.SetTag("site.id", site.Id);
        currentSpan?.SetStatus(ActivityStatusCode.Ok);

        return site;
    }
}
```

**Example 3: Error Handling with Validation**

```csharp
public class UserService : IUserService
{
    [Span(IncludeArguments = true)]
    public virtual async Task<User> RegisterUserAsync(RegisterUserRequest request)
    {
        var currentSpan = Activity.Current;
        var validationErrors = new List<string>();

        // Validate email
        if (!IsValidEmail(request.Email))
        {
            validationErrors.Add("Invalid email format");
            currentSpan?.SetTag("validation.email.valid", false);
        }
        else
        {
            currentSpan?.SetTag("validation.email.valid", true);
        }

        // Check if email exists
        var existingUser = await _repository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            validationErrors.Add("Email already registered");
            currentSpan?.SetTag("validation.email.exists", true);
        }

        if (validationErrors.Any())
        {
            currentSpan?.SetStatus(ActivityStatusCode.Error, "Validation failed");
            currentSpan?.SetTag("validation.error_count", validationErrors.Count);
            currentSpan?.SetTag("validation.errors", string.Join("; ", validationErrors));

            throw new ValidationException(string.Join(", ", validationErrors));
        }

        var user = await _repository.CreateAsync(request);
        currentSpan?.SetTag("user.id", user.Id);

        return user;
    }
}
```

**Example 4: Performance Metrics**

```csharp
public class ReportService : IReportService
{
    [Span("Reports.Generate")]
    public virtual async Task<Report> GenerateReportAsync(ReportRequest request)
    {
        var currentSpan = Activity.Current;
        var startTime = DateTime.UtcNow;

        currentSpan?.SetTag("report.type", request.Type);

        // Fetch data
        var dataFetchStart = DateTime.UtcNow;
        var data = await _repository.GetDataAsync(request);
        var dataFetchDuration = DateTime.UtcNow - dataFetchStart;

        currentSpan?.SetTag("performance.data_fetch_ms", dataFetchDuration.TotalMilliseconds);
        currentSpan?.SetTag("data.record_count", data.Count);

        // Process data
        var processingStart = DateTime.UtcNow;
        var processedData = ProcessData(data);
        var processingDuration = DateTime.UtcNow - processingStart;

        currentSpan?.SetTag("performance.processing_ms", processingDuration.TotalMilliseconds);

        // Generate report
        var report = await GeneratePdfAsync(processedData);

        var totalDuration = DateTime.UtcNow - startTime;
        currentSpan?.SetTag("performance.total_ms", totalDuration.TotalMilliseconds);
        currentSpan?.SetTag("performance.records_per_second", data.Count / totalDuration.TotalSeconds);

        return report;
    }
}
```

### Method 2: Automatic Instrumentation

Automatic instrumentation is enabled by default for:

1. **ASP.NET Core** - HTTP requests, routing, middleware
2. **HTTP Client** - Outgoing HTTP requests
3. **Entity Framework Core** - Database queries

No code changes required! Just configure and run.

### Method 3: Manual Instrumentation

Use `ActivitySourceProvider` to create custom spans for operations not covered by decorators:

```csharp
using Adapters.Telemetry;
using System.Diagnostics;

public async Task<Site> GetSiteAsync(Guid id)
{
    using var activity = ActivitySourceProvider.StartActivity("GetSite");
    activity?.SetTag("site.id", id);

    try
    {
        var site = await _repository.GetByIdAsync(id);
        activity?.SetStatus(ActivityStatusCode.Ok);
        return site;
    }
    catch (Exception ex)
    {
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        activity?.RecordException(ex);
        throw;
    }
}
```

**ActivitySourceProvider methods:**

```csharp
// Basic span
using var activity = ActivitySourceProvider.StartActivity("OperationName");

// With specific kind
using var activity = ActivitySourceProvider.StartActivity("ExternalAPI.Call", ActivityKind.Client);

// With initial tags
using var activity = ActivitySourceProvider.StartActivity("ProcessData", ActivityKind.Internal,
    new Dictionary<string, object>
    {
        { "data.size", 1000 },
        { "data.type", "sensor_readings" }
    });
```

---

## Integration with Serilog

Logs are automatically enriched with OpenTelemetry trace context:

```json
{
  "Timestamp": "2026-02-05T10:30:00.123Z",
  "Level": "Information",
  "Message": "Processing sensor reading",
  "TraceId": "4bf92f3577b34da6a3ce929d0e0e4736",
  "SpanId": "00f067aa0ba902b7",
  "sensor.id": "123e4567-e89b-12d3-a456-426614174000"
}
```

This allows you to:
- Correlate logs with traces
- Search logs by TraceId
- View logs in trace context in Jaeger/Datadog

---

## Best Practices

### When using [Span] decorator:

1. **Always make methods `virtual`** - Required for interception to work
2. **Use meaningful span names** - Help with debugging and monitoring
3. **Be careful with `IncludeArguments`** - Don't log sensitive data (passwords, tokens, PII)
4. **Use appropriate `ActivityKind`**:
   - `Internal` (default) - Internal operations
   - `Client` - Outgoing HTTP/gRPC calls, database queries
   - `Server` - Incoming requests (usually automatic)
5. **Add custom tags for business context** - Include operation type, resource, IDs
6. **Don't over-instrument** - Focus on important operations (service layer, repositories, external calls)
7. **Use `Activity.Current` for dynamic information** - Add tags based on runtime conditions

### General best practices:

1. **Name spans descriptively** - Use clear, hierarchical names like `Service.Operation`
2. **Add relevant tags** - Include IDs, parameters, and context
3. **Handle errors properly** - Always set status and record exceptions
4. **Use `using` statements** - Ensures spans are properly closed
5. **Combine approaches** - Use [Span] for methods, manual spans for specific operations

---

## Troubleshooting

### No traces appearing

1. Check `OTEL_ENABLED=true` in `.env`
2. Verify exporter configuration
3. Check network connectivity to OTLP endpoint
4. Look for errors in application logs

### [Span] decorator not working

1. Ensure method is `virtual`
2. Verify service is registered with `AddScopedWithSpan` (not regular `AddScoped`)
3. Check that interface is being injected (not concrete class)
4. Verify `OTEL_ENABLED=true`

### Performance impact

- Minimal overhead with proper configuration
- Disable in production if needed: `OTEL_ENABLED=false`
- Disable specific instrumentations via environment variables
- Use sampling for high-traffic scenarios

### Logs missing TraceId

- Ensure `Serilog.Enrichers.Span` is installed
- Check `.Enrich.WithSpan()` is configured in LoggerSetup
- Verify telemetry is enabled

### Datadog not receiving traces

**Using Datadog Agent:**
1. Verify agent is running: `docker ps | grep datadog`
2. Check agent logs: `docker logs datadog-agent`
3. Verify OTLP ports are exposed: `4317` (gRPC) and `4318` (HTTP)
4. Check agent configuration: `DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_GRPC_ENDPOINT=0.0.0.0:4317`

**Direct to Datadog (Agentless):**
1. Verify API key is correct in `OTEL_EXPORTER_OTLP_HEADERS`
2. Use `http/protobuf` protocol (not `grpc`)
3. Check endpoint: `https://api.datadoghq.com:443` (US) or `https://api.datadoghq.eu:443` (EU)
4. Verify header format: `dd-api-key=YOUR_API_KEY` (no spaces)
5. Check Datadog APM is enabled for your account

**Common issues:**
- Wrong region endpoint (US vs EU)
- Missing or invalid API key
- Firewall blocking outbound HTTPS traffic
- Using gRPC protocol instead of HTTP for direct ingestion

---

## Summary

**Quick Start:**

1. Configure environment variables in `.env`
2. Register services with `AddScopedWithSpan<TInterface, TImplementation>()`
3. Add `[Span]` attribute to methods (make them `virtual`)
4. Use `Activity.Current` to add custom tags and errors
5. Choose your backend:
   - **Jaeger (local)**: `docker-compose -f docker/jaeger-compose.yml up -d` → View at `http://localhost:16686`
   - **Datadog (simple)**: Set `OTEL_EXPORTER=datadog` and `OTEL_DATADOG_API_KEY=xxx` → View in Datadog APM
   - **Datadog (agent)**: `docker-compose -f docker/datadog-compose.yml up -d` → View in Datadog APM

**Three ways to instrument:**
- **[Span] Decorator** - Easiest, recommended for most cases
- **Automatic** - HTTP, ASP.NET Core, EF Core (enabled by default)
- **Manual** - `ActivitySourceProvider` for fine-grained control

