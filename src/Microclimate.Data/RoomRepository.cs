using Microclimate.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Microclimate.Data;

public class RoomRepository
{
    private readonly MicroclimateDbContext _db;
    public RoomRepository(MicroclimateDbContext db) => _db = db;

    public async Task<List<Room>> GetAllAsync(CancellationToken ct = default)
        => await _db.Rooms.AsNoTracking().Include(r => r.Sensors).OrderBy(r => r.Name).ToListAsync(ct);

    public async Task<Room> AddAsync(string name, string? description, CancellationToken ct = default)
    {
        name = name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва кімнати не може бути порожньою.", nameof(name));
        if (await _db.Rooms.AnyAsync(r => r.Name == name, ct))
            throw new InvalidOperationException($"Кімната «{name}» уже існує.");

        var room = new Room { Name = name, Description = description };
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync(ct);
        return room;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var room = await _db.Rooms.FindAsync(new object[] { id }, ct)
                   ?? throw new InvalidOperationException("Кімнату не знайдено.");
        _db.Rooms.Remove(room);
        await _db.SaveChangesAsync(ct);
    }
}