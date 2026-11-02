using Microclimate.Core.Models;
using Microclimate.Core.Services;

namespace Microclimate.Tests;

public class RuleEvaluatorTests
{
    private readonly RuleEvaluator _sut = new();

    [Theory]
    [InlineData(999, false)]
    [InlineData(1000, false)]   // межа
    [InlineData(1001, true)]
    public void Co2_MaxRule_Works(double value, bool expected)
    {
        var rule = new Rule { SensorType = SensorType.Co2, MaxValue = 1000 };
        Assert.Equal(expected, _sut.IsViolated(rule, value));
    }

    [Theory]
    [InlineData(17.9, true)]
    [InlineData(18, false)]     // межа
    [InlineData(25, false)]
    public void Temperature_MinRule_Works(double value, bool expected)
    {
        var rule = new Rule { SensorType = SensorType.Temperature, MinValue = 18 };
        Assert.Equal(expected, _sut.IsViolated(rule, value));
    }

    [Fact]
    public void NaN_Throws()
    {
        var rule = new Rule { MaxValue = 1 };
        Assert.Throws<ArgumentException>(() => _sut.IsViolated(rule, double.NaN));
    }

    [Fact]
    public void InvertedRange_Throws()
    {
        var rule = new Rule { MinValue = 30, MaxValue = 10 };
        Assert.Throws<ArgumentException>(() => _sut.IsViolated(rule, 20));
    }
}