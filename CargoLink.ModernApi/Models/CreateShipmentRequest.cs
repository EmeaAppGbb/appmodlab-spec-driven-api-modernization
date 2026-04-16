using System.ComponentModel.DataAnnotations;

namespace CargoLink.ModernApi.Models;

/// <summary>
/// Request payload for creating a new shipment.
/// </summary>
public class CreateShipmentRequest
{
    /// <summary>Origin street address.</summary>
    [Required]
    public string OriginAddress { get; set; } = string.Empty;

    /// <summary>Origin city name.</summary>
    public string OriginCity { get; set; } = string.Empty;

    /// <summary>Origin ZIP/postal code.</summary>
    [Required]
    public string OriginZipCode { get; set; } = string.Empty;

    /// <summary>Destination street address.</summary>
    [Required]
    public string DestinationAddress { get; set; } = string.Empty;

    /// <summary>Destination city name.</summary>
    public string DestinationCity { get; set; } = string.Empty;

    /// <summary>Destination ZIP/postal code.</summary>
    [Required]
    public string DestinationZipCode { get; set; } = string.Empty;

    /// <summary>Package weight in pounds (0.1–10,000).</summary>
    [Required]
    [Range(0.1, 10000)]
    public decimal Weight { get; set; }

    /// <summary>Package length in inches.</summary>
    public decimal? Length { get; set; }

    /// <summary>Package width in inches.</summary>
    public decimal? Width { get; set; }

    /// <summary>Package height in inches.</summary>
    public decimal? Height { get; set; }

    /// <summary>Shipping service level (e.g., Ground, Express, Overnight).</summary>
    [Required]
    public string ServiceLevel { get; set; } = string.Empty;
}
