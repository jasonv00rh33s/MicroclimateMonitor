using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Microclimate.Core.Models;
using Microclimate.Core.Services;

namespace Microclimate.App.ViewModels;

public partial class AlertViewModel : ObservableObject
{
    public int Id { get; }
    public string RoomName { get; }
    public string Time { get; }
    public string Text { get; }
    public string SeverityText { get; }
    public IBrush SeverityBrush { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAcknowledge))]
    [NotifyPropertyChangedFor(nameof(RowOpacity))]
    private bool _acknowledged;

    public bool CanAcknowledge => !Acknowledged;
    public double RowOpacity => Acknowledged ? 0.5 : 1.0;

    public AlertViewModel(Alert alert)
    {
        Id = alert.Id;
        RoomName = alert.Room?.Name ?? "—";
        Time = alert.Timestamp.ToLocalTime().ToString("dd.MM HH:mm:ss");
        Acknowledged = alert.Acknowledged;

        var severity = alert.Rule?.Severity ?? Severity.Info;
        (SeverityText, SeverityBrush) = severity switch
        {
            Severity.Critical => ("Критично", Brush.Parse("#EF4444")),
            Severity.Warning  => ("Увага", Brush.Parse("#F59E0B")),
            _                 => ("Інфо", Brush.Parse("#3B82F6"))
        };

        Text = alert.Rule is null
            ? $"Значення {alert.Value:F1}"
            : AlertFormatter.Format(alert.Rule, alert.Value);
    }
}