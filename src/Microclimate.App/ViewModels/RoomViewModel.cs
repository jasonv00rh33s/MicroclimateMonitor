using CommunityToolkit.Mvvm.ComponentModel;
using Microclimate.Core.Models;
using Microclimate.Core.Services;
using System.Linq;

namespace Microclimate.App.ViewModels;

public partial class RoomViewModel : ObservableObject
{
    private static readonly ComfortAssessor Assessor = new();

    public int Id { get; }
    public string Name { get; }

    [ObservableProperty] private string _temperature = "—";
    [ObservableProperty] private string _humidity = "—";
    [ObservableProperty] private string _co2 = "—";
    [ObservableProperty] private string _comfort = "Немає даних";

    public RoomViewModel(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public void Update(double? temp, double? hum, double? co2)
    {
        Temperature = temp is null ? "—" : $"{temp:F1} °C";
        Humidity = hum is null ? "—" : $"{hum:F0} %";
        Co2 = co2 is null ? "—" : $"{co2:F0} ppm";

        if (temp is null && hum is null && co2 is null)
        {
            Comfort = "Немає даних";
            return;
        }

        var issues = Assessor.Assess(temp, hum, co2);
        Comfort = string.Join(", ", issues.Select(ToText));
    }

    private static string ToText(ComfortLevel level) => level switch
    {
        ComfortLevel.Good   => "Добре",
        ComfortLevel.Stuffy => "Душно",
        ComfortLevel.Cold   => "Холодно",
        ComfortLevel.Hot    => "Жарко",
        ComfortLevel.Dry    => "Сухо",
        ComfortLevel.Humid  => "Волого",
        _ => level.ToString()
    };
}