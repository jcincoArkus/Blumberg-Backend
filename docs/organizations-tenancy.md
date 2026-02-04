# Backend Overview

## Project structure

- **Apps** – API and CLI entry points
- **Modules** – Feature modules (Auth, Admins, etc.): controllers, services, repositories, DTOs
- **Adapters** – Config, Database (EF + schemas), Jwt, Server (API setup, tenant context)
- **Shared** – Entities, base entity, enums, abstractions (e.g. `ITenantContext`), constants

---

## Organization domain

**Organization** is the top-level tenant. Each organization has:

- **Sites** → **Equipment** → **Sensors** → **SensorReadings** → **Alerts**
- **Admins** (email unique per organization)
- **SensorTypes**, **Thresholds** (shared/reference data)

All tenant-scoped entities have a required **OrganizationId** and **Organization** navigation.

---

## Tenancy

- **ITenantContext** (`Shared.Abstractions`): exposes `CurrentOrganizationId` for the request.
- **TenantContext** (Adapters.Server): resolves tenant from **X-Organization-Id** header (dev) or JWT **orgId** claim.
- **ApplicationDbContext**:
  - Global query filters: tenant-scoped entities only return rows for `CurrentOrganizationId`; all use soft delete (`DeletedAt == null`).
  - SaveChanges: validates no cross-tenant writes; sets `OrganizationId` on new tenant-scoped entities when empty.
- **Design-time**: `DesignTimeTenantContext` and `ApplicationDbContextFactory` use a null tenant for EF migrations and tooling.

---

## Auth & JWT

- Login: **POST /api/v1/auth/login** (email + password). Admin is resolved by email (IgnoreQueryFilters).
- JWT includes **orgId** claim from the admin’s **OrganizationId**.
- **AuthResponse** returns **Token**, **Email**, **FirstName**, **LastName**, **OrganizationId**, **ExpiresAt**.

---

## Dev

- Use **X-Organization-Id** header to set tenant when not using a JWT (e.g. Swagger, Postman).
- CORS allows all origins/headers for development.
