# Step 03 — API Pattern Analysis: CargoLink Express

> Analysis of SOAP-to-REST mapping patterns, data contract differences, error handling gaps, business logic duplication, and naming inconsistencies across the CargoLink Express codebase.

---

## 1. SOAP-to-REST Mapping Patterns

The table below maps each WCF `[OperationContract]` to its REST equivalent (or gap).

### 1.1 Shipment Service

| WCF Operation | SOAP Contract | REST Method | REST Route | Notes |
|---|---|---|---|---|
| `IShipmentService.CreateShipment(ShipmentRequest)` | POST (SOAP Action) | `POST` | `/api/shipments` | Functional equivalent; data shapes differ (see §2) |
| `IShipmentService.GetShipment(string shipmentId)` | POST (SOAP Action) | `GET` | `/api/shipments/{id}` | REST correctly uses GET; SOAP passes ID in request body |
| `IShipmentService.CancelShipment(string shipmentId)` | POST (SOAP Action) | **⚠️ No REST equivalent** | — | Cancel operation exists only in SOAP; REST API has no `DELETE` or `PATCH` endpoint |
| — | — | `GET` | `/api/shipments` | **⚠️ No SOAP equivalent** — list-all endpoint exists only in REST |

### 1.2 Tracking Service

| WCF Operation | SOAP Contract | REST Method | REST Route | Notes |
|---|---|---|---|---|
| `ITrackingService.GetTrackingInfo(TrackingRequest)` | POST (SOAP Action) | `GET` | `/api/tracking/{trackingNumber}` | SOAP wraps input in `TrackingRequest` object; REST uses path parameter |

### 1.3 Rate / Quote Service

| WCF Operation | SOAP Contract | REST Method | REST Route | Notes |
|---|---|---|---|---|
| `IRateService.GetRates(RateRequest)` | POST (SOAP Action) | `POST` | `/api/quotes` | Both use request body; REST uses different DTO (see §2) |
| `IRateService.GetRateForService(RateRequest, string serviceLevel)` | POST (SOAP Action) | **⚠️ No REST equivalent** | — | Filtered single-rate lookup exists only in SOAP |

### 1.4 Key Mapping Observations

- **Missing REST endpoints**: `CancelShipment` and `GetRateForService` have no REST equivalents — functionality loss for REST consumers.
- **Missing SOAP operations**: `GetShipments` (list all) is REST-only; SOAP clients cannot list shipments.
- **Verb semantics**: SOAP uses POST for everything; REST correctly maps reads to GET but uses POST for quote retrieval (should arguably be GET with query params since it's idempotent).
- **Parameter passing**: SOAP wraps every input in a DataContract object; REST mixes path params (`{id}`, `{trackingNumber}`) with body payloads.

---

## 2. Data Contract Differences

### 2.1 Shipment — Nested vs. Flat

| Aspect | SOAP (`ShipmentRequest` + `PackageDetails`) | REST (`CreateShipmentDto`) |
|---|---|---|
| **Structure** | Nested — `Package` property contains `Weight`, `Length`, `Width`, `Height`, `PackageType` | Flat — `Weight`, `Length`, `Width`, `Height` are top-level properties |
| **PackageType** | ✅ Present (`PackageDetails.PackageType`) | ❌ Missing — no equivalent field |
| **Address model** | Separate `OriginAddress`, `OriginCity`, `OriginZipCode` | `FromAddress`, `FromCity`, `FromZip` (abbreviated naming) |
| **Response model** | `ShipmentResponse` with `TrackingNumber`, `ShipmentId`, `TotalCost`, `EstimatedDeliveryDate` | `ShipmentDto` with `TrackingId`, `Id`, `Cost`, no estimated delivery |

**SOAP `ShipmentRequest`:**
```
ShipmentRequest
├── OriginAddress, OriginCity, OriginZipCode
├── DestinationAddress, DestinationCity, DestinationZipCode
├── Package (PackageDetails)
│   ├── Weight, Length, Width, Height
│   └── PackageType
└── ServiceLevel
```

**REST `CreateShipmentDto`:**
```
CreateShipmentDto
├── FromAddress, FromCity, FromZip
├── ToAddress, ToCity, ToZip
├── Weight, Length, Width, Height  ← flattened
└── Service                        ← renamed from ServiceLevel
```

### 2.2 Tracking — Events vs. History

| Aspect | SOAP (`TrackingResponse` + `TrackingEvent`) | REST (`TrackingDto` + `TrackingEventDto`) |
|---|---|---|
| **Events array** | `Events` (array of `TrackingEvent`) | `History` (array of `TrackingEventDto`) |
| **Status field** | `CurrentStatus` | `Status` |
| **Location field** | `CurrentLocation` | `Location` |
| **Event type** | `TrackingEvent.EventType` | `TrackingEventDto.Event` |
| **Event description** | `TrackingEvent.Description` | `TrackingEventDto.Notes` |

### 2.3 Rate Quote — Detailed vs. Simplified

| Aspect | SOAP (`RateQuote`) | REST (`RateQuoteDto`) |
|---|---|---|
| **Cost breakdown** | `BaseRate` + `FuelSurcharge` + `TotalCost` | `Price` only (single field) |
| **Service level** | `ServiceLevel` | `Service` |
| **Delivery estimate** | `EstimatedDays` | `Days` |
| **Request weight** | `RateRequest.PackageWeight` | `RateRequestDto.Weight` |
| **Request ServiceLevel** | `RateRequest.ServiceLevel` | ❌ Not present in REST request |

### 2.4 Summary of Missing Fields

| Field Present In | Missing From | Impact |
|---|---|---|
| SOAP `PackageDetails.PackageType` | REST `CreateShipmentDto` | REST cannot specify package type |
| SOAP `ShipmentResponse.EstimatedDeliveryDate` | REST `ShipmentDto` | REST responses lack delivery estimate |
| SOAP `RateQuote.BaseRate`, `FuelSurcharge` | REST `RateQuoteDto` | REST hides cost breakdown from consumers |
| SOAP `RateRequest.ServiceLevel` | REST `RateRequestDto` | REST cannot filter rates by service level |
| REST `ShipmentDto.FromAddress`, `ToAddress` | SOAP `ShipmentResponse` | SOAP response lacks address echo-back |

---

## 3. Error Handling Gaps

### 3.1 SOAP — No Fault Contracts

| Gap | Detail |
|---|---|
| **No `[FaultContract]` attributes** | None of the three `[ServiceContract]` interfaces declare `[FaultContract]` — all SOAP faults will be generic `FaultException` with no typed detail |
| **No input validation** | `CreateShipment` does not validate required fields (e.g., null `Package`, empty zip codes) |
| **Null return** | `RateService.GetRateForService` returns `null` when `serviceLevel` doesn't match — no fault, no empty response contract |
| **No exception handling** | No try/catch blocks anywhere; unhandled exceptions surface as raw SOAP faults |

### 3.2 REST — Minimal Error Responses

| Gap | Detail |
|---|---|
| **Only null-body check** | `ShipmentController.CreateShipment` returns `BadRequest` only when `dto == null`; individual field validation is absent |
| **No 404 responses** | `GetShipment` and `GetTracking` always return `200 OK` with hardcoded data — no "not found" path |
| **No 404 for cancel** | Cancel doesn't exist in REST at all (see §1) |
| **No error response schema** | The OpenAPI spec lists `400` for POST endpoints but with no response body schema — clients cannot parse error details |
| **No global exception filter** | No `ExceptionFilterAttribute` registered — unhandled exceptions return raw 500 with stack trace |
| **No model validation** | No `[Required]` attributes or `ModelState.IsValid` checks on any DTO |

### 3.3 Error Handling Comparison

```
SOAP Error Path:     Client → WCF → [No FaultContract] → Generic FaultException (500)
REST Error Path:     Client → Web API → [null check only] → BadRequest or raw 500
Ideal Error Path:    Client → API → Validation → Typed Error Response (400/404/409/500)
```

---

## 4. Business Logic Duplication

Cost calculation logic is duplicated in **three** separate locations with **inconsistent formulas**.

### 4.1 Cost Calculation Comparison

| Location | Formula | Includes Fuel Surcharge? | Distance Factor? |
|---|---|---|---|
| **`CargoLink.Core/Services/ShippingService.cs`** | `(weight × rateMultiplier × distanceFactor) + fuelSurcharge` | ✅ Yes ($5/$8/$12) | ✅ Yes (hardcoded 1.2) |
| **`CargoLink.SoapServices/ShipmentService.svc.cs`** | `weight × 0.50 × 1.2` | ❌ No | ✅ Hardcoded 1.2 |
| **`CargoLink.RestApi/Controllers/ShipmentController.cs`** | `weight × 0.50 × 1.2` | ❌ No | ✅ Hardcoded 1.2 |
| **`CargoLink.SoapServices/RateService.svc.cs`** | `(weight × rateMultiplier) + fuelSurcharge` | ✅ Yes ($5/$8/$12) | ❌ No |
| **`CargoLink.RestApi/Controllers/QuoteController.cs`** | `(weight × rateMultiplier) + fuelSurcharge` | ✅ Yes ($5/$8/$12) | ❌ No |

### 4.2 Specific Discrepancies

**Example: 10 lb Ground shipment**

| Calculation Location | Result |
|---|---|
| `ShippingService.cs` (Core) | (10 × 0.50 × 1.2) + 5.00 = **$11.00** |
| `ShipmentService.svc.cs` (SOAP) | 10 × 0.50 × 1.2 = **$6.00** |
| `ShipmentController.cs` (REST) | 10 × 0.50 × 1.2 = **$6.00** |
| `RateService.svc.cs` (SOAP rates) | (10 × 0.50) + 5.00 = **$10.00** |
| `QuoteController.cs` (REST quotes) | (10 × 0.50) + 5.00 = **$10.00** |

> ⚠️ **Three different prices** ($6.00, $10.00, $11.00) for the same shipment depending on which endpoint the client calls.

### 4.3 Core Service — Unused

`CargoLink.Core.Services.ShippingService` has the most complete implementation (rate multipliers, fuel surcharges, distance factors, estimated days) but is **never referenced** by either the SOAP services or the REST controllers. Both reimplement the logic inline.

---

## 5. Naming Inconsistencies

### 5.1 Field-Level Naming Differences

| Concept | SOAP DataContract | REST DTO | Core Service |
|---|---|---|---|
| Tracking identifier | `TrackingNumber` | `TrackingId` | — |
| Total price | `TotalCost` | `Cost` (shipment), `Price` (quote) | Returns `decimal` |
| Service tier | `ServiceLevel` | `Service` | `serviceLevel` (param) |
| Delivery estimate | `EstimatedDeliveryDate` | — (missing) | — |
| Delivery days | `EstimatedDays` | `Days` | `GetEstimatedDays()` |
| Events collection | `Events` | `History` | — |
| Event type/name | `EventType` | `Event` | — |
| Event description | `Description` | `Notes` | — |
| Current status | `CurrentStatus` (tracking), `Status` (shipment) | `CurrentStatus` (shipment), `Status` (tracking) | — |
| Current location | `CurrentLocation` | `Location` | — |
| Origin zip | `OriginZipCode` | `FromZip` | `originZip` (param) |
| Destination zip | `DestinationZipCode` | `ToZip` | `destZip` (param) |
| Package weight | `PackageDetails.Weight` / `RateRequest.PackageWeight` | `Weight` | `weight` (param) |
| Resource ID | `ShipmentId` | `Id` | — |

### 5.2 Convention Clashes

| Pattern | SOAP Convention | REST Convention |
|---|---|---|
| Address naming | `Origin`/`Destination` prefix | `From`/`To` prefix |
| Zip code naming | `ZipCode` suffix | `Zip` suffix |
| Identifier naming | Full name (`ShipmentId`, `TrackingNumber`) | Abbreviated (`Id`, `TrackingId`) |
| Cost naming | `TotalCost` everywhere | `Cost` (shipments), `Price` (quotes) |
| Status naming | Inconsistent within SOAP itself (`Status` vs `CurrentStatus`) | Also inconsistent (`CurrentStatus` vs `Status`) |

---

## 6. Modernization Recommendations

| # | Category | Finding | Recommendation | Priority |
|---|---|---|---|---|
| 1 | **Business Logic** | Cost calculation duplicated in 3 places with 3 different results | Centralize all pricing logic in `ShippingService.cs` and reference from both SOAP and REST layers | 🔴 Critical |
| 2 | **API Parity** | `CancelShipment` and `GetRateForService` missing from REST | Add `DELETE /api/shipments/{id}` and `GET /api/quotes?serviceLevel=X` endpoints | 🔴 Critical |
| 3 | **Error Handling** | No fault contracts (SOAP) and no structured errors (REST) | Define `[FaultContract]` types for SOAP; add `ProblemDetails` (RFC 7807) error responses for REST; add global exception filters | 🔴 Critical |
| 4 | **Validation** | No input validation on any endpoint | Add `[Required]`/`[Range]` attributes on DTOs; add `ModelState.IsValid` checks; add SOAP-side null checks with typed faults | 🟠 High |
| 5 | **Naming** | `TrackingId` vs `TrackingNumber`, `Cost` vs `TotalCost` vs `Price` | Adopt a single naming convention across all layers; recommend `trackingNumber`, `totalCost`, `serviceLevel` | 🟠 High |
| 6 | **Data Contracts** | Flat REST DTOs lose SOAP's `PackageDetails` structure and `PackageType` field | Adopt nested `package` object in REST; restore `packageType` and `estimatedDeliveryDate` fields | 🟠 High |
| 7 | **API Design** | `POST /api/quotes` should be `GET` (idempotent read) | Change to `GET /api/quotes?fromZip=X&toZip=Y&weight=Z` or keep POST but document idempotency | 🟡 Medium |
| 8 | **Rate Transparency** | REST hides `baseRate` and `fuelSurcharge` breakdown | Include cost breakdown in REST quote responses to match SOAP detail level | 🟡 Medium |
| 9 | **Response Gaps** | REST `ShipmentDto` missing `EstimatedDeliveryDate`; no 404 responses | Add missing response fields; implement proper not-found handling | 🟡 Medium |
| 10 | **OpenAPI Spec** | Spec lacks error schemas, cancel endpoint, single-rate endpoint | Update `openapi-legacy-rest.json` to reflect actual API surface and add error schemas | 🟡 Medium |
| 11 | **Core Library** | `CargoLink.Core` is completely unused | Wire `ShippingService` into both API layers or extract as shared NuGet package | 🟡 Medium |
| 12 | **Date Handling** | Dates returned as strings (`"yyyy-MM-dd"`) instead of ISO 8601 datetime | Use `DateTime`/`DateTimeOffset` types and let serializers handle ISO 8601 formatting | 🟢 Low |

---

## Appendix: File Reference

| File | Layer | Purpose |
|---|---|---|
| `CargoLink.SoapServices/ShipmentService.svc.cs` | SOAP | Shipment CRUD operations |
| `CargoLink.SoapServices/TrackingService.svc.cs` | SOAP | Tracking lookup |
| `CargoLink.SoapServices/RateService.svc.cs` | SOAP | Rate quoting |
| `CargoLink.SoapServices/DataContracts/ShipmentRequest.cs` | SOAP | `ShipmentRequest`, `PackageDetails`, `ShipmentResponse` |
| `CargoLink.SoapServices/DataContracts/TrackingResponse.cs` | SOAP | `TrackingRequest`, `TrackingResponse`, `TrackingEvent` |
| `CargoLink.SoapServices/DataContracts/RateQuote.cs` | SOAP | `RateRequest`, `RateQuote` |
| `CargoLink.RestApi/Controllers/ShipmentController.cs` | REST | Shipment endpoints |
| `CargoLink.RestApi/Controllers/TrackingController.cs` | REST | Tracking endpoint |
| `CargoLink.RestApi/Controllers/QuoteController.cs` | REST | Quote endpoint + inline DTOs |
| `CargoLink.RestApi/Models/ShipmentDto.cs` | REST | `ShipmentDto`, `CreateShipmentDto` |
| `CargoLink.RestApi/Models/TrackingDto.cs` | REST | `TrackingDto`, `TrackingEventDto` |
| `CargoLink.Core/Services/ShippingService.cs` | Core | Centralized (but unused) shipping logic |
| `openapi-legacy-rest.json` | Spec | OpenAPI 3.0 spec for REST API |
