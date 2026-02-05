---
name: module-pattern
description: Pattern for creating feature modules with Controller, Service, Repository, DTOs, and DI registration in ASP.NET Core. Use when adding a new feature or domain.
---

# Module Pattern

## Overview

Pattern for creating **feature modules** in a modular monolith architecture with **ASP.NET Core**. Each module contains Controller, Service, Repository, DTOs, and DI registration.

## Key Principles

1. **Self-Contained**: Each module owns its Controller, Service, Repository, and DTOs
2. **Interface-Based**: Services and repositories have interfaces for testability
3. **DI Registration**: Each module has a static extension method for registration
4. **Span Instrumentation**: Use `AddScopedWithSpan<>` for automatic tracing

## Module Structure

```
Modules/
└── Items/
    ├── ItemModule.cs              # DI registration
    ├── Controller/
    │   └── ItemController.cs      # HTTP endpoints
    ├── Service/
    │   ├── IItemService.cs        # Interface
    │   └── ItemService.cs         # Implementation
    ├── Repository/
    │   ├── IItemRepository.cs     # Interface
    │   └── ItemRepository.cs      # Implementation
    └── Dto/
        ├── ItemRequest.cs         # Input DTO
        ├── ItemResponse.cs        # Output DTO
        └── PagedResponse.cs       # Pagination wrapper
```

## Template: Module Registration (ItemModule.cs)

```csharp
using Adapters.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Modules.Items.Repository;
using Modules.Items.Service;

namespace Modules.Items;

/// <summary>
/// Extension methods for registering Items module services
/// </summary>
public static class ItemModule
{
    /// <summary>
    /// Adds Items module services to the service collection
    /// </summary>
    public static IServiceCollection AddItemModule(this IServiceCollection services)
    {
        services.AddScopedWithSpan<IItemRepository, ItemRepository>();
        services.AddScopedWithSpan<IItemService, ItemService>();

        return services;
    }
}
```

## Template: Register in ModulesSetup.cs

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace Modules;

public static class ModulesSetup
{
    public static IServiceCollection AddApplicationModules(this IServiceCollection services)
    {
        services.AddAuthModule();
        services.AddItemModule();  // Add new module here
        // ... other modules

        return services;
    }
}
```

## Checklist: Creating a New Module

1. Create folder structure under `Modules/{ModuleName}/`
2. Create `I{ModuleName}Repository.cs` interface
3. Create `{ModuleName}Repository.cs` implementation
4. Create `I{ModuleName}Service.cs` interface
5. Create `{ModuleName}Service.cs` implementation
6. Create `{ModuleName}Request.cs` DTO
7. Create `{ModuleName}Response.cs` DTO
8. Create `{ModuleName}Controller.cs`
9. Create `{ModuleName}Module.cs` with DI registration
10. Register module in `ModulesSetup.cs`

## Best Practices

✅ **DO:**
- Use `AddScopedWithSpan<>` for automatic OpenTelemetry instrumentation
- Keep modules self-contained with all related code together
- Use interfaces for all services and repositories
- Follow naming convention: `I{Name}Service`, `{Name}Service`

❌ **DON'T:**
- Reference other modules directly (use shared abstractions)
- Put business logic in controllers
- Skip the interface (needed for testing and span interception)
- Forget to register the module in `ModulesSetup.cs`
