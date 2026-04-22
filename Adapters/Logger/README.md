# Adapters.Logger

Structured logging adapter with support for both traditional and key-value pair syntax.

## Features

- ✅ **Structured Logging** - Log events with typed properties
- ✅ **Two Syntaxes** - Traditional placeholders or key-value pairs
- ✅ **Colored Console** - JSON output with syntax highlighting for development
- ✅ **JSON Format** - Machine-readable JSON for production
- ✅ **File Rotation** - Daily log files with configurable retention
- ✅ **Request Logging** - Automatic HTTP request/response logging
- ✅ **Enrichment** - Machine name, thread ID, environment, etc.

---

## Configuration

Configure via environment variables (`.env` file):

```bash
# Log level: Debug, Information, Warning, Error, Fatal
LOG_LEVEL=Debug

# Format: false = colored console (dev), true = JSON (prod)
LOG_FORMAT_JSON=false

# Write to file
LOG_WRITE_FILE=true
LOG_FILE_PATH=logs/blumberg-.json
LOG_RETENTION_DAYS=30
```

---

## Usage

### Import

```csharp
using Microsoft.Extensions.Logging;
using Adapters.Logger;  // For key-value pair methods
```

### Syntax 1: Traditional (Placeholders)

Use placeholders `{PropertyName}` in the message:

```csharp
// Simple property
logger.LogInformation("User {Email} logged in", email);

// Multiple properties
logger.LogWarning("Sensor {SensorId} reading {Value} exceeds threshold {Threshold}",
    sensorId, 85.5, 80.0);

// With exception
logger.LogError(ex, "Failed to save sensor {SensorId}", sensorId);
```

**Console Output:**
```
[14:23:45 INF] User admin@blumberg.com logged in
[14:23:46 WRN] Sensor abc-123 reading 85.5 exceeds threshold 80.0
```

---

### Syntax 2: Key-Value Pairs (Colored JSON)

Use `*WithProps` methods with key-value pairs:

```csharp
// Simple properties
logger.LogInfoWithProps("user logged in",
    "email", email,
    "ipAddress", ipAddress);

// Multiple types
logger.LogWarnWithProps("sensor reading exceeds threshold",
    "sensorId", sensorId,
    "value", 85.5,
    "threshold", 80.0,
    "isAlert", true,
    "timestamp", DateTime.UtcNow);

// With exception
logger.LogErrorWithProps(ex, "failed to save sensor",
    "sensorId", sensorId,
    "orgId", orgId);
```

**Console Output (with colors):**
```
[14:23:45 INF] user logged in
{
  "email": "admin@blumberg.com",
  "ipAddress": "192.168.1.100"
}

[14:23:46 WRN] sensor reading exceeds threshold
{
  "sensorId": "abc-123",
  "value": 85.5,
  "threshold": 80.0,
  "isAlert": true,
  "timestamp": "2026-02-05T14:23:46.123Z"
}
```

**Colors:**
- 🟡 **Keys** (yellow): `"email"`, `"sensorId"`
- 🔵 **Strings** (cyan): `"admin@blumberg.com"`, `"abc-123"`
- 🟢 **Numbers** (green): `85.5`, `80.0`
- 🟣 **Booleans** (magenta): `true`, `false`

---

## Available Methods

### Traditional Syntax
```csharp
logger.LogDebug("message {Property}", value);
logger.LogInformation("message {Property}", value);
logger.LogWarning("message {Property}", value);
logger.LogError("message {Property}", value);
logger.LogError(ex, "message {Property}", value);
logger.LogCritical("message {Property}", value);
logger.LogCritical(ex, "message {Property}", value);
```

### Key-Value Pair Syntax
```csharp
logger.LogDebugWithProps("message", "key1", val1, "key2", val2);
logger.LogInfoWithProps("message", "key1", val1, "key2", val2);
logger.LogWarnWithProps("message", "key1", val1, "key2", val2);
logger.LogErrorWithProps("message", "key1", val1, "key2", val2);
logger.LogErrorWithProps(ex, "message", "key1", val1, "key2", val2);
logger.LogCriticalWithProps("message", "key1", val1, "key2", val2);
logger.LogCriticalWithProps(ex, "message", "key1", val1, "key2", val2);
```

**Rules:**
- Must pass **even number** of arguments (key-value pairs)
- Keys are strings, values can be any type
- Properties appear as colored JSON in console

---

## Complete Examples

### Example 1: Controller with Traditional Syntax

```csharp
public class SensorController(ILogger<SensorController> logger)
{
    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(Guid id)
    {
        logger.LogInformation("Getting sensor {SensorId}", id);

        try
        {
            var sensor = await _service.GetByIdAsync(id);
            return Ok(sensor);
        }
        catch (KeyNotFoundException)
        {
            logger.LogWarning("Sensor {SensorId} not found", id);
            return NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sensor {SensorId}", id);
            return StatusCode(500);
        }
    }
}
```

### Example 2: Service with Key-Value Pairs

```csharp
public class AuthService(ILogger<AuthService> logger)
{
    public async Task<string> LoginAsync(string email, string password)
    {
        var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == email);

        if (admin == null)
        {
            logger.LogWarnWithProps("invalid username",
                "email", email,
                "ipAddress", _httpContext.Connection.RemoteIpAddress?.ToString());
            throw new UnauthorizedException("Invalid credentials");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash))
        {
            logger.LogWarnWithProps("invalid password",
                "email", email,
                "adminId", admin.Id);
            throw new UnauthorizedException("Invalid credentials");
        }

        logger.LogInfoWithProps("login successful",
            "email", admin.Email,
            "adminId", admin.Id,
            "orgId", admin.OrganizationId);

        return GenerateToken(admin);
    }
}
```

### Example 3: CLI Command

```csharp
public static Command MigrationUp(ILoggerFactory loggerFactory)
{
    var logger = loggerFactory.CreateLogger("MigrationUp");

    command.SetHandler(async () =>
    {
        try
        {
            await using var context = ApplicationDbContextFactory.Create();
            var pending = await context.Database.GetPendingMigrationsAsync();

            logger.LogInfoWithProps("applying migrations",
                "count", pending.Count(),
                "timestamp", DateTime.UtcNow);

            await context.Database.MigrateAsync();

            logger.LogInformation("Migrations applied successfully");
        }
        catch (Exception ex)
        {
            logger.LogErrorWithProps(ex, "failed to apply migrations",
                "errorType", ex.GetType().Name);
            Environment.Exit(1);
        }
    });
}
```

---

## Output Formats

### Development (LOG_FORMAT_JSON=false)

**Traditional Syntax:**
```
[14:23:45 INF] User admin@blumberg.com logged in
[14:23:46 WRN] Sensor abc-123 not found
```

**Key-Value Syntax:**
```
[14:23:45 INF] user logged in
{
  "email": "admin@blumberg.com",
  "ipAddress": "192.168.1.100"
}
```

### Production (LOG_FORMAT_JSON=true)

Both syntaxes produce the same structured JSON:

```json
{"@t":"2026-02-05T14:23:45.123Z","@mt":"user logged in","@l":"Information","email":"admin@blumberg.com","ipAddress":"192.168.1.100"}
```

---

## Log Levels

| Level | When to Use | Example |
|-------|-------------|---------|
| **Debug** | Detailed diagnostic info | Variable values, loop iterations |
| **Information** | Normal application flow | User logged in, request completed |
| **Warning** | Unexpected but recoverable | Validation failed, retry attempt |
| **Error** | Operation failed | Exception caught, database error |
| **Fatal/Critical** | Application crash | Out of memory, unrecoverable error |

---

## Best Practices

### ✅ DO

```csharp
// Use structured properties
logger.LogInformation("User {UserId} logged in", userId);

// Use key-value pairs for sensitive data (hidden in console message)
logger.LogWarnWithProps("invalid credentials", "email", email);

// Include context
logger.LogError(ex, "Failed to save sensor {SensorId} for org {OrgId}", sensorId, orgId);

// Log at appropriate levels
logger.LogDebug("Processing {Count} items", items.Count);
logger.LogInformation("Order {OrderId} completed", orderId);
logger.LogWarning("Retry attempt {Attempt} of {MaxAttempts}", attempt, maxAttempts);
```

### ❌ DON'T

```csharp
// Don't use string interpolation (loses structured properties)
logger.LogInformation($"User {userId} logged in");

// Don't log sensitive data in message
logger.LogWarning($"Invalid password for {email}");

// Don't concatenate strings
logger.LogError("Error: " + ex.Message);

// Don't log at wrong level
logger.LogError("User clicked button");  // Should be Debug or Information
```

---

## When to Use Each Syntax

| Scenario | Syntax | Reason |
|----------|--------|--------|
| **Development/Debugging** | Traditional | Properties visible in message |
| **Production/Sensitive Data** | Key-Value | Message generic, data in JSON only |
| **Simple Logs** | Traditional | More concise |
| **Complex Context** | Key-Value | Cleaner separation of message and data |
| **Security Logs** | Key-Value | Hide sensitive info from console |

---

## File Output

Log files are written to `logs/` directory:

- **Format**: Compact JSON (always)
- **Rotation**: Daily (one file per day)
- **Naming**: `blumberg-YYYYMMDD.json`
- **Retention**: Configurable (default 30 days)

---

## Integration

The JSON output is compatible with:

- Elasticsearch + Kibana
- Seq (https://datalust.co/seq)
- Splunk
- Datadog
- Azure Application Insights
- AWS CloudWatch