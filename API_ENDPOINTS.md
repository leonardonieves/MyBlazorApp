# 🔌 API ENDPOINTS DOCUMENTATION

**Base URL:** `https://localhost:7100`

---

## 🔐 AUTH ENDPOINTS

### Login
```
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}

Response:
{
  "success": true,
  "message": "Login successful",
  "user": {
    "id": 1,
    "username": "admin",
    "email": "admin@example.com",
    "role": "Admin",
    "stripeCustomerId": "cus_...",
    "isActive": true
  },
  "token": "eyJhbGciOiJIUzI1NiIs..."
}
```

### Register
```
POST /api/auth/register
Content-Type: application/json

{
  "username": "newuser",
  "email": "user@example.com",
  "password": "password123"
}

Response:
{
  "success": true,
  "message": "Registration successful",
  "user": { ... }
}
```

---

## 🎫 RAFFLE ENDPOINTS

### Get All Raffles
```
GET /api/raffles
No auth required

Response:
[
  {
    "id": 1,
    "title": "Argentina vs France",
    "shortDescription": "...",
    "ticketPrice": 25.00,
    "totalTickets": 500,
    "ticketsSold": 150,
    "ticketsAvailable": 350,
    "drawDate": "2025-05-25",
    "isFeatured": true,
    "prizes": [...],
    "images": [...]
  }
]
```

### Get Featured Raffles
```
GET /api/raffles/featured?count=3
No auth required

Response: [RaffleDto...]
```

### Get Raffle by ID
```
GET /api/raffles/{id}
No auth required

Response: { RaffleDto }
```

---

## 🎟️ TICKET ENDPOINTS

### Get Raffle Tickets
```
GET /api/tickets/raffle/{raffleId}
Authorization: Bearer {token}

Response: [TicketDto...]
```

### Buy Tickets
```
POST /api/tickets/buy
Authorization: Bearer {token}
Content-Type: application/json

{
  "raffleId": 1,
  "quantity": 5,
  "userId": 1,
  "buyerEmail": "user@example.com",
  "buyerName": "John Doe",
  "stripeCustomerId": "cus_...",
  "successUrl": "https://localhost:7200/success",
  "cancelUrl": "https://localhost:7200/cancel"
}

Response:
{
  "success": true,
  "checkoutUrl": "https://checkout.stripe.com/pay/...",
  "sessionId": "cs_..."
}
```

### Reserve Tickets
```
POST /api/tickets/reserve
Authorization: Bearer {token}
Content-Type: application/json

{
  "raffleId": 1,
  "ticketCount": 5
}

Response:
{
  "success": true,
  "message": "Tickets reserved",
  "reservedTicketIds": [10, 11, 12, 13, 14],
  "expiresAt": "2025-03-20T12:30:00Z"
}
```

### Get My Tickets
```
GET /api/tickets/my-tickets?email=user@example.com
No auth required (email es público)

Response: [TicketDto...]
```

---

## 🔄 SYNC ENDPOINTS (Admin Only)

### Sync from Stripe
```
POST /api/sync/stripe
Authorization: Bearer {token}
Content-Type: application/json

Response:
{
  "success": true,
  "message": "Synced 3 raffles from Stripe",
  "syncedCount": 3,
  "raffles": [
    {
      "id": 1,
      "title": "Argentina vs France",
      "stripeProductId": "prod_...",
      "totalTickets": 500,
      "ticketPrice": 25.00
    }
  ]
}
```

### Get Sync Status
```
GET /api/sync/status
Authorization: Bearer {token}

Response:
{
  "totalRaffles": 3,
  "activeRaffles": 2,
  "totalTicketsGenerated": 1500,
  "totalTicketsSold": 450,
  "lastChecked": "2025-03-20T12:00:00Z"
}
```

### Debug: List Stripe Products
```
GET /api/sync/stripe-products
Authorization: Bearer {token}

Response:
[
  {
    "id": "prod_...",
    "name": "Argentina vs France Raffle",
    "active": true,
    "metadata": { "type": "raffle" },
    "isDetectedAsRaffle": true,
    "activePriceId": "price_...",
    "priceAmount": 25.00
  }
]
```

### Generate Tickets for Raffle
```
POST /api/sync/generate-tickets/{raffleId}?count=100
Authorization: Bearer {token}

Response:
{
  "success": true,
  "message": "Generated 100 tickets for raffle 1",
  "raffleId": 1,
  "ticketCount": 100
}
```

---

## 🪝 WEBHOOK ENDPOINTS

### Stripe Webhook
```
POST /api/stripe-webhook
Content-Type: application/json
Stripe-Signature: t=...,v1=...

# Procesa eventos de Stripe:
# - checkout.session.completed
# - checkout.session.async_payment_succeeded
# - charge.succeeded
# - payment_intent.succeeded
# - etc.

Response: 200 OK
```

---

## 🔒 Authentication

Todos los endpoints que requieren autenticación necesitan:

```
Authorization: Bearer {token}
```

El `token` se obtiene del endpoint `/api/auth/login`.

### Headers Ejemplo:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwibmFtZSI6ImFkbWluIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

---

## 🔑 Data Models

### UserDto
```json
{
  "id": 1,
  "username": "admin",
  "email": "admin@example.com",
  "role": "Admin",
  "stripeCustomerId": "cus_...",
  "isActive": true
}
```

### RaffleDto
```json
{
  "id": 1,
  "title": "Argentina vs France",
  "shortDescription": "...",
  "fullDescription": "...",
  "ticketPrice": 25.00,
  "totalTickets": 500,
  "ticketsSold": 150,
  "maxTicketsPerUser": 10,
  "primaryImageUrl": "https://...",
  "salesStartDate": "2025-04-01T00:00:00Z",
  "salesEndDate": "2025-05-24T23:59:59Z",
  "drawDate": "2025-05-25T00:00:00Z",
  "status": "Active",
  "isFeatured": true,
  "ticketsAvailable": 350,
  "isSoldOut": false,
  "prizes": [...],
  "images": [...]
}
```

### TicketDto
```json
{
  "id": 1,
  "raffleId": 1,
  "displayNumber": "001",
  "ticketNumber": "ARG-001-TICKET",
  "status": "Sold",
  "buyerEmail": "user@example.com",
  "buyerName": "John Doe",
  "purchasedAt": "2025-03-20T10:30:00Z"
}
```

---

## 📊 Status Codes

| Code | Meaning |
|------|---------|
| 200 | OK |
| 201 | Created |
| 400 | Bad Request |
| 401 | Unauthorized |
| 403 | Forbidden (Admin only) |
| 404 | Not Found |
| 500 | Server Error |

---

## 🧪 Testing con Swagger

Visita: https://localhost:7100/swagger

Aquí puedes:
- Ver todos los endpoints
- Probar cada endpoint
- Ver esquemas de datos
- Copiar ejemplos de requests

---

## 🔗 Links Útiles

- **Stripe API**: https://stripe.com/docs/api
- **.NET Docs**: https://learn.microsoft.com/dotnet
- **JWT**: https://jwt.io
