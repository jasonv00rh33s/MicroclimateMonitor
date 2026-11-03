using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microclimate.Core.Models;
using Microclimate.Data;
using System.Threading.Tasks;
using System.Linq;
using Microclimate.Core.Services;

namespace Microclimate.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly RoomRepository _rooms;
    private readonly MeasurementRepository _measurements;
    private readonly SemaphoreSlim _dbLock = new(1, 1); // один контекст = одна операція за раз

    public ObservableCollection<RoomViewModel> Rooms { get; } = new();

    [ObservableProperty] private string _status = "Завантаження...";
    [ObservableProperty] private RoomViewModel? _selectedRoom;
    [ObservableProperty] private int _selectedMetricIndex;  // 0 = t, 1 = вологість, 2 = CO2 (порядок SensorType)
    [ObservableProperty] private int _selectedRangeIndex;   // 0 = година, 1 = доба, 2 = тиждень

    /// Подія: дані оновилися, графік варто перемалювати
    public event EventHandler? Refreshed;

    public MainWindowViewModel() : this(null!, null!) { }

    public MainWindowViewModel(RoomRepository rooms, MeasurementRepository measurements)
    {
        _rooms = rooms;
        _measurements = measurements;
    }

    public async Task RefreshAsync()
    {
        if (_rooms is null) return;

        await _dbLock.WaitAsync();
        try
        {
            var rooms = await _rooms.GetAllAsync();

            foreach (var r in rooms.Where(r => Rooms.All(vm => vm.Id != r.Id)))
                Rooms.Add(new RoomViewModel(r.Id, r.Name));

            foreach (var room in rooms)
            {
                var vm = Rooms.First(x => x.Id == room.Id);
                double? t = null, h = null, c = null;

                foreach (var sensor in room.Sensors)
                {
                    var last = await _measurements.GetLatestAsync(sensor.Id);
                    if (last is null) continue;
                    switch (sensor.Type)
                    {
                        case SensorType.Temperature: t = last.Value; break;
                        case SensorType.Humidity:    h = last.Value; break;
                        case SensorType.Co2:         c = last.Value; break;
                    }
                }
                vm.Update(t, h, c);
            }

            SelectedRoom ??= Rooms.FirstOrDefault();
            Status = $"Оновлено: {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            Status = $"Помилка оновлення: {ex.Message}";
        }
        finally
        {
            _dbLock.Release();
        }

        Refreshed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Точки для графіка за поточним вибором кімнати, показника й періоду.</summary>
    public async Task<IReadOnlyList<(DateTime Time, double Value)>> GetSeriesAsync()
    {
        var empty = Array.Empty<(DateTime, double)>();
        if (_rooms is null || SelectedRoom is null) return empty;

        await _dbLock.WaitAsync();
        try
        {
            var type = (SensorType)SelectedMetricIndex;
            var span = SelectedRangeIndex switch
            {
                0 => TimeSpan.FromHours(1),
                1 => TimeSpan.FromDays(1),
                _ => TimeSpan.FromDays(7)
            };

            var room = (await _rooms.GetAllAsync()).FirstOrDefault(r => r.Id == SelectedRoom.Id);
            var sensor = room?.Sensors.FirstOrDefault(s => s.Type == type);
            if (sensor is null) return empty;

            var to = DateTime.UtcNow;
            var data = await _measurements.GetRangeAsync(sensor.Id, to - span, to);
            return Downsampler.Downsample(data, 600);
        }
        catch (Exception ex)
        {
            Status = $"Помилка завантаження графіка: {ex.Message}";
            return empty;
        }
        finally
        {
            _dbLock.Release();
        }
    }
}