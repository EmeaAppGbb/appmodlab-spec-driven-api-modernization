using System.Collections.Concurrent;
using CargoLink.ModernApi.Models;

namespace CargoLink.ModernApi.Services;

public class ShipmentService : IShipmentService
{
    private static readonly ConcurrentDictionary<string, ShipmentResponse> _shipments = new();
    private readonly IRateService _rateService;

    public ShipmentService(IRateService rateService)
    {
        _rateService = rateService;
    }

    public Task<IEnumerable<ShipmentResponse>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<ShipmentResponse>>(_shipments.Values.ToList());
    }

    public Task<ShipmentResponse?> GetByIdAsync(string id)
    {
        _shipments.TryGetValue(id, out var shipment);
        return Task.FromResult(shipment);
    }

    public async Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request)
    {
        var rateRequest = new RateRequest
        {
            OriginZipCode = request.OriginZipCode,
            DestinationZipCode = request.DestinationZipCode,
            Weight = request.Weight,
            ServiceLevel = request.ServiceLevel
        };

        var rates = await _rateService.GetRatesAsync(rateRequest);
        var matchedRate = rates.FirstOrDefault(r =>
            string.Equals(r.ServiceLevel, request.ServiceLevel, StringComparison.OrdinalIgnoreCase))
            ?? rates.First();

        var shipmentId = Guid.NewGuid().ToString();
        var trackingNumber = "CL" + DateTime.UtcNow.Ticks.ToString().Substring(8);

        var shipment = new ShipmentResponse
        {
            Id = shipmentId,
            TrackingNumber = trackingNumber,
            Status = "Created",
            OriginAddress = request.OriginAddress,
            OriginCity = request.OriginCity,
            OriginZipCode = request.OriginZipCode,
            DestinationAddress = request.DestinationAddress,
            DestinationCity = request.DestinationCity,
            DestinationZipCode = request.DestinationZipCode,
            Weight = request.Weight,
            ServiceLevel = request.ServiceLevel,
            TotalCost = matchedRate.TotalCost,
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(matchedRate.EstimatedDays).ToString("yyyy-MM-dd"),
            CreatedAt = DateTime.UtcNow
        };

        _shipments[shipmentId] = shipment;
        return shipment;
    }

    public Task<bool> CancelAsync(string id)
    {
        if (!_shipments.TryGetValue(id, out var shipment))
            return Task.FromResult(false);

        if (shipment.Status == "Cancelled")
            return Task.FromResult(false);

        shipment.Status = "Cancelled";
        return Task.FromResult(true);
    }

    /// <summary>
    /// Internal lookup used by TrackingService to find a shipment by tracking number.
    /// </summary>
    public ShipmentResponse? FindByTrackingNumber(string trackingNumber)
    {
        return _shipments.Values.FirstOrDefault(s =>
            string.Equals(s.TrackingNumber, trackingNumber, StringComparison.OrdinalIgnoreCase));
    }
}
