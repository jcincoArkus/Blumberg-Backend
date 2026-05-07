# Inventory API Reference

> **Base path:** `api/v1/inventory`
> All endpoints require a valid Bearer token (`Authorization: Bearer <token>`).
> All list endpoints return a [`PagedResponse<T>`](#paged-response) and accept the common [pagination query params](#pagination).

---

## Table of Contents

1. [Common Types](#common-types)
2. [Categories](#categories)
3. [Sites](#sites)
4. [Site Zones](#site-zones)
5. [Suppliers](#suppliers)
6. [Products](#products)
7. [Lots](#lots)
8. [Movements](#movements)
9. [Intake Shipments](#intake-shipments)
10. [Shipment Lines](#shipment-lines)

---

## Common Types

### Pagination

All `GET` list endpoints accept these query parameters:

| Parameter  | Type    | Default | Description              |
|------------|---------|---------|--------------------------|
| `page`     | integer | `1`     | Page number (1-based)    |
| `pageSize` | integer | `20`    | Number of items per page |

### Paged Response

```json
{
  "items": [...],
  "totalCount": 42,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3
}
```

### Enum Values

| Field               | Allowed values                                        |
|---------------------|-------------------------------------------------------|
| `unit` (product)    | `"kg"` · `"unit"` · `"box"`                          |
| `unit` (lot/movement) | `"kg"` · `"unit"`                                 |
| `movementType`      | `"intake"` · `"output"` · `"waste"` · `"adjustment"` · `"transfer"` |
| `shipmentStatus`    | `"draft"` · `"received"` · `"cancelled"`             |

---

## Categories

**Base URL:** `api/v1/inventory/categories`

Product categories with a display colour (e.g. Fruit, Vegetables, Herbs).

### `GET api/v1/inventory/categories`

Returns a paginated list of all categories.

**Query parameters:** [pagination](#pagination)

**Response `200`**
```json
{
  "items": [
    { "id": "uuid", "name": "Fruit", "color": "teal", "createdAt": "...", "updatedAt": "..." }
  ],
  "totalCount": 5, "page": 1, "pageSize": 20, "totalPages": 1
}
```

---

### `GET api/v1/inventory/categories/{id}`

Returns a single category by UUID.

**Path parameter:** `id` — UUID

**Responses:** `200 OK` · `404 Not Found`

---

### `POST api/v1/inventory/categories`

Creates a new category.

**Request body (`application/json`)**

| Field   | Type   | Required | Max length | Description                      |
|---------|--------|----------|------------|----------------------------------|
| `name`  | string | ✅       | 100        | Category name                    |
| `color` | string | ✅       | 50         | UI colour token (e.g. `"teal"`)  |

**Responses:** `201 Created` · `400 Bad Request`

---

### `PUT api/v1/inventory/categories/{id}`

Updates an existing category.

**Path parameter:** `id` — UUID
**Request body:** same as `POST`

**Responses:** `200 OK` · `404 Not Found`

---

### `DELETE api/v1/inventory/categories/{id}`

Permanently deletes a category.

**Path parameter:** `id` — UUID

**Responses:** `200 OK` · `404 Not Found`

---

## Sites

**Base URL:** `api/v1/inventory/sites`

Physical warehouse / distribution sites that hold inventory. Distinct from the sensor-monitoring `sites` resource.

### `GET api/v1/inventory/sites`

Returns a paginated list of all inventory sites.

**Query parameters:** [pagination](#pagination)

---

### `GET api/v1/inventory/sites/{id}`

Returns a single site by UUID.

**Responses:** `200 OK` · `404 Not Found`

---

### `POST api/v1/inventory/sites`

Creates a new inventory site.

**Request body**

| Field  | Type   | Required | Max length | Description       |
|--------|--------|----------|------------|-------------------|
| `name` | string | ✅       | 200        | Site display name |

**Responses:** `201 Created`

---

### `PUT api/v1/inventory/sites/{id}`

Updates a site name.

**Path parameter:** `id` — UUID
**Request body:** same as `POST`

**Responses:** `200 OK` · `404 Not Found`

---

### `DELETE api/v1/inventory/sites/{id}`

Permanently deletes a site.

**Responses:** `200 OK` · `404 Not Found`

---

## Site Zones

**Base URL:** `api/v1/inventory/site-zones`

Named storage zones within a site (e.g. "Cold Room A", "Dock 3"). Zone names are unique per site.

### `GET api/v1/inventory/site-zones`

Returns a paginated list of zones, optionally filtered by site.

**Query parameters**

| Parameter | Type | Required | Description                           |
|-----------|------|----------|---------------------------------------|
| `siteId`  | UUID | ❌       | Filter zones to a specific site       |
| + [pagination](#pagination) | | | |

---

### `GET api/v1/inventory/site-zones/{id}`

Returns a single zone by UUID.

**Responses:** `200 OK` · `404 Not Found`

---

### `POST api/v1/inventory/site-zones`

Creates a new zone inside a site.

**Request body**

| Field    | Type   | Required | Max length | Description                          |
|----------|--------|----------|------------|--------------------------------------|
| `siteId` | UUID   | ✅       | —          | Parent site                          |
| `name`   | string | ✅       | 100        | Zone name (unique within the site)   |

**Responses:** `201 Created` · `400 Bad Request` (duplicate name in site)

---

### `PUT api/v1/inventory/site-zones/{id}`

Updates a zone.

**Responses:** `200 OK` · `404 Not Found`

---

### `DELETE api/v1/inventory/site-zones/{id}`

Permanently deletes a zone.

**Responses:** `200 OK` · `404 Not Found`

---

## Suppliers

**Base URL:** `api/v1/inventory/suppliers`

Vendors / suppliers associated with lots and intake shipments.

### `GET api/v1/inventory/suppliers`

Returns a paginated list of all suppliers.

**Query parameters:** [pagination](#pagination)

---

### `GET api/v1/inventory/suppliers/{id}`

Returns a single supplier by UUID.

**Responses:** `200 OK` · `404 Not Found`

---

### `POST api/v1/inventory/suppliers`

Creates a new supplier.

**Request body**

| Field  | Type   | Required | Max length | Description           |
|--------|--------|----------|------------|-----------------------|
| `name` | string | ✅       | 200        | Supplier company name |

**Responses:** `201 Created`

---

### `PUT api/v1/inventory/suppliers/{id}`

Updates a supplier.

**Responses:** `200 OK` · `404 Not Found`

---

### `DELETE api/v1/inventory/suppliers/{id}`

Permanently deletes a supplier.

**Responses:** `200 OK` · `404 Not Found`

---

## Products

**Base URL:** `api/v1/inventory/products`

SKU catalogue entries that describe what can be stored (e.g. "Avocado · Hass").

### `GET api/v1/inventory/products`

Returns a paginated, filterable list of products.

**Query parameters**

| Parameter    | Type   | Required | Description                              |
|--------------|--------|----------|------------------------------------------|
| `categoryId` | UUID   | ❌       | Filter by category                       |
| `unit`       | string | ❌       | Filter by unit (`"kg"` · `"unit"` · `"box"`) |
| + [pagination](#pagination) | | | |

---

### `GET api/v1/inventory/products/{id}`

Returns a single product by UUID.

**Responses:** `200 OK` · `404 Not Found`

---

### `POST api/v1/inventory/products`

Creates a new product. SKU must be unique.

**Request body**

| Field           | Type    | Required | Constraint            | Description                              |
|-----------------|---------|----------|-----------------------|------------------------------------------|
| `sku`           | string  | ✅       | max 50, unique        | SKU code (e.g. `"AVO-HASS"`)            |
| `name`          | string  | ✅       | max 200               | Display name                             |
| `categoryId`    | UUID    | ✅       | —                     | Parent category                          |
| `unit`          | string  | ✅       | `"kg"·"unit"·"box"`  | Unit of measure                          |
| `kgPerBox`      | decimal | ❌       | > 0                   | Required when `unit = "box"`             |
| `shelfLifeDays` | integer | ✅       | ≥ 1                   | Expected shelf life in days              |
| `price`         | decimal | ✅       | ≥ 0                   | Sale price                               |

**Responses:** `201 Created` · `400 Bad Request` (duplicate SKU)

---

### `PUT api/v1/inventory/products/{id}`

Updates a product. SKU must remain unique.

**Responses:** `200 OK` · `404 Not Found` · `400 Bad Request`

---

### `DELETE api/v1/inventory/products/{id}`

Permanently deletes a product.

**Responses:** `200 OK` · `404 Not Found`

---

## Lots

**Base URL:** `api/v1/inventory/lots`

Individual inventory lots identified by a natural string code (e.g. `L-250501-01`). Each lot tracks a physical batch of product at a site/zone.

> **Note:** The route parameter is `{lotCode}` (a string), not a UUID.

### `GET api/v1/inventory/lots`

Returns a paginated, filterable list of lots.

**Query parameters**

| Parameter       | Type     | Required | Description                              |
|-----------------|----------|----------|------------------------------------------|
| `productId`     | UUID     | ❌       | Filter by product                        |
| `siteId`        | UUID     | ❌       | Filter by site                           |
| `expiringBefore`| datetime | ❌       | Only lots expiring before this date (UTC)|
| + [pagination](#pagination) | | | |

---

### `GET api/v1/inventory/lots/{lotCode}`

Returns a single lot by its lot code.

**Path parameter:** `lotCode` — string (e.g. `L-250501-01`)

**Responses:** `200 OK` · `404 Not Found`

---

### `POST api/v1/inventory/lots`

Creates a new lot. Lot code must be unique; `expiresAt` must be after `entryAt`.

**Request body**

| Field         | Type     | Required | Constraint           | Description                         |
|---------------|----------|----------|----------------------|-------------------------------------|
| `lotCode`     | string   | ✅       | max 50, unique       | Natural lot identifier              |
| `productId`   | UUID     | ✅       | —                    | Product in this lot                 |
| `qty`         | decimal  | ✅       | ≥ 0                  | Quantity on hand                    |
| `unit`        | string   | ✅       | `"kg"·"unit"`        | Unit of measure                     |
| `entryAt`     | datetime | ✅       | —                    | When the lot entered storage (UTC)  |
| `expiresAt`   | datetime | ✅       | > `entryAt`          | Expiry date (UTC)                   |
| `siteId`      | UUID     | ✅       | —                    | Site where the lot is stored        |
| `zone`        | string   | ✅       | max 100              | Zone within the site                |
| `supplierId`  | UUID     | ❌       | —                    | Supplier (optional)                 |
| `costPerUnit` | decimal  | ✅       | ≥ 0                  | Cost per unit at receipt            |

**Responses:** `201 Created` · `400 Bad Request` (duplicate code or invalid dates)

---

### `PUT api/v1/inventory/lots/{lotCode}`

Updates a lot.

**Responses:** `200 OK` · `404 Not Found` · `400 Bad Request`

---

### `DELETE api/v1/inventory/lots/{lotCode}`

Permanently deletes a lot.

**Responses:** `200 OK` · `404 Not Found`

---

## Movements

**Base URL:** `api/v1/inventory/movements`

Append-only ledger of stock movements (intake, output, waste, adjustment, transfer). Records cannot be updated or deleted.

### `GET api/v1/inventory/movements`

Returns a paginated, filterable movement ledger (sorted by `occurredAt` DESC).

**Query parameters**

| Parameter   | Type     | Required | Description                                         |
|-------------|----------|----------|-----------------------------------------------------|
| `type`      | string   | ❌       | Filter by movement type (see [enum values](#enum-values)) |
| `productId` | UUID     | ❌       | Filter by product                                   |
| `siteId`    | UUID     | ❌       | Filter by source site                               |
| `lotCode`   | string   | ❌       | Filter by lot code                                  |
| `from`      | datetime | ❌       | Only movements at or after this date (UTC)          |
| `to`        | datetime | ❌       | Only movements at or before this date (UTC)         |
| + [pagination](#pagination) | | | |

---

### `GET api/v1/inventory/movements/{id}`

Returns a single movement record by UUID.

**Responses:** `200 OK` · `404 Not Found`

---

### `POST api/v1/inventory/movements`

Records a new stock movement. Transfers require `destSiteId`.

**Request body**

| Field         | Type     | Required | Constraint                             | Description                                      |
|---------------|----------|----------|----------------------------------------|--------------------------------------------------|
| `type`        | string   | ✅       | `"intake"·"output"·"waste"·"adjustment"·"transfer"` | Movement type              |
| `occurredAt`  | datetime | ✅       | —                                      | When it happened (UTC)                           |
| `productId`   | UUID     | ✅       | —                                      | Affected product                                 |
| `qty`         | decimal  | ✅       | —                                      | Quantity delta (positive = in, negative = out)   |
| `unit`        | string   | ✅       | `"kg"·"unit"`                          | Unit of measure                                  |
| `lotCode`     | string   | ❌       | max 50                                 | Associated lot                                   |
| `siteId`      | UUID     | ✅       | —                                      | Source site                                      |
| `destSiteId`  | UUID     | ❌ / ✅  | required when `type = "transfer"`      | Destination site                                 |
| `performedBy` | string   | ✅       | max 200                                | Name of person who performed the movement        |
| `note`        | string   | ❌       | —                                      | Free-text note                                   |

**Responses:** `201 Created` · `400 Bad Request` (transfer without `destSiteId`)

---

## Intake Shipments

**Base URL:** `api/v1/inventory/intake-shipments`

Purchase-order shipment headers tracking goods arriving at a site. Lines are managed separately via [Shipment Lines](#shipment-lines).

### `GET api/v1/inventory/intake-shipments`

Returns a paginated, filterable list of shipments.

**Query parameters**

| Parameter    | Type   | Required | Description                                          |
|--------------|--------|----------|------------------------------------------------------|
| `supplierId` | UUID   | ❌       | Filter by supplier                                   |
| `siteId`     | UUID   | ❌       | Filter by receiving site                             |
| `status`     | string | ❌       | Filter by status (`"draft"·"received"·"cancelled"`)  |
| + [pagination](#pagination) | | | |

---

### `GET api/v1/inventory/intake-shipments/{id}`

Returns a single shipment by UUID.

**Responses:** `200 OK` · `404 Not Found`

---

### `POST api/v1/inventory/intake-shipments`

Creates a new intake shipment. PO reference must be unique.

**Request body**

| Field              | Type     | Required | Constraint                              | Description                          |
|--------------------|----------|----------|-----------------------------------------|--------------------------------------|
| `poReference`      | string   | ✅       | max 100, unique                         | Purchase order number                |
| `supplierId`       | UUID     | ✅       | —                                       | Supplier                             |
| `vehicle`          | string   | ❌       | max 200                                 | Vehicle description or plate         |
| `driver`           | string   | ❌       | max 200                                 | Driver name                          |
| `siteId`           | UUID     | ✅       | —                                       | Receiving site                       |
| `receivingZone`    | string   | ✅       | max 100                                 | Zone where goods were unloaded       |
| `coldChainTempC`   | decimal  | ❌       | —                                       | Arrival temperature in °C            |
| `arrivedAt`        | datetime | ✅       | —                                       | Arrival timestamp (UTC)              |
| `receivedBy`       | string   | ✅       | max 200                                 | Staff member who received the goods  |
| `status`           | string   | ✅       | `"draft"·"received"·"cancelled"`        | Shipment status (default `"draft"`)  |

**Responses:** `201 Created` · `400 Bad Request` (duplicate PO reference)

---

### `PUT api/v1/inventory/intake-shipments/{id}`

Updates a shipment header.

**Responses:** `200 OK` · `404 Not Found` · `400 Bad Request`

---

### `DELETE api/v1/inventory/intake-shipments/{id}`

Permanently deletes a shipment and all its lines (cascade).

**Responses:** `200 OK` · `404 Not Found`

---

## Shipment Lines

**Base URL:** `api/v1/inventory/shipment-lines`

Individual product lines within an intake shipment. Each line specifies the product, lot, quantity, and cost received.

### `GET api/v1/inventory/shipment-lines`

Returns a paginated list of shipment lines, optionally filtered by parent shipment.

**Query parameters**

| Parameter    | Type | Required | Description                    |
|--------------|------|----------|--------------------------------|
| `shipmentId` | UUID | ❌       | Filter to a specific shipment  |
| + [pagination](#pagination) | | | |

---

### `GET api/v1/inventory/shipment-lines/{id}`

Returns a single shipment line by UUID.

**Responses:** `200 OK` · `404 Not Found`

---

### `POST api/v1/inventory/shipment-lines`

Adds a line to an existing shipment.

**Request body**

| Field         | Type    | Required | Constraint              | Description                              |
|---------------|---------|----------|-------------------------|------------------------------------------|
| `shipmentId`  | UUID    | ✅       | —                       | Parent shipment                          |
| `productId`   | UUID    | ✅       | —                       | Product received                         |
| `lotCode`     | string  | ❌       | max 50                  | Lot assigned when shipment is confirmed  |
| `qty`         | decimal | ✅       | > 0                     | Quantity received                        |
| `unit`        | string  | ✅       | `"kg"·"unit"·"box"`     | Unit of measure                          |
| `costPerUnit` | decimal | ✅       | ≥ 0                     | Cost per unit for this line              |

**Responses:** `201 Created` · `400 Bad Request`

---

### `PUT api/v1/inventory/shipment-lines/{id}`

Updates a shipment line.

**Responses:** `200 OK` · `404 Not Found`

---

### `DELETE api/v1/inventory/shipment-lines/{id}`

Permanently deletes a shipment line.

**Responses:** `200 OK` · `404 Not Found`
