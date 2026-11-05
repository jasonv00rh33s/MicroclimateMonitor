using Microclimate.Core.Models;
using Microclimate.Core.Services;

namespace Microclimate.Tests;

public class AlertFormatterTests
{
    [Fact]
    public void MaxOnlyRule_IsFormatted()
    {
        var rule = new Rule { SensorType = SensorType.Co2, MaxValue = 1000 };
        Assert.Equal("CO₂: 1500 ppm (норма ≤ 1000 ppm)", AlertFormatter.Format(rule, 1500));
    }

    [Fact]
    public void RangeRule_IsFormatted()
    {
        var rule = new Rule { SensorType = SensorType.Temperature, MinValue = 18, MaxValue = 26 };
        Assert.Equal("Температура: 15.5 °C (норма 18–26 °C)", AlertFormatter.Format(rule, 15.5));
    }

    [Fact]
    public void MinOnlyRule_IsFormatted()
    {
        var rule = new Rule { SensorType = SensorType.Humidity, MinValue = 30 };
        Assert.Equal("Вологість: 20 % (норма ≥ 30 %)", AlertFormatter.Format(rule, 20));
    }

    [Fact]
    public void RuleWithoutLimits_IsHandled()
    {
        var rule = new Rule { SensorType = SensorType.Co2 };
        Assert.Contains("межі не задані", AlertFormatter.Format(rule, 500));
    }
}