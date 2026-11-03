using Microclimate.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Microclimate.Tests;

public class DbInitializerTests
{
    [Fact]
    public void Initialize_SeedsOnce()
    {
        using var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();
        var options = new DbContextOptionsBuilder<MicroclimateDbContext>().UseSqlite(conn).Options;
        using var db = new MicroclimateDbContext(options);

        DbInitializer.Initialize(db);
        DbInitializer.Initialize(db); // повторний виклик не дублює дані

        Assert.Equal(3, db.Rooms.Count());
        Assert.Equal(9, db.Sensors.Count());
    }
}