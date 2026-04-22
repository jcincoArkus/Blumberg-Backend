---
name: migrations-pattern
description: Pattern for adding and applying EF Core migrations in this backend. Use when creating or modifying database migrations, when migration:up reports no pending migrations but a new migration file exists, or when working in Adapters/Database/Migrations.
---

# Migrations Pattern

Migrations live in `Adapters/Database/Migrations/`. This project uses the **CLI** to generate and apply them.

## Preferred: Generate via CLI

From **backend** root:

```bash
scripts/cli migration:generate <MigrationName>
```

Example: `scripts/cli migration:generate AddSensorIdToIngestionRejectedReading`

This produces both the migration `.cs` (Up/Down) and the `.Designer.cs` (target model). **EF Core only discovers a migration when both files exist** and the Designer has the `[Migration("timestamp_MigrationName")]` attribute.

## Applying Migrations

From **backend** root:

```bash
scripts/cli migration:status   # list applied and pending
scripts/cli migration:up      # apply pending
scripts/cli migration:down    # rollback last applied
```

## Why “No pending migrations” with a new .cs file?

EF treats a migration as pending only if:

1. The migration class file exists (e.g. `20260223200000_AddFoo.cs`) with `Up()` / `Down()`.
2. A **.Designer.cs** for that migration exists with:
   - `[Migration("20260223200000_AddFoo")]`
   - `partial class AddFoo` and `BuildTargetModel(ModelBuilder modelBuilder)` reflecting the model *after* that migration.

Without the Designer, the migration is never registered and never appears as pending.

## Adding a migration manually (no dotnet-ef)

If you create only the `.cs` file (e.g. hand-written or copied), add the Designer:

1. Copy the **latest** migration’s `.Designer.cs` (same folder).
2. Rename the copy to match your migration: `{Timestamp}_{MigrationName}.Designer.cs`.
3. In the copy:
   - Set `[Migration("Timestamp_MigrationName")]` to your migration id.
   - Change `partial class PreviousName` to `partial class YourMigrationName`.
   - In `BuildTargetModel`, update any entity blocks that changed (e.g. new column/index) so they match the **current** `ApplicationDbContextModelSnapshot.cs` for those entities.
4. Ensure the main migration class is `public partial class YourMigrationName : Migration`.

Result: EF sees the migration and it shows up as pending for `migration:up`.

## Conventions

- Run migration commands from the **backend** directory so the CLI finds `Adapters/Database`.
- Keep `ApplicationDbContextModelSnapshot.cs` in sync with the latest migration’s target model (the snapshot is updated when you generate via the CLI).
- Migration timestamp format: `yyyyMMddHHmmss` (e.g. `20260223200000`).
