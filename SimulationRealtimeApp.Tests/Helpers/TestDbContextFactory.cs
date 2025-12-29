using Microsoft.EntityFrameworkCore;
using SimulationRealtimeApp.Data;

namespace SimulationRealtimeApp.Tests.Helpers
{
    public static class TestDbContextFactory
    {
        public static SimulationDbContext Create(string? databaseName = null)
        {
            var options = new DbContextOptionsBuilder<SimulationDbContext>()
                .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
                .Options;

            var context = new SimulationDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
