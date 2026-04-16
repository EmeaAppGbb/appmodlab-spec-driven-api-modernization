using Asp.Versioning;
using CargoLink.ModernApi.Models;
using CargoLink.ModernApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CargoLink.ModernApi.Controllers;

/// <summary>
/// Provides shipment tracking information and event history.
/// </summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/tracking")]
[Produces("application/json")]
public class TrackingController : ControllerBase
{
    private readonly ITrackingService _trackingService;

    public TrackingController(ITrackingService trackingService)
    {
        _trackingService = trackingService;
    }

    /// <summary>
    /// Retrieves tracking information for a shipment by its tracking number.
    /// </summary>
    /// <param name="trackingNumber">The shipment tracking number.</param>
    /// <returns>Tracking details including status and event history.</returns>
    /// <response code="200">Returns the tracking information.</response>
    /// <response code="404">Tracking information not found.</response>
    [HttpGet("{trackingNumber}")]
    [ProducesResponseType(typeof(TrackingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByTrackingNumber(string trackingNumber)
    {
        var tracking = await _trackingService.GetTrackingAsync(trackingNumber);
        if (tracking is null)
            return NotFound(new ErrorResponse { Message = "Tracking information not found", Code = "TRACKING_NOT_FOUND" });

        return Ok(tracking);
    }
}
