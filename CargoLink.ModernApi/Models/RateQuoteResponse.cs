namespace CargoLink.ModernApi.Models;

public class RateQuoteResponse
{
    public string ServiceLevel { get; set; } = string.Empty;
    public decimal BaseRate { get; set; }
    public decimal FuelSurcharge { get; set; }
    public decimal TotalCost { get; set; }
    public int EstimatedDays { get; set; }
}
