# Step 01 – CargoLink Legacy API Exploration Report

> **Generated:** 2026-04-16  
> **Solution:** `CargoLink.sln` — 3 projects targeting **.NET Framework 4.6.2**

---

## 1. Solution Overview

| Project | SDK / Type | Target | Purpose |
|---------|-----------|--------|---------|
| **CargoLink.Core** | `Microsoft.NET.Sdk` (class library) | net462 | Shared business logic (shipping cost calculation) |
| **CargoLink.SoapServices** | `Microsoft.NET.Sdk.Web` | net462 | WCF SOAP services (`.svc.cs` files) |
| **CargoLink.RestApi** | `Microsoft.NET.Sdk.Web` | net462 | ASP.NET Web API 2 REST controllers |

### Key Dependencies

| Project | Package | Version |
|---------|---------|---------|
| Core | EntityFramework | 6.4.4 |
| Core | System.Data.SqlClient | 4.8.5 ⚠️ *known high-severity vulnerability* |
| SoapServices | System.ServiceModel.Http | 4.10.0 |
| SoapServices | System.ServiceModel.Primitives | 4.10.0 |
| RestApi | Microsoft.AspNet.WebApi | 5.2.9 |
| RestApi | Newtonsoft.Json | 13.0.1 |
| RestApi | Microsoft.AspNetCore.Mvc | 1.1.3 *(referenced in SoapServices csproj – likely unused/misplaced)* |

---

## 2. SOAP Service Contracts (CargoLink.SoapServices)

### 2.1 IShipmentService

**File:** `ShipmentService.svc.cs`  
**Namespace:** `http://cargolink.com/shipment`

| Operation | Signature | Description |
|-----------|-----------|-------------|
| `CreateShipment` | `ShipmentResponse CreateShipment(ShipmentRequest request)` | Creates a shipment, returns tracking number, ID, cost, ETA |
| `GetShipment` | `ShipmentResponse GetShipment(string shipmentId)` | Retrieves shipment by ID |
| `CancelShipment` | `bool CancelShipment(string shipmentId)` | Cancels a shipment (always returns `true`) |

### 2.2 ITrackingService

**File:** `TrackingService.svc.cs`  
**Namespace:** `http://cargolink.com/tracking`

| Operation | Signature | Description |
|-----------|-----------|-------------|
| `GetTrackingInfo` | `TrackingResponse GetTrackingInfo(TrackingRequest request)` | Returns tracking status, location, event history |

### 2.3 IRateService

**File:** `RateService.svc.cs`  
**Namespace:** `http://cargolink.com/rate`

| Operation | Signature | Description |
|-----------|-----------|-------------|
| `GetRates` | `RateQuote[] GetRates(RateRequest request)` | Returns quotes for all service levels (Ground, Express, Overnight) |
| `GetRateForService` | `RateQuote GetRateForService(RateRequest request, string serviceLevel)` | Returns quote for a specific service level |

---

## 3. SOAP Data Contracts

### ShipmentRequest
**Namespace:** `http://cargolink.com/shipment`

| Field | Type |
|-------|------|
| `OriginAddress` | string |
| `OriginCity` | string |
| `OriginZipCode` | string |
| `DestinationAddress` | string |
| `DestinationCity` | string |
| `DestinationZipCode` | string |
| `Package` | PackageDetails |
| `ServiceLevel` | string |

### PackageDetails
**Namespace:** `http://cargolink.com/shipment`

| Field | Type |
|-------|------|
| `Weight` | decimal |
| `Length` | decimal |
| `Width` | decimal |
| `Height` | decimal |
| `PackageType` | string |

### ShipmentResponse
**Namespace:** `http://cargolink.com/shipment`

| Field | Type |
|-------|------|
| `TrackingNumber` | string |
| `ShipmentId` | string |
| `Status` | string |
| `TotalCost` | decimal |
| `EstimatedDeliveryDate` | string |

### TrackingRequest
**Namespace:** `http://cargolink.com/tracking`

| Field | Type |
|-------|------|
| `TrackingNumber` | string |

### TrackingResponse
**Namespace:** `http://cargolink.com/tracking`

| Field | Type |
|-------|------|
| `TrackingNumber` | string |
| `CurrentStatus` | string |
| `CurrentLocation` | string |
| `Events` | TrackingEvent[] |
| `EstimatedDelivery` | string |

### TrackingEvent
**Namespace:** `http://cargolink.com/tracking`

| Field | Type |
|-------|------|
| `EventType` | string |
| `Location` | string |
| `Timestamp` | string |
| `Description` | string |

### RateRequest
**Namespace:** `http://cargolink.com/rate`

| Field | Type |
|-------|------|
| `OriginZipCode` | string |
| `DestinationZipCode` | string |
| `PackageWeight` | decimal |
| `ServiceLevel` | string |

### RateQuote
**Namespace:** `http://cargolink.com/rate`

| Field | Type |
|-------|------|
| `ServiceLevel` | string |
| `BaseRate` | decimal |
| `FuelSurcharge` | decimal |
| `TotalCost` | decimal |
| `EstimatedDays` | int |

---

## 4. REST API Endpoints (CargoLink.RestApi)

All controllers inherit from `System.Web.Http.ApiController` (ASP.NET Web API 2).

### 4.1 ShipmentController

**Route prefix:** `api/shipments`

| HTTP Method | Route | Action | Request Body | Response |
|-------------|-------|--------|-------------|----------|
| `GET` | `api/shipments` | `GetShipments()` | — | `ShipmentDto[]` |
| `GET` | `api/shipments/{id}` | `GetShipment(string id)` | — | `ShipmentDto` |
| `POST` | `api/shipments` | `CreateShipment(CreateShipmentDto)` | `CreateShipmentDto` | `ShipmentDto` (201 Created) |

### 4.2 TrackingController

**Route prefix:** `api/tracking`

| HTTP Method | Route | Action | Request Body | Response |
|-------------|-------|--------|-------------|----------|
| `GET` | `api/tracking/{trackingNumber}` | `GetTracking(string trackingNumber)` | — | `TrackingDto` |

### 4.3 QuoteController

**Route prefix:** `api/quotes`

| HTTP Method | Route | Action | Request Body | Response |
|-------------|-------|--------|-------------|----------|
| `POST` | `api/quotes` | `GetQuotes(RateRequestDto)` | `RateRequestDto` | `RateQuoteDto[]` |

---

## 5. REST DTOs (Models)

### ShipmentDto

| Field | Type |
|-------|------|
| `Id` | string |
| `TrackingId` | string |
| `FromAddress` | string |
| `FromZip` | string |
| `ToAddress` | string |
| `ToZip` | string |
| `Weight` | decimal |
| `Service` | string |
| `CurrentStatus` | string |
| `Cost` | decimal |

### CreateShipmentDto

| Field | Type |
|-------|------|
| `FromAddress` | string |
| `FromCity` | string |
| `FromZip` | string |
| `ToAddress` | string |
| `ToCity` | string |
| `ToZip` | string |
| `Weight` | decimal |
| `Length` | decimal |
| `Width` | decimal |
| `Height` | decimal |
| `Service` | string |

### TrackingDto

| Field | Type |
|-------|------|
| `TrackingNumber` | string |
| `Status` | string |
| `Location` | string |
| `EstimatedDelivery` | string |
| `History` | TrackingEventDto[] |

### TrackingEventDto

| Field | Type |
|-------|------|
| `Event` | string |
| `Location` | string |
| `Timestamp` | string |
| `Notes` | string |

### RateRequestDto *(defined inline in QuoteController.cs)*

| Field | Type |
|-------|------|
| `FromZip` | string |
| `ToZip` | string |
| `Weight` | decimal |

### RateQuoteDto *(defined inline in QuoteController.cs)*

| Field | Type |
|-------|------|
| `Service` | string |
| `Price` | decimal |
| `Days` | int |

---

## 6. Shared Business Logic (CargoLink.Core)

### ShippingService (`Core/Services/ShippingService.cs`)

| Method | Signature | Description |
|--------|-----------|-------------|
| `CalculateShippingCost` | `decimal CalculateShippingCost(decimal weight, string serviceLevel, string originZip, string destZip)` | Computes: `(weight × rateMultiplier × distanceFactor) + fuelSurcharge` |
| `GetEstimatedDays` | `int GetEstimatedDays(string serviceLevel)` | Returns estimated delivery days by service level |

**Service-level rate table (from Core):**

| Level | Rate Multiplier | Fuel Surcharge | Est. Days |
|-------|----------------|----------------|-----------|
| Ground | 0.50 | $5.00 | 5 |
| Express | 1.20 | $8.00 | 2 |
| Overnight | 2.50 | $12.00 | 1 |

> ⚠️ `CalculateDistanceFactor()` is **hardcoded** to return `1.2` — no actual distance calculation.

---

## 7. Inconsistencies Between SOAP and REST

### 7.1 Naming Discrepancies

| Concept | SOAP (DataContract) | REST (DTO) | Notes |
|---------|---------------------|------------|-------|
| Shipment identifier | `ShipmentId` | `Id` | Different field name |
| Tracking identifier | `TrackingNumber` | `TrackingId` | REST uses `TrackingId` in ShipmentDto |
| Origin zip code | `OriginZipCode` | `FromZip` | Completely different naming convention |
| Destination zip code | `DestinationZipCode` | `ToZip` | Completely different naming convention |
| Origin address | `OriginAddress` | `FromAddress` | Different prefix convention |
| Destination address | `DestinationAddress` | `ToAddress` | Different prefix convention |
| Service tier | `ServiceLevel` | `Service` | Abbreviated in REST |
| Shipping cost | `TotalCost` | `Cost` | Abbreviated in REST |
| Tracking status | `CurrentStatus` (tracking) | `Status` | Abbreviated in REST |
| Tracking location | `CurrentLocation` | `Location` | Abbreviated in REST |
| Event type | `EventType` | `Event` | Abbreviated in REST |
| Event description | `Description` | `Notes` | Different name entirely |
| Event collection | `Events` | `History` | Different name entirely |
| Package weight (rate) | `PackageWeight` | `Weight` | Abbreviated in REST |
| Rate cost | `TotalCost` | `Price` | Different name entirely |
| Delivery estimate | `EstimatedDays` | `Days` | Abbreviated in REST |

### 7.2 Structural Differences

| Aspect | SOAP | REST |
|--------|------|------|
| **Shipment creation input** | Nested `PackageDetails` object containing Weight, Length, Width, Height, PackageType | Flat DTO with Weight, Length, Width, Height at top level; no PackageType field |
| **Rate request** | Includes `ServiceLevel` field to filter | No ServiceLevel field — always returns all 3 tiers |
| **Service endpoint naming** | `IRateService` / `RateService` | `QuoteController` at `api/quotes` — different domain term |
| **Cancel operation** | `CancelShipment` exists in SOAP | **No cancel endpoint in REST** |
| **List all shipments** | **Not available** in SOAP | `GET api/shipments` exists in REST |
| **Get single rate** | `GetRateForService` operation exists | **No single-rate endpoint** in REST |
| **Tracking input** | Wrapped in `TrackingRequest` object | Direct path parameter `{trackingNumber}` |

### 7.3 Business Logic Divergence

| Area | SOAP Implementation | REST Implementation | Core Library |
|------|---------------------|---------------------|-------------|
| **Cost formula** | `weight × 0.50 × 1.2` (hardcoded Ground only, ignores ServiceLevel) | `weight × 0.50 × 1.2` (same hardcoded formula) | `(weight × rateMultiplier(serviceLevel) × distanceFactor) + fuelSurcharge` — **more complete** |
| **Core library usage** | ❌ Does **not** reference CargoLink.Core | ❌ References Core in `.csproj` but **does not call** `ShippingService` | — Unused by both |
| **Rate calculation** | Uses inline calculation per service level (correct multipliers + surcharges) | Uses inline calculation (same values) | Core has the same values but is never invoked |

### 7.4 Missing Features (Gaps)

| Feature | SOAP | REST | Notes |
|---------|------|------|-------|
| Cancel shipment | ✅ | ❌ | REST has no `DELETE` endpoint |
| List all shipments | ❌ | ✅ | SOAP has no list operation |
| Get rate for specific service | ✅ | ❌ | REST always returns all rates |
| Package type field | ✅ | ❌ | REST `CreateShipmentDto` omits it |
| City fields in response | ✅ (`OriginCity`, `DestinationCity`) | ❌ (not in `ShipmentDto`) | Lost in REST response |
| Estimated delivery in shipment response | ✅ `EstimatedDeliveryDate` | ❌ Not returned | REST shipment creation omits ETA |

---

## 8. Build Results

### 8.1 NuGet Restore via `nuget.exe` (MSBuild-based)

```
FAILED — MSBuild auto-detection uses VS 2022 BuildTools which lacks .NET SDK resolvers.
Error: Could not resolve SDK "Microsoft.NET.Sdk.Web" / "Microsoft.NET.Sdk"
```

The standalone MSBuild in VS 2022 BuildTools does not include the .NET SDK required to resolve SDK-style projects targeting `net462`.

### 8.2 `dotnet restore` (via .NET SDK)

```
✅ SUCCESS — All projects restored.
⚠️ NU1903: System.Data.SqlClient 4.8.5 has a known high severity vulnerability
```

### 8.3 `dotnet build`

```
FAILED — 2 errors, 2 warnings
```

| Project | Error | Explanation |
|---------|-------|-------------|
| CargoLink.RestApi | CS5001: No static `Main` method | Web SDK projects need a `Program.cs` or `<OutputType>Library</OutputType>` |
| CargoLink.SoapServices | CS5001: No static `Main` method | Same issue — uses `Microsoft.NET.Sdk.Web` but has no entry point |
| CargoLink.Core | — | ✅ **Builds successfully** |

### 8.4 MSBuild Direct (`MSBuild.exe`)

```
FAILED — Cannot resolve SDK-style projects without .NET SDK on PATH.
```

> **Root cause:** The two web projects use `Microsoft.NET.Sdk.Web` but are class-library-style projects (no entry point). They should either use `Microsoft.NET.Sdk` with `<OutputType>Library</OutputType>` or include a `Program.cs`. Additionally, standalone MSBuild (VS BuildTools) cannot resolve SDK-style `.csproj` files — `dotnet build` is required.

---

## 9. Summary of Key Findings

1. **Three distinct service boundaries** exist: Shipments, Tracking, and Rates/Quotes — good candidates for independent microservices or a well-structured API.

2. **Pervasive naming inconsistencies** between SOAP and REST layers — the REST API uses abbreviated, informal names (`FromZip`, `Cost`, `Days`) vs. the SOAP layer's verbose, explicit names (`OriginZipCode`, `TotalCost`, `EstimatedDays`).

3. **Feature asymmetry** — SOAP has `CancelShipment` and `GetRateForService` with no REST equivalents; REST has `ListShipments` with no SOAP equivalent.

4. **Dead shared library** — `CargoLink.Core.Services.ShippingService` contains the most complete cost calculation logic but is **never called** by either the SOAP or REST implementations.

5. **Hardcoded mock data** — All operations return stubbed/hardcoded responses with no database or persistent storage.

6. **Build issues** — The web projects use `Microsoft.NET.Sdk.Web` without entry points, causing `CS5001` errors. Only `CargoLink.Core` compiles successfully.

7. **Security vulnerability** — `System.Data.SqlClient 4.8.5` has a known high-severity CVE ([GHSA-98g6-xh36-x2p7](https://github.com/advisories/GHSA-98g6-xh36-x2p7)).

8. **Legacy framework** — All projects target .NET Framework 4.6.2 (EOL), using ASP.NET Web API 2 and WCF — strong candidates for modernization to ASP.NET Core / minimal APIs.
