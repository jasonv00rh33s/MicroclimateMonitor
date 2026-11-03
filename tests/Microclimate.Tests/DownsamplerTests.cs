using Microclimate.Core.Models;
using Microclimate.Core.Services;

namespace Microclimate.Tests;

public class DownsamplerTests
{
    private static List<Measurement> Make(int count)
        => Enumerable.Range(0, count)
            .Select(i => new Measurement { Timestamp = new DateTime(2026, 1, 1).AddSeconds(i), Value = i })
            .ToList();

    [Fact]
    public void SmallInput_IsReturnedAsIs()
        => Assert.Equal(5, Downsampler.Downsample(Make(5), 10).Count);

    [Fact]
    public void LargeInput_IsReducedToMaxPoints()
        => Assert.Equal(100, Downsampler.Downsample(Make(10_000), 100).Count);

    [Fact]
    public void Buckets_AreAveraged()
    {
        // 4 точки 2 кошики: (0+1)/2 і (2+3)/2
        var result = Downsampler.Downsample(Make(4), 2);
        Assert.Equal(0.5, result[0].Value);
        Assert.Equal(2.5, result[1].Value);
    }

    [Fact]
    public void EmptyInput_ReturnsEmpty()
        => Assert.Empty(Downsampler.Downsample(new List<Measurement>(), 10));

    [Fact]
    public void InvalidMaxPoints_Throws()
        => Assert.Throws<ArgumentException>(() => Downsampler.Downsample(Make(3), 0));
}