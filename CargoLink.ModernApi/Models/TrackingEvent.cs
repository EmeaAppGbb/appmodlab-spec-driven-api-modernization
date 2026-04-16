namespace CargoLink.ModernApi.Models;

/// <summary>
/// Represents a single tracking event in a shipment's journey.
/// </summary>
public class TrackingEvent
{
    /// <summary>Type of tracking event (e.g., PickedUp, InTransit, Delivered).</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Location where the event occurred.</summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>Timestamp when the event occurred.</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>Optional human-readable description of the event.</summary>
    public string? Description { get; set; }
}
