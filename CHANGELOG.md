# Changelog

All notable changes to SimulationRealtimeApp will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.1.0] - 2025-01-15

### Added
- Database persistence with SQLite and Entity Framework Core
- Session tracking for simulation runs
- History API endpoints:
  - `GET /api/history` - List all sessions (paginated)
  - `GET /api/history/{sessionId}` - Get session details with data points
  - `GET /api/history/range` - Query data by time range
- Repository pattern for data access (`ISimulationHistoryRepository`)
- Unit test project with xUnit, Moq, and FluentAssertions
- Tests for `SimulationService` and `SimulationHistoryRepository`
- GitHub Actions CI/CD pipeline
- Docker support with Dockerfile and docker-compose.yml
- Comprehensive documentation:
  - API reference for Simulation and History endpoints
  - Database documentation
  - Contributing guidelines

### Changed
- `SimulationService.Start()` now returns a session GUID
- Added `CurrentSessionId` property to `SimulationService`
- `SimulationBackgroundService` now persists data points to database

## [1.0.0] - 2025-01-01

### Added
- Initial release
- Real-time simulation data generation
- SignalR hub for live data streaming
- REST API for simulation control:
  - `GET /api/simulation/status` - Get simulation status
  - `POST /api/simulation/start` - Start simulation
  - `POST /api/simulation/stop` - Stop simulation
  - `GET /api/simulation/config` - Get configuration
  - `PUT /api/simulation/config` - Update configuration
  - `GET /api/simulation/snapshot` - Get single data snapshot
- Web dashboard with live charts
- Configurable simulation parameters
- Swagger/OpenAPI documentation
- Auto-reconnection for SignalR clients

### Technical Details
- ASP.NET Core 9.0
- SignalR for WebSocket communication
- Physics-based simulation with temperature, pressure, velocity, and energy
- Status determination based on parameter thresholds

[Unreleased]: https://github.com/YOUR_USERNAME/SimulationRealtimeApp/compare/v1.1.0...HEAD
[1.1.0]: https://github.com/YOUR_USERNAME/SimulationRealtimeApp/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/YOUR_USERNAME/SimulationRealtimeApp/releases/tag/v1.0.0
