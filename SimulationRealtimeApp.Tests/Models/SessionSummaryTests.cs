using FluentAssertions;
using SimulationRealtimeApp.Models;
using Xunit;

namespace SimulationRealtimeApp.Tests.Models
{
    public class SessionSummaryTests
    {
        #region DurationSeconds Tests

        [Fact]
        public void DurationSeconds_WhenSessionStopped_ShouldReturnCorrectDuration()
        {
            // Arrange
            var startedAt = DateTime.UtcNow.AddHours(-1);
            var stoppedAt = DateTime.UtcNow;

            var session = new SessionSummary
            {
                SessionId = Guid.NewGuid(),
                StartedAt = startedAt,
                StoppedAt = stoppedAt,
                IterationCount = 100
            };

            // Act
            var duration = session.DurationSeconds;

            // Assert
            duration.Should().BeApproximately(3600, 1); // 1 hour = 3600 seconds
        }

        [Fact]
        public void DurationSeconds_WhenSessionActive_ShouldReturnNull()
        {
            // Arrange
            var session = new SessionSummary
            {
                SessionId = Guid.NewGuid(),
                StartedAt = DateTime.UtcNow.AddHours(-1),
                StoppedAt = null,
                IterationCount = 50
            };

            // Act
            var duration = session.DurationSeconds;

            // Assert
            duration.Should().BeNull();
        }

        [Fact]
        public void DurationSeconds_WithShortSession_ShouldReturnCorrectValue()
        {
            // Arrange
            var startedAt = DateTime.UtcNow;
            var stoppedAt = startedAt.AddSeconds(30);

            var session = new SessionSummary
            {
                SessionId = Guid.NewGuid(),
                StartedAt = startedAt,
                StoppedAt = stoppedAt,
                IterationCount = 30
            };

            // Act
            var duration = session.DurationSeconds;

            // Assert
            duration.Should().BeApproximately(30, 0.001);
        }

        #endregion

        #region IsActive Tests

        [Fact]
        public void IsActive_WhenStoppedAtIsNull_ShouldReturnTrue()
        {
            // Arrange
            var session = new SessionSummary
            {
                SessionId = Guid.NewGuid(),
                StartedAt = DateTime.UtcNow,
                StoppedAt = null,
                IterationCount = 0
            };

            // Act & Assert
            session.IsActive.Should().BeTrue();
        }

        [Fact]
        public void IsActive_WhenStoppedAtHasValue_ShouldReturnFalse()
        {
            // Arrange
            var session = new SessionSummary
            {
                SessionId = Guid.NewGuid(),
                StartedAt = DateTime.UtcNow.AddHours(-1),
                StoppedAt = DateTime.UtcNow,
                IterationCount = 100
            };

            // Act & Assert
            session.IsActive.Should().BeFalse();
        }

        #endregion
    }

    public class SimulationConfigTests
    {
        [Fact]
        public void DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var config = new SimulationConfig();

            // Assert
            config.UpdateIntervalMs.Should().Be(1000);
            config.TemperatureMin.Should().Be(20.0);
            config.TemperatureMax.Should().Be(100.0);
            config.PressureMin.Should().Be(1.0);
            config.PressureMax.Should().Be(10.0);
            config.VelocityMin.Should().Be(0.0);
            config.VelocityMax.Should().Be(50.0);
        }

        [Fact]
        public void Properties_ShouldBeSettable()
        {
            // Arrange
            var config = new SimulationConfig();

            // Act
            config.UpdateIntervalMs = 500;
            config.TemperatureMin = 15.0;
            config.TemperatureMax = 95.0;
            config.PressureMin = 2.0;
            config.PressureMax = 8.0;
            config.VelocityMin = 5.0;
            config.VelocityMax = 45.0;

            // Assert
            config.UpdateIntervalMs.Should().Be(500);
            config.TemperatureMin.Should().Be(15.0);
            config.TemperatureMax.Should().Be(95.0);
            config.PressureMin.Should().Be(2.0);
            config.PressureMax.Should().Be(8.0);
            config.VelocityMin.Should().Be(5.0);
            config.VelocityMax.Should().Be(45.0);
        }
    }

    public class SimulationDataTests
    {
        [Fact]
        public void DefaultStatus_ShouldBeEmptyString()
        {
            // Arrange & Act
            var data = new SimulationData();

            // Assert
            data.Status.Should().BeEmpty();
        }

        [Fact]
        public void Properties_ShouldBeSettable()
        {
            // Arrange
            var timestamp = DateTime.UtcNow;

            // Act
            var data = new SimulationData
            {
                Timestamp = timestamp,
                Temperature = 45.5,
                Pressure = 5.2,
                Velocity = 25.3,
                Energy = 485.7,
                Status = "Normal",
                IterationNumber = 42
            };

            // Assert
            data.Timestamp.Should().Be(timestamp);
            data.Temperature.Should().Be(45.5);
            data.Pressure.Should().Be(5.2);
            data.Velocity.Should().Be(25.3);
            data.Energy.Should().Be(485.7);
            data.Status.Should().Be("Normal");
            data.IterationNumber.Should().Be(42);
        }
    }

    public class SimulationStatusTests
    {
        [Fact]
        public void Properties_ShouldBeSettable()
        {
            // Arrange
            var startTime = DateTime.UtcNow;

            // Act
            var status = new SimulationStatus
            {
                IsRunning = true,
                TotalIterations = 100,
                CurrentIteration = 50,
                StartTime = startTime,
                ConnectedClients = 5
            };

            // Assert
            status.IsRunning.Should().BeTrue();
            status.TotalIterations.Should().Be(100);
            status.CurrentIteration.Should().Be(50);
            status.StartTime.Should().Be(startTime);
            status.ConnectedClients.Should().Be(5);
        }
    }

    public class TimeRangeResponseTests
    {
        [Fact]
        public void DataPoints_ShouldDefaultToEmptyList()
        {
            // Arrange & Act
            var response = new TimeRangeResponse();

            // Assert
            response.DataPoints.Should().NotBeNull();
            response.DataPoints.Should().BeEmpty();
        }
    }

    public class SessionDetailsTests
    {
        [Fact]
        public void DataPoints_ShouldDefaultToEmptyList()
        {
            // Arrange & Act
            var details = new SessionDetails();

            // Assert
            details.DataPoints.Should().NotBeNull();
            details.DataPoints.Should().BeEmpty();
        }

        [Fact]
        public void ShouldInheritFromSessionSummary()
        {
            // Arrange & Act
            var details = new SessionDetails
            {
                SessionId = Guid.NewGuid(),
                StartedAt = DateTime.UtcNow,
                StoppedAt = null,
                IterationCount = 50
            };

            // Assert
            details.IsActive.Should().BeTrue();
            details.DurationSeconds.Should().BeNull();
        }
    }
}
