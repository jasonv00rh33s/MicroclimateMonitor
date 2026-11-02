csharp
using Microclimate.Core.Models;

namespace Microclimate.Sensors;

public interface ISensorSource
{
    /// Повертає наступне вимірювання або null, якщо датчик пропустив відлік
    Measurement? Read(Sensor sensor, DateTime now);
}