using Microclimate.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Microclimate.Data;

public static class DbInitializer
{
    public static void Initialize(MicroclimateDbContext db)
    {
        db.Database.Migrate();
        db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;"); // менше блокувань при читанні й записі одночасно

        if (db.Rooms.Any()) return;

        foreach (var name in new[] { "Кімната 101", "Гуртожиток, 214", "Лабораторія" })
        {
            var room = new Room { Name = name };
            room.Sensors.Add(new Sensor { Type = SensorType.Temperature, Unit = "°C" });
            room.Sensors.Add(new Sensor { Type = SensorType.Humidity, Unit = "%" });
            room.Sensors.Add(new Sensor { Type = SensorType.Co2, Unit = "ppm" });
            db.Rooms.Add(room);
        }
        db.SaveChanges();
    }
}