# CargoLink Express — Legacy Code Structure (Screenshot)

> Solution: CargoLink.sln (.NET Framework 4.6.2)

---

## Solution Structure

```
CargoLink.sln
│
├── CargoLink.Core/                    # Shared business logic
│   ├── CargoLink.Core.csproj          # net462, EF6, System.Data.SqlClient
│   └── Services/
│       └── ShippingService.cs         # Rate calculation, distance factor
│
├── CargoLink.SoapServices/           # WCF SOAP services (enterprise clients)
│   ├── CargoLink.SoapServices.csproj  # net462, System.ServiceModel
│   ├── ShipmentService.svc.cs         # [ServiceContract] CreateShipment, GetShipment, CancelShipment
│   ├── TrackingService.svc.cs         # [ServiceContract] GetTrackingInfo
│   ├── RateService.svc.cs             # [ServiceContract] GetRates, GetRateForService
│   └── DataContracts/
│       ├── ShipmentRequest.cs         # [DataContract] OriginAddress, DestinationAddress, PackageDetails
│       ├── TrackingResponse.cs        # [DataContract] CurrentStatus, CurrentLocation, Events[]
│       └── RateQuote.cs               # [DataContract] ServiceLevel, BaseRate, FuelSurcharge, TotalCost
│
├── CargoLink.RestApi/                # Legacy REST API (mobile apps)
│   ├── CargoLink.RestApi.csproj       # net462, Microsoft.AspNet.WebApi 5.2.9
│   ├── Controllers/
│   │   ├── ShipmentController.cs      # [RoutePrefix("api/shipments")] GET list, GET {id}, POST create
│   │   ├── TrackingController.cs      # [RoutePrefix("api/tracking")] GET {trackingNumber}
│   │   └── QuoteController.cs         # [RoutePrefix("api/quotes")] POST rate quotes
│   └── Models/
│       ├── ShipmentDto.cs             # Id, TrackingId, FromAddress, Weight, Cost, CurrentStatus
│       └── TrackingDto.cs             # TrackingNumber, Status, Location, History[]
│
├── openapi-legacy-rest.json          # Reverse-engineered OpenAPI 3.0 spec
├── APPMODLAB.MD                       # Lab metadata
├── SPEC2CLOUD.MD                      # Spec2Cloud configuration metadata
└── README.md                         # Full lab instructions & quest objectives
```

---

## Key Inconsistencies (SOAP vs REST)

| Concept | SOAP (DataContract) | REST (DTO) |
|---------|---------------------|------------|
| Origin address | `OriginAddress` | `FromAddress` |
| Destination | `DestinationAddress` | `ToAddress` |
| Zip code | `OriginZipCode` / `DestinationZipCode` | `FromZip` / `ToZip` |
| Status | `CurrentStatus` ("In Transit") | `CurrentStatus` ("In Transit") |
| Service tier | `ServiceLevel` | `Service` |
| Cost | `TotalCost` (decimal) | `Cost` (decimal) |
| Tracking events | `TrackingEvent.EventType` | `TrackingEventDto.Event` |
| Event description | `TrackingEvent.Description` | `TrackingEventDto.Notes` |
| Package details | Nested `PackageDetails` object | Flat fields on DTO |
| Rate breakdown | `BaseRate` + `FuelSurcharge` + `TotalCost` | Single `Price` field |
| Auth | WS-Security (SOAP) | Custom header API keys (REST) |

---

## Technology Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| SOAP Services | WCF / System.ServiceModel | .NET Framework 4.6.2 |
| REST API | ASP.NET Web API | 5.2.9 |
| Core Logic | Entity Framework | 6.4.4 |
| Database | System.Data.SqlClient | 4.8.5 |
| Target | .NET Framework | 4.6.2 |
