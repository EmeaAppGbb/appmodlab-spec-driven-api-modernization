using CargoLink.ModernApi.Models;
using CargoLink.ModernApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CargoLink.ModernApi.Controllers;

[ApiController]
[Route("api/v1/rates")]
public class RatesController : ControllerBase
{
    private readonly IRateService _rateService;

    public RatesController(IRateService rateService)
    {
        _rateService = rateService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(IEnumerable<RateQuoteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRates([FromBody] RateRequest request)
    {
        var quotes = await _rateService.GetRatesAsync(request);
        return Ok(quotes);
    }
}
