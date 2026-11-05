using System.Globalization;
using Microclimate.Core.Models;

namespace Microclimate.Core.Services;

public static class AlertFormatter
{
    public static string Format(Rule rule, double value)
    {
        var (name, unit) = rule.SensorType switch
        {
            SensorType.Temperature => ("Температура", "°C"),
            SensorType.Humidity    => ("Вологість", "%"),
            SensorType.Co2         => ("CO₂", "ppm"),
            _ => (rule.SensorType.ToString(), "")
        };

        string limit = (rule.MinValue, rule.MaxValue) switch
        {
            ({ } min, { } max) => $"норма {Num(min)}–{Num(max)} {unit}",
            ({ } min, null)    => $"норма ≥ {Num(min)} {unit}",
            (null, { } max)    => $"норма ≤ {Num(max)} {unit}",
            _                  => "межі не задані"
        };

        return $"{name}: {Num(value)} {unit} ({limit})";
    }

    private static string Num(double v) => v.ToString("0.#", CultureInfo.InvariantCulture);
}