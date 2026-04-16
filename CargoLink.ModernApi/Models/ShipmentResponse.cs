namespace CargoLink.ModernApi.Models;

/// <summary>
/// Represents a shipment with its full details.
/// </summary>
public class ShipmentResponse
{
    /// <summary>Unique shipment identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Tracking number used to track this shipment.</summary>
    public string TrackingNumber { get; set; } = string.Empty;

    /// <summary>Current shipment status (e.g., Created, InTransit, Delivered, Cancelled).</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Origin street address.</summary>
    public string OriginAddress { get; set; } = string.Empty;

    /// <summary>Origin city name.</summary>
    public string OriginCity { get; set; } = string.Empty;

    /// <summary>Origin ZIP/postal code.</summary>
    public string OriginZipCode { get; set; } = string.Empty;

    /// <summary>Destination street address.</summary>
    public string DestinationAddress { get; set; } = string.Empty;

    /// <summary>Destination city name.</summary>
    public string DestinationCity { get; set; } = string.Empty;

    /// <summary>Destination ZIP/postal code.</summary>
    public string DestinationZipCode { get; set; } = string.Empty;

    /// <summary>Package weight in pounds.</summary>
    public decimal Weight { get; set; }

    /// <summary>Selected shipping service level.</summary>
    public string ServiceLevel { get; set; } = string.Empty;

    /// <summary>Total shipping cost in USD.</summary>
    public decimal TotalCost { get; set; }

    /// <summary>Estimated delivery date (ISO 8601 format).</summary>
    public string? EstimatedDeliveryDate { get; set; }

    /// <summary>Timestamp when the shipment was created.</summary>
    public DateTime CreatedAt { get; set; }
}
