# Step 8 – Spec-vs-Implementation Validation Report

> **Generated:** 2026-04-16  
> **Spec file:** `assets/outputs/openapi-unified-v2.json` (OpenAPI 3.0.3, v2.0.0)  
> **Build status:** ✅ `dotnet build CargoLink.ModernApi` — **0 errors, 0 warnings**

---

## 1. Endpoint Compliance Matrix

| # | Spec Endpoint | Method | Impl Controller | Impl Action | Route Match | Status |
|---|---|---|---|---|---|---|
| 1 | `/api/shipments` | GET | `ShipmentsController` | `GetAll()` | ⚠️ Versioned: `/api/v{ver}/shipments` | ✅ Implemented |
| 2 | `/api/shipments` | POST | `ShipmentsController` | `Create()` | ⚠️ Versioned | ✅ Implemented |
| 3 | `/api/shipments/{shipmentId}` | GET | `ShipmentsController` | `GetById(id)` | ⚠️ Versioned; param `id` vs `shipmentId` | ✅ Implemented |
| 4 | `/api/shipments/{shipmentId}` | DELETE | `ShipmentsController` | `Cancel(id)` | ⚠️ Versioned; param `id` vs `shipmentId` | ⚠️ Deviation (see §3) |
| 5 | `/api/tracking/{trackingNumber}` | GET | `TrackingController` | `GetByTrackingNumber()` | ⚠️ Versioned | ✅ Implemented |
| 6 | `/api/rates` | POST | `RatesController` | `GetRates()` | ⚠️ Versioned | ✅ Implemented |
| 7 | `/api/rates/{serviceLevel}` | POST | `RatesController` | `GetRateForService()` | ⚠️ Versioned | ✅ Implemented |

**Summary:** All 7 spec endpoints are implemented. Routes include API versioning (`/api/v1/...`) not present in the spec — this is an intentional enhancement.

---

## 2. Schema Compliance Matrix

| # | Spec Schema | Impl Model | Structure Match | Field-Level Notes | Status |
|---|---|---|---|---|---|
| 1 | `CreateShipmentRequest` (nested: origin/destination/package) | `CreateShipmentRequest` (flat fields) | ❌ Flat vs nested | Spec uses `Address` + `PackageDetails` sub-objects; impl uses flat `OriginAddress`, `OriginCity`, `OriginZipCode`, `Weight`, etc. | ⚠️ Structural deviation |
| 2 | `Shipment` (nested) | `ShipmentResponse` (flat) | ❌ Flat vs nested | Spec `shipmentId` → impl `Id`; spec nests origin/destination/package; impl adds `CreatedAt` (not in spec) | ⚠️ Structural deviation |
| 3 | `CancelShipmentResponse` | *(none)* | ❌ Missing | Spec defines `{shipmentId, cancelled, message}`; impl returns `204 No Content` with no body | ❌ Missing model |
| 4 | `TrackingInfo` | `TrackingResponse` | ✅ Flat match | Spec `currentStatus` → impl `Status`; all other fields align | ⚠️ Minor naming diff |
| 5 | `TrackingEvent` | `TrackingEvent` | ✅ Match | `eventType`, `location`, `timestamp`, `description` all present | ✅ Compliant |
| 6 | `RateRequest` | `RateRequest` | ⚠️ Close | Spec `packageWeight` → impl `Weight`; impl adds optional `ServiceLevel` field | ⚠️ Naming + extra field |
| 7 | `RateQuote` | `RateQuoteResponse` | ✅ Match | `serviceLevel`, `baseRate`, `fuelSurcharge`, `totalCost`, `estimatedDays` all present | ✅ Compliant |
| 8 | `Error` | `ErrorResponse` | ⚠️ Partial | Spec `details` is `array<{field, message}>`; impl `Details` is `string?` | ⚠️ Type mismatch |
| 9 | `Address` | *(embedded as flat fields)* | ❌ Not a model | Spec defines reusable `Address` with `street`, `city`, `zipCode` | ❌ Missing model |
| 10 | `PackageDetails` | *(embedded as flat fields)* | ❌ Not a model | Spec defines reusable `PackageDetails` with `weight`, `length`, `width`, `height`, `packageType` | ❌ Missing model |

---

## 3. Response Code Compliance

| Endpoint | Spec Codes | Impl Codes | Match | Notes |
|---|---|---|---|---|
| `GET /api/shipments` | 200, 500 | 200 | ⚠️ | 500 handled by middleware (not explicit `[ProducesResponseType]`) |
| `POST /api/shipments` | 201, 400, 500 | 201, 400 | ⚠️ | 400 returns `ValidationProblemDetails` (not spec `Error` schema); 500 via middleware |
| `GET /api/shipments/{id}` | 200, 404, 500 | 200, 404 | ⚠️ | 404 uses `ErrorResponse` ✅; 500 via middleware |
| `DELETE /api/shipments/{id}` | **200**, 404, 500 | **204**, 404 | ❌ | Spec returns `200` + `CancelShipmentResponse` body; impl returns `204 No Content` |
| `GET /api/tracking/{trackingNumber}` | 200, 404, 500 | 200, 404 | ⚠️ | 500 via middleware |
| `POST /api/rates` | 200, 400, 500 | 200, 400 | ⚠️ | 400 uses `ValidationProblemDetails`; 500 via middleware |
| `POST /api/rates/{serviceLevel}` | 200, 400, 404, 500 | 200, 400, 404 | ⚠️ | Same pattern |

---

## 4. Detailed Findings

### 4.1 Structural: Flat vs Nested Models (High Impact)

The spec defines reusable `Address` and `PackageDetails` schemas used as nested objects in `CreateShipmentRequest` and `Shipment`. The implementation flattens these into top-level properties (e.g., `OriginAddress`, `OriginCity`, `OriginZipCode`).

**Impact:** Any client generated from the OpenAPI spec will send/expect nested JSON (`origin.zipCode`) while the API expects flat JSON (`originZipCode`). These are **wire-incompatible**.

### 4.2 Cancel Endpoint: Status Code + Response Body (Medium Impact)

| Aspect | Spec | Implementation |
|---|---|---|
| Status code | `200 OK` | `204 No Content` |
| Response body | `CancelShipmentResponse {shipmentId, cancelled, message}` | *(none)* |

### 4.3 Error Response Schema (Low-Medium Impact)

- Spec `Error.details` is `array<{field, message}>` for field-level validation errors.
- Impl `ErrorResponse.Details` is `string?` — cannot carry structured validation info.
- `POST` endpoints return ASP.NET's `ValidationProblemDetails` for 400 errors instead of the spec's `Error` schema.

### 4.4 Field Naming Differences (Low Impact)

| Spec Field | Impl Field | Context |
|---|---|---|
| `shipmentId` | `Id` | `Shipment` / `ShipmentResponse` |
| `currentStatus` | `Status` | `TrackingInfo` / `TrackingResponse` |
| `packageWeight` | `Weight` | `RateRequest` |

### 4.5 Extra Fields in Implementation (Low Impact)

| Model | Extra Field | Notes |
|---|---|---|
| `ShipmentResponse` | `CreatedAt` | Not in spec; additive — non-breaking |
| `RateRequest` | `ServiceLevel` | Not in spec; optional — non-breaking |

---

## 5. Summary Scorecard

| Category | Total Items | ✅ Compliant | ⚠️ Deviation | ❌ Missing/Incompatible |
|---|---|---|---|---|
| **Endpoints** | 7 | 6 | 1 (cancel code) | 0 |
| **Schemas** | 10 | 2 | 4 | 4 (flat models + cancel response) |
| **Response Codes** | 7 endpoints | 0 exact | 6 (500 implicit) | 1 (cancel 200→204) |
| **Build** | 1 | 1 | 0 | 0 |

**Overall:** All 7 endpoints are functionally implemented and the project compiles cleanly. The primary gaps are (1) flat-vs-nested model structures making the wire format incompatible with spec-generated clients, (2) the missing `CancelShipmentResponse` model with `204` instead of `200`, and (3) `ErrorResponse.Details` being `string` instead of an array of field errors.
