using Microclimate.Core.Models;

namespace Microclimate.Core.Services;

public class RuleEvaluator
{
    /// Повертає true якщо значення виходить за межі
    public bool IsViolated(Rule rule, double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentException("Значення має бути скінченним числом.", nameof(value));

        if (rule.MinValue.HasValue && rule.MaxValue.HasValue && rule.MinValue > rule.MaxValue)
            throw new ArgumentException("MinValue не може бути більшим за MaxValue.", nameof(rule));

        // Межі включені в норму: рівно 1000 ppm при Max=1000 це ще не порушення
        if (rule.MinValue.HasValue && value < rule.MinValue.Value) return true;
        if (rule.MaxValue.HasValue && value > rule.MaxValue.Value) return true;
        return false;
    }

    public IEnumerable<Rule> GetViolations(IEnumerable<Rule> rules, SensorType type, double value)
        => rules.Where(r => r.SensorType == type && IsViolated(r, value));
}