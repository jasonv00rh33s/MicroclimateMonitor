using Microclimate.Core.Models;
using Microclimate.Core.Services;
using Microclimate.Sensors;

namespace Microclimate.Tests;

public class SimulatedSensorSourceTests
{
    private static readonly DateTime Now = new(2026, 11, 2, 12, 0, 0);

    [Fact]
    public void SameSeed_GivesSameResults()
    {
        var sensor = new Sensor { Id = 1, Type = SensorType.Temperature };
        var a = new SimulatedSensorSource(seed: 42, dropoutProbability: 0);
        var b = new SimulatedSensorSource(seed: 42, dropoutProbability: 0);

        Assert.Equal(a.Read(sensor, Now)!.Value, b.Read(sensor, Now)!.Value);
    }

    [Fact]
    public void Dropout_ReturnsNull()
    {
        var sensor = new Sensor { Id = 1, Type = SensorType.Humidity };
        var source = new SimulatedSensorSource(seed: 1, dropoutProbability: 1.0);

        Assert.Null(source.Read(sensor, Now));
    }

    [Theory]
    [InlineData(SensorType.Temperature)]
    [InlineData(SensorType.Humidity)]
    [InlineData(SensorType.Co2)]
    public void GeneratedValues_PassPhysicalValidation(SensorType type)
    {
        var sensor = new Sensor { Id = 1, Type = type };
        var source = new SimulatedSensorSource(seed: 7, dropoutProbability: 0);
        var filter = new MeasurementFilter();

        for (int i = 0; i < 500; i++)
        {
            var m = source.Read(sensor, Now.AddMinutes(i));
            Assert.True(filter.IsValid(type, m!.Value));
        }
    }
}