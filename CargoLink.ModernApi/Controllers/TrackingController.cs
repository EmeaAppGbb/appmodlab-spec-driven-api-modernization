using CargoLink.ModernApi.Models;
using CargoLink.ModernApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CargoLink.ModernApi.Controllers;

[ApiController]
[Route("api/v1/tracking")]
[Produces("application/json")]
public class TrackingController : ControllerBase
{
    private readonly ITrackingService _trackingService;

    public TrackingController(ITrackingService trackingService)
    {
        _trackingService = trackingService;
    }

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
