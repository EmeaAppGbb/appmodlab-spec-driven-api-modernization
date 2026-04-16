# Step 6: Implement API Endpoints with Business Logic

## Overview

Ported all business logic from the legacy CargoLink SOAP/REST services into the modern ASP.NET Core 8 API scaffold at `CargoLink.ModernApi/`.

## Changes Made

### Services Implemented

#### `Services/ShipmentService.cs`
- **In-memory storage** using `ConcurrentDictionary<string, ShipmentResponse>` (thread-safe)
- **CreateAsync**: Generates tracking number (`"CL" + ticks`), calculates cost via `IRateService`, sets estimated delivery date, stores shipment
- **GetAllAsync / GetByIdAsync**: Reads from in-memory store
- **CancelAsync**: Sets status to `"Cancelled"`; returns false if not found or already cancelled
- **FindByTrackingNumber**: Internal lookup used by `TrackingService`

#### `Services/RateService.cs`
- **GetRatesAsync**: Returns Ground/Express/Overnight rate quotes matching legacy tiers
- **GetRateForServiceAsync**: Returns a single quote for a specific service level
- **Cost formula**: `(weight × rateMultiplier × distanceFactor) + fuelSurcharge` — matches `CargoLink.Core.Services.ShippingService.CalculateShippingCost`
- Rate multipliers: Ground=0.50, Express=1.20, Overnight=2.50
- Fuel surcharges: Ground=$5, Express=$8, Overnight=$12
- Distance factor: 1.2 (stub, matching legacy)

#### `Services/TrackingService.cs`
- **GetTrackingAsync**: Looks up shipment by tracking number, generates tracking events based on shipment status (Created → Picked Up, In Transit → distribution center event, Delivered/Cancelled events)

### Controllers Updated

| Controller | Endpoint | Status Codes | Notes |
|---|---|---|---|
| `ShipmentsController` | `GET /api/v1/shipments` | 200 | List all shipments |
| | `GET /api/v1/shipments/{id}` | 200, 404 | Get by ID |
| | `POST /api/v1/shipments` | 201, 400 | Create with validation |
| | `DELETE /api/v1/shipments/{id}` | 204, 404 | Cancel shipment |
| `TrackingController` | `GET /api/v1/tracking/{trackingNumber}` | 200, 404 | Get tracking info |
| `RatesController` | `POST /api/v1/rates` | 200, 400 | Get all rate quotes |
| | `POST /api/v1/rates/{serviceLevel}` | 200, 400, 404 | Get rate for specific service |

All controllers use `[Produces("application/json")]`, `[ProducesResponseType]` attributes, and `ValidationProblemDetails` for 400 responses.

### Middleware Updated

#### `Middleware/ErrorHandlingMiddleware.cs`
- Returns **RFC 7807 ProblemDetails** responses (`application/problem+json`)
- Maps exception types: `ArgumentException` → 400, `KeyNotFoundException` → 404, `InvalidOperationException` → 409, default → 500
- Suppresses internal error details for 500 responses

### DI Registration (`Program.cs`)
- All services registered as **Singleton** to preserve in-memory shipment state across requests
- `ShipmentService` registered both as concrete type and via `IShipmentService` interface so `TrackingService` can access `FindByTrackingNumber`

## Build Status

```
Build succeeded. 0 Warning(s), 0 Error(s)
```

## Legacy → Modern Mapping

| Legacy Service | Modern Service | Key Logic Preserved |
|---|---|---|
| `ShipmentService.svc.cs` | `Services/ShipmentService.cs` | Tracking number generation, cost calculation |
| `TrackingService.svc.cs` | `Services/TrackingService.cs` | Status-based event generation |
| `RateService.svc.cs` | `Services/RateService.cs` | 3-tier rate quotes, per-service lookup |
| `Core/ShippingService.cs` | `Services/RateService.cs` | `weight × rate × distanceFactor + fuelSurcharge` formula |
