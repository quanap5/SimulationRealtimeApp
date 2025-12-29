namespace SimulationRealtimeApp.Data.Entities
{
    public class SimulationDataEntity
    {
        public long Id { get; set; }
        public Guid SessionId { get; set; }
        public DateTime Timestamp { get; set; }
        public double Temperature { get; set; }
        public double Pressure { get; set; }
        public double Velocity { get; set; }
        public double Energy { get; set; }
        public string Status { get; set; } = string.Empty;
        public int IterationNumber { get; set; }

        // Navigation property
        public SimulationSession Session { get; set; } = null!;
    }
}
