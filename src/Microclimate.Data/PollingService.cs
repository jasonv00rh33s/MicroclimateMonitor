using Microclimate.Core.Models;
using Microclimate.Core.Services;
using Microclimate.Sensors;

namespace Microclimate.Data;

public class PollingService
{
    private readonly ISensorSource _source;
    private readonly MeasurementRepository _measurements;
    private readonly AlertRepository _alerts;
    private readonly RoomRepository _rooms;
    private readonly MeasurementFilter _filter;
    private readonly RuleEvaluator _evaluator;
    private readonly TimeSpan _alertCooldown;

    private readonly Dictionary<(int RoomId, int RuleId), DateTime> _lastAlert = new();

    public PollingService(
        ISensorSource source,
        MeasurementRepository measurements,
        AlertRepository alerts,
        RoomRepository rooms,
        MeasurementFilter filter,
        RuleEvaluator evaluator,
        TimeSpan? alertCooldown = null)
    {
        _source = source;
        _measurements = measurements;
        _alerts = alerts;
        _rooms = rooms;
        _filter = filter;
        _evaluator = evaluator;
        _alertCooldown = alertCooldown ?? TimeSpan.FromMinutes(10);
    }

    /// Один цикл опитування Повертає кількість збережених вимірювань
    public async Task<int> PollOnceAsync(DateTime now, CancellationToken ct = default)
    {
        var rooms = await _rooms.GetAllAsync(ct);
        var rules = await _alerts.GetRulesAsync(ct);

        var toSave = new List<Measurement>();
        var newAlerts = new List<Alert>();

        foreach (var room in rooms)
        {
            foreach (var sensor in room.Sensors)
            {
                var m = _source.Read(sensor, now);
                if (m is null) continue;                       // збій датчика
                if (!_filter.IsValid(sensor.Type, m.Value)) continue; // фізично неможливе значення

                toSave.Add(m);

                foreach (var rule in _evaluator.GetViolations(rules, sensor.Type, m.Value))
                {
                    var key = (room.Id, rule.Id);
                    if (_lastAlert.TryGetValue(key, out var last) && now - last < _alertCooldown)
                        continue;

                    _lastAlert[key] = now;
                    newAlerts.Add(new Alert
                    {
                        RoomId = room.Id,
                        RuleId = rule.Id,
                        Timestamp = now,
                        Value = m.Value
                    });
                }
            }
        }

        if (toSave.Count > 0) await _measurements.AddRangeAsync(toSave, ct);
        if (newAlerts.Count > 0) await _alerts.AddRangeAsync(newAlerts, ct);
        return toSave.Count;
    }

    /// Нескінченний цикл опитування зупиняється через CancellationToken
    public async Task RunAsync(TimeSpan interval, Func<DateTime> clock, CancellationToken ct,
                               Action<Exception>? onError = null)
    {
        using var timer = new PeriodicTimer(interval);
        try
        {
            while (await timer.WaitForNextTickAsync(ct))
            {
                try { await PollOnceAsync(clock(), ct); }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex) { onError?.Invoke(ex); } // помилка одного циклу не валить застосунок
            }
        }
        catch (OperationCanceledException) {  }
    }
}