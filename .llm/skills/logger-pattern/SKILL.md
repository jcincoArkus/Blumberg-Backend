---
name: logger-pattern
description: Pattern for structured logging with Serilog including log levels, message templates, and contextual enrichment. Use when adding logging to services.
---

# Logger Pattern

## Overview

Pattern for **structured logging** with **Serilog** including proper log levels, message templates with structured parameters, and contextual enrichment.

## Key Principles

1. **Structured Logging**: Use message templates with named parameters
2. **Log Levels**: Debug → Info → Warning → Error (appropriate level per situation)
3. **Contextual Data**: Include relevant IDs and counts in logs
4. **Correlation**: Logs automatically include TraceId/SpanId from spans

## Log Levels Guide

| Level | When to Use |
|-------|-------------|
| `Debug` | Method entry, detailed diagnostic info |
| `Information` | Successful operations, counts, business events |
| `Warning` | Expected failures (not found, validation), recoverable issues |
| `Error` | Unexpected exceptions, unrecoverable failures |

## Template: Service Logging

```csharp
using Microsoft.Extensions.Logging;

public class ItemService(IItemRepository repository, ILogger<ItemService> logger) : IItemService
{
    public async Task<ItemResponse> GetByIdAsync(Guid id)
    {
        // Debug: Method entry with parameters
        logger.LogDebug("Getting item by ID: {Id}", id);

        var item = await repository.GetByIdAsync(id);

        if (item == null)
        {
            // Warning: Expected failure (not found)
            logger.LogWarning("Item not found with ID: {Id}", id);
            throw new KeyNotFoundException($"Item with ID {id} was not found");
        }

        // Info: Successful operation
        logger.LogInformation("Retrieved item {Id}", id);

        return MapToResponse(item);
    }

    public async Task<List<ItemResponse>> GetAllAsync()
    {
        logger.LogDebug("Getting all items");

        var items = await repository.GetAllAsync();

        // Info: Include count in successful operations
        logger.LogInformation("Retrieved {Count} items", items.Count);

        return items.Select(MapToResponse).ToList();
    }

    public async Task<ItemResponse> CreateAsync(ItemRequest request)
    {
        // Debug: Include identifying info (not sensitive data)
        logger.LogDebug("Creating item: {Name}", request.Name);

        try
        {
            var created = await repository.CreateAsync(MapToEntity(request));

            // Info: Include created ID
            logger.LogInformation("Item created with ID: {Id}", created.Id);

            return MapToResponse(created);
        }
        catch (Exception ex)
        {
            // Error: Unexpected failures
            logger.LogError(ex, "Failed to create item: {Name}", request.Name);
            throw;
        }
    }
}
```

## Template: Controller Logging

```csharp
using Microsoft.Extensions.Logging;

public class ItemController(IItemService service, ILogger<ItemController> logger) : ControllerBase
{
    public async Task<ActionResult<ItemResponse>> GetById(Guid id)
    {
        logger.LogDebug("Getting item by ID: {Id}", id);

        try
        {
            var item = await service.GetByIdAsync(id);
            logger.LogInformation("Retrieved item {Id}", id);
            return Ok(item);
        }
        catch (KeyNotFoundException ex)
        {
            // Warning: Client error (not found)
            logger.LogWarning("Item not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // Error: Server error with exception
            logger.LogError(ex, "Error getting item {Id}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }
}
```

## Message Template Syntax

```csharp
// Named parameters (preferred) - creates structured properties
logger.LogInformation("Retrieved {Count} items for {UserId}", items.Count, userId);

// Multiple parameters
logger.LogInformation("Item {Id} updated by {UserId} at {Timestamp}",
    item.Id, userId, DateTime.UtcNow);

// Object destructuring with @ prefix
logger.LogDebug("Processing request: {@Request}", request);

// String formatting with $ prefix (rarely needed)
logger.LogInformation("Processing item {$Name}", item.Name);
```

## Common Logging Patterns

**Method Entry:**
```csharp
logger.LogDebug("Getting item by ID: {Id}", id);
logger.LogDebug("Creating item: {Name}", request.Name);
logger.LogDebug("Updating item {Id}", id);
logger.LogDebug("Deleting item {Id}", id);
```

**Successful Operations:**
```csharp
logger.LogInformation("Retrieved {Count} items", items.Count);
logger.LogInformation("Item created with ID: {Id}", item.Id);
logger.LogInformation("Item {Id} updated successfully", id);
logger.LogInformation("Item {Id} deleted successfully", id);
```

**Not Found:**
```csharp
logger.LogWarning("Item not found with ID: {Id}", id);
logger.LogWarning("User not found with email: {Email}", email);
```

**Validation/Business Errors:**
```csharp
logger.LogWarning("Invalid request: {Message}", validationResult.Error);
logger.LogWarning("Operation not allowed: {Reason}", reason);
```

**Exceptions:**
```csharp
// Include exception as first parameter
logger.LogError(ex, "Failed to process item {Id}", id);
logger.LogError(ex, "Database connection failed");
```

## Contextual Enrichment

Serilog automatically enriches logs with:
- `TraceId`, `SpanId` - OpenTelemetry context
- `MachineName` - Server hostname
- `ThreadId` - Thread identifier
- `Application` - Application name
- `RequestId` - HTTP request ID (in request scope)

## Configuration

```env
# .env configuration
LOG_LEVEL=Debug              # Debug, Information, Warning, Error
LOG_FORMAT_JSON=false        # true for production, false for development
LOG_WRITE_FILE=true          # Write to file
LOG_FILE_PATH=logs/app-.json # File path with rolling date
LOG_RETENTION_DAYS=30        # Days to keep log files
```

## Best Practices

✅ **DO:**
- Use named parameters: `{Id}`, `{Count}`, `{Name}`
- Include relevant context (IDs, counts, names)
- Use appropriate log levels consistently
- Include exception as first parameter in `LogError`

❌ **DON'T:**
- Log sensitive data (passwords, tokens, PII)
- Use string concatenation: `$"Item {id}"` (loses structure)
- Over-log (every line) or under-log (nothing)
- Use `LogError` for expected failures (use `LogWarning`)
