# History API Reference

This document provides complete API documentation for the History Controller endpoints.

## Base URL

```
/api/history
```

## Endpoints Overview

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | Get all sessions (paginated) |
| GET | `/{sessionId}` | Get session details with data points |
| GET | `/range` | Get data points by time range |

---

## GET /api/history

Get all simulation sessions, ordered by start time (newest first).

### Request

```http
GET /api/history?page=1&pageSize=50
```

### Query Parameters

| Parameter | Type | Default | Range | Description |
|-----------|------|---------|-------|-------------|
| page | integer | 1 | ≥ 1 | Page number (1-based) |
| pageSize | integer | 50 | 1-100 | Number of items per page |

### Response

**Status Code:** `200 OK`

```json
[
  {
    "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "startedAt": "2025-01-15T10:30:00Z",
    "stoppedAt": "2025-01-15T11:30:00Z",
    "iterationCount": 3600,
    "durationSeconds": 3600.0,
    "isActive": false
  },
  {
    "sessionId": "7ca12b89-1234-5678-abcd-ef1234567890",
    "startedAt": "2025-01-15T09:00:00Z",
    "stoppedAt": null,
    "iterationCount": 0,
    "durationSeconds": null,
    "isActive": true
  }
]
```

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| sessionId | GUID | Unique session identifier |
| startedAt | datetime | UTC timestamp when session started |
| stoppedAt | datetime? | UTC timestamp when session stopped (null if active) |
| iterationCount | integer | Total iterations in session |
| durationSeconds | number? | Session duration in seconds (null if active) |
| isActive | boolean | Whether session is currently running |

### Error Responses

**Status Code:** `400 Bad Request`

```json
{
  "message": "Page must be at least 1"
}
```

```json
{
  "message": "PageSize must be between 1 and 100"
}
```

---

## GET /api/history/{sessionId}

Get detailed information about a specific session, including all data points.

### Request

```http
GET /api/history/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| sessionId | GUID | The session identifier |

### Response (Success)

**Status Code:** `200 OK`

```json
{
  "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "startedAt": "2025-01-15T10:30:00Z",
  "stoppedAt": "2025-01-15T11:30:00Z",
  "iterationCount": 3600,
  "durationSeconds": 3600.0,
  "isActive": false,
  "dataPoints": [
    {
      "timestamp": "2025-01-15T10:30:01Z",
      "temperature": 45.67,
      "pressure": 5.23,
      "velocity": 12.45,
      "energy": 191.29,
      "status": "Normal",
      "iterationNumber": 1
    },
    {
      "timestamp": "2025-01-15T10:30:02Z",
      "temperature": 46.12,
      "pressure": 5.18,
      "velocity": 12.89,
      "energy": 198.45,
      "status": "Normal",
      "iterationNumber": 2
    }
  ]
}
```

### Response Fields

Includes all fields from `SessionSummary` plus:

| Field | Type | Description |
|-------|------|-------------|
| dataPoints | array | Array of simulation data points |

#### Data Point Fields

| Field | Type | Unit | Description |
|-------|------|------|-------------|
| timestamp | datetime | - | UTC timestamp of data generation |
| temperature | number | °C | Temperature value |
| pressure | number | Bar | Pressure value |
| velocity | number | m/s | Velocity value |
| energy | number | J | Energy value |
| status | string | - | Status string |
| iterationNumber | integer | - | Sequential iteration number |

### Response (Not Found)

**Status Code:** `404 Not Found`

```json
{
  "message": "Session 3fa85f64-5717-4562-b3fc-2c963f66afa6 not found"
}
```

---

## GET /api/history/range

Get data points within a specific time range, across all sessions.

### Request

```http
GET /api/history/range?start=2025-01-15T10:00:00Z&end=2025-01-15T11:00:00Z&page=1&pageSize=1000
```

### Query Parameters

| Parameter | Type | Required | Default | Range | Description |
|-----------|------|----------|---------|-------|-------------|
| start | datetime | Yes | - | - | Start of time range (ISO 8601) |
| end | datetime | Yes | - | - | End of time range (ISO 8601) |
| page | integer | No | 1 | ≥ 1 | Page number (1-based) |
| pageSize | integer | No | 1000 | 1-5000 | Number of items per page |

### Response

**Status Code:** `200 OK`

```json
{
  "startTime": "2025-01-15T10:00:00Z",
  "endTime": "2025-01-15T11:00:00Z",
  "returnedCount": 1000,
  "dataPoints": [
    {
      "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "timestamp": "2025-01-15T10:00:01Z",
      "temperature": 45.67,
      "pressure": 5.23,
      "velocity": 12.45,
      "energy": 191.29,
      "status": "Normal",
      "iterationNumber": 1
    }
  ]
}
```

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| startTime | datetime | Requested start time |
| endTime | datetime | Requested end time |
| returnedCount | integer | Number of data points returned in this page |
| dataPoints | array | Array of data points with session info |

#### Data Point Fields

Same as `SessionDetails.dataPoints` plus:

| Field | Type | Description |
|-------|------|-------------|
| sessionId | GUID | Session this data point belongs to |

### Error Responses

**Status Code:** `400 Bad Request`

```json
{
  "message": "Start time must be before end time"
}
```

```json
{
  "message": "Page must be at least 1"
}
```

```json
{
  "message": "PageSize must be between 1 and 5000"
}
```

---

## Pagination

### How Pagination Works

Both the sessions list and time range query support pagination:

1. **page**: 1-based page number
2. **pageSize**: Number of items per page

### Pagination Formula

```
skip = (page - 1) * pageSize
```

### Example: Fetching All Sessions

```javascript
async function fetchAllSessions() {
  let page = 1;
  const pageSize = 50;
  const allSessions = [];

  while (true) {
    const response = await fetch(`/api/history?page=${page}&pageSize=${pageSize}`);
    const sessions = await response.json();

    if (sessions.length === 0) break;

    allSessions.push(...sessions);
    page++;
  }

  return allSessions;
}
```

---

## Usage Examples

### cURL Examples

**List All Sessions:**
```bash
curl "http://localhost:5000/api/history?page=1&pageSize=10"
```

**Get Session Details:**
```bash
curl "http://localhost:5000/api/history/3fa85f64-5717-4562-b3fc-2c963f66afa6"
```

**Query by Time Range:**
```bash
curl "http://localhost:5000/api/history/range?start=2025-01-15T00:00:00Z&end=2025-01-16T00:00:00Z"
```

### JavaScript/Fetch Examples

**List Sessions:**
```javascript
const response = await fetch('/api/history?page=1&pageSize=10');
const sessions = await response.json();
sessions.forEach(s => {
  console.log(`Session ${s.sessionId}: ${s.iterationCount} iterations`);
});
```

**Get Session with Data:**
```javascript
const sessionId = '3fa85f64-5717-4562-b3fc-2c963f66afa6';
const response = await fetch(`/api/history/${sessionId}`);

if (response.ok) {
  const session = await response.json();
  console.log(`Session has ${session.dataPoints.length} data points`);

  // Calculate averages
  const avgTemp = session.dataPoints.reduce((sum, d) => sum + d.temperature, 0)
    / session.dataPoints.length;
  console.log(`Average temperature: ${avgTemp.toFixed(2)}°C`);
} else if (response.status === 404) {
  console.log('Session not found');
}
```

**Export Data to CSV:**
```javascript
async function exportToCsv(sessionId) {
  const response = await fetch(`/api/history/${sessionId}`);
  const session = await response.json();

  const headers = 'Timestamp,Temperature,Pressure,Velocity,Energy,Status\n';
  const rows = session.dataPoints.map(d =>
    `${d.timestamp},${d.temperature},${d.pressure},${d.velocity},${d.energy},${d.status}`
  ).join('\n');

  return headers + rows;
}
```

---

## Performance Considerations

### Large Sessions

Sessions with many data points (e.g., hours of simulation) may return large payloads. Consider:

1. **Client-side pagination**: Load data points in chunks
2. **Time range queries**: Use `/api/history/range` for specific periods
3. **Streaming**: For very large datasets, consider implementing streaming endpoints

### Query Performance

- Sessions are indexed by `StartedAt` for efficient listing
- Data points are indexed by `SessionId` and `Timestamp`
- Time range queries are optimized with composite indexes

### Recommended Limits

| Endpoint | Recommended Max PageSize |
|----------|-------------------------|
| /api/history | 100 |
| /api/history/{id} | N/A (returns all data points) |
| /api/history/range | 5000 |
