using System.ServiceModel;
using CargoLink.SoapServices.DataContracts;

namespace CargoLink.SoapServices
{
    [ServiceContract(Namespace = "http://cargolink.com/tracking")]
    public interface ITrackingService
    {
        [OperationContract]
        TrackingResponse GetTrackingInfo(TrackingRequest request);
    }

    public class TrackingService : ITrackingService
    {
        public TrackingResponse GetTrackingInfo(TrackingRequest request)
        {
            return new TrackingResponse
            {
                TrackingNumber = request.TrackingNumber,
                CurrentStatus = "In Transit",
                CurrentLocation = "Distribution Center - Chicago, IL",
                EstimatedDelivery = System.DateTime.Now.AddDays(2).ToString("yyyy-MM-dd"),
                Events = new[]
                {
                    new TrackingEvent
                    {
                        EventType = "Picked Up",
                        Location = "New York, NY",
                        Timestamp = System.DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                        Description = "Package picked up from sender"
                    },
                    new TrackingEvent
                    {
                        EventType = "In Transit",
                        Location = "Chicago, IL",
                        Timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Description = "Arrived at distribution center"
                    }
                }
            };
        }
    }
}
