# Real-Time Simulation Web Application

[![CI/CD Pipeline](https://github.com/YOUR_USERNAME/SimulationRealtimeApp/actions/workflows/ci.yml/badge.svg)](https://github.com/YOUR_USERNAME/SimulationRealtimeApp/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A real-time simulation dashboard built with **ASP.NET Core 9.0**, **SignalR**, and **Entity Framework Core** that streams live simulation data to connected clients with full history persistence.

## Features

- **Real-time Data Streaming**: Uses SignalR to push simulation data to all connected clients
- **Background Simulation Service**: Continuously generates realistic physics simulation data
- **Data Persistence**: SQLite database stores all simulation sessions and data points
- **History API**: Query historical data by session or time range
- **REST API**: Control simulation via HTTP endpoints
- **Interactive Dashboard**: Web interface with live charts
- **Auto-reconnection**: Clients automatically reconnect if connection is lost
- **Configurable Parameters**: Adjust simulation parameters on the fly

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- A modern web browser (Chrome, Firefox, Edge, Safari)
- [Docker](https://www.docker.com/) (optional, for containerized deployment)

## Quick Start

### Option 1: Run with .NET CLI

```bash
# Clone the repository
git clone https://github.com/YOUR_USERNAME/SimulationRealtimeApp.git
cd SimulationRealtimeApp

# Restore dependencies
dotnet restore

# Run the application
dotnet run --project SimulationRealtimeApp

# Open browser to https://localhost:7001/index.html
```

### Option 2: Run with Docker

```bash
# Build and run with Docker Compose
docker-compose up --build

# Open browser to http://localhost:8080/index.html
```

## Project Structure

```
SimulationRealtimeApp/
├── .github/workflows/
│   └── ci.yml                      # GitHub Actions CI/CD
├── SimulationRealtimeApp/
│   ├── Controllers/
│   │   ├── SimulationController.cs # Simulation control API
│   │   └── HistoryController.cs    # History query API
│   ├── Data/
│   │   ├── Entities/               # Database entities
│   │   └── SimulationDbContext.cs  # EF Core DbContext
│   ├── Repositories/
│   │   ├── ISimulationHistoryRepository.cs
│   │   └── SimulationHistoryRepository.cs
│   ├── Hubs/
│   │   └── SimulationHub.cs        # SignalR hub
│   ├── Models/
│   │   ├── SimulationModels.cs     # Core DTOs
│   │   └── HistoryModels.cs        # History DTOs
│   ├── Services/
│   │   ├── SimulationService.cs    # Simulation logic
│   │   └── SimulationBackgroundService.cs
│   ├── docs/                       # Documentation
│   └── wwwroot/                    # Web dashboard
├── SimulationRealtimeApp.Tests/    # Unit tests
├── Dockerfile
├── docker-compose.yml
├── CONTRIBUTING.md
├── CHANGELOG.md
└── LICENSE
```

## API Endpoints

### Simulation Control

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/simulation/status` | Get simulation status |
| POST | `/api/simulation/start` | Start simulation (returns sessionId) |
| POST | `/api/simulation/stop` | Stop simulation |
| GET | `/api/simulation/config` | Get configuration |
| PUT | `/api/simulation/config` | Update configuration |
| GET | `/api/simulation/snapshot` | Get single data snapshot |

### History API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/history` | List all sessions (paginated) |
| GET | `/api/history/{sessionId}` | Get session with all data points |
| GET | `/api/history/range` | Query data by time range |

### Example: Start Simulation

```bash
curl -X POST http://localhost:5000/api/simulation/start
```

Response:
```json
{
  "message": "Simulation started successfully",
  "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

### Example: Query History

```bash
curl "http://localhost:5000/api/history?page=1&pageSize=10"
```

For complete API documentation, see [docs/api/](docs/api/).

## SignalR Hub

### Hub URL
```
wss://localhost:7001/simulationHub
```

### Events

| Event | Direction | Description |
|-------|-----------|-------------|
| `ReceiveSimulationData` | Server → Client | Real-time data |
| `SimulationStarted` | Server → Client | Simulation started |
| `SimulationStopped` | Server → Client | Simulation stopped |

## Data Model

### SimulationData

```json
{
  "timestamp": "2025-01-15T10:30:00Z",
  "temperature": 75.34,
  "pressure": 5.67,
  "velocity": 25.89,
  "energy": 450.23,
  "status": "Normal",
  "iterationNumber": 150
}
```

### Status Values

| Status | Condition |
|--------|-----------|
| Normal | All parameters in normal range |
| Warning | Temperature > 75°C OR Pressure < 2 Bar |
| Critical | Temperature > 90°C |
| High Activity | Velocity > 40 m/s |

## Configuration

Default simulation parameters:

| Parameter | Default | Range |
|-----------|---------|-------|
| Update Interval | 1000ms | Configurable |
| Temperature | 20-100°C | Configurable |
| Pressure | 1-10 Bar | Configurable |
| Velocity | 0-50 m/s | Configurable |

## Database

The application uses SQLite for data persistence. The database file (`simulation_history.db`) is created automatically on first run.

For database schema and query examples, see [docs/database.md](docs/database.md).

## Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Development

### Building

```bash
dotnet build
```

### Running in Development

```bash
cd SimulationRealtimeApp
dotnet run
```

### Swagger UI

Access API documentation at: `https://localhost:7001/swagger`

## Production Deployment

### Docker

```bash
docker build -t simulation-app .
docker run -p 8080:8080 simulation-app
```

### Manual Deployment

```bash
dotnet publish -c Release -o ./publish
```

For detailed deployment instructions, see [docs/deployment.md](docs/deployment.md).

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for a list of changes.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Documentation

- [API Reference: Simulation](docs/api/simulation-api.md)
- [API Reference: History](docs/api/history-api.md)
- [Database Documentation](docs/database.md)
- [Contributing Guide](CONTRIBUTING.md)

## Support

- Open an issue on GitHub
- Check [ASP.NET Core docs](https://docs.microsoft.com/aspnet/core)
- Check [SignalR docs](https://docs.microsoft.com/aspnet/core/signalr)
