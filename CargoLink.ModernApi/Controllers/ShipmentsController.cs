using CargoLink.ModernApi.Models;
using CargoLink.ModernApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CargoLink.ModernApi.Controllers;

[ApiController]
[Route("api/v1/shipments")]
[Produces("application/json")]
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentService _shipmentService;

    public ShipmentsController(IShipmentService shipmentService)
    {
        _shipmentService = shipmentService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ShipmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var shipments = await _shipmentService.GetAllAsync();
        return Ok(shipments);
    }

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

    [HttpPost]
    [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateShipmentRequest request)
    {
        var shipment = await _shipmentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = shipment.Id }, shipment);
    }

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
