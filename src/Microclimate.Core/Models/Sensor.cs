namespace Microclimate.Core.Models;

public class Sensor
{
    public int Id {get; set;}
    public int RoomId {get; set;}
    public SensorType Type {get; set;}
    public string Unit {get; set;} = string.Empty;
    public Room? Room { get; set; }
    public List<Measurement> Measurements { get; set; } = new();
}