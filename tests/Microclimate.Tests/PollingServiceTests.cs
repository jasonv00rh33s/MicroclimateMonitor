using Microclimate.Core.Models;
using Microclimate.Core.Services;
using Microclimate.Data;
using Microclimate.Sensors;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Microclimate.Tests;

public class PollingServiceTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly MicroclimateDbContext _db;

    // Датчик-заглушка повертає задане значення (або null)
    private class FixedSource : ISensorSource
    {
        public double? Value { get; set; }
        public Measurement? Read(Sensor sensor, DateTime now)
            => Value is null ? null : new Measurement { SensorId = sensor.Id, Timestamp = now, Value = Value.Value };
    }

    public PollingServiceTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        var options = new DbContextOptionsBuilder<MicroclimateDbContext>().UseSqlite(_conn).Options;
        _db = new MicroclimateDbContext(options);
        _db.Database.EnsureCreated();
    }

    public void Dispose() { _db.Dispose(); _conn.Dispose(); }

    private async Task<(PollingService svc, FixedSource src)> ArrangeAsync(TimeSpan? cooldown = null)
    {
        var room = await new RoomRepository(_db).AddAsync("Тест", null);
        _db.Sensors.Add(new Sensor { RoomId = room.Id, Type = SensorType.Co2, Unit = "ppm" });
        await _db.SaveChangesAsync();

        var src = new FixedSource { Value = 600 };
        var svc = new PollingService(src, new MeasurementRepository(_db), new AlertRepository(_db),
            new RoomRepository(_db), new MeasurementFilter(), new RuleEvaluator(), cooldown);
        return (svc, src);
    }

    [Fact]
    public async Task NormalValue_IsSaved_NoAlert()
    {
        var (svc, _) = await ArrangeAsync();
        var saved = await svc.PollOnceAsync(DateTime.UtcNow);

        Assert.Equal(1, saved);
        Assert.Equal(0, await _db.Alerts.CountAsync());
    }

    [Fact]
    public async Task HighCo2_CreatesAlert()
    {
        var (svc, src) = await ArrangeAsync();
        src.Value = 1500;

        await svc.PollOnceAsync(DateTime.UtcNow);

        Assert.Equal(1, await _db.Alerts.CountAsync());
    }

    [Fact]
    public async Task Cooldown_PreventsDuplicateAlerts()
    {
        var (svc, src) = await ArrangeAsync(TimeSpan.FromMinutes(10));
        src.Value = 1500;
        var t = DateTime.UtcNow;

        await svc.PollOnceAsync(t);
        await svc.PollOnceAsync(t.AddMinutes(1));   // у межах cooldown
        Assert.Equal(1, await _db.Alerts.CountAsync());

        await svc.PollOnceAsync(t.AddMinutes(11));  // cooldown минув
        Assert.Equal(2, await _db.Alerts.CountAsync());
    }

    [Fact]
    public async Task SensorDropout_IsSkipped()
    {
        var (svc, src) = await ArrangeAsync();
        src.Value = null;

        var saved = await svc.PollOnceAsync(DateTime.UtcNow);

        Assert.Equal(0, saved);
        Assert.Equal(0, await _db.Measurements.CountAsync());
    }

    [Fact]
    public async Task PhysicallyImpossibleValue_IsDiscarded()
    {
        var (svc, src) = await ArrangeAsync();
        src.Value = 99999; // CO2 поза діапазоном датчика

        var saved = await svc.PollOnceAsync(DateTime.UtcNow);

        Assert.Equal(0, saved);
    }

    [Fact]
    public async Task RunAsync_Cancellation_StopsGracefully()
    {
        var (svc, _) = await ArrangeAsync();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));

        await svc.RunAsync(TimeSpan.FromMilliseconds(50), () => DateTime.UtcNow, cts.Token);
    }
}