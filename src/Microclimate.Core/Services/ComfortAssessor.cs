using Microclimate.Core.Models;

namespace Microclimate.Core.Services;

public class ComfortAssessor
{
    public const double Co2Limit = 1000;
    public const double TempMin = 18;
    public const double TempMax = 26;
    public const double HumidityMin = 30;
    public const double HumidityMax = 60;

    /// Повертає перелік проблем комфорту Порожній список = усе добре
    /// Значення null означає що датчика немає або дані відсутні

    public IReadOnlyList<ComfortLevel> Assess(double? temperature, double? humidity, double? co2)
    {
        var issues = new List<ComfortLevel>();

        if (co2 is > Co2Limit) issues.Add(ComfortLevel.Stuffy);

        if (temperature is < TempMin) issues.Add(ComfortLevel.Cold);
        else if (temperature is > TempMax) issues.Add(ComfortLevel.Hot);

        if (humidity is < HumidityMin) issues.Add(ComfortLevel.Dry);
        else if (humidity is > HumidityMax) issues.Add(ComfortLevel.Humid);

        if (issues.Count == 0) issues.Add(ComfortLevel.Good);
        return issues;
    }
}