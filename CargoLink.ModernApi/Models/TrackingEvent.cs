namespace CargoLink.ModernApi.Models;

public class TrackingEvent
{
    public string EventType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Description { get; set; }
}
