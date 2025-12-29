using FluentAssertions;
using SimulationRealtimeApp.Models;
using SimulationRealtimeApp.Services;
using Xunit;

namespace SimulationRealtimeApp.Tests.Services
{
    public class SimulationServiceTests
    {
        private readonly SimulationService _sut;

        public SimulationServiceTests()
        {
            _sut = new SimulationService();
        }

        [Fact]
        public void Start_ShouldSetIsRunningToTrue()
        {
            // Act
            _sut.Start();

            // Assert
            _sut.IsRunning.Should().BeTrue();
        }

        [Fact]
        public void Start_ShouldResetIterationNumber()
        {
            // Arrange
            _sut.Start();
            _sut.GenerateSimulationData();
            _sut.GenerateSimulationData();
            _sut.Stop();

            // Act
            _sut.Start();

            // Assert
            _sut.CurrentIteration.Should().Be(0);
        }

        [Fact]
        public void Start_ShouldReturnNewSessionId()
        {
            // Act
            var sessionId = _sut.Start();

            // Assert
            sessionId.Should().NotBe(Guid.Empty);
            _sut.CurrentSessionId.Should().Be(sessionId);
        }

        [Fact]
        public void Start_ShouldGenerateUniqueSessionIds()
        {
            // Act
            var sessionId1 = _sut.Start();
            _sut.Stop();
            var sessionId2 = _sut.Start();

            // Assert
            sessionId1.Should().NotBe(sessionId2);
        }

        [Fact]
        public void Stop_ShouldSetIsRunningToFalse()
        {
            // Arrange
            _sut.Start();

            // Act
            _sut.Stop();

            // Assert
            _sut.IsRunning.Should().BeFalse();
        }

        [Fact]
        public void Stop_ShouldClearCurrentSessionId()
        {
            // Arrange
            _sut.Start();

            // Act
            _sut.Stop();

            // Assert
            _sut.CurrentSessionId.Should().BeNull();
        }

        [Fact]
        public void GenerateSimulationData_WhenNotRunning_ShouldReturnNull()
        {
            // Act
            var result = _sut.GenerateSimulationData();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GenerateSimulationData_WhenRunning_ShouldReturnData()
        {
            // Arrange
            _sut.Start();

            // Act
            var result = _sut.GenerateSimulationData();

            // Assert
            result.Should().NotBeNull();
            result.IterationNumber.Should().Be(1);
        }

        [Fact]
        public void GenerateSimulationData_ShouldIncrementIterationNumber()
        {
            // Arrange
            _sut.Start();

            // Act
            var result1 = _sut.GenerateSimulationData();
            var result2 = _sut.GenerateSimulationData();
            var result3 = _sut.GenerateSimulationData();

            // Assert
            result1.IterationNumber.Should().Be(1);
            result2.IterationNumber.Should().Be(2);
            result3.IterationNumber.Should().Be(3);
            _sut.CurrentIteration.Should().Be(3);
        }

        [Fact]
        public void GenerateSimulationData_ShouldReturnDataWithinConfiguredRanges()
        {
            // Arrange
            _sut.Start();
            var config = _sut.GetConfig();

            // Act
            var result = _sut.GenerateSimulationData();

            // Assert
            result.Temperature.Should().BeInRange(config.TemperatureMin - 10, config.TemperatureMax + 10);
            result.Pressure.Should().BeInRange(config.PressureMin - 1, config.PressureMax + 1);
            result.Velocity.Should().BeInRange(config.VelocityMin - 5, config.VelocityMax + 5);
        }

        [Fact]
        public void GenerateSimulationData_ShouldReturnValidStatus()
        {
            // Arrange
            _sut.Start();

            // Act
            var result = _sut.GenerateSimulationData();

            // Assert
            result.Status.Should().BeOneOf("Normal", "Warning", "Critical", "High Activity");
        }

        [Fact]
        public void GenerateSimulationData_ShouldReturnPositiveEnergy()
        {
            // Arrange
            _sut.Start();

            // Act
            var result = _sut.GenerateSimulationData();

            // Assert
            result.Energy.Should().BeGreaterThan(0);
        }

        [Fact]
        public void GenerateSimulationData_ShouldSetTimestamp()
        {
            // Arrange
            _sut.Start();
            var before = DateTime.UtcNow;

            // Act
            var result = _sut.GenerateSimulationData();

            // Assert
            result.Timestamp.Should().BeOnOrAfter(before);
            result.Timestamp.Should().BeOnOrBefore(DateTime.UtcNow);
        }

        [Fact]
        public void GetStatus_ShouldReturnCorrectRunningState()
        {
            // Act
            var statusBefore = _sut.GetStatus(5);
            _sut.Start();
            var statusAfter = _sut.GetStatus(5);

            // Assert
            statusBefore.IsRunning.Should().BeFalse();
            statusAfter.IsRunning.Should().BeTrue();
        }

        [Fact]
        public void GetStatus_ShouldReturnConnectedClients()
        {
            // Act
            var status = _sut.GetStatus(10);

            // Assert
            status.ConnectedClients.Should().Be(10);
        }

        [Fact]
        public void GetStatus_ShouldReturnCorrectIterationCounts()
        {
            // Arrange
            _sut.Start();
            _sut.GenerateSimulationData();
            _sut.GenerateSimulationData();

            // Act
            var status = _sut.GetStatus(0);

            // Assert
            status.TotalIterations.Should().Be(2);
            status.CurrentIteration.Should().Be(2);
        }

        [Fact]
        public void UpdateConfig_ShouldUpdateAllValues()
        {
            // Arrange
            var newConfig = new SimulationConfig
            {
                UpdateIntervalMs = 500,
                TemperatureMin = 15.0,
                TemperatureMax = 85.0,
                PressureMin = 2.0,
                PressureMax = 8.0,
                VelocityMin = 5.0,
                VelocityMax = 45.0
            };

            // Act
            _sut.UpdateConfig(newConfig);
            var result = _sut.GetConfig();

            // Assert
            result.UpdateIntervalMs.Should().Be(500);
            result.TemperatureMin.Should().Be(15.0);
            result.TemperatureMax.Should().Be(85.0);
            result.PressureMin.Should().Be(2.0);
            result.PressureMax.Should().Be(8.0);
            result.VelocityMin.Should().Be(5.0);
            result.VelocityMax.Should().Be(45.0);
        }

        [Fact]
        public void GetConfig_ShouldReturnDefaultValues()
        {
            // Act
            var config = _sut.GetConfig();

            // Assert
            config.UpdateIntervalMs.Should().Be(1000);
            config.TemperatureMin.Should().Be(20.0);
            config.TemperatureMax.Should().Be(100.0);
            config.PressureMin.Should().Be(1.0);
            config.PressureMax.Should().Be(10.0);
            config.VelocityMin.Should().Be(0.0);
            config.VelocityMax.Should().Be(50.0);
        }
    }
}
