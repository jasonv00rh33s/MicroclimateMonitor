using Microclimate.Core.Models;
using Microclimate.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Microclimate.Tests;

public class AlertRepositoryTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly MicroclimateDbContext _db;

    public AlertRepositoryTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        var options = new DbContextOptionsBuilder<MicroclimateDbContext>().UseSqlite(_conn).Options;
        _db = new MicroclimateDbContext(options);
        _db.Database.EnsureCreated(); 
    }

    public void Dispose() 
    { 
        _db.Dispose();
        _conn.Dispose(); 
        GC.SuppressFinalize(this);
    }

    private async Task<(AlertRepository repo, Alert alert)> ArrangeAsync()
    {
        var room = await new RoomRepository(_db).AddAsync("Тест", null);
        var alert = new Alert { RoomId = room.Id, RuleId = 1, Timestamp = DateTime.UtcNow, Value = 1500 };
        var repo = new AlertRepository(_db);
        await repo.AddRangeAsync(new[] { alert });
        return (repo, alert);
    }

    [Fact]
    public async Task Acknowledge_SetsFlag_AndUpdatesCount()
    {
        var (repo, alert) = await ArrangeAsync();
        Assert.Equal(1, await repo.GetUnacknowledgedCountAsync());

        Assert.True(await repo.AcknowledgeAsync(alert.Id));

        Assert.Equal(0, await repo.GetUnacknowledgedCountAsync());
    }

    [Fact]
    public async Task Acknowledge_UnknownId_ReturnsFalse()
    {
        var (repo, _) = await ArrangeAsync();
        Assert.False(await repo.AcknowledgeAsync(9999));
    }

    [Fact]
    public async Task GetRecent_IncludesRoomAndRule_NewestFirst()
    {
        var (repo, _) = await ArrangeAsync();
        var room = _db.Rooms.First();
        await repo.AddRangeAsync(new[]
        {
            new Alert { RoomId = room.Id, RuleId = 2, Timestamp = DateTime.UtcNow.AddMinutes(5), Value = 30 }
        });

        var result = await repo.GetRecentAsync(10);

        Assert.Equal(2, result.Count);
        Assert.Equal(2, result[0].RuleId);   // найновіше першим
        Assert.NotNull(result[0].Room);
        Assert.NotNull(result[0].Rule);
    }
}