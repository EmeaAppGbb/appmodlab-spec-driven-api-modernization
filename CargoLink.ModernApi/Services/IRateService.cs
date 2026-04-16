using CargoLink.ModernApi.Models;

namespace CargoLink.ModernApi.Services;

public interface IRateService
{
    Task<IEnumerable<RateQuoteResponse>> GetRatesAsync(RateRequest request);
    Task<RateQuoteResponse?> GetRateForServiceAsync(RateRequest request, string serviceLevel);
}
