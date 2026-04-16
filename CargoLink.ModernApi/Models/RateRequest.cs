using System.ComponentModel.DataAnnotations;

namespace CargoLink.ModernApi.Models;

/// <summary>
/// Request payload for obtaining shipping rate quotes.
/// </summary>
public class RateRequest
{
    /// <summary>Origin ZIP/postal code.</summary>
    [Required]
    public string OriginZipCode { get; set; } = string.Empty;

    /// <summary>Destination ZIP/postal code.</summary>
    [Required]
    public string DestinationZipCode { get; set; } = string.Empty;

    /// <summary>Package weight in pounds (0.1–10,000).</summary>
    [Required]
    [Range(0.1, 10000)]
    public decimal Weight { get; set; }

    /// <summary>Optional preferred service level to filter results.</summary>
    public string? ServiceLevel { get; set; }
}
