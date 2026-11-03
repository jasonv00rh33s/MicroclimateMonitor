using Microclimate.Core.Models;

namespace Microclimate.Core.Services;

public static class Downsampler
{
    /// Зменшує кількість точок до maxPoints усереднюючи значення в кожному кошику
    public static IReadOnlyList<(DateTime Time, double Value)> Downsample(
        IReadOnlyList<Measurement> data, int maxPoints)
    {
        if (maxPoints < 1)
            throw new ArgumentException("maxPoints має бути додатним.", nameof(maxPoints));

        if (data.Count <= maxPoints)
            return data.Select(m => (m.Timestamp, m.Value)).ToList();

        var result = new List<(DateTime, double)>(maxPoints);
        double step = (double)data.Count / maxPoints;

        for (int i = 0; i < maxPoints; i++)
        {
            int from = (int)(i * step);
            int to = Math.Min(data.Count, (int)((i + 1) * step));
            if (to <= from) to = from + 1;

            double sum = 0;
            for (int j = from; j < to; j++) sum += data[j].Value;

            result.Add((data[from + (to - from) / 2].Timestamp, sum / (to - from)));
        }
        return result;
    }
}