using Microsoft.EntityFrameworkCore;
using SimulationRealtimeApp.Data.Entities;

namespace SimulationRealtimeApp.Data
{
    public class SimulationDbContext : DbContext
    {
        public SimulationDbContext(DbContextOptions<SimulationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SimulationSession> Sessions { get; set; }
        public DbSet<SimulationDataEntity> DataPoints { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure SimulationSession
            modelBuilder.Entity<SimulationSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.StartedAt).IsRequired();
                entity.Property(e => e.StoppedAt).IsRequired(false);
                entity.Property(e => e.IterationCount).IsRequired();

                entity.HasIndex(e => e.StartedAt);
            });

            // Configure SimulationDataEntity
            modelBuilder.Entity<SimulationDataEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.SessionId).IsRequired();
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.Temperature).IsRequired();
                entity.Property(e => e.Pressure).IsRequired();
                entity.Property(e => e.Velocity).IsRequired();
                entity.Property(e => e.Energy).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IterationNumber).IsRequired();

                // Indexes for efficient queries
                entity.HasIndex(e => e.SessionId);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => new { e.SessionId, e.Timestamp });

                // Relationship
                entity.HasOne(e => e.Session)
                      .WithMany(s => s.DataPoints)
                      .HasForeignKey(e => e.SessionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
