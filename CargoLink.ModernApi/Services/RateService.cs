using CargoLink.ModernApi.Models;

namespace CargoLink.ModernApi.Services;

public class RateService : IRateService
{
    public Task<IEnumerable<RateQuoteResponse>> GetRatesAsync(RateRequest request)
    {
        // TODO: Implement with actual rate calculation logic
        return Task.FromResult(Enumerable.Empty<RateQuoteResponse>());
    }
}
