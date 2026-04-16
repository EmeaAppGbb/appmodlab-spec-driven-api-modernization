using Asp.Versioning;
using CargoLink.ModernApi.Models;
using CargoLink.ModernApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CargoLink.ModernApi.Controllers;

/// <summary>
/// Provides shipping rate quotes for various service levels.
/// </summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/rates")]
[Produces("application/json")]
public class RatesController : ControllerBase
{
    private readonly IRateService _rateService;

    public RatesController(IRateService rateService)
    {
        _rateService = rateService;
    }

    /// <summary>
    /// Gets rate quotes for all available service levels.
    /// </summary>
    /// <param name="request">The rate quote request with origin, destination, and weight.</param>
    /// <returns>A list of rate quotes across service levels.</returns>
    /// <response code="200">Returns the list of rate quotes.</response>
    /// <response code="400">Invalid request data.</response>
    [HttpPost]
    [ProducesResponseType(typeof(IEnumerable<RateQuoteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRates([FromBody] RateRequest request)
    {
        var quotes = await _rateService.GetRatesAsync(request);
        return Ok(quotes);
    }

    /// <summary>
    /// Gets a rate quote for a specific service level.
    /// </summary>
    /// <param name="request">The rate quote request with origin, destination, and weight.</param>
    /// <param name="serviceLevel">The target service level (e.g., Ground, Express, Overnight).</param>
    /// <returns>A rate quote for the specified service level.</returns>
    /// <response code="200">Returns the rate quote.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="404">Service level not found.</response>
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
