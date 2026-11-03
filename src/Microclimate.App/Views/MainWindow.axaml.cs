using Avalonia.Controls;
using Microclimate.App.ViewModels;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Microclimate.App.Views;

public partial class MainWindow : Window
{
    private static readonly string[] MetricNames = { "Температура, °C", "Вологість, %", "CO₂, ppm" };
    private MainWindowViewModel? _vm;

    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_vm is not null) _vm.PropertyChanged -= OnVmPropertyChanged;

        _vm = DataContext as MainWindowViewModel;
        if (_vm is null) return;

        _vm.PropertyChanged += OnVmPropertyChanged;
        _vm.Refreshed += async (_, _) => await RedrawAsync();
    }

    private async void OnVmPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MainWindowViewModel.SelectedRoom)
                           or nameof(MainWindowViewModel.SelectedMetricIndex)
                           or nameof(MainWindowViewModel.SelectedRangeIndex))
            await RedrawAsync();
    }

    private async Task RedrawAsync()
    {
        if (_vm is null) return;

        var series = await _vm.GetSeriesAsync();

        Chart.Plot.Clear();
        if (series.Count > 0)
        {
            // Час у БД зберігається в UTC, на графіку показуємо локальний
            var xs = series.Select(p => p.Time.ToLocalTime().ToOADate()).ToArray();
            var ys = series.Select(p => p.Value).ToArray();

            var line = Chart.Plot.Add.Scatter(xs, ys);
            line.MarkerSize = 0;
            line.LineWidth = 2;

            Chart.Plot.Axes.DateTimeTicksBottom();
            Chart.Plot.Axes.AutoScale();
        }

        var room = _vm.SelectedRoom?.Name ?? "—";
        Chart.Plot.Title($"{room}: {MetricNames[Math.Clamp(_vm.SelectedMetricIndex, 0, 2)]}");
        Chart.Refresh();
    }
}