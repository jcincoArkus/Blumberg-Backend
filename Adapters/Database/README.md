# Adapters.Database

PostgreSQL database adapter using Entity Framework Core with Npgsql provider.

## Features

- ✅ **PostgreSQL** - Npgsql provider for EF Core
- ✅ **Manual Entity Configuration** - Explicit Fluent API configurations for full control
- ✅ **Retry on Failure** - Automatic retry logic for transient failures
- ✅ **Connection Testing** - Built-in connection testing utilities
- ✅ **Development Logging** - Detailed logging in development mode
- ✅ **Configuration Integration** - Works seamlessly with Adapters.Config
- ✅ **No Auto-Migrations** - Migrations managed via CLI tools and scripts

## Components

### ApplicationDbContext

Main database context that manages all entity sets and database operations.

**Key Features:**
- Applies all entity configurations from assembly (IEntityTypeConfiguration)
- Override `SaveChangesAsync` for custom logic (timestamps, soft deletes, etc.)
- Clean and simple - entity configurations are in separate files

### DatabaseSetup

Extension methods for easy database configuration in dependency injection.

**Methods:**
- `AddDatabase()` - Registers DbContext with PostgreSQL
- `TestDatabaseConnectionAsync()` - Tests database connectivity

## Usage

### 1. In Program.cs

```csharp
using Adapters.Config;
using Adapters.Database;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
builder.Services.AddAppConfiguration();
var config = builder.Services.BuildServiceProvider().GetRequiredService<AppConfig>();

// Add database
builder.Services.AddDatabase(
    config,
    isDevelopment: builder.Environment.IsDevelopment()
);

var app = builder.Build();

// Test connection (optional)
await app.Services.TestDatabaseConnectionAsync();

app.Run();
```

### 2. Alternative: Direct Connection String

```csharp
builder.Services.AddDatabase(
    connectionString: "Host=localhost;Port=5432;Database=blumberg;Username=blumberg;Password=secret",
    enableSensitiveDataLogging: true,  // Development only
    enableDetailedErrors: true          // Development only
);
```

### 3. Using DbContext in Services

```csharp
public class AdminService
{
    private readonly ApplicationDbContext _dbContext;

    public AdminService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Admin?> GetAdminByIdAsync(Guid id)
    {
        return await _dbContext.Admins
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Admin> CreateAdminAsync(Admin admin)
    {
        _dbContext.Admins.Add(admin);
        await _dbContext.SaveChangesAsync();
        return admin;
    }
}
```

## Entity Configuration

Entity configurations are defined using Fluent API in separate configuration classes.
This follows PostgreSQL snake_case naming conventions.

**Example Configuration:**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adapters.Database.Configurations;

public class AdminConfiguration : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        builder.ToTable("admins");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(255);
        builder.HasIndex(e => e.Email).IsUnique();

        builder.Property(e => e.FirstName).HasColumnName("first_name").IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).HasColumnName("last_name").IsRequired().HasMaxLength(100);

        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");
    }
}
```

**Resulting PostgreSQL table:**

```sql
CREATE TABLE admins (
    id UUID PRIMARY KEY,
    email VARCHAR(255) NOT NULL UNIQUE,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP,
    deleted_at TIMESTAMP
);
```

## Configuration

### Environment Variables

See `Adapters.Config` for database configuration:

```env
DB_HOST=localhost
DB_PORT=5432
DB_NAME=blumberg
DB_USER=blumberg
DB_PASSWORD=your-secure-password
```

### Connection String Format

```
Host={DB_HOST};Port={DB_PORT};Database={DB_NAME};Username={DB_USER};Password={DB_PASSWORD}
```

## Migrations

⚠️ **Migrations are NOT applied automatically**. They will be managed via CLI tools and SQL scripts.

### Create Migration

```bash
# From the root directory
dotnet ef migrations add InitialCreate --project Adapters/Database --startup-project API

# Or from the Database project
cd Adapters/Database
dotnet ef migrations add InitialCreate
```

### Generate SQL Script

```bash
# Generate SQL script from migrations
dotnet ef migrations script --project Adapters/Database --output migrations.sql

# Generate script for specific migration range
dotnet ef migrations script FromMigration ToMigration --project Adapters/Database
```

### Apply Migrations (Manual)

Migrations should be applied manually using SQL scripts or a dedicated CLI tool (to be created later).

```bash
# Example: Apply via psql
psql -h localhost -U blumberg -d blumberg -f migrations.sql
```

### Remove Last Migration

```bash
dotnet ef migrations remove --project Adapters/Database
```

## Database Utilities

### Test Connection

```csharp
var canConnect = await app.Services.TestDatabaseConnectionAsync();
if (!canConnect)
{
    throw new Exception("Cannot connect to database");
}
```

## Next Steps

After setting up the database adapter, you'll typically:

1. **Create Shared/BaseEntity** - Base class for all entities (Id, CreatedAt, UpdatedAt, etc.)
2. **Create Entity Configurations** - Fluent API configurations in separate files
3. **Add DbSets** - Register entities in ApplicationDbContext
4. **Create Migrations** - Generate migrations with `dotnet ef migrations add`
5. **Generate SQL Scripts** - Export migrations to SQL files
6. **Create CLI Tool** - Build migration management CLI (planned for later)
7. **Create Repositories** - Data access layer (optional, can use DbContext directly)

## Dependencies

- Microsoft.EntityFrameworkCore 10.0.2
- Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0
- Microsoft.EntityFrameworkCore.Design 10.0.2
- Adapters.Config (project reference)

