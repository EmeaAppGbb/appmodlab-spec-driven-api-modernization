using System.Web.Http;
using CargoLink.RestApi.Models;

namespace CargoLink.RestApi.Controllers
{
    [RoutePrefix("api/tracking")]
    public class TrackingController : ApiController
    {
        [HttpGet]
        [Route("{trackingNumber}")]
        public IHttpActionResult GetTracking(string trackingNumber)
        {
            var tracking = new TrackingDto
            {
                TrackingNumber = trackingNumber,
                Status = "In Transit",
                Location = "Chicago, IL",
                EstimatedDelivery = System.DateTime.Now.AddDays(2).ToString("yyyy-MM-dd"),
                History = new[]
                {
                    new TrackingEventDto
                    {
                        Event = "Picked Up",
                        Location = "New York, NY",
                        Timestamp = System.DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                        Notes = "Package picked up"
                    },
                    new TrackingEventDto
                    {
                        Event = "In Transit",
                        Location = "Chicago, IL",
                        Timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Notes = "At distribution center"
                    }
                }
            };

            return Ok(tracking);
        }
    }
}
