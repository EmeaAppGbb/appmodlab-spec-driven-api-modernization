using System.Runtime.Serialization;

namespace CargoLink.SoapServices.DataContracts
{
    [DataContract(Namespace = "http://cargolink.com/rate")]
    public class RateRequest
    {
        [DataMember]
        public string OriginZipCode { get; set; }

        [DataMember]
        public string DestinationZipCode { get; set; }

        [DataMember]
        public decimal PackageWeight { get; set; }

        [DataMember]
        public string ServiceLevel { get; set; }
    }

    [DataContract(Namespace = "http://cargolink.com/rate")]
    public class RateQuote
    {
        [DataMember]
        public string ServiceLevel { get; set; }

        [DataMember]
        public decimal BaseRate { get; set; }

        [DataMember]
        public decimal FuelSurcharge { get; set; }

        [DataMember]
        public decimal TotalCost { get; set; }

        [DataMember]
        public int EstimatedDays { get; set; }
    }
}
