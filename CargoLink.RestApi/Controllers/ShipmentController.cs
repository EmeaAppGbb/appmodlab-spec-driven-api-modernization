using System.Web.Http;
using CargoLink.RestApi.Models;

namespace CargoLink.RestApi.Controllers
{
    [RoutePrefix("api/shipments")]
    public class ShipmentController : ApiController
    {
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetShipments()
        {
            var shipments = new[]
            {
                new ShipmentDto
                {
                    Id = "ship_001",
                    TrackingId = "CL123456789",
                    FromZip = "10001",
                    ToZip = "90001",
                    Weight = 5.5m,
                    Service = "Ground",
                    CurrentStatus = "In Transit",
                    Cost = 25.99m
                }
            };

            return Ok(shipments);
        }

        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetShipment(string id)
        {
            var shipment = new ShipmentDto
            {
                Id = id,
                TrackingId = "CL123456789",
                FromAddress = "123 Main St",
                FromZip = "10001",
                ToAddress = "456 Oak Ave",
                ToZip = "90001",
                Weight = 5.5m,
                Service = "Ground",
                CurrentStatus = "In Transit",
                Cost = 25.99m
            };

            return Ok(shipment);
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult CreateShipment([FromBody] CreateShipmentDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid shipment data");

            var shipment = new ShipmentDto
            {
                Id = "ship_" + System.Guid.NewGuid().ToString().Substring(0, 8),
                TrackingId = "CL" + System.DateTime.Now.Ticks.ToString().Substring(8),
                FromAddress = dto.FromAddress,
                FromZip = dto.FromZip,
                ToAddress = dto.ToAddress,
                ToZip = dto.ToZip,
                Weight = dto.Weight,
                Service = dto.Service,
                CurrentStatus = "Created",
                Cost = dto.Weight * 0.50m * 1.2m
            };

            return Created($"api/shipments/{shipment.Id}", shipment);
        }
    }
}
