using CargoLink.ModernApi.Models;

namespace CargoLink.ModernApi.Services;

public class TrackingService : ITrackingService
{
    private readonly ShipmentService _shipmentService;

    public TrackingService(ShipmentService shipmentService)
    {
        _shipmentService = shipmentService;
    }

    public Task<TrackingResponse?> GetTrackingAsync(string trackingNumber)
    {
        var shipment = _shipmentService.FindByTrackingNumber(trackingNumber);
        if (shipment is null)
            return Task.FromResult<TrackingResponse?>(null);

        var events = new List<TrackingEvent>
        {
            new()
            {
                EventType = "Picked Up",
                Location = $"{shipment.OriginCity}, {shipment.OriginZipCode}",
                Timestamp = shipment.CreatedAt,
                Description = "Package picked up from sender"
            }
        };

        if (shipment.Status is "In Transit" or "Delivered")
        {
            events.Add(new TrackingEvent
            {
                EventType = "In Transit",
                Location = "Distribution Center",
                Timestamp = shipment.CreatedAt.AddHours(12),
                Description = "Arrived at distribution center"
            });
        }

        if (shipment.Status == "Delivered")
        {
            events.Add(new TrackingEvent
            {
                EventType = "Delivered",
                Location = $"{shipment.DestinationCity}, {shipment.DestinationZipCode}",
                Timestamp = shipment.CreatedAt.AddDays(1),
                Description = "Package delivered"
            });
        }

        if (shipment.Status == "Cancelled")
        {
            events.Add(new TrackingEvent
            {
                EventType = "Cancelled",
                Location = shipment.OriginCity,
                Timestamp = DateTime.UtcNow,
                Description = "Shipment cancelled"
            });
        }

        var response = new TrackingResponse
        {
            TrackingNumber = shipment.TrackingNumber,
            Status = shipment.Status,
            CurrentLocation = events.Last().Location,
            EstimatedDelivery = shipment.EstimatedDeliveryDate,
            Events = events
        };

        return Task.FromResult<TrackingResponse?>(response);
    }
}
