using CargoLink.ModernApi.Models;

namespace CargoLink.ModernApi.Services;

public class RateService : IRateService
{
    public Task<IEnumerable<RateQuoteResponse>> GetRatesAsync(RateRequest request)
    {
        var distanceFactor = CalculateDistanceFactor(request.OriginZipCode, request.DestinationZipCode);

        var quotes = new List<RateQuoteResponse>
        {
            BuildQuote("Ground", request.Weight, 0.50m, 5.00m, 5, distanceFactor),
            BuildQuote("Express", request.Weight, 1.20m, 8.00m, 2, distanceFactor),
            BuildQuote("Overnight", request.Weight, 2.50m, 12.00m, 1, distanceFactor)
        };

        return Task.FromResult<IEnumerable<RateQuoteResponse>>(quotes);
    }

    public Task<RateQuoteResponse?> GetRateForServiceAsync(RateRequest request, string serviceLevel)
    {
        var distanceFactor = CalculateDistanceFactor(request.OriginZipCode, request.DestinationZipCode);

        var (rateMultiplier, fuelSurcharge, estimatedDays) = serviceLevel.ToLowerInvariant() switch
        {
            "ground" => (0.50m, 5.00m, 5),
            "express" => (1.20m, 8.00m, 2),
            "overnight" => (2.50m, 12.00m, 1),
            _ => (0m, 0m, 0)
        };

        if (rateMultiplier == 0m)
            return Task.FromResult<RateQuoteResponse?>(null);

        var quote = BuildQuote(serviceLevel, request.Weight, rateMultiplier, fuelSurcharge, estimatedDays, distanceFactor);
        return Task.FromResult<RateQuoteResponse?>(quote);
    }

    // Matches legacy: weight * rateMultiplier * distanceFactor + fuelSurcharge
    private static RateQuoteResponse BuildQuote(
        string serviceLevel, decimal weight, decimal rateMultiplier,
        decimal fuelSurcharge, int estimatedDays, decimal distanceFactor)
    {
        var baseRate = weight * rateMultiplier;
        return new RateQuoteResponse
        {
            ServiceLevel = serviceLevel,
            BaseRate = baseRate,
            FuelSurcharge = fuelSurcharge,
            TotalCost = (baseRate * distanceFactor) + fuelSurcharge,
            EstimatedDays = estimatedDays
        };
    }

    // Stub matching legacy CalculateDistanceFactor
    private static decimal CalculateDistanceFactor(string originZip, string destinationZip)
    {
        return 1.2m;
    }
}
