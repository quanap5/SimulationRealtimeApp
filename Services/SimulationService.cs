using SimulationRealtimeApp.Models;

namespace SimulationRealtimeApp.Services
{
    public class SimulationService
    {
        private readonly Random _random = new Random();
        private int _iterationNumber = 0;
        private bool _isRunning = false;
        private DateTime _startTime;
        private Guid? _currentSessionId;
        private readonly SimulationConfig _config = new SimulationConfig();

        public bool IsRunning => _isRunning;
        public int CurrentIteration => _iterationNumber;
        public Guid? CurrentSessionId => _currentSessionId;

        public Guid Start()
        {
            _isRunning = true;
            _startTime = DateTime.UtcNow;
            _iterationNumber = 0;
            _currentSessionId = Guid.NewGuid();
            return _currentSessionId.Value;
        }

        public void Stop()
        {
            _isRunning = false;
            _currentSessionId = null;
        }

        public SimulationData GenerateSimulationData()
        {
            if (!_isRunning)
                return null!;

            _iterationNumber++;

            // Simulate realistic physics data with some randomness
            var timeElapsed = (DateTime.UtcNow - _startTime).TotalSeconds;
            
            // Temperature increases with time and oscillates
            var temperature = _config.TemperatureMin + 
                (_config.TemperatureMax - _config.TemperatureMin) * 
                (0.5 + 0.3 * Math.Sin(timeElapsed / 10) + 0.2 * _random.NextDouble());

            // Pressure varies inversely with temperature
            var pressure = _config.PressureMax - 
                (temperature - _config.TemperatureMin) / 
                (_config.TemperatureMax - _config.TemperatureMin) * 
                (_config.PressureMax - _config.PressureMin) +
                _random.NextDouble() * 0.5;

            // Velocity increases gradually
            var velocity = _config.VelocityMin + 
                (_config.VelocityMax - _config.VelocityMin) * 
                Math.Min(timeElapsed / 60, 1.0) +
                (_random.NextDouble() - 0.5) * 5;

            // Energy is calculated from other parameters
            var energy = 0.5 * velocity * velocity + temperature * 2.5;

            // Determine status based on parameters
            var status = DetermineStatus(temperature, pressure, velocity);

            return new SimulationData
            {
                Timestamp = DateTime.UtcNow,
                Temperature = Math.Round(temperature, 2),
                Pressure = Math.Round(pressure, 2),
                Velocity = Math.Round(velocity, 2),
                Energy = Math.Round(energy, 2),
                Status = status,
                IterationNumber = _iterationNumber
            };
        }

        private string DetermineStatus(double temperature, double pressure, double velocity)
        {
            if (temperature > 90)
                return "Critical";
            else if (temperature > 75 || pressure < 2)
                return "Warning";
            else if (velocity > 40)
                return "High Activity";
            else
                return "Normal";
        }

        public SimulationStatus GetStatus(int connectedClients)
        {
            return new SimulationStatus
            {
                IsRunning = _isRunning,
                TotalIterations = _iterationNumber,
                CurrentIteration = _iterationNumber,
                StartTime = _startTime,
                ConnectedClients = connectedClients
            };
        }

        public void UpdateConfig(SimulationConfig config)
        {
            _config.UpdateIntervalMs = config.UpdateIntervalMs;
            _config.TemperatureMin = config.TemperatureMin;
            _config.TemperatureMax = config.TemperatureMax;
            _config.PressureMin = config.PressureMin;
            _config.PressureMax = config.PressureMax;
            _config.VelocityMin = config.VelocityMin;
            _config.VelocityMax = config.VelocityMax;
        }

        public SimulationConfig GetConfig()
        {
            return _config;
        }
    }
}
