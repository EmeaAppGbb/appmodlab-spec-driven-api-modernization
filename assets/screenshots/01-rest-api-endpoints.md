# CargoLink Express — Legacy REST API Endpoints (Screenshot)

> Captured from live server running on `http://localhost:5800`
> Host: ASP.NET Core minimal API re-hosting the legacy Web API 1.x controllers

---

## GET /api/shipments — List All Shipments

```http
GET http://localhost:5800/api/shipments
```

**Response 200 OK:**
```json
[
  {
    "id": "ship_001",
    "trackingId": "CL123456789",
    "fromZip": "10001",
    "toZip": "90001",
    "weight": 5.5,
    "service": "Ground",
    "currentStatus": "In Transit",
    "cost": 25.99
  }
]
```

---

## GET /api/shipments/{id} — Get Shipment by ID

```http
GET http://localhost:5800/api/shipments/ship_001
```

**Response 200 OK:**
```json
{
  "id": "ship_001",
  "trackingId": "CL123456789",
  "fromAddress": "123 Main St",
  "fromZip": "10001",
  "toAddress": "456 Oak Ave",
  "toZip": "90001",
  "weight": 5.5,
  "service": "Ground",
  "currentStatus": "In Transit",
  "cost": 25.99
}
```

---

## POST /api/shipments — Create Shipment

```http
POST http://localhost:5800/api/shipments
Content-Type: application/json

{
  "FromAddress": "123 Main St",
  "FromZip": "10001",
  "ToAddress": "456 Oak Ave",
  "ToZip": "90001",
  "Weight": 5.5,
  "Service": "Express"
}
```

**Response 201 Created:**
```json
{
  "id": "ship_eec006d0",
  "trackingId": "CL0354590461",
  "fromAddress": "123 Main St",
  "fromZip": "10001",
  "toAddress": "456 Oak Ave",
  "toZip": "90001",
  "weight": 5.5,
  "service": "Express",
  "currentStatus": "Created",
  "cost": 3.3
}
```

---

## GET /api/tracking/{trackingNumber} — Track Shipment

```http
GET http://localhost:5800/api/tracking/CL123456789
```

**Response 200 OK:**
```json
{
  "trackingNumber": "CL123456789",
  "status": "In Transit",
  "location": "Chicago, IL",
  "estimatedDelivery": "2026-04-18",
  "history": [
    {
      "event": "Picked Up",
      "location": "New York, NY",
      "timestamp": "2026-04-15 01:50:35",
      "notes": "Package picked up"
    },
    {
      "event": "In Transit",
      "location": "Chicago, IL",
      "timestamp": "2026-04-16 01:50:35",
      "notes": "At distribution center"
    }
  ]
}
```

---

## POST /api/quotes — Get Rate Quotes

```http
POST http://localhost:5800/api/quotes
Content-Type: application/json

{
  "FromZip": "10001",
  "ToZip": "90001",
  "Weight": 10.0
}
```

**Response 200 OK:**
```json
[
  { "service": "Ground",    "price": 10.0,  "days": 5 },
  { "service": "Express",   "price": 20.0,  "days": 2 },
  { "service": "Overnight", "price": 37.0,  "days": 1 }
]
```

---

## Swagger UI

Available at: `http://localhost:5800/swagger`
OpenAPI spec at: `http://localhost:5800/openapi/v1.json`

Swagger UI renders three endpoint groups:
- **Shipments** — GET list, GET by id, POST create
- **Tracking** — GET by tracking number
- **Quotes** — POST rate request
