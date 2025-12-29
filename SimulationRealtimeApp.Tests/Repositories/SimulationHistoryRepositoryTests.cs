using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SimulationRealtimeApp.Models;
using SimulationRealtimeApp.Repositories;
using SimulationRealtimeApp.Tests.Helpers;
using Xunit;

namespace SimulationRealtimeApp.Tests.Repositories
{
    public class SimulationHistoryRepositoryTests : IDisposable
    {
        private readonly Data.SimulationDbContext _context;
        private readonly SimulationHistoryRepository _sut;
        private readonly Mock<ILogger<SimulationHistoryRepository>> _loggerMock;

        public SimulationHistoryRepositoryTests()
        {
            _context = TestDbContextFactory.Create();
            _loggerMock = new Mock<ILogger<SimulationHistoryRepository>>();
            _sut = new SimulationHistoryRepository(_context, _loggerMock.Object);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task CreateSessionAsync_ShouldCreateSession()
        {
            // Arrange
            var sessionId = Guid.NewGuid();

            // Act
            await _sut.CreateSessionAsync(sessionId);

            // Assert
            var session = await _context.Sessions.FindAsync(sessionId);
            session.Should().NotBeNull();
            session!.Id.Should().Be(sessionId);
            session.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            session.StoppedAt.Should().BeNull();
            session.IterationCount.Should().Be(0);
        }

        [Fact]
        public async Task EndSessionAsync_ShouldUpdateSession()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            await _sut.CreateSessionAsync(sessionId);

            // Act
            await _sut.EndSessionAsync(sessionId, 100);

            // Assert
            var session = await _context.Sessions.FindAsync(sessionId);
            session.Should().NotBeNull();
            session!.StoppedAt.Should().NotBeNull();
            session.StoppedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            session.IterationCount.Should().Be(100);
        }

        [Fact]
        public async Task EndSessionAsync_WithInvalidSessionId_ShouldNotThrow()
        {
            // Arrange
            var invalidSessionId = Guid.NewGuid();

            // Act & Assert
            await _sut.Invoking(r => r.EndSessionAsync(invalidSessionId, 100))
                .Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetSessionByIdAsync_ShouldReturnSessionWithDataPoints()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            await _sut.CreateSessionAsync(sessionId);

            var data1 = CreateSimulationData(1);
            var data2 = CreateSimulationData(2);
            await _sut.SaveDataPointAsync(sessionId, data1);
            await _sut.SaveDataPointAsync(sessionId, data2);

            // Act
            var result = await _sut.GetSessionByIdAsync(sessionId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(sessionId);
            result.DataPoints.Should().HaveCount(2);
            //result.DataPoints[0].IterationNumber.Should().Be(1);
            //result.DataPoints[1].IterationNumber.Should().Be(2);
        }

        [Fact]
        public async Task GetSessionByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var invalidSessionId = Guid.NewGuid();

            // Act
            var result = await _sut.GetSessionByIdAsync(invalidSessionId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllSessionsAsync_ShouldReturnSessionsOrderedByStartedAtDesc()
        {
            // Arrange
            var sessionId1 = Guid.NewGuid();
            var sessionId2 = Guid.NewGuid();
            var sessionId3 = Guid.NewGuid();

            await _sut.CreateSessionAsync(sessionId1);
            await Task.Delay(10);
            await _sut.CreateSessionAsync(sessionId2);
            await Task.Delay(10);
            await _sut.CreateSessionAsync(sessionId3);

            // Act
            var result = await _sut.GetAllSessionsAsync();

            // Assert
            result.Should().HaveCount(3);
            result[0].Id.Should().Be(sessionId3);
            result[1].Id.Should().Be(sessionId2);
            result[2].Id.Should().Be(sessionId1);
        }

        [Fact]
        public async Task GetAllSessionsAsync_ShouldRespectPagination()
        {
            // Arrange
            for (int i = 0; i < 5; i++)
            {
                await _sut.CreateSessionAsync(Guid.NewGuid());
                await Task.Delay(10);
            }

            // Act
            var page1 = await _sut.GetAllSessionsAsync(skip: 0, take: 2);
            var page2 = await _sut.GetAllSessionsAsync(skip: 2, take: 2);
            var page3 = await _sut.GetAllSessionsAsync(skip: 4, take: 2);

            // Assert
            page1.Should().HaveCount(2);
            page2.Should().HaveCount(2);
            page3.Should().HaveCount(1);
        }

        [Fact]
        public async Task SaveDataPointAsync_ShouldPersistData()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            await _sut.CreateSessionAsync(sessionId);

            var data = new SimulationData
            {
                Timestamp = DateTime.UtcNow,
                Temperature = 45.5,
                Pressure = 5.2,
                Velocity = 25.3,
                Energy = 485.7,
                Status = "Normal",
                IterationNumber = 1
            };

            // Act
            await _sut.SaveDataPointAsync(sessionId, data);

            // Assert
            var dataPoints = await _sut.GetDataPointsBySessionAsync(sessionId);
            dataPoints.Should().HaveCount(1);
            dataPoints[0].Temperature.Should().Be(45.5);
            dataPoints[0].Pressure.Should().Be(5.2);
            dataPoints[0].Velocity.Should().Be(25.3);
            dataPoints[0].Energy.Should().Be(485.7);
            dataPoints[0].Status.Should().Be("Normal");
            dataPoints[0].IterationNumber.Should().Be(1);
        }

        [Fact]
        public async Task GetDataPointsBySessionAsync_ShouldReturnOrderedByIterationNumber()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            await _sut.CreateSessionAsync(sessionId);

            await _sut.SaveDataPointAsync(sessionId, CreateSimulationData(3));
            await _sut.SaveDataPointAsync(sessionId, CreateSimulationData(1));
            await _sut.SaveDataPointAsync(sessionId, CreateSimulationData(2));

            // Act
            var result = await _sut.GetDataPointsBySessionAsync(sessionId);

            // Assert
            result.Should().HaveCount(3);
            result[0].IterationNumber.Should().Be(1);
            result[1].IterationNumber.Should().Be(2);
            result[2].IterationNumber.Should().Be(3);
        }

        [Fact]
        public async Task GetDataPointsByTimeRangeAsync_ShouldFilterByTimeRange()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            await _sut.CreateSessionAsync(sessionId);

            var baseTime = DateTime.UtcNow;
            await _sut.SaveDataPointAsync(sessionId, CreateSimulationData(1, baseTime.AddMinutes(-10)));
            await _sut.SaveDataPointAsync(sessionId, CreateSimulationData(2, baseTime.AddMinutes(-5)));
            await _sut.SaveDataPointAsync(sessionId, CreateSimulationData(3, baseTime));
            await _sut.SaveDataPointAsync(sessionId, CreateSimulationData(4, baseTime.AddMinutes(5)));
            await _sut.SaveDataPointAsync(sessionId, CreateSimulationData(5, baseTime.AddMinutes(10)));

            // Act
            var result = await _sut.GetDataPointsByTimeRangeAsync(
                baseTime.AddMinutes(-6),
                baseTime.AddMinutes(6));

            // Assert
            result.Should().HaveCount(3);
            result.Select(d => d.IterationNumber).Should().BeEquivalentTo(new[] { 2, 3, 4 });
        }

        [Fact]
        public async Task GetDataPointsByTimeRangeAsync_ShouldRespectPagination()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            await _sut.CreateSessionAsync(sessionId);

            var baseTime = DateTime.UtcNow;
            for (int i = 0; i < 10; i++)
            {
                await _sut.SaveDataPointAsync(sessionId, CreateSimulationData(i + 1, baseTime.AddSeconds(i)));
            }

            // Act
            var page1 = await _sut.GetDataPointsByTimeRangeAsync(
                baseTime.AddSeconds(-1),
                baseTime.AddSeconds(15),
                skip: 0,
                take: 3);
            var page2 = await _sut.GetDataPointsByTimeRangeAsync(
                baseTime.AddSeconds(-1),
                baseTime.AddSeconds(15),
                skip: 3,
                take: 3);

            // Assert
            page1.Should().HaveCount(3);
            page2.Should().HaveCount(3);
            page1[0].IterationNumber.Should().Be(1);
            page2[0].IterationNumber.Should().Be(4);
        }

        private static SimulationData CreateSimulationData(int iterationNumber, DateTime? timestamp = null)
        {
            return new SimulationData
            {
                Timestamp = timestamp ?? DateTime.UtcNow,
                Temperature = 45.0 + iterationNumber,
                Pressure = 5.0,
                Velocity = 25.0,
                Energy = 400.0,
                Status = "Normal",
                IterationNumber = iterationNumber
            };
        }
    }
}
