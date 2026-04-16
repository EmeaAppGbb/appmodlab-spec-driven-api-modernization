# Step 02 — SOAP-to-REST API Specification Mapping

This document explains how the legacy CargoLink WCF SOAP services and ASP.NET Web API 2 REST controllers were consolidated into a single unified OpenAPI 3.0.3 specification (`openapi-unified-v2.json`).

---

## Source Systems Analyzed

| Layer | Project | Technology | Contracts |
|-------|---------|------------|-----------|
| SOAP | `CargoLink.SoapServices` | WCF (.NET Framework 4.6.2) | `IShipmentService`, `ITrackingService`, `IRateService` |
| REST | `CargoLink.RestApi` | ASP.NET Web API 2 (.NET Framework 4.6.2) | `ShipmentController`, `TrackingController`, `QuoteController` |
| Core | `CargoLink.Core` | Shared library | `ShippingService` (business logic) |

---

## Operation Mapping

### Shipments

| Unified Endpoint | HTTP | SOAP Source | REST Source | Notes |
|---|---|---|---|---|
| `/api/shipments` | `GET` | — | `ShipmentController.GetShipments()` | REST-only; no SOAP equivalent |
| `/api/shipments` | `POST` | `IShipmentService.CreateShipment(ShipmentRequest)` | `ShipmentController.CreateShipment(CreateShipmentDto)` | Merged; unified request schema |
| `/api/shipments/{shipmentId}` | `GET` | `IShipmentService.GetShipment(string)` | `ShipmentController.GetShipment(string)` | Merged; unified response schema |
| `/api/shipments/{shipmentId}` | `DELETE` | `IShipmentService.CancelShipment(string)` → `bool` | — | SOAP-only; mapped to HTTP DELETE with structured response |

### Tracking

| Unified Endpoint | HTTP | SOAP Source | REST Source | Notes |
|---|---|---|---|---|
| `/api/tracking/{trackingNumber}` | `GET` | `ITrackingService.GetTrackingInfo(TrackingRequest)` | `TrackingController.GetTracking(string)` | Merged; SOAP required a request object with `TrackingNumber`, REST used a path parameter. Unified as path parameter. |

### Rates

| Unified Endpoint | HTTP | SOAP Source | REST Source | Notes |
|---|---|---|---|---|
| `/api/rates` | `POST` | `IRateService.GetRates(RateRequest)` | `QuoteController.GetQuotes(RateRequestDto)` | Merged; renamed from `/api/quotes` to `/api/rates` for consistency |
| `/api/rates/{serviceLevel}` | `POST` | `IRateService.GetRateForService(RateRequest, string)` | — | SOAP-only; `serviceLevel` moved from method parameter to path parameter |

---

## Schema Mapping Decisions

### Address Normalization

The SOAP `ShipmentRequest` used six flat fields (`OriginAddress`, `OriginCity`, `OriginZipCode`, `DestinationAddress`, `DestinationCity`, `DestinationZipCode`). The REST `CreateShipmentDto` used a similar pattern (`FromAddress`, `FromCity`, `FromZip`, `ToAddress`, `ToCity`, `ToZip`).

**Decision:** Introduce a reusable `Address` schema (`street`, `city`, `zipCode`) and reference it as `origin` and `destination` in the request/response. This removes duplication, improves readability, and follows RESTful nested-resource conventions.

### Package Details

The SOAP `PackageDetails` was a nested `DataContract` with `Weight`, `Length`, `Width`, `Height`, `PackageType`. The REST `CreateShipmentDto` had `Weight`, `Length`, `Width`, `Height` as flat fields with no `PackageType`.

**Decision:** Retain the SOAP `PackageDetails` structure as a unified `PackageDetails` schema, including `packageType` (from SOAP). This provides the superset of both APIs.

### Shipment Response Unification

The SOAP `ShipmentResponse` had: `TrackingNumber`, `ShipmentId`, `Status`, `TotalCost`, `EstimatedDeliveryDate`. The REST `ShipmentDto` had: `Id`, `TrackingId`, `FromAddress`, `FromZip`, `ToAddress`, `ToZip`, `Weight`, `Service`, `CurrentStatus`, `Cost`.

**Decision:** The unified `Shipment` schema combines all fields using consistent camelCase naming:
- `ShipmentId`/`Id` → `shipmentId`
- `TrackingNumber`/`TrackingId` → `trackingNumber`
- `Status`/`CurrentStatus` → `status`
- `TotalCost`/`Cost` → `totalCost`
- Added `origin`, `destination`, `package`, `serviceLevel` from both sources

### Tracking Info Unification

The SOAP `TrackingResponse` had: `TrackingNumber`, `CurrentStatus`, `CurrentLocation`, `EstimatedDelivery`, `Events[]`. The REST `TrackingDto` had: `TrackingNumber`, `Status`, `Location`, `EstimatedDelivery`, `History[]`.

**Decision:** The unified `TrackingInfo` uses the more descriptive SOAP field names:
- `Status`/`CurrentStatus` → `currentStatus`
- `Location`/`CurrentLocation` → `currentLocation`
- `History`/`Events` → `events` (SOAP name, more semantically accurate)

### Tracking Event Unification

SOAP `TrackingEvent`: `EventType`, `Location`, `Timestamp`, `Description`. REST `TrackingEventDto`: `Event`, `Location`, `Timestamp`, `Notes`.

**Decision:** Unified as `TrackingEvent` using the more descriptive names:
- `Event`/`EventType` → `eventType`
- `Notes`/`Description` → `description`
- `Timestamp` → `timestamp` (upgraded to `date-time` format)

### Rate Quote Unification

SOAP `RateQuote`: `ServiceLevel`, `BaseRate`, `FuelSurcharge`, `TotalCost`, `EstimatedDays`. REST `RateQuoteDto`: `Service`, `Price`, `Days`.

**Decision:** The unified `RateQuote` retains the detailed SOAP cost breakdown (`baseRate`, `fuelSurcharge`, `totalCost`) that was lost in the simplified REST DTO. The REST `Price` mapped to `totalCost`, and `Days` mapped to `estimatedDays`.

### Rate Request Unification

SOAP `RateRequest`: `OriginZipCode`, `DestinationZipCode`, `PackageWeight`, `ServiceLevel`. REST `RateRequestDto`: `FromZip`, `ToZip`, `Weight`.

**Decision:** Unified as `RateRequest` with full SOAP field names (`originZipCode`, `destinationZipCode`, `packageWeight`). The `ServiceLevel` field from SOAP is omitted because it is redundant — it is used as a path parameter on the `/api/rates/{serviceLevel}` endpoint instead.

---

## Key Design Decisions

### 1. CancelShipment → HTTP DELETE

The SOAP `CancelShipment` returned a bare `bool`. In the unified API, this maps to `DELETE /api/shipments/{shipmentId}` with a structured `CancelShipmentResponse` containing `shipmentId`, `cancelled` (boolean), and `message`. This is more RESTful and provides better client feedback than a raw boolean.

### 2. Renaming `/api/quotes` → `/api/rates`

The REST API used `/api/quotes` while the SOAP service was `IRateService`. The term "rates" is more aligned with the domain and the SOAP contract. The unified API uses `/api/rates` for consistency.

### 3. GetRateForService as Path Parameter

The SOAP `GetRateForService(RateRequest, string serviceLevel)` accepted `serviceLevel` as a method parameter. In REST, this naturally maps to a path parameter: `POST /api/rates/{serviceLevel}`. This allows clients to get a single rate quote without filtering the full array.

### 4. Structured Error Responses

Neither the SOAP services nor the REST controllers had consistent error handling. The unified API introduces a standard `Error` schema with `code`, `message`, and optional `details` array. All endpoints include `400`, `404`, and `500` responses where applicable.

### 5. Naming Convention: camelCase

Both SOAP DataContracts (PascalCase `DataMember` names) and REST DTOs (PascalCase C# properties) used PascalCase. The unified API adopts **camelCase** for all JSON property names, following the JSON/JavaScript convention standard in modern REST APIs.

### 6. Date/Time Formats

SOAP services returned dates as formatted strings (`yyyy-MM-dd`, `yyyy-MM-dd HH:mm:ss`). The unified API specifies proper OpenAPI `format: date` and `format: date-time` annotations for ISO 8601 compliance.

---

## Coverage Summary

| Source Operation | Unified? | Unified Endpoint |
|---|---|---|
| SOAP `IShipmentService.CreateShipment` | ✅ | `POST /api/shipments` |
| SOAP `IShipmentService.GetShipment` | ✅ | `GET /api/shipments/{shipmentId}` |
| SOAP `IShipmentService.CancelShipment` | ✅ | `DELETE /api/shipments/{shipmentId}` |
| SOAP `ITrackingService.GetTrackingInfo` | ✅ | `GET /api/tracking/{trackingNumber}` |
| SOAP `IRateService.GetRates` | ✅ | `POST /api/rates` |
| SOAP `IRateService.GetRateForService` | ✅ | `POST /api/rates/{serviceLevel}` |
| REST `ShipmentController.GetShipments` | ✅ | `GET /api/shipments` |
| REST `ShipmentController.GetShipment` | ✅ | `GET /api/shipments/{shipmentId}` |
| REST `ShipmentController.CreateShipment` | ✅ | `POST /api/shipments` |
| REST `TrackingController.GetTracking` | ✅ | `GET /api/tracking/{trackingNumber}` |
| REST `QuoteController.GetQuotes` | ✅ | `POST /api/rates` |

**All 11 operations from both SOAP and REST are covered in the unified specification.**
