using System.Runtime.Serialization;

namespace CargoLink.SoapServices.DataContracts
{
    [DataContract(Namespace = "http://cargolink.com/tracking")]
    public class TrackingRequest
    {
        [DataMember]
        public string TrackingNumber { get; set; }
    }

    [DataContract(Namespace = "http://cargolink.com/tracking")]
    public class TrackingResponse
    {
        [DataMember]
        public string TrackingNumber { get; set; }

        [DataMember]
        public string CurrentStatus { get; set; }

        [DataMember]
        public string CurrentLocation { get; set; }

        [DataMember]
        public TrackingEvent[] Events { get; set; }

        [DataMember]
        public string EstimatedDelivery { get; set; }
    }

    [DataContract(Namespace = "http://cargolink.com/tracking")]
    public class TrackingEvent
    {
        [DataMember]
        public string EventType { get; set; }

        [DataMember]
        public string Location { get; set; }

        [DataMember]
        public string Timestamp { get; set; }

        [DataMember]
        public string Description { get; set; }
    }
}
