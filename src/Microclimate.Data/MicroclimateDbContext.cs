using Microclimate.Core.Models;
using Microsoft.EntityFrameworkCore;


namespace Microclimate.Data;

public class MicroclimateDbContext : DbContext
{
    public MicroclimateDbContext(DbContextOptions<MicroclimateDbContext> options) : base(options) {}

    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Sensor> Sensors => Set<Sensor>();
    public DbSet<Measurement> Measurements => Set<Measurement>();
    public DbSet<Rule> Rules => Set<Rule>();
    public DbSet<Alert> Alerts => Set<Alert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>(e =>
        {
            e.Property(r => r.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(r => r.Name).IsUnique();
        });

        modelBuilder.Entity<Sensor>(e =>
        {
            e.Property(s => s.Unit).HasMaxLength(20);
            e.HasOne(s => s.Room).WithMany(r => r.Sensors)
             .HasForeignKey(s => s.RoomId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Measurement>(e =>
        {
            e.HasOne(m => m.Sensor).WithMany(s => s.Measurements)
             .HasForeignKey(m => m.SensorId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(m => new { m.SensorId, m.Timestamp });
        });

        modelBuilder.Entity<Alert>(e =>
        {
            e.HasOne(a => a.Room).WithMany().HasForeignKey(a => a.RoomId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Rule).WithMany().HasForeignKey(a => a.RuleId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(a => new { a.RoomId, a.Timestamp });
        });

        modelBuilder.Entity<Rule>().HasData(
            new Rule { Id = 1, SensorType = SensorType.Co2, MaxValue = 1000, Severity = Severity.Warning },
            new Rule { Id = 2, SensorType = SensorType.Temperature, MinValue = 18, MaxValue = 26, Severity = Severity.Warning },
            new Rule { Id = 3, SensorType = SensorType.Humidity, MinValue = 30, MaxValue = 60, Severity = Severity.Info });
    }
}
