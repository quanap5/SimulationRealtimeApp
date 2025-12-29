namespace SimulationRealtimeApp.Data.Entities
{
    public class SimulationSession
    {
        public Guid Id { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? StoppedAt { get; set; }
        public int IterationCount { get; set; }

        // Navigation property
        public ICollection<SimulationDataEntity> DataPoints { get; set; } = new List<SimulationDataEntity>();
    }
}
