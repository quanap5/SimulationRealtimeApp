using Microsoft.EntityFrameworkCore;
using SimulationRealtimeApp.Data;
using SimulationRealtimeApp.Data.Entities;
using SimulationRealtimeApp.Models;

namespace SimulationRealtimeApp.Repositories
{
    public class SimulationHistoryRepository : ISimulationHistoryRepository
    {
        private readonly SimulationDbContext _context;
        private readonly ILogger<SimulationHistoryRepository> _logger;

        public SimulationHistoryRepository(
            SimulationDbContext context,
            ILogger<SimulationHistoryRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CreateSessionAsync(Guid sessionId)
        {
            var session = new SimulationSession
            {
                Id = sessionId,
                StartedAt = DateTime.UtcNow,
                StoppedAt = null,
                IterationCount = 0
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created simulation session: {SessionId}", sessionId);
        }

        public async Task EndSessionAsync(Guid sessionId, int iterationCount)
        {
            var session = await _context.Sessions.FindAsync(sessionId);
            if (session == null)
            {
                _logger.LogWarning("Session not found for ending: {SessionId}", sessionId);
                return;
            }

            session.StoppedAt = DateTime.UtcNow;
            session.IterationCount = iterationCount;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Ended simulation session: {SessionId} with {IterationCount} iterations",
                sessionId, iterationCount);
        }

        public async Task<SimulationSession?> GetSessionByIdAsync(Guid sessionId)
        {
            var session = await _context.Sessions
                .Include(s => s.DataPoints)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session != null)
            {
                // Order data points in memory after loading
                session.DataPoints = session.DataPoints.OrderBy(d => d.IterationNumber).ToList();
            }

            return session;
        }

        public async Task<List<SimulationSession>> GetAllSessionsAsync(int skip = 0, int take = 50)
        {
            return await _context.Sessions
                .OrderByDescending(s => s.StartedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task SaveDataPointAsync(Guid sessionId, SimulationData data)
        {
            var entity = new SimulationDataEntity
            {
                SessionId = sessionId,
                Timestamp = data.Timestamp,
                Temperature = data.Temperature,
                Pressure = data.Pressure,
                Velocity = data.Velocity,
                Energy = data.Energy,
                Status = data.Status,
                IterationNumber = data.IterationNumber
            };

            _context.DataPoints.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<SimulationDataEntity>> GetDataPointsBySessionAsync(Guid sessionId)
        {
            return await _context.DataPoints
                .Where(d => d.SessionId == sessionId)
                .OrderBy(d => d.IterationNumber)
                .ToListAsync();
        }

        public async Task<List<SimulationDataEntity>> GetDataPointsByTimeRangeAsync(
            DateTime start,
            DateTime end,
            int skip = 0,
            int take = 1000)
        {
            return await _context.DataPoints
                .Where(d => d.Timestamp >= start && d.Timestamp <= end)
                .OrderBy(d => d.Timestamp)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
    }
}
