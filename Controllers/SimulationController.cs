using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SimulationRealtimeApp.Hubs;
using SimulationRealtimeApp.Models;
using SimulationRealtimeApp.Repositories;
using SimulationRealtimeApp.Services;

namespace SimulationRealtimeApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SimulationController : ControllerBase
    {
        private readonly SimulationService _simulationService;
        private readonly IHubContext<SimulationHub> _hubContext;
        private readonly ILogger<SimulationController> _logger;
        private readonly ISimulationHistoryRepository _historyRepository;

        public SimulationController(
            SimulationService simulationService,
            IHubContext<SimulationHub> hubContext,
            ILogger<SimulationController> logger,
            ISimulationHistoryRepository historyRepository)
        {
            _simulationService = simulationService;
            _hubContext = hubContext;
            _logger = logger;
            _historyRepository = historyRepository;
        }

        /// <summary>
        /// Get the current simulation status
        /// </summary>
        [HttpGet("status")]
        public ActionResult<SimulationStatus> GetStatus()
        {
            var status = _simulationService.GetStatus(0);
            return Ok(status);
        }

        /// <summary>
        /// Start the simulation
        /// </summary>
        [HttpPost("start")]
        public async Task<IActionResult> StartSimulation()
        {
            var sessionId = _simulationService.Start();
            _logger.LogInformation("Simulation started via API - Session: {SessionId}", sessionId);

            // Create session in database
            await _historyRepository.CreateSessionAsync(sessionId);

            await _hubContext.Clients.All.SendAsync("SimulationStarted", new
            {
                message = "Simulation started",
                sessionId
            });

            return Ok(new { message = "Simulation started successfully", sessionId });
        }

        /// <summary>
        /// Stop the simulation
        /// </summary>
        [HttpPost("stop")]
        public async Task<IActionResult> StopSimulation()
        {
            var currentSessionId = _simulationService.CurrentSessionId;
            var currentIteration = _simulationService.CurrentIteration;

            _simulationService.Stop();
            _logger.LogInformation("Simulation stopped via API");

            // End session in database
            if (currentSessionId.HasValue)
            {
                await _historyRepository.EndSessionAsync(currentSessionId.Value, currentIteration);
            }

            await _hubContext.Clients.All.SendAsync("SimulationStopped", "Simulation stopped");

            return Ok(new { message = "Simulation stopped successfully" });
        }

        /// <summary>
        /// Get current simulation configuration
        /// </summary>
        [HttpGet("config")]
        public ActionResult<SimulationConfig> GetConfig()
        {
            return Ok(_simulationService.GetConfig());
        }

        /// <summary>
        /// Update simulation configuration
        /// </summary>
        [HttpPut("config")]
        public IActionResult UpdateConfig([FromBody] SimulationConfig config)
        {
            _simulationService.UpdateConfig(config);
            _logger.LogInformation("Simulation configuration updated");

            return Ok(new { message = "Configuration updated successfully" });
        }

        /// <summary>
        /// Get a single snapshot of simulation data
        /// </summary>
        [HttpGet("snapshot")]
        public ActionResult<SimulationData> GetSnapshot()
        {
            var data = _simulationService.GenerateSimulationData();

            if (data == null)
            {
                return BadRequest(new { message = "Simulation is not running" });
            }

            return Ok(data);
        }
    }
}
