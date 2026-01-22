# Adapters.OpenApi

OpenAPI/Swagger adapter for Blumberg API with automatic JWT Bearer authentication support.

## Features

- ✅ **Swagger UI** - Interactive API documentation
- ✅ **JWT Bearer Authentication** - Automatic security scheme configuration
- ✅ **Auto-detection** - Automatically adds auth requirements to `[Authorize]` endpoints
- ✅ **Deep Linking** - Direct links to specific operations
- ✅ **Request Duration** - Display request execution time
- ✅ **XML Comments** - Support for XML documentation comments from multiple assemblies
- ✅ **YAML Export** - Export OpenAPI spec to YAML file
- ✅ **Customizable** - Easy to configure title, version, and description

## Components

### AuthorizeCheckOperationFilter

Operation filter that automatically detects endpoints with `[Authorize]` attribute and adds JWT Bearer authentication requirement in Swagger UI.

**How it works:**
- Scans controller and action methods for `[Authorize]` attribute
- Respects `[AllowAnonymous]` attribute (overrides `[Authorize]`)
- Automatically adds "Authorize" button in Swagger UI for protected endpoints

### OpenApiSetup

Extension methods for easy Swagger configuration.

**Methods:**
- `AddOpenApiDocumentation()` - Registers Swagger services
- `UseOpenApiDocumentation()` - Configures Swagger UI middleware

## Usage

### 1. In Program.cs

```csharp
using System.Reflection;
using Adapters.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add Swagger with JWT support and XML documentation
builder.Services.AddOpenApiDocumentation(
    title: "Blumberg API",
    version: "v1",
    description: "Enterprise backend API for Blumberg",
    xmlDocAssemblies: [
        Assembly.GetExecutingAssembly(),  // API project
        typeof(Modules.Admin.AdminController).Assembly  // Admin module
    ]
);

var app = builder.Build();

// Enable Swagger UI (typically only in Development)
if (app.Environment.IsDevelopment())
{
    app.UseOpenApiDocumentation(
        routePrefix: "swagger",
        documentTitle: "Blumberg API Documentation"
    );
}

// Export OpenAPI spec to YAML (optional, via command-line flag)
if (args.Contains("--export-openapi"))
{
    app.ExportOpenApiSpec("docs/openapi.yml");
    return;
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 2. Enable XML Documentation in .csproj

To enable XML comments in Swagger, add this to your API and Module projects:

```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn> <!-- Suppress missing XML comment warnings -->
</PropertyGroup>
```

### 3. In Controllers (with XML comments)

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Admin management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    /// <summary>
    /// Gets public data (no authentication required)
    /// </summary>
    /// <returns>Public data</returns>
    /// <response code="200">Returns public data</response>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetPublicData()
    {
        return Ok("Public data");
    }

    /// <summary>
    /// Gets protected data (requires JWT token)
    /// </summary>
    /// <returns>Protected data</returns>
    /// <response code="200">Returns protected data</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    [HttpGet("protected")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetProtectedData()
    {
        return Ok("Protected data");
    }
}
```

### 4. Using Swagger UI

1. Navigate to `http://localhost:5000/swagger`
2. Click the **"Authorize"** button (lock icon)
3. Enter your JWT token in the format: `Bearer {your-token}`
4. Click **"Authorize"**
5. Now you can test protected endpoints

### 5. Exporting OpenAPI Spec to YAML

You can export the OpenAPI specification to a YAML file for documentation or client generation:

```bash
# Export to default location (openapi.yml)
dotnet run --export-openapi

# Or specify custom path in Program.cs
app.ExportOpenApiSpec("docs/api-spec.yml");
```

This is useful for:
- **Version control** - Track API changes over time
- **Client generation** - Generate SDKs with tools like OpenAPI Generator
- **Documentation** - Share API spec with external teams
- **Contract testing** - Validate API responses against the spec

## Configuration Options

### AddOpenApiDocumentation

```csharp
services.AddOpenApiDocumentation(
    title: "My API",                    // API title
    version: "v1",                      // API version
    description: "API Description",     // Optional description
    xmlDocAssemblies: [                 // Optional: assemblies with XML docs
        Assembly.GetExecutingAssembly(),
        typeof(MyModule.MyController).Assembly
    ]
);
```

### UseOpenApiDocumentation

```csharp
app.UseOpenApiDocumentation(
    routePrefix: "swagger",                    // URL: /swagger
    documentTitle: "My API Documentation"      // Browser tab title
);
```

### ExportOpenApiSpec

```csharp
app.ExportOpenApiSpec(
    outputPath: "docs/openapi.yml",  // Output file path
    version: "v1"                     // API version to export
);
```

## Swagger UI Features

The configuration enables several useful features:

- **Deep Linking** - Share direct links to specific operations
- **Request Duration** - See how long each request takes
- **Filter** - Search/filter operations
- **Validator** - Validate API responses against schema
- **Persistent Authorization** - Token persists across page refreshes (in browser storage)

## Security Notes

⚠️ **Production Considerations:**

1. **Disable Swagger in Production** - Only enable in Development/Staging
2. **Protect Swagger UI** - If needed in production, add authentication
3. **HTTPS Only** - Always use HTTPS in production
4. **Token Security** - Tokens in Swagger UI are stored in browser localStorage

```csharp
// Only enable in Development
if (app.Environment.IsDevelopment())
{
    app.UseOpenApiDocumentation();
}
```

## Example: Complete Setup

```csharp
using Adapters.Config;
using Adapters.Jwt;
using Adapters.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
builder.Services.AddAppConfiguration();
var config = builder.Services.BuildServiceProvider().GetRequiredService<AppConfig>();

// Add controllers
builder.Services.AddControllers();

// Add JWT authentication
builder.Services.AddJwtAuthentication(
    secretKey: config.Jwt.SecretKey,
    issuer: config.Jwt.Issuer,
    audience: config.Jwt.Audience,
    expirationHours: config.Jwt.ExpirationHours
);

// Add Swagger
builder.Services.AddOpenApiDocumentation(
    title: config.Application.Name,
    version: config.Application.Version
);

var app = builder.Build();

// Enable Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseOpenApiDocumentation();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

