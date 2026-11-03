using Microclimate.Core.Models;
using Microclimate.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Microclimate.Tests;

public class RepositoryTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly MicroclimateDbContext _db;

    public RepositoryTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        var options = new DbContextOptionsBuilder<MicroclimateDbContext>().UseSqlite(_conn).Options;
        _db = new MicroclimateDbContext(options);
        _db.Database.EnsureCreated();
    }

    public void Dispose() { _db.Dispose(); _conn.Dispose(); }

    [Fact]
    public async Task AddRoom_Duplicate_Throws()
    {
        var repo = new RoomRepository(_db);
        await repo.AddAsync("Кімната 101", null);
        await Assert.ThrowsAsync<InvalidOperationException>(() => repo.AddAsync("Кімната 101", null));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddRoom_EmptyName_Throws(string name)
    {
        var repo = new RoomRepository(_db);
        await Assert.ThrowsAsync<ArgumentException>(() => repo.AddAsync(name, null));
    }

    [Fact]
    public async Task AddRange_SavesAndReadsBack()
    {
        var room = await new RoomRepository(_db).AddAsync("Лабораторія", null);
        var sensor = new Sensor { RoomId = room.Id, Type = SensorType.Temperature, Unit = "°C" };
        _db.Sensors.Add(sensor);
        await _db.SaveChangesAsync();

        var repo = new MeasurementRepository(_db);
        var t = new DateTime(2026, 11, 2, 12, 0, 0);
        await repo.AddRangeAsync(new[]
        {
            new Measurement { SensorId = sensor.Id, Timestamp = t, Value = 21 },
            new Measurement { SensorId = sensor.Id, Timestamp = t.AddMinutes(1), Value = 22 }
        });

        var result = await repo.GetRangeAsync(sensor.Id, t, t.AddMinutes(5));
        Assert.Equal(2, result.Count);
        Assert.Equal(22, (await repo.GetLatestAsync(sensor.Id))!.Value);
    }

    [Fact]
    public async Task AddRange_InvalidFk_RollsBackEverything()
    {
        var repo = new MeasurementRepository(_db);
        var t = DateTime.UtcNow;

        // SensorId=999 не існує, FK порушується, весь пакет має відкотитися
        await Assert.ThrowsAnyAsync<Exception>(() => repo.AddRangeAsync(new[]
        {
            new Measurement { SensorId = 999, Timestamp = t, Value = 21 }
        }));

        Assert.Equal(0, await _db.Measurements.CountAsync());
    }
}