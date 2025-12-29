using Microsoft.AspNetCore.SignalR;
using SimulationRealtimeApp.Models;

namespace SimulationRealtimeApp.Hubs
{
    public class SimulationHub : Hub
    {
        private readonly ILogger<SimulationHub> _logger;

        public SimulationHub(ILogger<SimulationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        // Client can call this method to start receiving simulation data
        public async Task StartSimulation()
        {
            _logger.LogInformation($"Client {Context.ConnectionId} started simulation");
            await Clients.Caller.SendAsync("SimulationStarted", "Simulation started successfully");
        }

        // Client can call this method to stop receiving simulation data
        public async Task StopSimulation()
        {
            _logger.LogInformation($"Client {Context.ConnectionId} stopped simulation");
            await Clients.Caller.SendAsync("SimulationStopped", "Simulation stopped");
        }

        // Method to send simulation data to all connected clients
        public async Task BroadcastSimulationData(SimulationData data)
        {
            await Clients.All.SendAsync("ReceiveSimulationData", data);
        }

        // Method to send simulation data to specific client
        public async Task SendSimulationDataToClient(string connectionId, SimulationData data)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveSimulationData", data);
        }
    }
}
