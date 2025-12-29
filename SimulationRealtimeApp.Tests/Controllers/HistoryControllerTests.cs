using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SimulationRealtimeApp.Controllers;
using SimulationRealtimeApp.Data.Entities;
using SimulationRealtimeApp.Models;
using SimulationRealtimeApp.Repositories;
using Xunit;

namespace SimulationRealtimeApp.Tests.Controllers
{
    public class HistoryControllerTests
    {
        private readonly Mock<ISimulationHistoryRepository> _repositoryMock;
        private readonly Mock<ILogger<HistoryController>> _loggerMock;
        private readonly HistoryController _sut;

        public HistoryControllerTests()
        {
            _repositoryMock = new Mock<ISimulationHistoryRepository>();
            _loggerMock = new Mock<ILogger<HistoryController>>();
            _sut = new HistoryController(_repositoryMock.Object, _loggerMock.Object);
        }

        #region GetAllSessions Tests

        [Fact]
        public async Task GetAllSessions_ShouldReturnOkWithSessions()
        {
            // Arrange
            var sessions = new List<SimulationSession>
            {
                new() { Id = Guid.NewGuid(), StartedAt = DateTime.UtcNow.AddHours(-2), StoppedAt = DateTime.UtcNow.AddHours(-1), IterationCount = 100 },
                new() { Id = Guid.NewGuid(), StartedAt = DateTime.UtcNow.AddHours(-1), StoppedAt = null, IterationCount = 50 }
            };

            _repositoryMock.Setup(r => r.GetAllSessionsAsync(0, 50))
                .ReturnsAsync(sessions);

            // Act
            var result = await _sut.GetAllSessions();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedSessions = okResult.Value.Should().BeAssignableTo<List<SessionSummary>>().Subject;
            returnedSessions.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllSessions_WithPagination_ShouldCalculateSkipCorrectly()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetAllSessionsAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<SimulationSession>());

            // Act
            await _sut.GetAllSessions(page: 3, pageSize: 25);

            // Assert
            _repositoryMock.Verify(r => r.GetAllSessionsAsync(50, 25), Times.Once);
        }

        [Fact]
        public async Task GetAllSessions_WithInvalidPage_ShouldReturnBadRequest()
        {
            // Act
            var result = await _sut.GetAllSessions(page: 0);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetAllSessions_WithInvalidPageSize_ShouldReturnBadRequest()
        {
            // Act
            var result = await _sut.GetAllSessions(pageSize: 101);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetAllSessions_WithZeroPageSize_ShouldReturnBadRequest()
        {
            // Act
            var result = await _sut.GetAllSessions(pageSize: 0);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetAllSessions_ShouldMapSessionsCorrectly()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var startedAt = DateTime.UtcNow.AddHours(-2);
            var stoppedAt = DateTime.UtcNow.AddHours(-1);

            var sessions = new List<SimulationSession>
            {
                new() { Id = sessionId, StartedAt = startedAt, StoppedAt = stoppedAt, IterationCount = 150 }
            };

            _repositoryMock.Setup(r => r.GetAllSessionsAsync(0, 50))
                .ReturnsAsync(sessions);

            // Act
            var result = await _sut.GetAllSessions();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedSessions = okResult.Value.Should().BeAssignableTo<List<SessionSummary>>().Subject;

            returnedSessions[0].SessionId.Should().Be(sessionId);
            returnedSessions[0].StartedAt.Should().Be(startedAt);
            returnedSessions[0].StoppedAt.Should().Be(stoppedAt);
            returnedSessions[0].IterationCount.Should().Be(150);
        }

        #endregion

        #region GetSessionDetails Tests

        [Fact]
        public async Task GetSessionDetails_WithValidId_ShouldReturnOkWithDetails()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var session = new SimulationSession
            {
                Id = sessionId,
                StartedAt = DateTime.UtcNow.AddHours(-1),
                StoppedAt = DateTime.UtcNow,
                IterationCount = 100,
                DataPoints = new List<SimulationDataEntity>
                {
                    new() { Id = 1, SessionId = sessionId, Temperature = 45.0, Pressure = 5.0, Velocity = 25.0, Energy = 400.0, Status = "Normal", IterationNumber = 1, Timestamp = DateTime.UtcNow }
                }
            };

            _repositoryMock.Setup(r => r.GetSessionByIdAsync(sessionId))
                .ReturnsAsync(session);

            // Act
            var result = await _sut.GetSessionDetails(sessionId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var details = okResult.Value.Should().BeOfType<SessionDetails>().Subject;
            details.SessionId.Should().Be(sessionId);
            details.DataPoints.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetSessionDetails_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var invalidId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetSessionByIdAsync(invalidId))
                .ReturnsAsync((SimulationSession?)null);

            // Act
            var result = await _sut.GetSessionDetails(invalidId);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetSessionDetails_ShouldMapDataPointsCorrectly()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var timestamp = DateTime.UtcNow;
            var session = new SimulationSession
            {
                Id = sessionId,
                StartedAt = DateTime.UtcNow.AddHours(-1),
                IterationCount = 1,
                DataPoints = new List<SimulationDataEntity>
                {
                    new()
                    {
                        Id = 1,
                        SessionId = sessionId,
                        Timestamp = timestamp,
                        Temperature = 45.5,
                        Pressure = 5.2,
                        Velocity = 25.3,
                        Energy = 485.7,
                        Status = "Warning",
                        IterationNumber = 1
                    }
                }
            };

            _repositoryMock.Setup(r => r.GetSessionByIdAsync(sessionId))
                .ReturnsAsync(session);

            // Act
            var result = await _sut.GetSessionDetails(sessionId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var details = okResult.Value.Should().BeOfType<SessionDetails>().Subject;
            var dataPoint = details.DataPoints[0];

            dataPoint.Timestamp.Should().Be(timestamp);
            dataPoint.Temperature.Should().Be(45.5);
            dataPoint.Pressure.Should().Be(5.2);
            dataPoint.Velocity.Should().Be(25.3);
            dataPoint.Energy.Should().Be(485.7);
            dataPoint.Status.Should().Be("Warning");
            dataPoint.IterationNumber.Should().Be(1);
        }

        #endregion

        #region GetDataPointsByRange Tests

        [Fact]
        public async Task GetDataPointsByRange_WithValidRange_ShouldReturnOk()
        {
            // Arrange
            var start = DateTime.UtcNow.AddHours(-1);
            var end = DateTime.UtcNow;
            var sessionId = Guid.NewGuid();

            var dataPoints = new List<SimulationDataEntity>
            {
                new() { Id = 1, SessionId = sessionId, Timestamp = start.AddMinutes(30), Temperature = 45.0, Pressure = 5.0, Velocity = 25.0, Energy = 400.0, Status = "Normal", IterationNumber = 1 }
            };

            _repositoryMock.Setup(r => r.GetDataPointsByTimeRangeAsync(start, end, 0, 1000))
                .ReturnsAsync(dataPoints);

            // Act
            var result = await _sut.GetDataPointsByRange(start, end);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<TimeRangeResponse>().Subject;
            response.StartTime.Should().Be(start);
            response.EndTime.Should().Be(end);
            response.ReturnedCount.Should().Be(1);
            response.DataPoints.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetDataPointsByRange_WithInvalidRange_ShouldReturnBadRequest()
        {
            // Arrange
            var start = DateTime.UtcNow;
            var end = DateTime.UtcNow.AddHours(-1); // end before start

            // Act
            var result = await _sut.GetDataPointsByRange(start, end);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetDataPointsByRange_WithInvalidPage_ShouldReturnBadRequest()
        {
            // Arrange
            var start = DateTime.UtcNow.AddHours(-1);
            var end = DateTime.UtcNow;

            // Act
            var result = await _sut.GetDataPointsByRange(start, end, page: 0);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetDataPointsByRange_WithInvalidPageSize_ShouldReturnBadRequest()
        {
            // Arrange
            var start = DateTime.UtcNow.AddHours(-1);
            var end = DateTime.UtcNow;

            // Act
            var result = await _sut.GetDataPointsByRange(start, end, pageSize: 5001);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetDataPointsByRange_WithPagination_ShouldCalculateSkipCorrectly()
        {
            // Arrange
            var start = DateTime.UtcNow.AddHours(-1);
            var end = DateTime.UtcNow;

            _repositoryMock.Setup(r => r.GetDataPointsByTimeRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<SimulationDataEntity>());

            // Act
            await _sut.GetDataPointsByRange(start, end, page: 3, pageSize: 500);

            // Assert
            _repositoryMock.Verify(r => r.GetDataPointsByTimeRangeAsync(start, end, 1000, 500), Times.Once);
        }

        [Fact]
        public async Task GetDataPointsByRange_ShouldIncludeSessionIdInDataPoints()
        {
            // Arrange
            var start = DateTime.UtcNow.AddHours(-1);
            var end = DateTime.UtcNow;
            var sessionId = Guid.NewGuid();

            var dataPoints = new List<SimulationDataEntity>
            {
                new() { Id = 1, SessionId = sessionId, Timestamp = start.AddMinutes(30), Temperature = 45.0, Pressure = 5.0, Velocity = 25.0, Energy = 400.0, Status = "Normal", IterationNumber = 1 }
            };

            _repositoryMock.Setup(r => r.GetDataPointsByTimeRangeAsync(start, end, 0, 1000))
                .ReturnsAsync(dataPoints);

            // Act
            var result = await _sut.GetDataPointsByRange(start, end);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<TimeRangeResponse>().Subject;
            response.DataPoints[0].SessionId.Should().Be(sessionId);
        }

        #endregion
    }
}
