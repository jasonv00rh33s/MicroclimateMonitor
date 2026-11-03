using Microclimate.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Microclimate.Data;

public class MeasurementRepository
{
    private readonly MicroclimateDbContext _db;
    public MeasurementRepository(MicroclimateDbContext db) => _db = db;

    /// >Пакетний запис в одній транзакції: або всі або жодного
    public async Task AddRangeAsync(IEnumerable<Measurement> items, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            await _db.Measurements.AddRangeAsync(items, ct);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            _db.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task<List<Measurement>> GetRangeAsync(
        int sensorId, DateTime from, DateTime to, CancellationToken ct = default)
        => await _db.Measurements.AsNoTracking()
            .Where(m => m.SensorId == sensorId && m.Timestamp >= from && m.Timestamp <= to)
            .OrderBy(m => m.Timestamp)
            .ToListAsync(ct);

    public async Task<Measurement?> GetLatestAsync(int sensorId, CancellationToken ct = default)
        => await _db.Measurements.AsNoTracking()
            .Where(m => m.SensorId == sensorId)
            .OrderByDescending(m => m.Timestamp)
            .FirstOrDefaultAsync(ct);
}