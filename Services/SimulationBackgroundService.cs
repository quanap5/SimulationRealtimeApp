using Microsoft.AspNetCore.SignalR;
using SimulationRealtimeApp.Hubs;
using SimulationRealtimeApp.Repositories;

namespace SimulationRealtimeApp.Services
{
    public class SimulationBackgroundService : BackgroundService
    {
        private readonly ILogger<SimulationBackgroundService> _logger;
        private readonly IHubContext<SimulationHub> _hubContext;
        private readonly SimulationService _simulationService;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public SimulationBackgroundService(
            ILogger<SimulationBackgroundService> logger,
            IHubContext<SimulationHub> hubContext,
            SimulationService simulationService,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _hubContext = hubContext;
            _simulationService = simulationService;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Simulation Background Service is starting.");

            // Auto-start the simulation and create session
            var sessionId = _simulationService.Start();

            // Create session in database
            await CreateSessionInDatabaseAsync(sessionId);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_simulationService.IsRunning)
                    {
                        var data = _simulationService.GenerateSimulationData();

                        if (data != null)
                        {
                            // Broadcast to all connected clients
                            await _hubContext.Clients.All.SendAsync(
                                "ReceiveSimulationData",
                                data,
                                stoppingToken);

                            // Persist data asynchronously (fire-and-forget to avoid blocking SignalR)
                            _ = PersistDataPointAsync(sessionId, data);

                            _logger.LogDebug("Broadcasted simulation data: Iteration {IterationNumber}", data.IterationNumber);
                        }
                    }

                    // Wait for the configured interval
                    await Task.Delay(_simulationService.GetConfig().UpdateIntervalMs, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when stopping
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in simulation background service");
                }
            }

            // End session when stopping
            await EndSessionInDatabaseAsync(sessionId, _simulationService.CurrentIteration);

            _logger.LogInformation("Simulation Background Service is stopping.");
        }

        private async Task CreateSessionInDatabaseAsync(Guid sessionId)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ISimulationHistoryRepository>();
                await repository.CreateSessionAsync(sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create session in database: {SessionId}", sessionId);
            }
        }

        private async Task PersistDataPointAsync(Guid sessionId, Models.SimulationData data)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ISimulationHistoryRepository>();
                await repository.SaveDataPointAsync(sessionId, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist data point for iteration {IterationNumber}", data.IterationNumber);
            }
        }

        private async Task EndSessionInDatabaseAsync(Guid sessionId, int iterationCount)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ISimulationHistoryRepository>();
                await repository.EndSessionAsync(sessionId, iterationCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to end session in database: {SessionId}", sessionId);
            }
        }
    }
}
