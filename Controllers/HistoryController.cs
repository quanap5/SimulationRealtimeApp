using Microsoft.AspNetCore.Mvc;
using SimulationRealtimeApp.Models;
using SimulationRealtimeApp.Repositories;

namespace SimulationRealtimeApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HistoryController : ControllerBase
    {
        private readonly ISimulationHistoryRepository _historyRepository;
        private readonly ILogger<HistoryController> _logger;

        public HistoryController(
            ISimulationHistoryRepository historyRepository,
            ILogger<HistoryController> logger)
        {
            _historyRepository = historyRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all simulation sessions (paginated)
        /// </summary>
        /// <param name="page">Page number (1-based, default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 50, max: 100)</param>
        [HttpGet]
        [ProducesResponseType(typeof(List<SessionSummary>), 200)]
        public async Task<ActionResult<List<SessionSummary>>> GetAllSessions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            if (page < 1)
                return BadRequest(new { message = "Page must be at least 1" });

            if (pageSize < 1 || pageSize > 100)
                return BadRequest(new { message = "PageSize must be between 1 and 100" });

            var skip = (page - 1) * pageSize;
            var sessions = await _historyRepository.GetAllSessionsAsync(skip, pageSize);

            var result = sessions.Select(s => new SessionSummary
            {
                SessionId = s.Id,
                StartedAt = s.StartedAt,
                StoppedAt = s.StoppedAt,
                IterationCount = s.IterationCount
            }).ToList();

            return Ok(result);
        }

        /// <summary>
        /// Get a specific session with all data points
        /// </summary>
        /// <param name="sessionId">The session ID</param>
        [HttpGet("{sessionId}")]
        [ProducesResponseType(typeof(SessionDetails), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<SessionDetails>> GetSessionDetails(Guid sessionId)
        {
            var session = await _historyRepository.GetSessionByIdAsync(sessionId);

            if (session == null)
            {
                return NotFound(new { message = $"Session {sessionId} not found" });
            }

            var result = new SessionDetails
            {
                SessionId = session.Id,
                StartedAt = session.StartedAt,
                StoppedAt = session.StoppedAt,
                IterationCount = session.IterationCount,
                DataPoints = session.DataPoints.Select(d => new SimulationData
                {
                    Timestamp = d.Timestamp,
                    Temperature = d.Temperature,
                    Pressure = d.Pressure,
                    Velocity = d.Velocity,
                    Energy = d.Energy,
                    Status = d.Status,
                    IterationNumber = d.IterationNumber
                }).ToList()
            };

            return Ok(result);
        }

        /// <summary>
        /// Get data points within a time range (across all sessions)
        /// </summary>
        /// <param name="start">Start time (ISO 8601 format)</param>
        /// <param name="end">End time (ISO 8601 format)</param>
        /// <param name="page">Page number (1-based, default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 1000, max: 5000)</param>
        [HttpGet("range")]
        [ProducesResponseType(typeof(TimeRangeResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<TimeRangeResponse>> GetDataPointsByRange(
            [FromQuery] DateTime start,
            [FromQuery] DateTime end,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 1000)
        {
            if (start >= end)
                return BadRequest(new { message = "Start time must be before end time" });

            if (page < 1)
                return BadRequest(new { message = "Page must be at least 1" });

            if (pageSize < 1 || pageSize > 5000)
                return BadRequest(new { message = "PageSize must be between 1 and 5000" });

            var skip = (page - 1) * pageSize;
            var dataPoints = await _historyRepository.GetDataPointsByTimeRangeAsync(start, end, skip, pageSize);

            var result = new TimeRangeResponse
            {
                StartTime = start,
                EndTime = end,
                ReturnedCount = dataPoints.Count,
                DataPoints = dataPoints.Select(d => new SimulationDataWithSession
                {
                    SessionId = d.SessionId,
                    Timestamp = d.Timestamp,
                    Temperature = d.Temperature,
                    Pressure = d.Pressure,
                    Velocity = d.Velocity,
                    Energy = d.Energy,
                    Status = d.Status,
                    IterationNumber = d.IterationNumber
                }).ToList()
            };

            return Ok(result);
        }
    }
}
