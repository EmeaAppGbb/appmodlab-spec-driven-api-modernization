namespace CargoLink.RestApi.Models
{
    public class TrackingDto
    {
        public string TrackingNumber { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public string EstimatedDelivery { get; set; }
        public TrackingEventDto[] History { get; set; }
    }

    public class TrackingEventDto
    {
        public string Event { get; set; }
        public string Location { get; set; }
        public string Timestamp { get; set; }
        public string Notes { get; set; }
    }
}
