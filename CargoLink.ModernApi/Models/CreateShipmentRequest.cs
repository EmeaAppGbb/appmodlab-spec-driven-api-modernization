using System.ComponentModel.DataAnnotations;

namespace CargoLink.ModernApi.Models;

public class CreateShipmentRequest
{
    [Required]
    public string OriginAddress { get; set; } = string.Empty;

    public string OriginCity { get; set; } = string.Empty;

    [Required]
    public string OriginZipCode { get; set; } = string.Empty;

    [Required]
    public string DestinationAddress { get; set; } = string.Empty;

    public string DestinationCity { get; set; } = string.Empty;

    [Required]
    public string DestinationZipCode { get; set; } = string.Empty;

    [Required]
    [Range(0.1, 10000)]
    public decimal Weight { get; set; }

    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }

    [Required]
    public string ServiceLevel { get; set; } = string.Empty;
}
