# Adapters.Config

Configuration adapter for Blumberg API, inspired by [gookit/config](https://github.com/gookit/config) (Go) and [node-config](https://www.npmjs.com/package/config) (Node.js).

## Features

- ✅ **Type-safe configuration** - Strongly typed configuration classes
- ✅ **Environment variables** - Load from .env files and environment variables
- ✅ **Self-validating** - Each config validates itself on initialization
- ✅ **Fluent API** - Chain `Init().Validate()` for clean initialization
- ✅ **Singleton pattern** - Configuration loaded once and cached
- ✅ **Dependency Injection** - Easy integration with ASP.NET Core DI

## Configuration Sections

### DatabaseConfig
PostgreSQL database connection settings.

**Environment Variables:**
- `DB_HOST` - Database host (default: `localhost`)
- `DB_PORT` - Database port (default: `5432`)
- `DB_NAME` - Database name (required)
- `DB_USER` - Database username (required)
- `DB_PASSWORD` - Database password (required)

### JwtConfig
JWT authentication settings.

**Environment Variables:**
- `JWT_SECRET_KEY` - Secret key for signing tokens (required, min 32 chars)
- `JWT_ISSUER` - Token issuer (default: `Blumberg.API`)
- `JWT_AUDIENCE` - Token audience (default: `Blumberg.Client`)
- `JWT_EXPIRATION_HOURS` - Token expiration in hours (default: `24`)

### ApplicationConfig
General application settings.

**Environment Variables:**
- `ASPNETCORE_ENVIRONMENT` - Environment name (default: `Development`)
- `ASPNETCORE_URLS` - URLs to listen on (default: `http://localhost:5000`)
- `APP_NAME` - Application name (default: `Blumberg API`)
- `APP_VERSION` - Application version (default: `1.0.0`)

## Usage

### 1. Create .env file

```bash
# Database
DB_HOST=localhost
DB_PORT=5432
DB_NAME=blumberg
DB_USER=blumberg
DB_PASSWORD=your-secure-password

# JWT
JWT_SECRET_KEY=your-super-secret-jwt-key-min-32-characters-long
JWT_ISSUER=Blumberg.API
JWT_AUDIENCE=Blumberg.Client
JWT_EXPIRATION_HOURS=24

# Application
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5000
```

### 2. Load configuration

```csharp
using Adapters.Config;

// Load configuration (singleton)
var config = ConfigLoader.Load();

// Access configuration
var connectionString = config.Database.GetConnectionString();
var jwtSecret = config.Jwt.SecretKey;
var isDev = config.Application.IsDevelopment;
```

### 3. Use with Dependency Injection

```csharp
using Adapters.Config;

var builder = WebApplication.CreateBuilder(args);

// Register configuration
builder.Services.AddAppConfiguration();

// Now you can inject AppConfig, DatabaseConfig, JwtConfig, or ApplicationConfig
public class MyService
{
    private readonly DatabaseConfig _dbConfig;

    public MyService(DatabaseConfig dbConfig)
    {
        _dbConfig = dbConfig;
    }

    public void Connect()
    {
        var connectionString = _dbConfig.GetConnectionString();
        // ...
    }
}
```

## Architecture

Each configuration class follows the **self-initializing pattern**:

```csharp
// Each config is responsible for its own initialization and validation
Database = new DatabaseConfig().Init().Validate(),
Jwt = new JwtConfig().Init().Validate(),
Application = new ApplicationConfig().Init().Validate()
```

**Benefits:**
- ✅ **Single Responsibility** - Each config manages itself
- ✅ **Maintainable** - Easy to add new configs without modifying ConfigLoader
- ✅ **Testable** - Each config can be tested independently
- ✅ **Fluent** - Clean, readable initialization chain
- ✅ **Immutable** - Properties have private setters after initialization

## Adding New Configuration

1. Create a new config class (e.g., `RedisConfig.cs`)
2. Add `Init()` method to load from environment variables
3. Add `Validate()` method to validate the configuration
4. Add property to `AppConfig.cs`
5. Initialize in `ConfigLoader.Load()`

Example:

```csharp
public class RedisConfig
{
    public string Host { get; private set; } = "localhost";
    public int Port { get; private set; } = 6379;

    public RedisConfig Init()
    {
        Host = EnvHelper.GetEnv("REDIS_HOST", "localhost");
        Port = EnvHelper.GetEnvInt("REDIS_PORT", 6379);
        return this;
    }

    public RedisConfig Validate()
    {
        if (Port <= 0 || Port > 65535)
            throw new InvalidOperationException("Invalid Redis port");
        return this;
    }
}
```

Then in `ConfigLoader.cs`:

```csharp
_config = new AppConfig
{
    Database = new DatabaseConfig().Init().Validate(),
    Jwt = new JwtConfig().Init().Validate(),
    Application = new ApplicationConfig().Init().Validate(),
    Redis = new RedisConfig().Init().Validate() // ← Add here
};
```

