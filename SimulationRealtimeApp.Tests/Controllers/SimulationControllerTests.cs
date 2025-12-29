using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using SimulationRealtimeApp.Controllers;
using SimulationRealtimeApp.Hubs;
using SimulationRealtimeApp.Models;
using SimulationRealtimeApp.Repositories;
using SimulationRealtimeApp.Services;
using Xunit;

namespace SimulationRealtimeApp.Tests.Controllers
{
    public class SimulationControllerTests
    {
        private readonly SimulationService _simulationService;
        private readonly Mock<IHubContext<SimulationHub>> _hubContextMock;
        private readonly Mock<ILogger<SimulationController>> _loggerMock;
        private readonly Mock<ISimulationHistoryRepository> _repositoryMock;
        private readonly SimulationController _sut;

        public SimulationControllerTests()
        {
            _simulationService = new SimulationService();
            _hubContextMock = new Mock<IHubContext<SimulationHub>>();
            _loggerMock = new Mock<ILogger<SimulationController>>();
            _repositoryMock = new Mock<ISimulationHistoryRepository>();

            // Setup hub context mock
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);
            _hubContextMock.Setup(h => h.Clients).Returns(mockClients.Object);

            _sut = new SimulationController(
                _simulationService,
                _hubContextMock.Object,
                _loggerMock.Object,
                _repositoryMock.Object);
        }

        #region GetStatus Tests

        [Fact]
        public void GetStatus_ShouldReturnOkWithStatus()
        {
            // Act
            var result = _sut.GetStatus();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var status = okResult.Value.Should().BeOfType<SimulationStatus>().Subject;
            status.IsRunning.Should().BeFalse();
        }

        [Fact]
        public void GetStatus_WhenSimulationRunning_ShouldReturnRunningTrue()
        {
            // Arrange
            _simulationService.Start();

            // Act
            var result = _sut.GetStatus();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var status = okResult.Value.Should().BeOfType<SimulationStatus>().Subject;
            status.IsRunning.Should().BeTrue();
        }

        #endregion

        #region StartSimulation Tests

        [Fact]
        public async Task StartSimulation_ShouldReturnOkWithSessionId()
        {
            // Act
            var result = await _sut.StartSimulation();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();

            _simulationService.IsRunning.Should().BeTrue();
            _simulationService.CurrentSessionId.Should().NotBeNull();
        }

        [Fact]
        public async Task StartSimulation_ShouldCreateSessionInRepository()
        {
            // Act
            await _sut.StartSimulation();

            // Assert
            _repositoryMock.Verify(
                r => r.CreateSessionAsync(It.IsAny<Guid>()),
                Times.Once);
        }

        [Fact]
        public async Task StartSimulation_ShouldNotifyClientsViaSignalR()
        {
            // Arrange
            var mockClientProxy = new Mock<IClientProxy>();
            var mockClients = new Mock<IHubClients>();
            mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);
            _hubContextMock.Setup(h => h.Clients).Returns(mockClients.Object);

            // Act
            await _sut.StartSimulation();

            // Assert
            mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    "SimulationStarted",
                    It.IsAny<object[]>(),
                    default),
                Times.Once);
        }

        #endregion

        #region StopSimulation Tests

        [Fact]
        public async Task StopSimulation_ShouldReturnOk()
        {
            // Arrange
            _simulationService.Start();

            // Act
            var result = await _sut.StopSimulation();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _simulationService.IsRunning.Should().BeFalse();
        }

        [Fact]
        public async Task StopSimulation_ShouldEndSessionInRepository()
        {
            // Arrange
            _simulationService.Start();
            _simulationService.GenerateSimulationData();
            _simulationService.GenerateSimulationData();

            // Act
            await _sut.StopSimulation();

            // Assert
            _repositoryMock.Verify(
                r => r.EndSessionAsync(It.IsAny<Guid>(), 2),
                Times.Once);
        }

        [Fact]
        public async Task StopSimulation_WhenNoActiveSession_ShouldNotCallEndSession()
        {
            // Act
            await _sut.StopSimulation();

            // Assert
            _repositoryMock.Verify(
                r => r.EndSessionAsync(It.IsAny<Guid>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task StopSimulation_ShouldNotifyClientsViaSignalR()
        {
            // Arrange
            var mockClientProxy = new Mock<IClientProxy>();
            var mockClients = new Mock<IHubClients>();
            mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);
            _hubContextMock.Setup(h => h.Clients).Returns(mockClients.Object);

            _simulationService.Start();

            // Act
            await _sut.StopSimulation();

            // Assert
            mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    "SimulationStopped",
                    It.IsAny<object[]>(),
                    default),
                Times.Once);
        }

        #endregion

        #region GetConfig Tests

        [Fact]
        public void GetConfig_ShouldReturnOkWithConfig()
        {
            // Act
            var result = _sut.GetConfig();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var config = okResult.Value.Should().BeOfType<SimulationConfig>().Subject;
            config.UpdateIntervalMs.Should().Be(1000);
        }

        #endregion

        #region UpdateConfig Tests

        [Fact]
        public void UpdateConfig_ShouldReturnOk()
        {
            // Arrange
            var newConfig = new SimulationConfig
            {
                UpdateIntervalMs = 500,
                TemperatureMin = 25.0,
                TemperatureMax = 90.0
            };

            // Act
            var result = _sut.UpdateConfig(newConfig);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void UpdateConfig_ShouldUpdateServiceConfig()
        {
            // Arrange
            var newConfig = new SimulationConfig
            {
                UpdateIntervalMs = 500,
                TemperatureMin = 25.0,
                TemperatureMax = 90.0
            };

            // Act
            _sut.UpdateConfig(newConfig);

            // Assert
            var config = _simulationService.GetConfig();
            config.UpdateIntervalMs.Should().Be(500);
            config.TemperatureMin.Should().Be(25.0);
            config.TemperatureMax.Should().Be(90.0);
        }

        #endregion

        #region GetSnapshot Tests

        [Fact]
        public void GetSnapshot_WhenNotRunning_ShouldReturnBadRequest()
        {
            // Act
            var result = _sut.GetSnapshot();

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void GetSnapshot_WhenRunning_ShouldReturnOkWithData()
        {
            // Arrange
            _simulationService.Start();

            // Act
            var result = _sut.GetSnapshot();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var data = okResult.Value.Should().BeOfType<SimulationData>().Subject;
            data.IterationNumber.Should().Be(1);
        }

        [Fact]
        public void GetSnapshot_ShouldIncrementIterationEachCall()
        {
            // Arrange
            _simulationService.Start();

            // Act
            var result1 = _sut.GetSnapshot();
            var result2 = _sut.GetSnapshot();

            // Assert
            var data1 = ((OkObjectResult)result1.Result!).Value as SimulationData;
            var data2 = ((OkObjectResult)result2.Result!).Value as SimulationData;

            data1!.IterationNumber.Should().Be(1);
            data2!.IterationNumber.Should().Be(2);
        }

        #endregion
    }
}
