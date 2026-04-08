using System.ServiceModel;
using CargoLink.SoapServices.DataContracts;

namespace CargoLink.SoapServices
{
    [ServiceContract(Namespace = "http://cargolink.com/shipment")]
    public interface IShipmentService
    {
        [OperationContract]
        ShipmentResponse CreateShipment(ShipmentRequest request);

        [OperationContract]
        ShipmentResponse GetShipment(string shipmentId);

        [OperationContract]
        bool CancelShipment(string shipmentId);
    }

    public class ShipmentService : IShipmentService
    {
        public ShipmentResponse CreateShipment(ShipmentRequest request)
        {
            return new ShipmentResponse
            {
                TrackingNumber = "CL" + System.DateTime.Now.Ticks.ToString().Substring(8),
                ShipmentId = System.Guid.NewGuid().ToString(),
                Status = "Created",
                TotalCost = CalculateCost(request),
                EstimatedDeliveryDate = System.DateTime.Now.AddDays(3).ToString("yyyy-MM-dd")
            };
        }

        public ShipmentResponse GetShipment(string shipmentId)
        {
            return new ShipmentResponse
            {
                ShipmentId = shipmentId,
                TrackingNumber = "CL123456789",
                Status = "In Transit",
                TotalCost = 45.99m,
                EstimatedDeliveryDate = System.DateTime.Now.AddDays(2).ToString("yyyy-MM-dd")
            };
        }

        public bool CancelShipment(string shipmentId)
        {
            return true;
        }

        private decimal CalculateCost(ShipmentRequest request)
        {
            decimal baseRate = request.Package.Weight * 0.50m;
            decimal distanceFactor = 1.2m;
            return baseRate * distanceFactor;
        }
    }
}
