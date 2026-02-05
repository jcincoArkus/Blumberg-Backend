# Testing the API

## 1. Prerequisites

- **Database**: Start PostgreSQL (e.g. `docker compose up -d`). Default port is `15432` (mapped from container `5432`).
- **Config**: Copy `.env.example` to `.env` and set `DATABASE__CONNECTION_STRING` (and any JWT/other vars). Example:
  - `Host=localhost;Port=15432;Database=blumberg;Username=postgres;Password=postgres`
- **Seed data** (optional): Run the CLI to create schema and seed the test organization + admins:
  ```bash
  dotnet run --project Apps/CLI -- nukeAndPave
  ```

## 2. Run the API

From the repo root:

```bash
dotnet run --project Apps/API
```

Or in VS Code: **Run and Debug** → **Debug API**. This builds, runs the API, and can open Swagger when the app is listening.

The API listens on the URLs shown in the console (e.g. `https://localhost:5001` or `http://localhost:5000`).

## 3. Swagger UI (easiest)

1. Open **Swagger** in the browser:
   - Development: `https://localhost:5001/swagger` (or `http://localhost:5000/swagger` — use the URL printed in the console).
2. **Login** to get a JWT:
   - **POST /api/v1/auth/login**
   - Body (JSON): `{ "email": "admin@blumberg.com", "password": "Admin123." }`
   - Copy the `token` from the response.
3. **Authorize** in Swagger:
   - Click **Authorize**
   - Value: `Bearer <paste-your-token>`
   - Click **Authorize**, then **Close**
4. Call any tenant-scoped endpoint (e.g. **GET /api/v1/sites**). The JWT already contains `orgId`, so no extra header is needed.

**Troubleshooting:** CRUD endpoints require a valid JWT. If you get **401 Unauthorized**, click **Authorize** and enter your token. In the "Value" field you can enter either `YOUR_TOKEN` or `Bearer YOUR_TOKEN` (Swagger often adds "Bearer " for you). After authorizing, try the request again. If you still get empty results or errors, ensure the token is from a recent login (not expired) and that the same `.env` JWT secret/issuer/audience are used.

## 4. Testing without JWT (e.g. Postman or curl)

For dev, you can send the tenant in a header instead of using a JWT:

- **X-Organization-Id**: set to the test organization’s GUID (from the seeded org).

**Login (no auth required):**

```bash
curl -X POST https://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@blumberg.com","password":"Admin123."}'
```

Use the returned `token` for authenticated requests:

```bash
curl -X GET https://localhost:5001/api/v1/sites \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

If you’re not using a JWT (e.g. custom client), send the tenant header:

```bash
curl -X GET https://localhost:5001/api/v1/sites \
  -H "X-Organization-Id: YOUR_ORG_GUID"
```

(Replace `YOUR_ORG_GUID` with the test organization’s Id, e.g. from the login response’s `organizationId` or from the database.)

## 5. Quick checklist

| Step              | Command / Action                                      |
|-------------------|--------------------------------------------------------|
| Start DB          | `docker compose up -d`                                |
| Configure         | `.env` with `DATABASE__CONNECTION_STRING`              |
| Seed (optional)   | `dotnet run --project Apps/CLI -- nukeAndPave`        |
| Run API           | `dotnet run --project Apps/API` or **Debug API**      |
| Open Swagger      | `https://localhost:5001/swagger` (or port from console) |
| Login             | **POST /api/v1/auth/login** with seeded admin          |
| Use API           | **Authorize** with `Bearer <token>` in Swagger         |

## 6. Tenant context (recap)

- **With JWT**: `orgId` in the token is used as the current organization; no extra header needed.
- **Without JWT (dev)**: Send **X-Organization-Id** with the organization GUID so tenant-scoped endpoints (sites, equipment, sensors, admins) return and write data for that org only.
