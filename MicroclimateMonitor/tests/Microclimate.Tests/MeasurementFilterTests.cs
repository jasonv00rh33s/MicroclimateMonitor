using Microclimate.Core.Models;
using Microclimate.Core.Services;

namespace Microclimate.Tests;

public class MeasurementFilterTests
{
    private readonly MeasurementFilter _sut = new();

    [Theory]
    [InlineData(SensorType.Temperature, 22, true)]
    [InlineData(SensorType.Temperature, -41, false)]
    [InlineData(SensorType.Humidity, 100, true)]   // межа
    [InlineData(SensorType.Humidity, 101, false)]
    [InlineData(SensorType.Co2, 299, false)]
    [InlineData(SensorType.Co2, 450, true)]
    public void IsValid_ChecksPhysicalRange(SensorType type, double value, bool expected)
        => Assert.Equal(expected, _sut.IsValid(type, value));

    [Fact]
    public void IsValid_RejectsNaNAndInfinity()
    {
        Assert.False(_sut.IsValid(SensorType.Temperature, double.NaN));
        Assert.False(_sut.IsValid(SensorType.Temperature, double.PositiveInfinity));
    }

    [Fact]
    public void RemoveInvalid_DropsBadMeasurements()
    {
        var data = new[]
        {
            new Measurement { Value = 21 },
            new Measurement { Value = double.NaN },
            new Measurement { Value = 500 },   // нереально для температури
            new Measurement { Value = 22 }
        };

        var result = _sut.RemoveInvalid(SensorType.Temperature, data).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void MedianSmooth_RemovesSpike()
    {
        var result = _sut.MedianSmooth(new double[] { 20, 20, 90, 20, 20 }, 3);
        Assert.Equal(20, result[2]);
    }

    [Fact]
    public void MedianSmooth_EmptyInput_ReturnsEmpty()
        => Assert.Empty(_sut.MedianSmooth(Array.Empty<double>()));

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void MedianSmooth_InvalidWindow_Throws(int window)
        => Assert.Throws<ArgumentException>(() => _sut.MedianSmooth(new double[] { 1, 2, 3 }, window));
}