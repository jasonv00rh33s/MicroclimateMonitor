using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microclimate.App.ViewModels;
using Microclimate.App.Views;
using Microclimate.Core.Services;
using Microclimate.Data;
using Microclimate.Sensors;
using Microsoft.EntityFrameworkCore;
using Serilog;


namespace Microclimate.App;

public partial class App : Application
{
    private readonly CancellationTokenSource _cts = new();

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MicroclimateMonitor");
            Directory.CreateDirectory(dir);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(Path.Combine(dir, "logs", "app-.log"), rollingInterval: RollingInterval.Day)
                .CreateLogger();
                Log.Information("Застосунок запущено");

            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
                Log.Fatal(e.ExceptionObject as Exception, "Необроблений виняток");
            TaskScheduler.UnobservedTaskException += (_, e) =>
                Log.Error(e.Exception, "Необроблений виняток у задачі");      

            var options = new DbContextOptionsBuilder<MicroclimateDbContext>()
                .UseSqlite($"Data Source={Path.Combine(dir, "microclimate.db")}")
                .Options;

            using (var initDb = new MicroclimateDbContext(options))
                DbInitializer.Initialize(initDb);

            // Контекст №1: тільки для UI-потоку
            var uiDb = new MicroclimateDbContext(options);
            var vm = new MainWindowViewModel(
                new RoomRepository(uiDb),
                new MeasurementRepository(uiDb),
                new AlertRepository(uiDb));

            // Контекст №2: тільки для фонового опитування
            var pollDb = new MicroclimateDbContext(options);
            var polling = new PollingService(
                new SimulatedSensorSource(),
                new MeasurementRepository(pollDb),
                new AlertRepository(pollDb),
                new RoomRepository(pollDb),
                new MeasurementFilter(),
                new RuleEvaluator());

            _ = Task.Run(() => polling.RunAsync(
                TimeSpan.FromSeconds(3), () => DateTime.UtcNow, _cts.Token,
                ex => Log.Error(ex, "Помилка циклу опитування")));

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            timer.Tick += async (_, _) => await vm.RefreshAsync();
            timer.Start();

            desktop.MainWindow = new MainWindow { DataContext = vm };
            desktop.Exit += (_, _) =>
            {
                timer.Stop();
                _cts.Cancel();
                uiDb.Dispose();
                pollDb.Dispose();
                Log.CloseAndFlush();
                Log.Information("Застосунок зупинено");
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}