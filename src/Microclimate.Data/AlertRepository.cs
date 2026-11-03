using Microclimate.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Microclimate.Data;

public class AlertRepository
{
    private readonly MicroclimateDbContext _db;
    public AlertRepository(MicroclimateDbContext db) => _db = db;

    public async Task<List<Rule>> GetRulesAsync(CancellationToken ct = default)
        => await _db.Rules.AsNoTracking().ToListAsync(ct);

    public async Task AddRangeAsync(IEnumerable<Alert> alerts, CancellationToken ct = default)
    {
        await _db.Alerts.AddRangeAsync(alerts, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<Alert>> GetRecentAsync(int count = 50, CancellationToken ct = default)
        => await _db.Alerts.AsNoTracking()
            .Include(a => a.Room).Include(a => a.Rule)
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync(ct);
}