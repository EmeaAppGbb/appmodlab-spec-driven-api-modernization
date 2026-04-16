using CargoLink.ModernApi.Models;

namespace CargoLink.ModernApi.Services;

public interface ITrackingService
{
    Task<TrackingResponse?> GetTrackingAsync(string trackingNumber);
}
