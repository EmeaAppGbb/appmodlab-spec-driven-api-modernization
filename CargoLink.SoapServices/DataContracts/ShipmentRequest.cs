using System.Runtime.Serialization;

namespace CargoLink.SoapServices.DataContracts
{
    [DataContract(Namespace = "http://cargolink.com/shipment")]
    public class ShipmentRequest
    {
        [DataMember]
        public string OriginAddress { get; set; }

        [DataMember]
        public string OriginCity { get; set; }

        [DataMember]
        public string OriginZipCode { get; set; }

        [DataMember]
        public string DestinationAddress { get; set; }

        [DataMember]
        public string DestinationCity { get; set; }

        [DataMember]
        public string DestinationZipCode { get; set; }

        [DataMember]
        public PackageDetails Package { get; set; }

        [DataMember]
        public string ServiceLevel { get; set; }
    }

    [DataContract(Namespace = "http://cargolink.com/shipment")]
    public class PackageDetails
    {
        [DataMember]
        public decimal Weight { get; set; }

        [DataMember]
        public decimal Length { get; set; }

        [DataMember]
        public decimal Width { get; set; }

        [DataMember]
        public decimal Height { get; set; }

        [DataMember]
        public string PackageType { get; set; }
    }

    [DataContract(Namespace = "http://cargolink.com/shipment")]
    public class ShipmentResponse
    {
        [DataMember]
        public string TrackingNumber { get; set; }

        [DataMember]
        public string ShipmentId { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public decimal TotalCost { get; set; }

        [DataMember]
        public string EstimatedDeliveryDate { get; set; }
    }
}
