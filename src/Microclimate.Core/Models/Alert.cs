namespace Microclimate.Core.Models;

public class Alert
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public int RuleId { get; set; }
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public bool Acknowledged { get; set; }

    public Room? Room { get; set; }
    public Rule? Rule { get; set; }
}