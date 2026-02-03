# Entities & Relationships

## Entities

| Entity       | Table            | Description                                                                 |
| ------------ | ---------------- | --------------------------------------------------------------------------- |
| **Site**     | `sites`          | Location (name, address, city, state, postal code, country).                 |
| **Equipment**| `equipment`      | Piece of equipment at a site (name, equipment type).                         |
| **Sensor**   | `sensors`        | Sensor device (serial, status: Available / Unavailable / Unknown).           |
| **SensorType** | `sensor_types` | Type of sensor (type name, unit).                                             |
| **Threshold**  | `thresholds`   | Min/max/duration used to configure a sensor.                                 |
| **SensorReading** | `sensor_readings` | Single reading from a sensor (value).                                    |
| **Alert**    | `alerts`         | Alert (title, description, type, status).                                   |

All entities extend **BaseEntity** (Id, CreatedAt, UpdatedAt, DeletedAt) and use soft delete via `DeletedAt`.

---

## Relationships

| From           | Cardinality | To             | Relationship |
| -------------- | ----------- | -------------- | ------------ |
| **Site**       | 1 → 0..*    | **Equipment**  | Site **has** Equipment (`equipment.site_id` → `sites.id`) |
| **Equipment**  | 1 → 0..*    | **Sensor**     | Equipment **contains** Sensor (`sensors.equipment_id` → `equipment.id`) |
| **SensorType** | 1 → 0..*    | **Sensor**     | SensorType **classifies** Sensor (`sensors.sensor_type_id` → `sensor_types.id`) |
| **Sensor**     | 1 ↔ 1       | **Threshold**  | Sensor **configuredBy** Threshold (`sensors.threshold_id` → `thresholds.id`, unique) |
| **Sensor**     | 1 → 0..*    | **SensorReading** | Sensor **produces** SensorReading (`sensor_readings.sensor_id` → `sensors.id`) |
| **SensorReading** | 1 → 0..* | **Alert**      | SensorReading **triggers** Alert (`alerts.sensor_reading_id` → `sensor_readings.id`) |

---

## Dependency Chain

```
Site → Equipment → Sensor ← SensorType
              Sensor ↔ Threshold
              Sensor → SensorReading → Alert
```

Foreign keys use snake_case column names and named constraints (`fk_*`); delete behavior is `Restrict` on all relationships.
