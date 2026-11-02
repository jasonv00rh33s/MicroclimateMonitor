namespace Microclimate.Core.Models;

public class Measurement
{
    public int Id {get; set;}
    public int SensorId {get; set;}
    public DateTime Timestamp {get; set;}
    public double Value {get; set;}
}