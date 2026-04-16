# Step 04 — Modern API Design: CargoLink Express on ASP.NET Core 8.0

> Architecture design for the modernized CargoLink Express shipping API, targeting ASP.NET Core 8.0 Web API. This design is derived from the unified OpenAPI specification (`openapi-unified-v2.json`) and the legacy pattern analysis (`step-03-analyze-patterns.md`).

---

## 1. Project Structure

The modernized solution replaces all three legacy projects (`CargoLink.Core`, `CargoLink.SoapServices`, `CargoLink.RestApi`) with a single ASP.NET Core 8.0 Web API project.

```
CargoLink.Api/
├── CargoLink.Api.csproj              # .NET 8.0 Web API project
├── Program.cs                        # Minimal hosting, DI, middleware pipeline
├── appsettings.json                  # Configuration (connection strings, API keys, rate tables)
├── appsettings.Development.json      # Dev-specific overrides
│
├── Controllers/                      # API controllers (thin — delegate to services)
│   ├── ShipmentsController.cs        # /api/v1/shipments endpoints
│   ├── TrackingController.cs         # /api/v1/tracking endpoints
│   └── RatesController.cs            # /api/v1/rates endpoints
│
├── Models/                           # DTOs (request/response) and enums
│   ├── Requests/
│   │   ├── CreateShipmentRequest.cs  # Shipment creation DTO
│   │   └── RateRequest.cs            # Rate quote request DTO
│   ├── Responses/
│   │   ├── ShipmentResponse.cs       # Full shipment resource
│   │   ├── CancelShipmentResponse.cs # Cancellation confirmation
│   │   ├── TrackingInfoResponse.cs   # Tracking info with event history
│   │   ├── TrackingEventResponse.cs  # Single tracking event
│   │   ├── RateQuoteResponse.cs      # Rate quote with cost breakdown
│   │   └── ErrorResponse.cs          # Structured error (RFC 7807-aligned)
│   ├── Address.cs                    # Shared address model
│   ├── PackageDetails.cs             # Package dimensions and weight
│   └── ServiceLevel.cs              # Enum: Ground, Express, Overnight
│
├── Services/                         # Business logic layer
│   ├── Interfaces/
│   │   ├── IShipmentService.cs       # Shipment operations contract
│   │   ├── ITrackingService.cs       # Tracking operations contract
│   │   └── IRateService.cs           # Rate calculation contract
│   ├── ShipmentService.cs            # Shipment business logic
│   ├── TrackingService.cs            # Tracking business logic
│   └── RateService.cs                # Centralized rate calculation
│
├── Middleware/                        # Cross-cutting concerns
│   ├── GlobalExceptionMiddleware.cs  # Catches unhandled exceptions → ErrorResponse
│   ├── RequestValidationMiddleware.cs # Validates Content-Type, body presence
│   └── ApiKeyAuthMiddleware.cs       # API key authentication
│
└── Configuration/                    # Options and DI setup
    ├── RateOptions.cs                # Strongly-typed rate configuration
    └── ServiceCollectionExtensions.cs # DI registration extension methods
```

### Design Rationale

| Decision | Reason |
|---|---|
| Single project (not Clean Architecture layers) | The domain is simple (three resources, no complex aggregates); splitting into 4+ projects adds overhead without benefit at this scale |
| `Models/Requests/` and `Models/Responses/` separation | Prevents accidental reuse of request DTOs as responses and vice versa; clear input/output boundary |
| `Services/Interfaces/` sub-folder | Keeps contracts co-located with implementations while remaining easy to extract into a shared assembly if needed later |
| Thin controllers | Controllers handle HTTP concerns only (model binding, status codes, route mapping); all business logic lives in services |
| Middleware for cross-cutting | Error handling, auth, and request validation are applied uniformly via the pipeline rather than repeated in each controller |

---

## 2. RESTful Route Design

All routes are prefixed with `/api/v1/` for URL-path versioning (see §7).

### 2.1 Route Table

| HTTP Method | Route | Controller Method | Description | Source |
|---|---|---|---|---|
| `GET` | `/api/v1/shipments` | `ListShipments` | List all shipments | REST-only (legacy) |
| `POST` | `/api/v1/shipments` | `CreateShipment` | Create a new shipment | SOAP + REST |
| `GET` | `/api/v1/shipments/{id}` | `GetShipment` | Get shipment by ID | SOAP + REST |
| `POST` | `/api/v1/shipments/{id}/cancel` | `CancelShipment` | Cancel a shipment | SOAP-only (legacy) |
| `GET` | `/api/v1/tracking/{trackingNumber}` | `GetTrackingInfo` | Get tracking information | SOAP + REST |
| `POST` | `/api/v1/rates` | `GetRates` | Get all rate quotes | SOAP + REST |
| `POST` | `/api/v1/rates/{serviceLevel}` | `GetRateForService` | Get rate for specific service | SOAP-only (legacy) |

### 2.2 Route Design Decisions

| Decision | Rationale |
|---|---|
| `POST /shipments/{id}/cancel` instead of `DELETE /shipments/{id}` | Cancel is a business action that changes status (not a resource deletion). Using a sub-resource action makes the intent explicit and allows the shipment to remain queryable after cancellation. The OpenAPI spec uses DELETE, but a POST action endpoint better models the domain semantics. |
| `POST /rates` (not `GET` with query params) | Rate calculation is a computation over a request body with multiple required fields. While idempotent, using POST avoids URL length limits and keeps the request body clean. |
| `POST /rates/{serviceLevel}` | Combines path-based service level selection with body-based package details, matching the SOAP `GetRateForService(RateRequest, serviceLevel)` pattern. |
| Plural resource names (`/shipments`, `/rates`) | Standard REST convention for collections. |
| `{trackingNumber}` not `{id}` for tracking | Tracking numbers are the natural external identifier for tracking lookups (prefixed with `CL`), distinct from internal shipment IDs. |

### 2.3 Response Status Codes

| Endpoint | Success | Client Error | Not Found | Server Error |
|---|---|---|---|---|
| `GET /shipments` | `200 OK` | — | — | `500` |
| `POST /shipments` | `201 Created` + `Location` header | `400 Bad Request` | — | `500` |
| `GET /shipments/{id}` | `200 OK` | — | `404 Not Found` | `500` |
| `POST /shipments/{id}/cancel` | `200 OK` | — | `404 Not Found` | `500` |
| `GET /tracking/{trackingNumber}` | `200 OK` | — | `404 Not Found` | `500` |
| `POST /rates` | `200 OK` | `400 Bad Request` | — | `500` |
| `POST /rates/{serviceLevel}` | `200 OK` | `400 Bad Request` | `404 Not Found` | `500` |

---

## 3. DTO Design

DTOs unify the legacy SOAP DataContracts and REST models, resolving naming inconsistencies and restoring missing fields identified in the pattern analysis (§2 and §5 of step-03).

### 3.1 Shared Models

```csharp
// Models/ServiceLevel.cs
using System.Text.Json.Serialization;

namespace CargoLink.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ServiceLevel
{
    Ground,
    Express,
    Overnight
}
```

```csharp
// Models/Address.cs
using System.ComponentModel.DataAnnotations;

namespace CargoLink.Api.Models;

/// <summary>
/// Structured address. Unifies SOAP Origin*/Destination* fields
/// and REST From*/To* fields into a single reusable model.
/// </summary>
public class Address
{
    [Required]
    [StringLength(200)]
    public required string Street { get; init; }

    [Required]
    [StringLength(100)]
    public required string City { get; init; }

    [Required]
    [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Invalid ZIP code format")]
    public required string ZipCode { get; init; }
}
```

```csharp
// Models/PackageDetails.cs
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CargoLink.Api.Models;

/// <summary>
/// Package dimensions and type. Restores PackageType from SOAP
/// DataContract that was missing in the legacy REST API.
/// </summary>
public class PackageDetails
{
    [Required]
    [Range(0.1, 150.0, ErrorMessage = "Weight must be between 0.1 and 150 lbs")]
    public double Weight { get; init; }

    [Range(0, 108)]
    public double? Length { get; init; }

    [Range(0, 108)]
    public double? Width { get; init; }

    [Range(0, 108)]
    public double? Height { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PackageType? PackageType { get; init; }
}

public enum PackageType
{
    Box,
    Envelope,
    Tube,
    Pallet
}
```

### 3.2 Request DTOs

```csharp
// Models/Requests/CreateShipmentRequest.cs
using System.ComponentModel.DataAnnotations;

namespace CargoLink.Api.Models.Requests;

/// <summary>
/// Shipment creation request. Unifies:
/// - SOAP: ShipmentRequest (nested PackageDetails, Origin*/Destination* fields)
/// - REST: CreateShipmentDto (flat Weight/Length/Width/Height, From*/To* fields)
///
/// Resolution: Uses nested Address and PackageDetails objects (SOAP-style nesting)
/// with consistent naming (unified naming convention).
/// </summary>
public class CreateShipmentRequest
{
    [Required]
    public required Address Origin { get; init; }

    [Required]
    public required Address Destination { get; init; }

    [Required]
    public required PackageDetails Package { get; init; }

    [Required]
    public required ServiceLevel ServiceLevel { get; init; }
}
```

```csharp
// Models/Requests/RateRequest.cs
using System.ComponentModel.DataAnnotations;

namespace CargoLink.Api.Models.Requests;

/// <summary>
/// Rate quote request. Unifies:
/// - SOAP: RateRequest (PackageWeight, OriginZipCode, DestinationZipCode)
/// - REST: RateRequestDto (Weight, FromZip, ToZip)
///
/// Resolution: Uses consistent naming (originZipCode, destinationZipCode, packageWeight)
/// matching the unified OpenAPI spec.
/// </summary>
public class RateRequest
{
    [Required]
    [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Invalid ZIP code format")]
    public required string OriginZipCode { get; init; }

    [Required]
    [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Invalid ZIP code format")]
    public required string DestinationZipCode { get; init; }

    [Required]
    [Range(0.1, 150.0, ErrorMessage = "Package weight must be between 0.1 and 150 lbs")]
    public double PackageWeight { get; init; }
}
```

### 3.3 Response DTOs

```csharp
// Models/Responses/ShipmentResponse.cs
namespace CargoLink.Api.Models.Responses;

/// <summary>
/// Full shipment resource. Unifies:
/// - SOAP: ShipmentResponse (ShipmentId, TrackingNumber, TotalCost, EstimatedDeliveryDate)
/// - REST: ShipmentDto (Id, TrackingId, Cost — missing EstimatedDeliveryDate)
///
/// Resolution: Includes all fields from both sources. Restores EstimatedDeliveryDate
/// that was missing from the legacy REST API. Uses consistent naming from the OpenAPI spec.
/// </summary>
public class ShipmentResponse
{
    public required string ShipmentId { get; init; }
    public required string TrackingNumber { get; init; }
    public required Address Origin { get; init; }
    public required Address Destination { get; init; }
    public required PackageDetails Package { get; init; }
    public required ServiceLevel ServiceLevel { get; init; }
    public required string Status { get; init; }
    public double TotalCost { get; init; }
    public DateOnly? EstimatedDeliveryDate { get; init; }
}
```

```csharp
// Models/Responses/CancelShipmentResponse.cs
namespace CargoLink.Api.Models.Responses;

public class CancelShipmentResponse
{
    public required string ShipmentId { get; init; }
    public bool Cancelled { get; init; }
    public required string Message { get; init; }
}
```

```csharp
// Models/Responses/TrackingInfoResponse.cs
namespace CargoLink.Api.Models.Responses;

/// <summary>
/// Tracking information. Unifies:
/// - SOAP: TrackingResponse (CurrentStatus, CurrentLocation, Events[])
/// - REST: TrackingDto (Status, Location, History[])
///
/// Resolution: Uses SOAP's more descriptive naming (CurrentStatus, CurrentLocation, Events)
/// and adds EstimatedDelivery from the unified spec.
/// </summary>
public class TrackingInfoResponse
{
    public required string TrackingNumber { get; init; }
    public required string CurrentStatus { get; init; }
    public required string CurrentLocation { get; init; }
    public DateOnly? EstimatedDelivery { get; init; }
    public required List<TrackingEventResponse> Events { get; init; }
}
```

```csharp
// Models/Responses/TrackingEventResponse.cs
namespace CargoLink.Api.Models.Responses;

/// <summary>
/// Single tracking event. Unifies:
/// - SOAP: TrackingEvent (EventType, Location, Timestamp, Description)
/// - REST: TrackingEventDto (Event, Location, Timestamp, Notes)
///
/// Resolution: Uses SOAP's more explicit naming (EventType, Description)
/// which better communicates intent.
/// </summary>
public class TrackingEventResponse
{
    public required string EventType { get; init; }
    public required string Location { get; init; }
    public DateTime Timestamp { get; init; }
    public string? Description { get; init; }
}
```

```csharp
// Models/Responses/RateQuoteResponse.cs
namespace CargoLink.Api.Models.Responses;

/// <summary>
/// Rate quote with cost breakdown. Unifies:
/// - SOAP: RateQuote (ServiceLevel, BaseRate, FuelSurcharge, TotalCost, EstimatedDays)
/// - REST: RateQuoteDto (Service, Price, Days — missing BaseRate, FuelSurcharge)
///
/// Resolution: Includes full cost breakdown from SOAP that was hidden in the legacy REST API.
/// Uses consistent naming (ServiceLevel, TotalCost, EstimatedDays).
/// </summary>
public class RateQuoteResponse
{
    public required ServiceLevel ServiceLevel { get; init; }
    public double BaseRate { get; init; }
    public double FuelSurcharge { get; init; }
    public double TotalCost { get; init; }
    public int EstimatedDays { get; init; }
}
```

```csharp
// Models/Responses/ErrorResponse.cs
using System.Text.Json.Serialization;

namespace CargoLink.Api.Models.Responses;

/// <summary>
/// Structured error response aligned with RFC 7807 Problem Details.
/// Replaces both the untyped SOAP FaultExceptions and the unstructured
/// REST error responses from the legacy APIs.
/// </summary>
public class ErrorResponse
{
    public required string Code { get; init; }
    public required string Message { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<FieldError>? Details { get; init; }
}

public class FieldError
{
    public required string Field { get; init; }
    public required string Message { get; init; }
}
```

### 3.4 Naming Resolution Summary

The table below shows how each legacy naming conflict was resolved in the modern DTOs.

| Concept | SOAP Name | REST Name | Modern Name | Rationale |
|---|---|---|---|---|
| Tracking ID | `TrackingNumber` | `TrackingId` | `TrackingNumber` | More descriptive; industry standard |
| Total price | `TotalCost` | `Cost` / `Price` | `TotalCost` | Explicit and unambiguous |
| Service tier | `ServiceLevel` | `Service` | `ServiceLevel` | More descriptive |
| Delivery estimate | `EstimatedDeliveryDate` | *(missing)* | `EstimatedDeliveryDate` | Restored from SOAP |
| Delivery days | `EstimatedDays` | `Days` | `EstimatedDays` | More descriptive |
| Events collection | `Events` | `History` | `Events` | Standard tracking terminology |
| Event type | `EventType` | `Event` | `EventType` | Avoids ambiguity with the event object itself |
| Event description | `Description` | `Notes` | `Description` | Standard and intuitive |
| Status | `CurrentStatus` | `Status` | `CurrentStatus` (tracking) / `Status` (shipment) | `Current` prefix for tracking to distinguish from historical event statuses |
| Location | `CurrentLocation` | `Location` | `CurrentLocation` | Same rationale as status |
| Origin zip | `OriginZipCode` | `FromZip` | `Address.ZipCode` on `Origin` object | Nested under Address makes prefix unnecessary |
| Resource ID | `ShipmentId` | `Id` | `ShipmentId` | Explicit identification in API responses |

---

## 4. Service Layer Interfaces

Services encapsulate all business logic, resolving the duplication identified in the pattern analysis where cost calculation was implemented three different ways across five files.

### 4.1 IShipmentService

```csharp
// Services/Interfaces/IShipmentService.cs
using CargoLink.Api.Models;
using CargoLink.Api.Models.Requests;
using CargoLink.Api.Models.Responses;

namespace CargoLink.Api.Services.Interfaces;

public interface IShipmentService
{
    /// <summary>
    /// Lists all shipments.
    /// Origin: REST-only (ShipmentController.GetShipments).
    /// </summary>
    Task<IReadOnlyList<ShipmentResponse>> ListShipmentsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new shipment with cost calculation and tracking number generation.
    /// Origin: SOAP IShipmentService.CreateShipment + REST ShipmentController.CreateShipment.
    /// Uses centralized cost calculation from IRateService to eliminate the pricing
    /// discrepancy between legacy SOAP ($6.00), REST ($6.00), and Core ($11.00).
    /// </summary>
    Task<ShipmentResponse> CreateShipmentAsync(
        CreateShipmentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a shipment by its unique identifier.
    /// Origin: SOAP IShipmentService.GetShipment + REST ShipmentController.GetShipment.
    /// Returns null when shipment is not found (controller maps to 404).
    /// </summary>
    Task<ShipmentResponse?> GetShipmentAsync(
        string shipmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a shipment, updating its status to Cancelled.
    /// Origin: SOAP-only IShipmentService.CancelShipment (was missing from REST).
    /// Returns null when shipment is not found (controller maps to 404).
    /// </summary>
    Task<CancelShipmentResponse?> CancelShipmentAsync(
        string shipmentId,
        CancellationToken cancellationToken = default);
}
```

### 4.2 ITrackingService

```csharp
// Services/Interfaces/ITrackingService.cs
using CargoLink.Api.Models.Responses;

namespace CargoLink.Api.Services.Interfaces;

public interface ITrackingService
{
    /// <summary>
    /// Retrieves tracking information by tracking number.
    /// Origin: SOAP ITrackingService.GetTrackingInfo + REST TrackingController.GetTracking.
    /// Returns null when tracking number is not found (controller maps to 404).
    /// </summary>
    Task<TrackingInfoResponse?> GetTrackingInfoAsync(
        string trackingNumber,
        CancellationToken cancellationToken = default);
}
```

### 4.3 IRateService

```csharp
// Services/Interfaces/IRateService.cs
using CargoLink.Api.Models;
using CargoLink.Api.Models.Requests;
using CargoLink.Api.Models.Responses;

namespace CargoLink.Api.Services.Interfaces;

public interface IRateService
{
    /// <summary>
    /// Calculates rate quotes for all available service levels.
    /// Origin: SOAP IRateService.GetRates + REST QuoteController.GetQuotes.
    ///
    /// IMPORTANT: This is the single source of truth for cost calculation,
    /// eliminating the three-way pricing discrepancy from the legacy system:
    ///   - Core: (weight × rateMultiplier × distanceFactor) + fuelSurcharge
    ///   - SOAP/REST shipment: weight × 0.50 × 1.2  (missing fuel surcharge)
    ///   - SOAP/REST rates: (weight × rateMultiplier) + fuelSurcharge (missing distance)
    ///
    /// Correct formula: (weight × rateMultiplier × distanceFactor) + fuelSurcharge
    /// </summary>
    Task<IReadOnlyList<RateQuoteResponse>> GetRatesAsync(
        RateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates rate quote for a specific service level.
    /// Origin: SOAP-only IRateService.GetRateForService (was missing from REST).
    /// Returns null when serviceLevel is invalid (controller maps to 404).
    /// </summary>
    Task<RateQuoteResponse?> GetRateForServiceAsync(
        RateRequest request,
        ServiceLevel serviceLevel,
        CancellationToken cancellationToken = default);
}
```

### 4.4 Service Design Principles

| Principle | Implementation |
|---|---|
| **Single source of truth** | All cost calculation goes through `IRateService`. `IShipmentService.CreateShipmentAsync` calls `IRateService` internally for pricing. |
| **Null = not found** | Services return `null` for missing resources; controllers translate to `404`. No exceptions for expected business cases. |
| **Async by default** | All methods return `Task<T>` with `CancellationToken` support for database/HTTP call readiness. |
| **Immutable responses** | Response DTOs use `init` setters — services create them once, controllers never modify them. |
| **No HTTP concerns** | Services never throw `HttpException` or return status codes. They return domain results; controllers map to HTTP. |

---

## 5. Controller Design

Controllers are thin HTTP adapters that map routes to service methods, handle model binding, and return appropriate status codes.

### 5.1 ShipmentsController

```csharp
// Controllers/ShipmentsController.cs
using CargoLink.Api.Models.Requests;
using CargoLink.Api.Models.Responses;
using CargoLink.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CargoLink.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentService _shipmentService;

    public ShipmentsController(IShipmentService shipmentService)
    {
        _shipmentService = shipmentService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ShipmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListShipments(CancellationToken cancellationToken)
    {
        var shipments = await _shipmentService.ListShipmentsAsync(cancellationToken);
        return Ok(shipments);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateShipment(
        [FromBody] CreateShipmentRequest request,
        CancellationToken cancellationToken)
    {
        var shipment = await _shipmentService.CreateShipmentAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetShipment),
            new { id = shipment.ShipmentId },
            shipment);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShipment(string id, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentService.GetShipmentAsync(id, cancellationToken);
        return shipment is not null ? Ok(shipment) : NotFound(new ErrorResponse
        {
            Code = "NOT_FOUND",
            Message = $"Shipment '{id}' was not found."
        });
    }

    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(CancelShipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelShipment(string id, CancellationToken cancellationToken)
    {
        var result = await _shipmentService.CancelShipmentAsync(id, cancellationToken);
        return result is not null ? Ok(result) : NotFound(new ErrorResponse
        {
            Code = "NOT_FOUND",
            Message = $"Shipment '{id}' was not found."
        });
    }
}
```

### 5.2 TrackingController

```csharp
// Controllers/TrackingController.cs
using CargoLink.Api.Models.Responses;
using CargoLink.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CargoLink.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class TrackingController : ControllerBase
{
    private readonly ITrackingService _trackingService;

    public TrackingController(ITrackingService trackingService)
    {
        _trackingService = trackingService;
    }

    [HttpGet("{trackingNumber}")]
    [ProducesResponseType(typeof(TrackingInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrackingInfo(
        string trackingNumber,
        CancellationToken cancellationToken)
    {
        var tracking = await _trackingService.GetTrackingInfoAsync(
            trackingNumber, cancellationToken);
        return tracking is not null ? Ok(tracking) : NotFound(new ErrorResponse
        {
            Code = "NOT_FOUND",
            Message = $"Tracking number '{trackingNumber}' was not found."
        });
    }
}
```

### 5.3 RatesController

```csharp
// Controllers/RatesController.cs
using CargoLink.Api.Models;
using CargoLink.Api.Models.Requests;
using CargoLink.Api.Models.Responses;
using CargoLink.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CargoLink.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class RatesController : ControllerBase
{
    private readonly IRateService _rateService;

    public RatesController(IRateService rateService)
    {
        _rateService = rateService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(IReadOnlyList<RateQuoteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRates(
        [FromBody] RateRequest request,
        CancellationToken cancellationToken)
    {
        var rates = await _rateService.GetRatesAsync(request, cancellationToken);
        return Ok(rates);
    }

    [HttpPost("{serviceLevel}")]
    [ProducesResponseType(typeof(RateQuoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRateForService(
        ServiceLevel serviceLevel,
        [FromBody] RateRequest request,
        CancellationToken cancellationToken)
    {
        var rate = await _rateService.GetRateForServiceAsync(
            request, serviceLevel, cancellationToken);
        return rate is not null ? Ok(rate) : NotFound(new ErrorResponse
        {
            Code = "NOT_FOUND",
            Message = $"Service level '{serviceLevel}' is not available."
        });
    }
}
```

---

## 6. Middleware Design

### 6.1 Global Exception Handling Middleware

Replaces the missing exception handling from both legacy APIs (no `[FaultContract]` in SOAP, no `ExceptionFilterAttribute` in REST).

```csharp
// Middleware/GlobalExceptionMiddleware.cs
using System.Net;
using System.Text.Json;
using CargoLink.Api.Models.Responses;

namespace CargoLink.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var error = new ErrorResponse
            {
                Code = "INTERNAL_ERROR",
                Message = "An unexpected error occurred. Please try again later."
            };

            var json = JsonSerializer.Serialize(error, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
```

### 6.2 API Key Authentication Middleware

Simple API key authentication for initial deployment. Can be replaced with OAuth 2.0 / JWT later.

```csharp
// Middleware/ApiKeyAuthMiddleware.cs
using System.Net;
using System.Text.Json;
using CargoLink.Api.Models.Responses;

namespace CargoLink.Api.Middleware;

public class ApiKeyAuthMiddleware
{
    private const string ApiKeyHeaderName = "X-Api-Key";
    private readonly RequestDelegate _next;

    public ApiKeyAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        // Skip auth for health check and OpenAPI endpoints
        var path = context.Request.Path.Value?.ToLowerInvariant();
        if (path is "/health" or "/swagger" || path?.StartsWith("/swagger/") == true)
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedKey))
        {
            await WriteErrorAsync(context, HttpStatusCode.Unauthorized,
                "MISSING_API_KEY", "The X-Api-Key header is required.");
            return;
        }

        var configuredKey = configuration["ApiKey"];
        if (!string.Equals(providedKey, configuredKey, StringComparison.Ordinal))
        {
            await WriteErrorAsync(context, HttpStatusCode.Unauthorized,
                "INVALID_API_KEY", "The provided API key is invalid.");
            return;
        }

        await _next(context);
    }

    private static async Task WriteErrorAsync(
        HttpContext context, HttpStatusCode statusCode, string code, string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var error = new ErrorResponse { Code = code, Message = message };
        var json = JsonSerializer.Serialize(error, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
```

### 6.3 Request Validation

ASP.NET Core's built-in model validation with `[ApiController]` handles request validation automatically. The `[ApiController]` attribute:

1. Automatically returns `400 Bad Request` with `ValidationProblemDetails` when `ModelState` is invalid
2. Infers `[FromBody]` for complex types
3. Requires non-nullable route parameters

We customize the validation response to match our `ErrorResponse` format:

```csharp
// In Program.cs — configure validation error responses
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .Select(e => new FieldError
                {
                    Field = e.Key,
                    Message = e.Value!.Errors.First().ErrorMessage
                })
                .ToList();

            var response = new ErrorResponse
            {
                Code = "VALIDATION_ERROR",
                Message = "One or more validation errors occurred.",
                Details = errors
            };

            return new BadRequestObjectResult(response);
        };
    });
```

### 6.4 Middleware Pipeline Order

```
Request
  │
  ├─ 1. GlobalExceptionMiddleware     ← catches all unhandled exceptions
  ├─ 2. ApiKeyAuthMiddleware           ← authenticates via X-Api-Key header
  ├─ 3. Routing                        ← built-in ASP.NET Core routing
  ├─ 4. [ApiController] validation     ← automatic model state validation
  └─ 5. Controller action              ← thin HTTP adapter → service call
```

---

## 7. Dependency Injection Configuration

### 7.1 Service Registration

```csharp
// Configuration/ServiceCollectionExtensions.cs
using CargoLink.Api.Services;
using CargoLink.Api.Services.Interfaces;

namespace CargoLink.Api.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCargoLinkServices(this IServiceCollection services)
    {
        services.AddScoped<IShipmentService, ShipmentService>();
        services.AddScoped<ITrackingService, TrackingService>();
        services.AddScoped<IRateService, RateService>();

        return services;
    }
}
```

### 7.2 Rate Configuration Options

```csharp
// Configuration/RateOptions.cs
namespace CargoLink.Api.Configuration;

/// <summary>
/// Strongly-typed rate configuration. Externalizes the hardcoded rate
/// multipliers and fuel surcharges from the legacy code.
/// </summary>
public class RateOptions
{
    public const string SectionName = "Rates";

    public double DistanceFactor { get; set; } = 1.2;

    public Dictionary<string, ServiceRateConfig> ServiceLevels { get; set; } = new()
    {
        ["Ground"] = new() { RateMultiplier = 0.50, FuelSurcharge = 5.00, EstimatedDays = 5 },
        ["Express"] = new() { RateMultiplier = 0.75, FuelSurcharge = 8.00, EstimatedDays = 3 },
        ["Overnight"] = new() { RateMultiplier = 1.00, FuelSurcharge = 12.00, EstimatedDays = 1 }
    };
}

public class ServiceRateConfig
{
    public double RateMultiplier { get; set; }
    public double FuelSurcharge { get; set; }
    public int EstimatedDays { get; set; }
}
```

### 7.3 Program.cs — Full Pipeline

```csharp
// Program.cs
using System.Text.Json;
using System.Text.Json.Serialization;
using CargoLink.Api.Configuration;
using CargoLink.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// --- Service Registration ---

// Options
builder.Services.Configure<RateOptions>(
    builder.Configuration.GetSection(RateOptions.SectionName));

// Application services
builder.Services.AddCargoLinkServices();

// Controllers + JSON serialization
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .Select(e => new CargoLink.Api.Models.Responses.FieldError
                {
                    Field = e.Key,
                    Message = e.Value!.Errors.First().ErrorMessage
                })
                .ToList();

            var response = new CargoLink.Api.Models.Responses.ErrorResponse
            {
                Code = "VALIDATION_ERROR",
                Message = "One or more validation errors occurred.",
                Details = errors
            };

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
        };
    });

// OpenAPI / Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CargoLink Express API",
        Version = "v1",
        Description = "Modern shipping API for CargoLink Express"
    });
});

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// --- Middleware Pipeline ---

// 1. Global exception handler (outermost)
app.UseMiddleware<GlobalExceptionMiddleware>();

// 2. Swagger (dev only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 3. API key authentication
app.UseMiddleware<ApiKeyAuthMiddleware>();

// 4. Routing + controllers
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
```

### 7.4 appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ApiKey": "CHANGE_ME_IN_PRODUCTION",
  "Rates": {
    "DistanceFactor": 1.2,
    "ServiceLevels": {
      "Ground":    { "RateMultiplier": 0.50, "FuelSurcharge": 5.00,  "EstimatedDays": 5 },
      "Express":   { "RateMultiplier": 0.75, "FuelSurcharge": 8.00,  "EstimatedDays": 3 },
      "Overnight": { "RateMultiplier": 1.00, "FuelSurcharge": 12.00, "EstimatedDays": 1 }
    }
  }
}
```

---

## 8. API Versioning Strategy

### 8.1 Approach: URL Path Versioning

The API uses URL path versioning with the format `/api/v{N}/`.

| Strategy | Format | Chosen? | Rationale |
|---|---|---|---|
| **URL path** | `/api/v1/shipments` | ✅ **Yes** | Most visible and explicit; easy to route, test, and document; widely adopted (Stripe, GitHub) |
| Header | `Api-Version: 1` | No | Hidden from URL; harder to test in browser; curl requires extra flags |
| Query string | `?api-version=1` | No | Pollutes URL; easy to forget; caching complications |
| Content negotiation | `Accept: application/vnd.cargolink.v1+json` | No | Complex; poor tooling support |

### 8.2 Implementation

URL path versioning is implemented via route attributes (no additional NuGet packages required):

```csharp
// All controllers use the v1 prefix in their route template
[Route("api/v1/[controller]")]
public class ShipmentsController : ControllerBase { }
```

When **v2** is needed in the future:

1. Create new controller classes in a `Controllers/V2/` folder
2. Apply `[Route("api/v2/[controller]")]`
3. Keep v1 controllers running for backward compatibility
4. Deprecate v1 with response headers: `Sunset: <date>`, `Deprecation: true`

### 8.3 Version Lifecycle

```
v1 (current)  →  v1 (deprecated)  →  v1 (sunset)  →  v1 (removed)
                  v2 (current)        v2 (current)     v2 (current)

Timeline:         +6 months           +12 months        +18 months
```

---

## 9. Legacy-to-Modern Migration Map

This table maps every legacy endpoint to its modern equivalent, confirming full feature parity.

| Legacy Layer | Legacy Operation | Modern Route | Status |
|---|---|---|---|
| SOAP | `IShipmentService.CreateShipment` | `POST /api/v1/shipments` | ✅ Mapped |
| SOAP | `IShipmentService.GetShipment` | `GET /api/v1/shipments/{id}` | ✅ Mapped |
| SOAP | `IShipmentService.CancelShipment` | `POST /api/v1/shipments/{id}/cancel` | ✅ Mapped (was REST-missing) |
| SOAP | `ITrackingService.GetTrackingInfo` | `GET /api/v1/tracking/{trackingNumber}` | ✅ Mapped |
| SOAP | `IRateService.GetRates` | `POST /api/v1/rates` | ✅ Mapped |
| SOAP | `IRateService.GetRateForService` | `POST /api/v1/rates/{serviceLevel}` | ✅ Mapped (was REST-missing) |
| REST | `GET /api/shipments` | `GET /api/v1/shipments` | ✅ Mapped (was SOAP-missing) |
| REST | `POST /api/shipments` | `POST /api/v1/shipments` | ✅ Mapped |
| REST | `GET /api/shipments/{id}` | `GET /api/v1/shipments/{id}` | ✅ Mapped |
| REST | `GET /api/tracking/{trackingNumber}` | `GET /api/v1/tracking/{trackingNumber}` | ✅ Mapped |
| REST | `POST /api/quotes` | `POST /api/v1/rates` | ✅ Mapped (renamed) |

**All 11 legacy operations are covered. Zero functionality loss.**

---

## 10. Key Issues Resolved

| Legacy Issue (from step-03) | Resolution in Modern Design |
|---|---|
| Cost calculation duplicated in 3 places with 3 different results | Single `IRateService` with configurable `RateOptions`; `IShipmentService` delegates pricing to `IRateService` |
| `CancelShipment` missing from REST | `POST /api/v1/shipments/{id}/cancel` |
| `GetRateForService` missing from REST | `POST /api/v1/rates/{serviceLevel}` |
| No error handling (SOAP or REST) | `GlobalExceptionMiddleware` + structured `ErrorResponse` + validation attributes |
| No input validation | `[Required]`, `[Range]`, `[RegularExpression]` on all DTOs + `[ApiController]` auto-validation |
| Inconsistent naming (12+ conflicts) | Unified naming convention across all DTOs (see §3.4) |
| REST missing `PackageType`, `EstimatedDeliveryDate`, cost breakdown | All fields restored in modern DTOs |
| No authentication | `ApiKeyAuthMiddleware` (extensible to OAuth/JWT) |
| `CargoLink.Core` unused | Eliminated; logic centralized in service layer |
