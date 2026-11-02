using Microclimate.Core.Models;
using Microclimate.Core.Services;

namespace Microclimate.Tests;

public class ComfortAssessorTests
{
    private readonly ComfortAssessor _sut = new();

    [Fact]
    public void NormalValues_ReturnGood()
    {
        var result = _sut.Assess(22, 45, 600);
        Assert.Equal(new[] { ComfortLevel.Good }, result);
    }

    [Fact]
    public void BoundaryValues_AreStillGood()
    {
        var result = _sut.Assess(18, 30, 1000);
        Assert.Equal(new[] { ComfortLevel.Good }, result);
    }

    [Fact]
    public void HighCo2_ReturnsStuffy()
        => Assert.Contains(ComfortLevel.Stuffy, _sut.Assess(22, 45, 1200));

    [Fact]
    public void LowTemperature_ReturnsCold()
        => Assert.Contains(ComfortLevel.Cold, _sut.Assess(15, 45, 600));

    [Fact]
    public void HighTemperature_ReturnsHot()
        => Assert.Contains(ComfortLevel.Hot, _sut.Assess(30, 45, 600));

    [Fact]
    public void LowHumidity_ReturnsDry()
        => Assert.Contains(ComfortLevel.Dry, _sut.Assess(22, 20, 600));

    [Fact]
    public void MultipleProblems_AreAllReported()
    {
        var result = _sut.Assess(15, 20, 1500);
        Assert.Contains(ComfortLevel.Stuffy, result);
        Assert.Contains(ComfortLevel.Cold, result);
        Assert.Contains(ComfortLevel.Dry, result);
    }

    [Fact]
    public void MissingData_IsIgnored()
    {
        var result = _sut.Assess(null, null, null);
        Assert.Equal(new[] { ComfortLevel.Good }, result);
    }
}