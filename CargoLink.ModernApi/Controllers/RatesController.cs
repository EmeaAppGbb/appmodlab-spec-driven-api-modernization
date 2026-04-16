using CargoLink.ModernApi.Models;
using CargoLink.ModernApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CargoLink.ModernApi.Controllers;

[ApiController]
[Route("api/v1/rates")]
[Produces("application/json")]
public class RatesController : ControllerBase
{
    private readonly IRateService _rateService;

    public RatesController(IRateService rateService)
    {
        _rateService = rateService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(IEnumerable<RateQuoteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRates([FromBody] RateRequest request)
    {
        var quotes = await _rateService.GetRatesAsync(request);
        return Ok(quotes);
    }

    [HttpPost("{serviceLevel}")]
    [ProducesResponseType(typeof(RateQuoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRateForService([FromBody] RateRequest request, string serviceLevel)
    {
        var quote = await _rateService.GetRateForServiceAsync(request, serviceLevel);
        if (quote is null)
            return NotFound(new ErrorResponse { Message = $"Service level '{serviceLevel}' not found", Code = "SERVICE_LEVEL_NOT_FOUND" });

        return Ok(quote);
    }
}
