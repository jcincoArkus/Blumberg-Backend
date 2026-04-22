# Logger Usage Examples

## 🎯 Sintaxis Zap-like con Key-Value Pairs

### **Import Necesario**

```csharp
using Adapters.Logger;  // Para usar los extension methods
using Microsoft.Extensions.Logging;
```

---

## 📝 **Ejemplos de Uso**

### **1. LogErrorWithProps - Error con propiedades**

```csharp
// Sintaxis Zap-like
logger.LogErrorWithProps("invalid username",
    "email", email);

// Múltiples propiedades con diferentes tipos
logger.LogErrorWithProps("database connection failed",
    "host", "localhost",
    "port", 5432,
    "database", "blumberg",
    "retryCount", 3,
    "isConnected", false);
```

**Output Consola (con colores):**
```
[22:44:49 ERR] invalid username
{
  "email": "admin@blumberg.com"
}

[22:44:50 ERR] database connection failed
{
  "host": "localhost",
  "port": 5432,
  "database": "blumberg",
  "retryCount": 3,
  "isConnected": false
}
```

**Colores en Consola:**
- 🟡 **Keys** (amarillo): `"email"`, `"host"`, `"port"`
- 🔵 **Strings** (cyan): `"admin@blumberg.com"`, `"localhost"`
- 🟢 **Numbers** (verde): `5432`, `3`
- 🟣 **Booleans** (magenta): `false`

**Output JSON (cuando LOG_FORMAT_JSON=true):**
```json
{
  "@t": "2026-02-05T22:44:49.1234567Z",
  "@mt": "invalid username",
  "@l": "Error",
  "email": "admin@blumberg.com"
}
```

---

### **2. LogErrorWithProps - Error con Exception**

```csharp
try {
    // código que falla
} catch (Exception ex) {
    logger.LogErrorWithProps(ex, "failed to save sensor", 
        "sensorId", sensorId, 
        "orgId", orgId);
}
```

**Output JSON:**
```json
{
  "@t": "2026-02-05T22:44:49.1234567Z",
  "@mt": "failed to save sensor",
  "@l": "Error",
  "@x": "System.InvalidOperationException: ...",
  "sensorId": "abc-123",
  "orgId": "org-456"
}
```

---

### **3. LogWarnWithProps - Warning con propiedades**

```csharp
logger.LogWarnWithProps("sensor reading exceeds threshold", 
    "sensorId", sensorId, 
    "value", 85.5, 
    "threshold", 80.0);
```

---

### **4. LogInfoWithProps - Information con propiedades**

```csharp
logger.LogInfoWithProps("user logged in", 
    "email", email, 
    "ipAddress", ipAddress, 
    "timestamp", DateTime.UtcNow);
```

---

### **5. LogDebugWithProps - Debug con propiedades**

```csharp
logger.LogDebugWithProps("processing batch", 
    "batchId", batchId, 
    "count", readings.Count, 
    "duration", elapsed.TotalMilliseconds);
```

---

### **6. LogCriticalWithProps - Critical/Fatal con propiedades**

```csharp
logger.LogCriticalWithProps("out of memory", 
    "availableMemory", availableMemory, 
    "requestedMemory", requestedMemory);

// Con excepción
logger.LogCriticalWithProps(ex, "application crash", 
    "component", "SensorProcessor");
```

---

## 🆚 **Comparación: Sintaxis Tradicional vs Zap-like**

### **Sintaxis Tradicional (Placeholders)**

```csharp
// ✅ Propiedades visibles en el mensaje
logger.LogError("Invalid username for {Email}", email);
logger.LogWarning("Sensor {SensorId} reading {Value} exceeds threshold {Threshold}", 
    sensorId, value, threshold);
```

**Output Consola:**
```
[22:44:49 ERR] Invalid username for admin@blumberg.com
[22:44:50 WRN] Sensor abc-123 reading 85.5 exceeds threshold 80.0
```

---

### **Sintaxis Zap-like (Key-Value Pairs)**

```csharp
// ✅ Propiedades ocultas en consola, visibles en JSON
logger.LogErrorWithProps("invalid username", 
    "email", email);

logger.LogWarnWithProps("sensor reading exceeds threshold", 
    "sensorId", sensorId, 
    "value", value, 
    "threshold", threshold);
```

**Output Consola:**
```
[22:44:49 ERR] invalid username
[22:44:50 WRN] sensor reading exceeds threshold
```

**Output JSON (ambos casos):**
```json
{
  "@mt": "invalid username",
  "email": "admin@blumberg.com"
}
```

---

## 🎯 **Cuándo Usar Cada Sintaxis**

| Sintaxis | Cuándo Usar | Ventaja |
|----------|-------------|---------|
| **Tradicional** (`{Placeholder}`) | Desarrollo, debugging, logs legibles | Mensaje descriptivo en consola |
| **Zap-like** (`"key", value`) | Producción, datos sensibles | Mensaje genérico, datos en JSON |

---

## ⚠️ **Reglas Importantes**

1. **Número par de argumentos**: Siempre pasa pares `key-value`
   ```csharp
   // ✅ CORRECTO
   logger.LogErrorWithProps("error", "key1", val1, "key2", val2);
   
   // ❌ INCORRECTO (número impar)
   logger.LogErrorWithProps("error", "key1", val1, "key2");
   // Lanza: ArgumentException
   ```

2. **Keys como strings**: Las keys deben ser strings
   ```csharp
   // ✅ CORRECTO
   logger.LogErrorWithProps("error", "email", email);
   
   // ⚠️ Se convierte a string automáticamente
   logger.LogErrorWithProps("error", 123, email);  // key = "123"
   ```

3. **Values pueden ser cualquier tipo**
   ```csharp
   logger.LogInfoWithProps("data", 
       "string", "value",
       "int", 42,
       "bool", true,
       "object", new { Id = 1, Name = "Test" },
       "array", new[] { 1, 2, 3 });
   ```

---

## 🚀 **Ejemplo Completo en Controller**

```csharp
using Adapters.Logger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly ApplicationDbContext _context;

    public AuthController(ILogger<AuthController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginRequest request)
    {
        var admin = await _context.Admins
            .FirstOrDefaultAsync(a => a.Email == request.Email);

        if (admin == null)
        {
            // Sintaxis Zap-like: mensaje genérico, email en JSON
            _logger.LogWarnWithProps("invalid username", 
                "email", request.Email, 
                "ipAddress", HttpContext.Connection.RemoteIpAddress?.ToString());
            return Unauthorized("Invalid credentials");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
        {
            _logger.LogWarnWithProps("invalid password", 
                "email", request.Email, 
                "adminId", admin.Id);
            return Unauthorized("Invalid credentials");
        }

        _logger.LogInfoWithProps("login successful", 
            "email", admin.Email, 
            "adminId", admin.Id, 
            "orgId", admin.OrganizationId);

        var token = GenerateToken(admin);
        return Ok(new { token });
    }
}
```

---

## 📊 **Output Comparison**

### **Consola (Desarrollo)**
```
[22:44:49 WRN] invalid username
[22:44:50 WRN] invalid password
[22:44:51 INF] login successful
```

### **JSON (Producción)**
```json
{"@t":"2026-02-05T22:44:49Z","@mt":"invalid username","@l":"Warning","email":"admin@blumberg.com","ipAddress":"192.168.1.100"}
{"@t":"2026-02-05T22:44:50Z","@mt":"invalid password","@l":"Warning","email":"admin@blumberg.com","adminId":"a1b2c3d4"}
{"@t":"2026-02-05T22:44:51Z","@mt":"login successful","@l":"Information","email":"admin@blumberg.com","adminId":"a1b2c3d4","orgId":"org-123"}
```

