namespace CargoLink.RestApi.Models
{
    public class ShipmentDto
    {
        public string Id { get; set; }
        public string TrackingId { get; set; }
        public string FromAddress { get; set; }
        public string FromZip { get; set; }
        public string ToAddress { get; set; }
        public string ToZip { get; set; }
        public decimal Weight { get; set; }
        public string Service { get; set; }
        public string CurrentStatus { get; set; }
        public decimal Cost { get; set; }
    }

    public class CreateShipmentDto
    {
        public string FromAddress { get; set; }
        public string FromCity { get; set; }
        public string FromZip { get; set; }
        public string ToAddress { get; set; }
        public string ToCity { get; set; }
        public string ToZip { get; set; }
        public decimal Weight { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public string Service { get; set; }
    }
}
