using Microclimate.Core.Models;

namespace Microclimate.Sensors;

public class SimulatedSensorSource : ISensorSource
{
    private readonly Random _random;
    private readonly double _dropoutProbability;

    // Стан CO2 по датчику: росте, поки в кімнаті люди, падає при провітрюванні
    private readonly Dictionary<int, double> _co2 = new();

    public SimulatedSensorSource(int? seed = null, double dropoutProbability = 0.02)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
        _dropoutProbability = dropoutProbability;
    }

    public Measurement? Read(Sensor sensor, DateTime now)
    {
        if (_random.NextDouble() < _dropoutProbability)
            return null; // імітація збою

        double value = sensor.Type switch
        {
            SensorType.Temperature => 21 + 2 * Math.Sin(HourFraction(now) * 2 * Math.PI) + Noise(0.3),
            SensorType.Humidity    => 45 + 5 * Math.Cos(HourFraction(now) * 2 * Math.PI) + Noise(1.0),
            SensorType.Co2         => NextCo2(sensor.Id),
            _ => throw new ArgumentOutOfRangeException(nameof(sensor))
        };

        return new Measurement { SensorId = sensor.Id, Timestamp = now, Value = Math.Round(value, 1) };
    }

    private double NextCo2(int sensorId)
    {
        double current = _co2.GetValueOrDefault(sensorId, 450);
        current += 15 + Noise(5);                       // люди дихають
        if (_random.NextDouble() < 0.03) current -= 400; // провітрювання
        current = Math.Clamp(current, 400, 2500);
        _co2[sensorId] = current;
        return current;
    }

    private static double HourFraction(DateTime t) => (t.Hour + t.Minute / 60.0) / 24.0;
    private double Noise(double amplitude) => (_random.NextDouble() - 0.5) * 2 * amplitude;
}