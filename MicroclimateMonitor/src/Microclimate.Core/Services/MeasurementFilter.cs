using Microclimate.Core.Models;

namespace Microclimate.Core.Services;

public class MeasurementFilter
{
    private static readonly Dictionary<SensorType, (double Min, double Max)> PhysicalRanges = new()
    {
        [SensorType.Temperature] = (-40, 85),
        [SensorType.Humidity]    = (0, 100),
        [SensorType.Co2]         = (300, 5000)
    };

    /// Чи є значення фізично можливим для даного типу датчика
    public bool IsValid(SensorType type, double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value)) return false;
        var (min, max) = PhysicalRanges[type];
        return value >= min && value <= max;
    }

    /// Залишає лише коректні вимірювання, порядок зберігається
    public IEnumerable<Measurement> RemoveInvalid(SensorType type, IEnumerable<Measurement> measurements)
        => measurements.Where(m => IsValid(type, m.Value));

    /// Згладжування медіанним вікном (непарний розмір вікна, наприклад 3 або 5)
    public IReadOnlyList<double> MedianSmooth(IReadOnlyList<double> values, int window = 3)
    {
        if (window < 1 || window % 2 == 0)
            throw new ArgumentException("Розмір вікна має бути непарним і додатним.", nameof(window));

        var result = new double[values.Count];
        int half = window / 2;

        for (int i = 0; i < values.Count; i++)
        {
            int from = Math.Max(0, i - half);
            int to = Math.Min(values.Count - 1, i + half);
            var slice = values.Skip(from).Take(to - from + 1).OrderBy(v => v).ToList();
            result[i] = slice[slice.Count / 2];
        }
        return result;
    }
}