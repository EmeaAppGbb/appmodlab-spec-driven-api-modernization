namespace CargoLink.ModernApi.Models;

/// <summary>
/// A shipping rate quote for a specific service level.
/// </summary>
public class RateQuoteResponse
{
    /// <summary>The shipping service level (e.g., Ground, Express, Overnight).</summary>
    public string ServiceLevel { get; set; } = string.Empty;

    /// <summary>Base shipping rate in USD.</summary>
    public decimal BaseRate { get; set; }

    /// <summary>Fuel surcharge in USD.</summary>
    public decimal FuelSurcharge { get; set; }

    /// <summary>Total cost including all surcharges in USD.</summary>
    public decimal TotalCost { get; set; }

    /// <summary>Estimated delivery time in business days.</summary>
    public int EstimatedDays { get; set; }
}
