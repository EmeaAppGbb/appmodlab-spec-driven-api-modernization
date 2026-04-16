# CargoLink Express — SOAP Service Contracts (Screenshot)

> WCF Service Contracts extracted from CargoLink.SoapServices project

---

## ShipmentService — `http://cargolink.com/shipment`

```csharp
[ServiceContract(Namespace = "http://cargolink.com/shipment")]
public interface IShipmentService
{
    [OperationContract] ShipmentResponse CreateShipment(ShipmentRequest request);
    [OperationContract] ShipmentResponse GetShipment(string shipmentId);
    [OperationContract] bool CancelShipment(string shipmentId);
}
```

### Data Contracts:
```csharp
[DataContract] ShipmentRequest
├── OriginAddress      (string)
├── OriginCity         (string)
├── OriginZipCode      (string)
├── DestinationAddress (string)
├── DestinationCity    (string)
├── DestinationZipCode (string)
├── Package            (PackageDetails)
│   ├── Weight         (decimal)
│   ├── Length         (decimal)
│   ├── Width          (decimal)
│   ├── Height         (decimal)
│   └── PackageType    (string)
└── ServiceLevel       (string)

[DataContract] ShipmentResponse
├── TrackingNumber        (string)
├── ShipmentId            (string)
├── Status                (string)
├── TotalCost             (decimal)
└── EstimatedDeliveryDate (string)
```

---

## TrackingService — `http://cargolink.com/tracking`

```csharp
[ServiceContract(Namespace = "http://cargolink.com/tracking")]
public interface ITrackingService
{
    [OperationContract] TrackingResponse GetTrackingInfo(TrackingRequest request);
}
```

### Data Contracts:
```csharp
[DataContract] TrackingRequest
└── TrackingNumber (string)

[DataContract] TrackingResponse
├── TrackingNumber  (string)
├── CurrentStatus   (string)
├── CurrentLocation (string)
├── Events[]        (TrackingEvent[])
│   ├── EventType   (string)
│   ├── Location    (string)
│   ├── Timestamp   (string)
│   └── Description (string)
└── EstimatedDelivery (string)
```

---

## RateService — `http://cargolink.com/rate`

```csharp
[ServiceContract(Namespace = "http://cargolink.com/rate")]
public interface IRateService
{
    [OperationContract] RateQuote[] GetRates(RateRequest request);
    [OperationContract] RateQuote GetRateForService(RateRequest request, string serviceLevel);
}
```

### Data Contracts:
```csharp
[DataContract] RateRequest
├── OriginZipCode      (string)
├── DestinationZipCode (string)
├── PackageWeight      (decimal)
└── ServiceLevel       (string)

[DataContract] RateQuote
├── ServiceLevel   (string)
├── BaseRate       (decimal)
├── FuelSurcharge  (decimal)
├── TotalCost      (decimal)
└── EstimatedDays  (int)
```
