namespace Microclimate.Core.Models;

public enum Severity { Info, Warning, Critical }

public class Rule
{
    public int Id {get; set;}
    public SensorType SensorType {get; set;}
    public int? MinValue {get; set;}
    public int? MaxValue {get; set;}
    public Severity Severity {get; set;}
}