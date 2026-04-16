using CargoLink.ModernApi.Models;

namespace CargoLink.ModernApi.Services;

public class TrackingService : ITrackingService
{
    public Task<TrackingResponse?> GetTrackingAsync(string trackingNumber)
    {
        // TODO: Implement with actual data store
        return Task.FromResult<TrackingResponse?>(null);
    }
}
