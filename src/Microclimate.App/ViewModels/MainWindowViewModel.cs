using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microclimate.Core.Models;
using Microclimate.Data;
using System.Threading.Tasks;
using System.Linq;

namespace Microclimate.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly RoomRepository _rooms;
    private readonly MeasurementRepository _measurements;

    public ObservableCollection<RoomViewModel> Rooms { get; } = new();

    [ObservableProperty] private string _status = "Завантаження...";

    // Конструктор без параметрів потрібен для дизайнера Avalonia
    public MainWindowViewModel() : this(null!, null!) { }

    public MainWindowViewModel(RoomRepository rooms, MeasurementRepository measurements)
    {
        _rooms = rooms;
        _measurements = measurements;
    }

    public async Task RefreshAsync()
    {
        if (_rooms is null) return;

        try
        {
            var rooms = await _rooms.GetAllAsync();

            // Синхронізуємо список (додаємо нові кімнати)
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

            Status = $"Оновлено: {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            Status = $"Помилка оновлення: {ex.Message}";
        }
    }
}