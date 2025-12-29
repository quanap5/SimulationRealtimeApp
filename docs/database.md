# Database Documentation

This document describes the database architecture, schema, and usage patterns for SimulationRealtimeApp.

## Overview

SimulationRealtimeApp uses **SQLite** with **Entity Framework Core 9.0** for data persistence. The database stores simulation session history and all generated data points.

### Technology Stack

| Component | Technology |
|-----------|------------|
| Database | SQLite |
| ORM | Entity Framework Core 9.0 |
| Provider | Microsoft.EntityFrameworkCore.Sqlite |

### Database Location

The database file is created in the application directory:

```
./simulation_history.db
```

## Schema

### Entity Relationship Diagram

```
┌─────────────────────────┐       ┌─────────────────────────────┐
│   SimulationSession     │       │   SimulationDataEntity      │
├─────────────────────────┤       ├─────────────────────────────┤
│ Id (GUID) [PK]          │──┐    │ Id (BIGINT) [PK]            │
│ StartedAt (DateTime)    │  │    │ SessionId (GUID) [FK]       │──┘
│ StoppedAt (DateTime?)   │  └───<│ Timestamp (DateTime)        │
│ IterationCount (INT)    │       │ Temperature (DOUBLE)        │
└─────────────────────────┘       │ Pressure (DOUBLE)           │
                                  │ Velocity (DOUBLE)           │
                                  │ Energy (DOUBLE)             │
                                  │ Status (VARCHAR(50))        │
                                  │ IterationNumber (INT)       │
                                  └─────────────────────────────┘
```

### Entities

#### SimulationSession

Represents a single simulation run from start to stop.

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | GUID | No | Primary key (GUID, not auto-generated) |
| StartedAt | DateTime | No | UTC timestamp when session started |
| StoppedAt | DateTime | Yes | UTC timestamp when session stopped (null if active) |
| IterationCount | INT | No | Total number of iterations in session |

**Indexes:**
- Primary Key: `Id`
- Index: `StartedAt` (for ordering sessions)

#### SimulationDataEntity

Represents a single data point generated during simulation.

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | BIGINT | No | Primary key (auto-increment) |
| SessionId | GUID | No | Foreign key to SimulationSession |
| Timestamp | DateTime | No | UTC timestamp of data generation |
| Temperature | DOUBLE | No | Temperature in Celsius |
| Pressure | DOUBLE | No | Pressure in Bar |
| Velocity | DOUBLE | No | Velocity in m/s |
| Energy | DOUBLE | No | Energy in Joules |
| Status | VARCHAR(50) | No | Status string (Normal/Warning/Critical/High Activity) |
| IterationNumber | INT | No | Sequential iteration number within session |

**Indexes:**
- Primary Key: `Id`
- Index: `SessionId` (for session queries)
- Index: `Timestamp` (for time-range queries)
- Composite Index: `SessionId, Timestamp` (for efficient filtered queries)

**Relationships:**
- Many-to-One with SimulationSession (Cascade Delete)

## DbContext Configuration

The `SimulationDbContext` is configured in `Data/SimulationDbContext.cs`:

```csharp
public class SimulationDbContext : DbContext
{
    public DbSet<SimulationSession> Sessions { get; set; }
    public DbSet<SimulationDataEntity> DataPoints { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Session configuration
        modelBuilder.Entity<SimulationSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            // ... more configuration
        });

        // DataPoint configuration
        modelBuilder.Entity<SimulationDataEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            // ... indexes and relationships
        });
    }
}
```

## Repository Pattern

Data access is abstracted through the `ISimulationHistoryRepository` interface:

```csharp
public interface ISimulationHistoryRepository
{
    // Session operations
    Task CreateSessionAsync(Guid sessionId);
    Task EndSessionAsync(Guid sessionId, int iterationCount);
    Task<SimulationSession?> GetSessionByIdAsync(Guid sessionId);
    Task<List<SimulationSession>> GetAllSessionsAsync(int skip = 0, int take = 50);

    // Data point operations
    Task SaveDataPointAsync(Guid sessionId, SimulationData data);
    Task<List<SimulationDataEntity>> GetDataPointsBySessionAsync(Guid sessionId);
    Task<List<SimulationDataEntity>> GetDataPointsByTimeRangeAsync(
        DateTime start, DateTime end, int skip = 0, int take = 1000);
}
```

## Common Queries

### Get All Sessions (Paginated)

```csharp
var sessions = await _repository.GetAllSessionsAsync(skip: 0, take: 50);
```

SQL equivalent:
```sql
SELECT * FROM Sessions
ORDER BY StartedAt DESC
LIMIT 50 OFFSET 0;
```

### Get Session with Data Points

```csharp
var session = await _repository.GetSessionByIdAsync(sessionId);
// Includes DataPoints navigation property, ordered by IterationNumber
```

SQL equivalent:
```sql
SELECT s.*, d.*
FROM Sessions s
LEFT JOIN DataPoints d ON s.Id = d.SessionId
WHERE s.Id = @sessionId
ORDER BY d.IterationNumber;
```

### Get Data Points by Time Range

```csharp
var dataPoints = await _repository.GetDataPointsByTimeRangeAsync(
    start: DateTime.UtcNow.AddHours(-1),
    end: DateTime.UtcNow,
    skip: 0,
    take: 1000
);
```

SQL equivalent:
```sql
SELECT * FROM DataPoints
WHERE Timestamp >= @start AND Timestamp <= @end
ORDER BY Timestamp
LIMIT 1000 OFFSET 0;
```

## Database Initialization

The database is automatically created on application startup using `EnsureCreated()`:

```csharp
// Program.cs
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SimulationDbContext>();
    dbContext.Database.EnsureCreated();
}
```

## Migrations

For schema changes in production, you can use EF Core migrations:

### Create Migration

```bash
dotnet ef migrations add MigrationName --project SimulationRealtimeApp
```

### Apply Migrations

```bash
dotnet ef database update --project SimulationRealtimeApp
```

### Generate SQL Script

```bash
dotnet ef migrations script --project SimulationRealtimeApp
```

## Performance Considerations

### Data Volume

At default settings (1 data point per second):
- 1 hour = 3,600 data points
- 1 day = 86,400 data points
- 1 week = 604,800 data points

### Index Strategy

The indexes are designed for:
1. **Session listing**: `StartedAt DESC` for reverse chronological order
2. **Session lookup**: Primary key on `Id`
3. **Time-based queries**: `Timestamp` index for range scans
4. **Combined queries**: Composite `SessionId, Timestamp` for filtered time ranges

### Query Optimization Tips

1. **Use Pagination**: Always use `skip` and `take` parameters
2. **Avoid Large Includes**: For sessions with many data points, consider separate queries
3. **Time Range Limits**: Limit time range queries to reasonable periods

## Data Retention

Currently, data is retained indefinitely. For production deployments, consider implementing:

1. **Automatic Cleanup**: Background job to delete old sessions
2. **Archival**: Move old data to cold storage
3. **Aggregation**: Summarize old data to reduce storage

### Example Cleanup Query

```sql
-- Delete sessions older than 30 days (cascade deletes data points)
DELETE FROM Sessions
WHERE StoppedAt IS NOT NULL
AND StoppedAt < datetime('now', '-30 days');
```

## Backup and Recovery

### Backup

SQLite databases can be backed up by copying the file:

```bash
cp simulation_history.db simulation_history_backup_$(date +%Y%m%d).db
```

### Recovery

To restore from backup:
1. Stop the application
2. Replace the database file
3. Restart the application

## Troubleshooting

### Common Issues

**Database locked error**:
- SQLite allows only one write at a time
- Ensure proper async/await usage
- Consider WAL mode for better concurrency

**Slow queries**:
- Check if indexes exist using: `SELECT * FROM sqlite_master WHERE type='index';`
- Use query profiling to identify slow queries
- Consider adding additional indexes for your query patterns

**Large database size**:
- Run `VACUUM` to reclaim space: `VACUUM;`
- Implement data retention policies
- Consider data archival strategies
