using Asp.Versioning;
using CargoLink.ModernApi.Models;
using CargoLink.ModernApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CargoLink.ModernApi.Controllers;

/// <summary>
/// Manages shipment lifecycle operations including creation, retrieval, and cancellation.
/// </summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/shipments")]
[Produces("application/json")]
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentService _shipmentService;

    public ShipmentsController(IShipmentService shipmentService)
    {
        _shipmentService = shipmentService;
    }

    /// <summary>
    /// Retrieves all shipments.
    /// </summary>
    /// <returns>A list of all shipments.</returns>
    /// <response code="200">Returns the list of shipments.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ShipmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var shipments = await _shipmentService.GetAllAsync();
        return Ok(shipments);
    }

    /// <summary>
    /// Retrieves a shipment by its unique identifier.
    /// </summary>
    /// <param name="id">The shipment identifier.</param>
    /// <returns>The shipment details.</returns>
    /// <response code="200">Returns the requested shipment.</response>
    /// <response code="404">Shipment not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var shipment = await _shipmentService.GetByIdAsync(id);
        if (shipment is null)
            return NotFound(new ErrorResponse { Message = "Shipment not found", Code = "SHIPMENT_NOT_FOUND" });

        return Ok(shipment);
    }

    /// <summary>
    /// Creates a new shipment.
    /// </summary>
    /// <param name="request">The shipment creation request.</param>
    /// <returns>The newly created shipment.</returns>
    /// <response code="201">Shipment created successfully.</response>
    /// <response code="400">Invalid request data.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateShipmentRequest request)
    {
        var shipment = await _shipmentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = shipment.Id }, shipment);
    }

    /// <summary>
    /// Cancels an existing shipment.
    /// </summary>
    /// <param name="id">The shipment identifier to cancel.</param>
    /// <response code="204">Shipment cancelled successfully.</response>
    /// <response code="404">Shipment not found or already cancelled.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(string id)
    {
        var cancelled = await _shipmentService.CancelAsync(id);
        if (!cancelled)
            return NotFound(new ErrorResponse { Message = "Shipment not found or already cancelled", Code = "SHIPMENT_NOT_FOUND" });

        return NoContent();
    }
}
