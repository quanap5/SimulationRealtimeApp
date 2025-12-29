# System Architecture

## Overview

This real-time simulation application uses a modern architecture with ASP.NET Core Web API, SignalR for real-time communication, and a responsive web client.

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                     Client Layer                             │
├─────────────────────────────────────────────────────────────┤
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │   Browser    │  │   Browser    │  │  Console     │      │
│  │   Client 1   │  │   Client 2   │  │   Client     │      │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘      │
│         │                  │                  │              │
│         └──────────────────┼──────────────────┘              │
│                            │                                 │
│                    SignalR WebSocket                         │
│                            │                                 │
└────────────────────────────┼─────────────────────────────────┘
                             │
┌────────────────────────────┼─────────────────────────────────┐
│                     API Layer                                │
├────────────────────────────┼─────────────────────────────────┤
│                            │                                 │
│  ┌─────────────────────────▼──────────────────────────┐     │
│  │           SignalR Hub (SimulationHub)              │     │
│  │  - Manages client connections                      │     │
│  │  - Broadcasts simulation data                      │     │
│  │  - Handles client method calls                     │     │
│  └─────────────────────────┬──────────────────────────┘     │
│                            │                                 │
│  ┌─────────────────────────▼──────────────────────────┐     │
│  │       REST API (SimulationController)              │     │
│  │  - GET  /api/simulation/status                     │     │
│  │  - POST /api/simulation/start                      │     │
│  │  - POST /api/simulation/stop                       │     │
│  │  - GET  /api/simulation/config                     │     │
│  │  - PUT  /api/simulation/config                     │     │
│  │  - GET  /api/simulation/snapshot                   │     │
│  └─────────────────────────┬──────────────────────────┘     │
│                            │                                 │
└────────────────────────────┼─────────────────────────────────┘
                             │
┌────────────────────────────┼─────────────────────────────────┐
│                   Business Logic Layer                       │
├────────────────────────────┼─────────────────────────────────┤
│                            │                                 │
│  ┌─────────────────────────▼──────────────────────────┐     │
│  │         SimulationService                          │     │
│  │  - Generates simulation data                       │     │
│  │  - Manages simulation state                        │     │
│  │  - Applies physics algorithms                      │     │
│  │  - Handles configuration                           │     │
│  └─────────────────────────┬──────────────────────────┘     │
│                            │                                 │
│  ┌─────────────────────────▼──────────────────────────┐     │
│  │     SimulationBackgroundService                    │     │
│  │  - Runs continuously in background                 │     │
│  │  - Calls SimulationService every interval          │     │
│  │  - Broadcasts data via SignalR Hub                 │     │
│  └────────────────────────────────────────────────────┘     │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

## Component Details

### 1. Client Layer

**Web Browser Client (index.html)**
- Built with vanilla JavaScript and Chart.js
- Uses SignalR JavaScript client library
- Features:
  - Real-time data visualization
  - Live charts with Chart.js
  - Connection status monitoring
  - Control buttons for simulation
  - Event logging

**Console Client (client.js)**
- Node.js based testing client
- Useful for debugging and testing
- Displays simulation data in terminal

### 2. API Layer

**SignalR Hub (SimulationHub.cs)**
- Central hub for real-time communication
- Manages WebSocket connections
- Methods:
  - `OnConnectedAsync()`: Handle new connections
  - `OnDisconnectedAsync()`: Handle disconnections
  - `StartSimulation()`: Client-callable start method
  - `StopSimulation()`: Client-callable stop method
  - `BroadcastSimulationData()`: Server method to send data

**REST API Controller (SimulationController.cs)**
- Traditional REST endpoints
- HTTP-based control
- Swagger documentation enabled
- Allows alternative control mechanism

### 3. Business Logic Layer

**SimulationService.cs**
- Core simulation logic
- Generates realistic physics data:
  - Temperature (oscillating with time)
  - Pressure (inverse to temperature)
  - Velocity (gradual increase)
  - Energy (calculated from other params)
- Manages simulation state
- Thread-safe singleton

**SimulationBackgroundService.cs**
- Hosted service running in background
- Continuously generates and broadcasts data
- Configurable update interval
- Automatic lifecycle management

### 4. Data Models

**SimulationData**
```csharp
- Timestamp: DateTime
- Temperature: double
- Pressure: double
- Velocity: double
- Energy: double
- Status: string
- IterationNumber: int
```

**SimulationStatus**
```csharp
- IsRunning: bool
- TotalIterations: int
- CurrentIteration: int
- StartTime: DateTime
- ConnectedClients: int
```

**SimulationConfig**
```csharp
- UpdateIntervalMs: int
- TemperatureMin: double
- TemperatureMax: double
- PressureMin: double
- PressureMax: double
- VelocityMin: double
- VelocityMax: double
```

## Communication Flow

### Real-Time Data Flow

```
1. Background Service Timer Triggers
                ↓
2. SimulationService.GenerateSimulationData()
                ↓
3. Background Service gets data
                ↓
4. Hub.Clients.All.SendAsync("ReceiveSimulationData", data)
                ↓
5. SignalR broadcasts to all connected clients
                ↓
6. Client receives data via WebSocket
                ↓
7. Client updates UI (charts, metrics, logs)
```

### Control Flow (Start Simulation)

```
Client clicks "Start" button
                ↓
JavaScript calls connection.invoke("StartSimulation")
                ↓
OR
JavaScript calls fetch('/api/simulation/start', {method: 'POST'})
                ↓
SignalR Hub or REST Controller receives request
                ↓
SimulationService.Start() called
                ↓
Background service starts generating data
                ↓
Data flows to clients (see Real-Time Data Flow)
```

## Technology Stack

### Backend
- **ASP.NET Core 8.0**: Web API framework
- **SignalR**: Real-time communication library
- **C# 12**: Programming language
- **Background Services**: IHostedService for background tasks

### Frontend
- **HTML5/CSS3**: Structure and styling
- **JavaScript (ES6+)**: Client logic
- **SignalR JavaScript Client**: Real-time connection
- **Chart.js 4.4.0**: Data visualization
- **Responsive Design**: Mobile-friendly layout

### Development Tools
- **Swagger/OpenAPI**: API documentation
- **Visual Studio / VS Code**: IDEs
- **.NET CLI**: Build and run tools

## Security Considerations

### Current Implementation (Development)
- CORS allows all origins
- No authentication/authorization
- HTTPS with development certificate
- Suitable for local development only

### Production Recommendations
1. **Authentication**: Implement JWT or cookie-based auth
2. **Authorization**: Add role-based access control
3. **CORS**: Restrict to specific origins
4. **Rate Limiting**: Prevent abuse
5. **Input Validation**: Validate all user inputs
6. **HTTPS**: Use valid SSL certificates
7. **SignalR Security**: 
   - Implement authentication in hub
   - Validate user permissions
   - Use access tokens

## Scalability Considerations

### Current Setup
- Single server instance
- In-memory state
- Suitable for development and small deployments

### Scaling Options

**Horizontal Scaling (Multiple Servers)**
1. Use Azure SignalR Service or Redis backplane
2. Configure sticky sessions on load balancer
3. Share simulation state via distributed cache

**Vertical Scaling**
1. Increase server resources
2. Optimize data generation algorithms
3. Reduce broadcast frequency

**Performance Optimizations**
1. Implement message batching
2. Use protocol buffers instead of JSON
3. Add client-side throttling
4. Implement server-sent events for one-way streams

## Deployment Architecture

### Development
```
Developer Machine
    ↓
dotnet run
    ↓
Kestrel (http://localhost:5000, https://localhost:7001)
```

### Production (Example)

```
Internet
    ↓
Cloud Load Balancer (AWS ALB / Azure App Gateway)
    ↓
┌─────────────────┬─────────────────┐
│   Web Server 1  │   Web Server 2  │
│   (IIS/nginx)   │   (IIS/nginx)   │
│       ↓         │       ↓         │
│   ASP.NET Core  │   ASP.NET Core  │
│       ↓         │       ↓         │
└────────┬────────┴────────┬────────┘
         │                 │
         └────────┬────────┘
                  ↓
        Azure SignalR Service / Redis
                  ↓
        Shared State (Redis/SQL)
```

## Monitoring and Logging

### Built-in Logging
- Console logging (development)
- File logging (production)
- Log levels: Debug, Information, Warning, Error

### Metrics to Monitor
1. Active connections count
2. Message throughput
3. CPU and memory usage
4. Connection/disconnection rates
5. Error rates
6. Response times

### Recommended Tools
- Application Insights (Azure)
- ELK Stack (Elasticsearch, Logstash, Kibana)
- Prometheus + Grafana
- Seq for structured logging

## Testing Strategy

### Unit Tests
- Test simulation algorithms
- Test data generation logic
- Mock SignalR hub context

### Integration Tests
- Test API endpoints
- Test SignalR hub methods
- Test background service behavior

### Load Tests
- Simulate multiple concurrent connections
- Measure throughput and latency
- Tools: JMeter, Artillery, SignalR.Client

### End-to-End Tests
- Selenium for browser automation
- Test full user workflows
- Verify real-time updates

## Future Enhancements

1. **Persistence**: Save simulation history to database
2. **Replay**: Ability to replay past simulations
3. **Multiple Simulations**: Support multiple concurrent simulations
4. **User Profiles**: Personal simulation configurations
5. **Advanced Visualizations**: 3D charts, heatmaps
6. **Alerts**: Threshold-based notifications
7. **Export**: Download data as CSV/JSON
8. **Collaboration**: Multi-user simulation controls
9. **Mobile App**: Native iOS/Android clients
10. **Machine Learning**: Predictive analytics on simulation data

## Troubleshooting Guide

### SignalR Connection Issues
- Check firewall settings
- Verify CORS configuration
- Trust development certificates
- Check WebSocket support

### Performance Issues
- Reduce update frequency
- Implement client-side throttling
- Check for memory leaks
- Profile CPU usage

### Data Accuracy Issues
- Review simulation algorithms
- Check random number generation
- Verify time synchronization

## Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [SignalR Documentation](https://docs.microsoft.com/aspnet/core/signalr)
- [Chart.js Documentation](https://www.chartjs.org/docs/)
- [WebSocket Protocol RFC](https://tools.ietf.org/html/rfc6455)

---

**Last Updated**: December 2024
