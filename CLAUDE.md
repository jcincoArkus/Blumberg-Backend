# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

**Build:**
```bash
dotnet build
```

**Run API locally:**
```bash
dotnet run --project Apps/API
```

**Run with Docker:**
```bash
docker compose -f compose.yml up -d --build
```

**Database initialization (reset + migrate + seed):**
```bash
# Local
./scripts/cli nukeAndPave

# Inside Docker (Windows)
./scripts/cli-docker.ps1 nukeAndPave

# Inside Docker (Unix)
./scripts/cli-docker nukeAndPave
```

**EF Core migrations:**
```bash
# Add a migration
dotnet ef migrations add MigrationName --project Adapters/Database

# Generate SQL script
dotnet ef migrations script --project Adapters/Database
```

**Connect to ECS container (production):**
```bash
./scripts/connect-ecs-exec.ps1   # Windows
./scripts/connect-ecs-exec.sh    # Unix
```

## Architecture

This is a **.NET 10 ASP.NET Core Web API** following **Clean Architecture**. The solution is split into four top-level layers:

### `Apps/`
Entry points:
- **API** — The HTTP web API. `Program.cs` bootstraps via `ApiServer.Create(args).Run()`.
- **CLI** — Used internally for database operations (`nukeAndPave`, simulations).

### `Adapters/`
Infrastructure glue — no domain logic lives here:
- **Server** — ASP.NET Core setup, middleware, auth handlers, module registration.
- **Database** — `ApplicationDbContext`, EF Core configurations (`Configurations/`), Fluent API schemas (`Schemas/`), migrations (`Migrations/`), and dev seeders (`Seeders/`).
- **Config** — Reads `.env` and maps to typed config objects.
- **Jwt / Permissions** — JWT auth and Casbin RBAC.
- **Email** — SMTP + AWS SES/SQS integration.
- **Telemetry / Logger** — OpenTelemetry (Datadog/OTLP) and structured JSON logging.

### `Modules/`
Feature modules, each self-contained with `Controller/`, `Service/`, `Repository/`, and `Dto/` subdirectories:
- Auth, Admins, Sites, Equipment, Sensors, SensorTypes, Thresholds, Alerts, Ingestion, Inventory, Permissions

New modules are registered in `Adapters/Server/Modules/ModulesSetup.cs`.

### `Shared/`
Cross-cutting concerns used by all modules:
- **Entity/** — EF Core entity models.
- **Dto/** — Request/response contracts.
- **Abstractions/** — Base interfaces and generic repository/service patterns.
- **Constants/** — Enums and app-wide constants.
- **Permissions/** — Permission name definitions referenced by Casbin.

## Database

- **Engine:** PostgreSQL 17
- **ORM:** EF Core 10 (Npgsql provider)
- Migrations are **not auto-applied** — always run `nukeAndPave` or apply manually in dev.
- Entity configurations live in `Adapters/Database/Configurations/` (Fluent API).
- Dev seeders (`Adapters/Database/Seeders/`) are idempotent and run automatically in `Development` environment.

**Default local connection (`.env`):**
```
DB_HOST=localhost
DB_PORT=5432
DB_NAME=blumberg
DB_USER=blumberg
DB_PASSWORD=blumberg
```

Copy `.env.example` to `.env` and adjust values for your environment.

## Authentication

Two auth schemes run in parallel:
- **JWT** — Used by human users. Login via `POST /auth/login`, then `Authorization: Bearer <token>`.
- **API Key** — Used by M2M data ingestion. Pass via `X-API-Key` header.

Authorization uses **Casbin RBAC**. Permission names are defined in `Shared/Permissions/` and enforced in `Adapters/Permissions/`.

## Adding a New Module

1. Create `Modules/<Name>/` with `Controller/`, `Service/`, `Repository/`, `Dto/` subdirectories.
2. Add EF entity in `Shared/Entity/` and configure in `Adapters/Database/Configurations/`.
3. Register the module in `Adapters/Server/Modules/ModulesSetup.cs`.
4. Add permissions in `Shared/Permissions/` and seed Casbin policies.
5. Create and apply an EF migration.

## Environment Variables

See `.env.example` for all variables. Key groups:
- `ASPNETCORE_*` — Runtime and URL binding.
- `DB_*` — PostgreSQL connection.
- `JWT_*` — Token signing and expiry.
- `LOG_*` / `OTEL_*` — Observability.
- `SMTP_*` / `ALERT_EMAIL_PROVIDER` — Email notifications.
- `WORKER_QUEUE_URL` — AWS SQS (production only).

## API Documentation

- Swagger UI is available at `/swagger` when running locally.
- `INVENTORY_API.md` documents the inventory endpoints with request/response examples.
- `inventory-postman-collection.json` is a ready-to-import Postman collection.
