using System.Web.Http;
using CargoLink.RestApi.Models;

namespace CargoLink.RestApi.Controllers
{
    [RoutePrefix("api/quotes")]
    public class QuoteController : ApiController
    {
        [HttpPost]
        [Route("")]
        public IHttpActionResult GetQuotes([FromBody] RateRequestDto request)
        {
            if (request == null)
                return BadRequest("Invalid request");

            var quotes = new[]
            {
                new RateQuoteDto
                {
                    Service = "Ground",
                    Price = request.Weight * 0.50m + 5.00m,
                    Days = 5
                },
                new RateQuoteDto
                {
                    Service = "Express",
                    Price = request.Weight * 1.20m + 8.00m,
                    Days = 2
                },
                new RateQuoteDto
                {
                    Service = "Overnight",
                    Price = request.Weight * 2.50m + 12.00m,
                    Days = 1
                }
            };

            return Ok(quotes);
        }
    }

    public class RateRequestDto
    {
        public string FromZip { get; set; }
        public string ToZip { get; set; }
        public decimal Weight { get; set; }
    }

    public class RateQuoteDto
    {
        public string Service { get; set; }
        public decimal Price { get; set; }
        public int Days { get; set; }
    }
}
