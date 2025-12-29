using SimulationRealtimeApp.Data.Entities;
using SimulationRealtimeApp.Models;

namespace SimulationRealtimeApp.Repositories
{
    public interface ISimulationHistoryRepository
    {
        // Session operations
        Task CreateSessionAsync(Guid sessionId);
        Task EndSessionAsync(Guid sessionId, int iterationCount);
        Task<SimulationSession?> GetSessionByIdAsync(Guid sessionId);
        Task<List<SimulationSession>> GetAllSessionsAsync(int skip = 0, int take = 50);

        // Data point operations
        Task SaveDataPointAsync(Guid sessionId, SimulationData data);
        Task<List<SimulationDataEntity>> GetDataPointsBySessionAsync(Guid sessionId);
        Task<List<SimulationDataEntity>> GetDataPointsByTimeRangeAsync(DateTime start, DateTime end, int skip = 0, int take = 1000);
    }
}
