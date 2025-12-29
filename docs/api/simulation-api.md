# Simulation API Reference

This document provides complete API documentation for the Simulation Controller endpoints.

## Base URL

```
/api/simulation
```

## Endpoints Overview

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/status` | Get current simulation status |
| POST | `/start` | Start the simulation |
| POST | `/stop` | Stop the simulation |
| GET | `/config` | Get current configuration |
| PUT | `/config` | Update configuration |
| GET | `/snapshot` | Get single data snapshot |

---

## GET /api/simulation/status

Get the current status of the simulation.

### Request

```http
GET /api/simulation/status
```

### Response

**Status Code:** `200 OK`

```json
{
  "isRunning": true,
  "totalIterations": 150,
  "currentIteration": 150,
  "startTime": "2025-01-15T10:30:00Z",
  "connectedClients": 5
}
```

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| isRunning | boolean | Whether simulation is currently running |
| totalIterations | integer | Total number of iterations executed |
| currentIteration | integer | Current iteration number |
| startTime | datetime | UTC timestamp when simulation started |
| connectedClients | integer | Number of connected SignalR clients |

---

## POST /api/simulation/start

Start the simulation. Creates a new session and begins generating data.

### Request

```http
POST /api/simulation/start
Content-Type: application/json
```

No request body required.

### Response

**Status Code:** `200 OK`

```json
{
  "message": "Simulation started successfully",
  "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| message | string | Success message |
| sessionId | GUID | Unique identifier for this simulation session |

### Side Effects

- Creates a new session in the database
- Begins broadcasting data via SignalR
- Sends `SimulationStarted` event to all connected clients

---

## POST /api/simulation/stop

Stop the simulation. Ends the current session and stops data generation.

### Request

```http
POST /api/simulation/stop
Content-Type: application/json
```

No request body required.

### Response

**Status Code:** `200 OK`

```json
{
  "message": "Simulation stopped successfully"
}
```

### Side Effects

- Ends the current session in the database (sets `StoppedAt` and `IterationCount`)
- Stops SignalR broadcasting
- Sends `SimulationStopped` event to all connected clients

---

## GET /api/simulation/config

Get the current simulation configuration.

### Request

```http
GET /api/simulation/config
```

### Response

**Status Code:** `200 OK`

```json
{
  "updateIntervalMs": 1000,
  "temperatureMin": 20.0,
  "temperatureMax": 100.0,
  "pressureMin": 1.0,
  "pressureMax": 10.0,
  "velocityMin": 0.0,
  "velocityMax": 50.0
}
```

### Response Fields

| Field | Type | Unit | Default | Description |
|-------|------|------|---------|-------------|
| updateIntervalMs | integer | ms | 1000 | Milliseconds between data generation |
| temperatureMin | number | °C | 20.0 | Minimum temperature value |
| temperatureMax | number | °C | 100.0 | Maximum temperature value |
| pressureMin | number | Bar | 1.0 | Minimum pressure value |
| pressureMax | number | Bar | 10.0 | Maximum pressure value |
| velocityMin | number | m/s | 0.0 | Minimum velocity value |
| velocityMax | number | m/s | 50.0 | Maximum velocity value |

---

## PUT /api/simulation/config

Update the simulation configuration. Changes take effect immediately.

### Request

```http
PUT /api/simulation/config
Content-Type: application/json

{
  "updateIntervalMs": 500,
  "temperatureMin": 15.0,
  "temperatureMax": 80.0,
  "pressureMin": 2.0,
  "pressureMax": 8.0,
  "velocityMin": 5.0,
  "velocityMax": 40.0
}
```

### Request Body

All fields are optional. Only include fields you want to change.

| Field | Type | Constraints |
|-------|------|-------------|
| updateIntervalMs | integer | Must be positive |
| temperatureMin | number | Must be less than temperatureMax |
| temperatureMax | number | Must be greater than temperatureMin |
| pressureMin | number | Must be less than pressureMax |
| pressureMax | number | Must be greater than pressureMin |
| velocityMin | number | Must be less than velocityMax |
| velocityMax | number | Must be greater than velocityMin |

### Response

**Status Code:** `200 OK`

```json
{
  "message": "Configuration updated successfully"
}
```

---

## GET /api/simulation/snapshot

Get a single snapshot of simulation data. Only works when simulation is running.

### Request

```http
GET /api/simulation/snapshot
```

### Response (Success)

**Status Code:** `200 OK`

```json
{
  "timestamp": "2025-01-15T10:30:45.123Z",
  "temperature": 65.42,
  "pressure": 4.87,
  "velocity": 25.33,
  "energy": 485.67,
  "status": "Normal",
  "iterationNumber": 45
}
```

### Response Fields

| Field | Type | Unit | Description |
|-------|------|------|-------------|
| timestamp | datetime | - | UTC timestamp of data generation |
| temperature | number | °C | Current temperature value |
| pressure | number | Bar | Current pressure value |
| velocity | number | m/s | Current velocity value |
| energy | number | J | Calculated energy (0.5*v² + temp*2.5) |
| status | string | - | Status: Normal, Warning, Critical, or High Activity |
| iterationNumber | integer | - | Sequential iteration number |

### Response (Error)

**Status Code:** `400 Bad Request`

```json
{
  "message": "Simulation is not running"
}
```

---

## Status Values

The `status` field in simulation data can have the following values:

| Status | Condition |
|--------|-----------|
| Critical | Temperature > 90°C |
| Warning | Temperature > 75°C OR Pressure < 2 Bar |
| High Activity | Velocity > 40 m/s |
| Normal | All other conditions |

---

## Error Responses

### Common Error Format

```json
{
  "message": "Error description"
}
```

### HTTP Status Codes

| Code | Description |
|------|-------------|
| 200 | Success |
| 400 | Bad Request (validation error) |
| 500 | Internal Server Error |

---

## Usage Examples

### cURL Examples

**Start Simulation:**
```bash
curl -X POST http://localhost:5000/api/simulation/start \
  -H "Content-Type: application/json"
```

**Get Status:**
```bash
curl http://localhost:5000/api/simulation/status
```

**Update Config:**
```bash
curl -X PUT http://localhost:5000/api/simulation/config \
  -H "Content-Type: application/json" \
  -d '{"updateIntervalMs": 500}'
```

### JavaScript/Fetch Examples

**Start Simulation:**
```javascript
const response = await fetch('/api/simulation/start', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' }
});
const data = await response.json();
console.log('Session ID:', data.sessionId);
```

**Get Snapshot:**
```javascript
const response = await fetch('/api/simulation/snapshot');
if (response.ok) {
  const data = await response.json();
  console.log(`Temperature: ${data.temperature}°C`);
}
```
