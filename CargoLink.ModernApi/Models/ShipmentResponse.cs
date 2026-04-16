namespace CargoLink.ModernApi.Models;

public class ShipmentResponse
{
    public string Id { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string OriginAddress { get; set; } = string.Empty;
    public string OriginCity { get; set; } = string.Empty;
    public string OriginZipCode { get; set; } = string.Empty;
    public string DestinationAddress { get; set; } = string.Empty;
    public string DestinationCity { get; set; } = string.Empty;
    public string DestinationZipCode { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string ServiceLevel { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public string? EstimatedDeliveryDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
