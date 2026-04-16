using CargoLink.ModernApi.Models;

namespace CargoLink.ModernApi.Services;

public interface IShipmentService
{
    Task<IEnumerable<ShipmentResponse>> GetAllAsync();
    Task<ShipmentResponse?> GetByIdAsync(string id);
    Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request);
    Task<bool> CancelAsync(string id);
}
