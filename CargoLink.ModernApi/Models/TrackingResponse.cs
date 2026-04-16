namespace CargoLink.ModernApi.Models;

/// <summary>
/// Tracking information for a shipment including its event history.
/// </summary>
public class TrackingResponse
{
    /// <summary>The shipment tracking number.</summary>
    public string TrackingNumber { get; set; } = string.Empty;

    /// <summary>Current shipment status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Most recent known location of the shipment.</summary>
    public string CurrentLocation { get; set; } = string.Empty;

    /// <summary>Estimated delivery date (ISO 8601 format).</summary>
    public string? EstimatedDelivery { get; set; }

    /// <summary>Chronological list of tracking events.</summary>
    public List<TrackingEvent> Events { get; set; } = new();
}
