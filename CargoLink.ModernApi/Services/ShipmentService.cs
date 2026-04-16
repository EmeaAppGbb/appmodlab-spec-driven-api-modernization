using CargoLink.ModernApi.Models;

namespace CargoLink.ModernApi.Services;

public class ShipmentService : IShipmentService
{
    public Task<IEnumerable<ShipmentResponse>> GetAllAsync()
    {
        // TODO: Implement with actual data store
        return Task.FromResult(Enumerable.Empty<ShipmentResponse>());
    }

    public Task<ShipmentResponse?> GetByIdAsync(string id)
    {
        // TODO: Implement with actual data store
        return Task.FromResult<ShipmentResponse?>(null);
    }

    public Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request)
    {
        // TODO: Implement with actual business logic
        throw new NotImplementedException();
    }

    public Task<bool> CancelAsync(string id)
    {
        // TODO: Implement with actual data store
        return Task.FromResult(false);
    }
}
