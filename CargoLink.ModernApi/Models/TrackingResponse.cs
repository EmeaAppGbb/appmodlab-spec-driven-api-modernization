namespace CargoLink.ModernApi.Models;

public class TrackingResponse
{
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CurrentLocation { get; set; } = string.Empty;
    public string? EstimatedDelivery { get; set; }
    public List<TrackingEvent> Events { get; set; } = new();
}
