# Domain Enums

All domain enums in this folder are persisted as **strings** in the database (VARCHAR 50).

- **Adding new enum values does NOT require a migration** — only new columns or tables do.
- EF Core is configured (in `ApplicationDbContext` and entity schemas) to use `HasConversion<string>().HasMaxLength(50)` for these enums.
- Use these enums on entities and DTOs; they will be stored and read as their string names (e.g. `Active`, `Temperature`, `Celsius`).

## Enums

| Enum | Values |
|------|--------|
| **SensorStatus** | Active, Inactive, Maintenance, Unknown |
| **SensorTypeKind** | Temperature, Humidity, Co2, O2, Pressure, Energy, Custom |
| **Unit** | Celsius, Fahrenheit, Percent, Ppm, Psi, Bar, Kw, Kwh, Custom |
| **IngestionSource** | Api, Csv, Simulated |
| **IngestionStatus** | Pending, InProgress, Success, PartialSuccess, Failed |
| **SensorHealthStatus** | Healthy, Warning, Critical, Stale, Offline |

**Note:** The enum is named **SensorTypeKind** to avoid conflict with the `Shared.Entity.SensorType` entity.
