namespace SimulationRealtimeApp.Models
{
    public class SimulationData
    {
        public DateTime Timestamp { get; set; }
        public double Temperature { get; set; }
        public double Pressure { get; set; }
        public double Velocity { get; set; }
        public double Energy { get; set; }
        public string Status { get; set; } = string.Empty;
        public int IterationNumber { get; set; }
    }

    public class SimulationStatus
    {
        public bool IsRunning { get; set; }
        public int TotalIterations { get; set; }
        public int CurrentIteration { get; set; }
        public DateTime StartTime { get; set; }
        public int ConnectedClients { get; set; }
    }

    public class SimulationConfig
    {
        public int UpdateIntervalMs { get; set; } = 1000;
        public double TemperatureMin { get; set; } = 20.0;
        public double TemperatureMax { get; set; } = 100.0;
        public double PressureMin { get; set; } = 1.0;
        public double PressureMax { get; set; } = 10.0;
        public double VelocityMin { get; set; } = 0.0;
        public double VelocityMax { get; set; } = 50.0;
    }
}
