namespace SimulationRealtimeApp.Models
{
    public class SessionSummary
    {
        public Guid SessionId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? StoppedAt { get; set; }
        public int IterationCount { get; set; }
        public double? DurationSeconds => StoppedAt.HasValue
            ? (StoppedAt.Value - StartedAt).TotalSeconds
            : null;
        public bool IsActive => !StoppedAt.HasValue;
    }

    public class SessionDetails : SessionSummary
    {
        public List<SimulationData> DataPoints { get; set; } = new();
    }

    public class TimeRangeResponse
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int ReturnedCount { get; set; }
        public List<SimulationDataWithSession> DataPoints { get; set; } = new();
    }

    public class SimulationDataWithSession
    {
        public Guid SessionId { get; set; }
        public DateTime Timestamp { get; set; }
        public double Temperature { get; set; }
        public double Pressure { get; set; }
        public double Velocity { get; set; }
        public double Energy { get; set; }
        public string Status { get; set; } = string.Empty;
        public int IterationNumber { get; set; }
    }
}
