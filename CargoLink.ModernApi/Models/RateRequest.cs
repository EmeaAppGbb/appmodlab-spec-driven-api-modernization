using System.ComponentModel.DataAnnotations;

namespace CargoLink.ModernApi.Models;

public class RateRequest
{
    [Required]
    public string OriginZipCode { get; set; } = string.Empty;

    [Required]
    public string DestinationZipCode { get; set; } = string.Empty;

    [Required]
    [Range(0.1, 10000)]
    public decimal Weight { get; set; }

    public string? ServiceLevel { get; set; }
}
