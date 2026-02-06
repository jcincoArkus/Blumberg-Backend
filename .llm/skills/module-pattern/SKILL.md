---
name: module-pattern
description: Pattern for creating feature modules with Controller, Service, Repository, DTOs, and DI registration in ASP.NET Core. Use when adding a new feature or domain.
---

# Module Pattern

Each feature is a self-contained module under `Modules/{Feature}/`.

## Folder Structure

```
Modules/{Feature}/
├── {Feature}Module.cs          # DI registration
├── Controller/
│   └── {Feature}Controller.cs
├── Service/
│   ├── I{Feature}Service.cs
│   └── {Feature}Service.cs
├── Repository/
│   ├── I{Feature}Repository.cs
│   └── {Feature}Repository.cs
└── Dto/
    ├── {Feature}Request.cs
    ├── {Feature}Response.cs
    └── PagedResponse.cs        # Only if module needs pagination
```

## Module Registration

```csharp
using Adapters.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Modules.Items.Repository;
using Modules.Items.Service;

namespace Modules.Items;

public static class ItemModule
{
    public static IServiceCollection AddItemModule(this IServiceCollection services)
    {
        services.AddScopedWithSpan<IItemRepository, ItemRepository>();
        services.AddScopedWithSpan<IItemService, ItemService>();
        return services;
    }
}
```

## Register in ModulesSetup.cs

Add two things in `Modules/ModulesSetup.cs`:

1. Call `services.AddItemModule()` in `AddApplicationModules()`
2. Yield the controller assembly in `GetControllerAssemblies()`

```csharp
public static IServiceCollection AddApplicationModules(this IServiceCollection services)
{
    // ... existing modules
    services.AddItemModule();
    return services;
}

public static IEnumerable<Assembly> GetControllerAssemblies()
{
    // ... existing assemblies
    yield return typeof(Items.Controller.ItemController).Assembly;
}
```

## Checklist

1. Create entity in `Shared/Entity/` (see entity-pattern skill)
2. Add `DbSet<T>` + global query filters in `ApplicationDbContext` (see ef-configuration-pattern skill)
3. Create folder structure under `Modules/{Feature}/`
4. Create Repository interface + implementation
5. Create Service interface + implementation
6. Create Request/Response DTOs
7. Create Controller
8. Create `{Feature}Module.cs` with DI registration using `AddScopedWithSpan<>`
9. Register in `ModulesSetup.cs`
10. Add EF migration: `dotnet ef migrations add AddItem --project Adapters/Database --startup-project Apps/API`

## Key Rules

- **Always use `AddScopedWithSpan<>`** — not `AddScoped<>` — for automatic OpenTelemetry proxying
- **All interfaces required** — Castle.DynamicProxy needs interfaces to create proxies
- **Don't cross-reference modules directly** — use shared abstractions in `Shared/`
