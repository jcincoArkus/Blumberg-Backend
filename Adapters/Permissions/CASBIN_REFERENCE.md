# Casbin RBAC Reference

## Overview
This document describes the complete RBAC model implemented with Casbin.NET for the Blumberg backend.

## Model Structure

### Request Format
```
r = sub, obj, act
```
- **sub** (subject): The user's role (admin, standard, readonly)
- **obj** (object): The resource being accessed
- **act** (action): The operation being performed (read, write, acknowledge)

### Policy Format
```
p = sub, obj, act
```
Defines what actions a role can perform on a resource.

### Role Inheritance
```
g = _, _
```
Maps users to roles (managed dynamically via database).

---

## Subjects (Roles)

| Role ID | Name | Description |
|---------|------|-------------|
| `admin` | Administrator | Full access to all resources and operations |
| `standard` | Standard User | Read/write access to operational resources, read-only for admin functions |
| `readonly` | Read-Only User | Read-only access to most resources, no write permissions |

---

## Objects (Resources)

Based on the 6 permission categories from the frontend:

### 1. Overview Category
| Object | Description |
|--------|-------------|
| `system-overview` | System-wide dashboard and statistics |
| `site-overview` | Site-specific overview and statistics |

### 2. Equipment Category
| Object | Description |
|--------|-------------|
| `equipment` | Equipment management (CRUD operations) |
| `equipment-config` | Equipment configuration settings |

### 3. Alerts & Events Category
| Object | Description |
|--------|-------------|
| `alerts` | Alert viewing and management |
| `alerts-config` | Alert rules and notification configuration |

### 4. Sensors Category
| Object | Description |
|--------|-------------|
| `sensors` | Sensor management (CRUD operations) |
| `sensor-health` | Sensor health monitoring and diagnostics |

### 5. Data Management Category
| Object | Description |
|--------|-------------|
| `ingestion` | Data ingestion runs and management |
| `reports` | Historical reports and analytics |

### 6. Administration Category
| Object | Description |
|--------|-------------|
| `users` | User management (CRUD operations) |
| `roles` | Role and permission management |

---

## Actions (Operations)

| Action | Description | Typical HTTP Methods |
|--------|-------------|---------------------|
| `read` | View/retrieve resource data | GET |
| `write` | Create, update, or delete resources | POST, PUT, DELETE |
| `acknowledge` | Special action for acknowledging alerts | POST |

---

## Permission Matrix

### Admin Role
✅ **Full Access** - All resources, all actions

| Resource | read | write | acknowledge |
|----------|------|-------|-------------|
| system-overview | ✅ | ✅ | - |
| site-overview | ✅ | ✅ | - |
| equipment | ✅ | ✅ | - |
| equipment-config | ✅ | ✅ | - |
| alerts | ✅ | ✅ | ✅ |
| alerts-config | ✅ | ✅ | - |
| sensors | ✅ | ✅ | - |
| sensor-health | ✅ | ✅ | - |
| ingestion | ✅ | ✅ | - |
| reports | ✅ | ✅ | - |
| users | ✅ | ✅ | - |
| roles | ✅ | ✅ | - |

### Standard Role
📝 **Operational Access** - Read/write on operations, read-only on admin

| Resource | read | write | acknowledge |
|----------|------|-------|-------------|
| system-overview | ✅ | ❌ | - |
| site-overview | ✅ | ❌ | - |
| equipment | ✅ | ❌ | - |
| equipment-config | ✅ | ❌ | - |
| alerts | ✅ | ❌ | ✅ |
| alerts-config | ✅ | ❌ | - |
| sensors | ✅ | ❌ | - |
| sensor-health | ✅ | ❌ | - |
| ingestion | ✅ | ❌ | - |
| reports | ✅ | ❌ | - |
| users | ✅ | ❌ | - |
| roles | ✅ | ❌ | - |

### ReadOnly Role
👁️ **View Only** - Read-only access to all resources

| Resource | read | write | acknowledge |
|----------|------|-------|-------------|
| system-overview | ✅ | ❌ | - |
| site-overview | ✅ | ❌ | - |
| equipment | ✅ | ❌ | - |
| equipment-config | ✅ | ❌ | - |
| alerts | ✅ | ❌ | ❌ |
| alerts-config | ✅ | ❌ | - |
| sensors | ✅ | ❌ | - |
| sensor-health | ✅ | ❌ | - |
| ingestion | ✅ | ❌ | - |
| reports | ✅ | ❌ | - |
| users | ✅ | ❌ | - |
| roles | ✅ | ❌ | - |

---

## Usage Examples

### Check Permission in Code
```csharp
// Check if user with role "standard" can write to equipment
bool allowed = await enforcer.EnforceAsync("standard", "equipment", "write");
// Returns: false

// Check if user with role "admin" can write to equipment
bool allowed = await enforcer.EnforceAsync("admin", "equipment", "write");
// Returns: true

// Check if user with role "standard" can acknowledge alerts
bool allowed = await enforcer.EnforceAsync("standard", "alerts", "acknowledge");
// Returns: true
```

### Controller Authorization
```csharp
[HttpPost]
[Authorize]
[RequirePermission("equipment", "write")]
public async Task<IActionResult> CreateEquipment([FromBody] EquipmentRequest request)
{
    // Only users with write permission on equipment can access this
}

[HttpGet]
[Authorize]
[RequirePermission("equipment", "read")]
public async Task<IActionResult> GetEquipment([FromRoute] Guid id)
{
    // Users with read permission on equipment can access this
}
```

---

## Files

- **`casbin_model.conf`** - Casbin model definition
- **`casbin_policy.csv`** - Policy rules (role → resource → action)
- **`casbin_roles.csv`** - Role assignments (user → role) - managed dynamically
- **`CASBIN_REFERENCE.md`** - This documentation

---

## Next Steps

1. ✅ Model and policies created
2. ⏳ Install Casbin.NET NuGet package
3. ⏳ Create Casbin service integration
4. ⏳ Create authorization attributes
5. ⏳ Add to ASP.NET Core pipeline
6. ⏳ Create Role and Permission entities
7. ⏳ Sync database roles with Casbin policies

---

**Last Updated:** 2026-02-05

