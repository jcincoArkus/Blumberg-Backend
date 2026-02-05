---
name: span-pattern
description: Pattern for automatic OpenTelemetry span instrumentation using the [Span] attribute and Castle.DynamicProxy. Use when adding distributed tracing to services.
---

# Span Pattern

## Overview

Pattern for automatic **OpenTelemetry span instrumentation** using the `[Span]` attribute and Castle.DynamicProxy for method interception.

## Key Principles

1. **Attribute-Based**: Decorate methods with `[Span]` for automatic tracing
2. **Virtual Methods**: Required for proxy interception to work
3. **DI Registration**: Use `AddScopedWithSpan<>` to enable interception
4. **Configurable**: Control span name, arguments, return value, and tags

## Basic Usage

```csharp
using Adapters.Telemetry;

public class ItemService : IItemService
{
    // Basic span - name: "ItemService.GetByIdAsync"
    [Span]
    public virtual async Task<Item> GetByIdAsync(Guid id)
    {
        // Method implementation
    }
}
```

## Span Attribute Options

```csharp
/// <summary>
/// Basic span with default name (ClassName.MethodName)
/// </summary>
[Span]
public virtual async Task<Item> GetAllAsync() { ... }

/// <summary>
/// Custom span name
/// </summary>
[Span("CustomOperationName")]
public virtual async Task<Item> GetByIdAsync(Guid id) { ... }

/// <summary>
/// Include method arguments as span tags
/// Useful for debugging, but be careful with sensitive data
/// </summary>
[Span(IncludeArguments = true)]
public virtual async Task<Item> CreateAsync(ItemRequest request) { ... }

/// <summary>
/// Include return value as span tag
/// </summary>
[Span(IncludeReturnValue = true)]
public virtual async Task<Item> GetByIdAsync(Guid id) { ... }

/// <summary>
/// Specify activity kind for external calls
/// </summary>
[Span(Kind = ActivityKind.Client)]
public virtual async Task<Data> CallExternalApiAsync() { ... }

/// <summary>
/// Add custom tags to the span
/// </summary>
[Span(Tags = "operation=read,entity=item")]
public virtual async Task<Item> GetByIdAsync(Guid id) { ... }

/// <summary>
/// Disable exception recording (default: true)
/// </summary>
[Span(RecordException = false)]
public virtual async Task<Item> GetByIdAsync(Guid id) { ... }

/// <summary>
/// Combine multiple options
/// </summary>
[Span("CreateItem", IncludeArguments = true, Tags = "operation=create")]
public virtual async Task<Item> CreateAsync(ItemRequest request) { ... }
```

## DI Registration

```csharp
using Adapters.Telemetry;
using Microsoft.Extensions.DependencyInjection;

public static class ItemModule
{
    public static IServiceCollection AddItemModule(this IServiceCollection services)
    {
        // Use AddScopedWithSpan instead of AddScoped
        services.AddScopedWithSpan<IItemRepository, ItemRepository>();
        services.AddScopedWithSpan<IItemService, ItemService>();

        return services;
    }
}
```

## Available Registration Methods

```csharp
// Scoped lifetime (per-request)
services.AddScopedWithSpan<IService, Service>();

// Transient lifetime (new instance each time)
services.AddTransientWithSpan<IService, Service>();

// Singleton lifetime (single instance)
services.AddSingletonWithSpan<IService, Service>();

// With factory function
services.AddScopedWithSpan<IService, Service>(provider =>
    new Service(provider.GetRequiredService<IDependency>()));
```

## Span Tags Added Automatically

| Tag | Description |
|-----|-------------|
| `code.function` | Method name |
| `code.namespace` | Namespace |
| `code.class` | Class name |
| `method.argument.{name}` | Argument values (if IncludeArguments = true) |
| `method.return_value` | Return value (if IncludeReturnValue = true) |

## Activity Kinds

| Kind | Use Case |
|------|----------|
| `ActivityKind.Internal` | Default, internal operations |
| `ActivityKind.Client` | Outgoing HTTP/RPC calls |
| `ActivityKind.Server` | Incoming HTTP requests |
| `ActivityKind.Producer` | Message queue producer |
| `ActivityKind.Consumer` | Message queue consumer |

## Requirements

1. **Interface Required**: Service must implement an interface
2. **Virtual Methods**: Methods must be marked `virtual`
3. **DI Registration**: Must use `AddScopedWithSpan<>` (not `AddScoped<>`)
4. **Telemetry Enabled**: `OTEL_ENABLED=true` in configuration

## Best Practices

✅ **DO:**
- Use `[Span]` on all public service and repository methods
- Use `[Span(IncludeArguments = true)]` for create/update operations
- Use `[Span(Kind = ActivityKind.Client)]` for external API calls
- Mark methods as `virtual` for proxy interception

❌ **DON'T:**
- Use `[Span]` on non-virtual methods (won't be intercepted)
- Include sensitive data in arguments (passwords, tokens)
- Use `[Span]` on private methods (not needed)
- Forget the interface (proxy needs interface to wrap)

## Correlation with Logs

Spans automatically correlate with Serilog logs via TraceId/SpanId:

```csharp
// Log automatically includes TraceId and SpanId from current span
logger.LogInformation("Processing item {Id}", id);
```

Output includes trace context:
```json
{
  "Message": "Processing item abc123",
  "TraceId": "4bf92f3577b34da6a3ce929d0e0e4736",
  "SpanId": "00f067aa0ba902b7"
}
```
