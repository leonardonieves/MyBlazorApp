# Raffle System Architecture Documentation

## 🏗️ **System Overview**

This document describes the complete architecture for the World Cup Sweepstakes raffle system, including entity models, Stripe integration, and implementation details.

---

## 📊 **Entity Relationship Diagram**

```
┌─────────────┐       ┌─────────────────┐       ┌─────────────┐
│    User     │       │     Raffle      │       │   Ticket    │
├─────────────┤       ├─────────────────┤       ├─────────────┤
│ Id          │──┐    │ Id              │◄──────│ Id          │
│ Username    │  │    │ Title           │       │ RaffleId    │
│ Email       │  │    │ ShortDescription│       │ UserId      │──┐
│ Password    │  │    │ FullDescription │       │ BuyerEmail  │  │
│ StripeId    │  │    │ TicketPrice     │       │ BuyerName   │  │
└─────────────┘  │    │ TotalTickets    │       │ TicketNumber│  │
                 │    │ TicketsSold     │       │ AmountPaid  │  │
                 │    │ MaxPerUser      │       │ Status(enum)│  │
                 │    │ Status (enum)   │       │ StripeIds   │  │
                 │    │ StripeProductId │       └─────────────┘  │
                 │    │ StripePriceId   │              │         │
                 │    └─────────────────┘              │         │
                 │           │                         │         │
                 │    ┌──────┴──────┐                  │         │
                 │    │             │                  │         │
                 │    ▼             ▼                  │         │
          ┌───────────────┐  ┌───────────────┐        │         │
          │  RafflePrize  │  │  RaffleImage  │        │         │
          ├───────────────┤  ├───────────────┤        │         │
          │ Id            │  │ Id            │        │         │
          │ RaffleId      │  │ RaffleId      │        │         │
          │ Description   │  │ ImageUrl      │        │         │
          │ Icon          │  │ AltText       │        │         │
          │ DisplayOrder  │  │ IsPrimary     │        │         │
          └───────────────┘  └───────────────┘        │         │
                                                      │         │
                                            ┌─────────▼─────────▼──┐
                                            │       Winner         │
                                            ├──────────────────────┤
                                            │ Id                   │
                                            │ RaffleId             │
                                            │ TicketId             │
                                            │ AnnouncedAt          │
                                            │ ContactedWinner      │
                                            │ PrizeDelivered       │
                                            └──────────────────────┘
```

---

## 📝 **Entity Models**

### **RaffleStatus Enum**
```csharp
public enum RaffleStatus
{
    Draft = 0,          // Not visible to public
    Active = 1,         // Accepting purchases
    SalesClosed = 2,    // Sales ended, waiting for draw
    Drawing = 3,        // Draw in progress
    Completed = 4,      // Winner announced
    Cancelled = 5       // Raffle cancelled
}
```

### **TicketStatus Enum**
```csharp
public enum TicketStatus
{
    Pending = 0,        // Awaiting payment
    Confirmed = 1,      // Payment confirmed
    Refunded = 2,       // Payment refunded
    Cancelled = 3,      // Cancelled
    Winner = 4          // This ticket won!
}
```

### **Key Entity Properties**

| Entity | Key Fields | Purpose |
|--------|------------|---------|
| **Raffle** | `Id`, `Title`, `TicketPrice`, `TotalTickets`, `Status`, `StripeProductId`, `StripePriceId` | Main raffle event |
| **RafflePrize** | `Id`, `RaffleId`, `Description`, `Icon`, `DisplayOrder` | Individual prizes within a raffle |
| **RaffleImage** | `Id`, `RaffleId`, `ImageUrl`, `IsPrimary` | Gallery images for raffle |
| **Ticket** | `Id`, `RaffleId`, `UserId`, `TicketNumber`, `Status`, `StripePaymentIntentId` | Purchased ticket |
| **Winner** | `Id`, `RaffleId`, `TicketId`, `AnnouncedAt` | Winner record |

---

## 🔄 **Purchase Flow**

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   Landing    │────▶│   Checkout   │────▶│    Stripe    │────▶│   Success    │
│    Page      │     │    Page      │     │   Checkout   │     │    Page      │
└──────────────┘     └──────────────┘     └──────────────┘     └──────────────┘
       │                    │                    │                    │
       │                    │                    │                    │
       ▼                    ▼                    ▼                    ▼
  Load Raffles        Validate:            Payment           Create Tickets
  from Database       - User logged in     Processing        via Webhook
                      - Tickets available  (Stripe handles)  (Automatic)
                      - Within limits
```

### **Step-by-Step Purchase**

1. **User clicks "Buy Tickets"** on RaffleCard
2. **System checks authentication** - redirects to login if needed
3. **Checkout page loads** - shows raffle details, quantity selector
4. **API validates purchase** - checks availability, user limits
5. **Stripe Checkout Session created** - with metadata (raffle_id, user_id, quantity)
6. **User redirected to Stripe** - secure payment processing
7. **On success → Webhook fires** - `checkout.session.completed`
8. **Tickets created in database** - using metadata from session
9. **User sees success page** - with ticket numbers

---

## 🎯 **Winner Drawing Flow**

```
┌────────────────┐     ┌────────────────┐     ┌────────────────┐
│  Close Sales   │────▶│   Draw Winner  │────▶│ Announce Winner│
│  (Admin)       │     │   (Secure RNG) │     │  (Update DB)   │
└────────────────┘     └────────────────┘     └────────────────┘
       │                      │                      │
       │                      │                      │
       ▼                      ▼                      ▼
 Status → SalesClosed   Select random       Status → Completed
 No more purchases      from confirmed      Winner notified
                        tickets             Prize delivery tracked
```

### **Drawing Process**

```csharp
// 1. Get all confirmed tickets
var confirmedTickets = await _context.Tickets
    .Where(t => t.RaffleId == raffleId && t.Status == TicketStatus.Confirmed)
    .ToListAsync();

// 2. Select random winner (cryptographically secure)
var randomIndex = Random.Shared.Next(confirmedTickets.Count);
var winningTicket = confirmedTickets[randomIndex];

// 3. Mark ticket as winner
winningTicket.Status = TicketStatus.Winner;

// 4. Create winner record
var winner = new Winner
{
    RaffleId = raffleId,
    TicketId = winningTicket.Id,
    AnnouncedAt = DateTime.UtcNow
};

// 5. Update raffle status
raffle.Status = RaffleStatus.Completed;
```

---

## 💳 **Stripe Integration**

### **Dashboard Configuration**

#### **1. Create Products**
In Stripe Dashboard → Products → Add Product:
- **Name**: `{Raffle Title} - Raffle Ticket`
- **Pricing**: One-time, Fixed price
- **Metadata**: 
  - `raffle_id`: Your raffle ID
  - `type`: `raffle_ticket`

#### **2. Configure Webhooks**
In Stripe Dashboard → Developers → Webhooks:
- **Endpoint URL**: `https://yourdomain.com/api/stripe/webhook`
- **Events to listen**:
  - `checkout.session.completed` ✓
  - `checkout.session.async_payment_succeeded` ✓
  - `checkout.session.async_payment_failed` ✓
  - `charge.refunded` ✓

#### **3. Get API Keys**
- **Publishable Key**: `pk_live_...` or `pk_test_...`
- **Secret Key**: `sk_live_...` or `sk_test_...`
- **Webhook Secret**: `whsec_...`

### **appsettings.json Configuration**
```json
{
  "Stripe": {
    "PublishableKey": "pk_test_...",
    "SecretKey": "sk_test_...",
    "WebhookSecret": "whsec_..."
  }
}
```

### **Stripe Service Methods**

| Method | Purpose |
|--------|---------|
| `CreateCustomerAsync()` | Create customer on user registration |
| `CreateRaffleProductAsync()` | Create Stripe product for a raffle |
| `CreateRafflePriceAsync()` | Create price for ticket |
| `CreateRaffleCheckoutSessionAsync()` | Create checkout session with metadata |
| `ArchiveProductAsync()` | Archive product when raffle completes |

---

## 🔒 **Security Considerations**

### **Purchase Validation**
- ✅ User must be logged in
- ✅ Raffle must be in `Active` status
- ✅ Within sales date range
- ✅ Tickets available
- ✅ Within per-user limit
- ✅ Stripe Customer ID linked

### **Webhook Security**
- ✅ Signature verification with `whsec_` secret
- ✅ Idempotent ticket creation
- ✅ Error handling and logging

### **Winner Drawing**
- ✅ Secure random number generation (`Random.Shared`)
- ✅ Only confirmed tickets eligible
- ✅ Atomic transaction for winner selection

---

## 📁 **File Structure**

```
MyBlazorApp/
├── Models/
│   ├── Raffle.cs          # Raffle, RafflePrize, RaffleImage, RaffleStatus
│   ├── Ticket.cs          # Ticket, TicketStatus
│   ├── Winner.cs          # Winner model
│   ├── User.cs            # User with StripeCustomerId
│   └── DTOs/
│       └── RaffleDtos.cs  # DTOs for API responses
│
├── Services/
│   ├── RaffleService.cs       # All raffle business logic
│   ├── StripeService.cs       # Stripe API integration
│   ├── StripeWebhookService.cs # Webhook handling
│   └── AuthService.cs         # Auth + Stripe Customer creation
│
├── Controllers/
│   ├── RafflesController.cs   # Raffle API endpoints
│   ├── TicketsController.cs   # Ticket purchase API
│   └── StripeWebhookController.cs # Webhook endpoint
│
├── Components/
│   ├── RaffleCard.razor       # Raffle display card
│   ├── RaffleNavbar.razor     # Navigation with auth
│   └── Pages/
│       ├── LandingPage.razor  # Dynamic raffle listing
│       ├── RaffleCheckout.razor # Purchase flow
│       ├── MyTickets.razor    # User's tickets
│       └── PaymentSuccess.razor # Post-payment
│
├── Data/
│   └── AppDbContext.cs        # EF Core context
│
└── Database/
    └── SeedRaffles.sql        # Sample data
```

---

## 🚀 **Deployment Checklist**

### **Pre-Launch**
- [ ] Update Stripe keys to production (`pk_live_`, `sk_live_`)
- [ ] Configure production webhook URL
- [ ] Test purchase flow end-to-end
- [ ] Verify webhook signature validation
- [ ] Set up error monitoring

### **Raffle Setup**
- [ ] Create raffle in database with `Status = Draft`
- [ ] Add prizes and images
- [ ] Create Stripe Product and Price
- [ ] Update raffle with `StripeProductId` and `StripePriceId`
- [ ] Set sales dates
- [ ] Change status to `Active`

### **Post-Sale**
- [ ] Close sales (`Status = SalesClosed`)
- [ ] Verify all tickets are confirmed
- [ ] Run winner drawing
- [ ] Contact winner
- [ ] Archive Stripe product

---

## 📈 **Future Enhancements**

- [ ] **Real-time updates** with SignalR for live ticket count
- [ ] **Admin dashboard** for raffle management
- [ ] **Email notifications** for purchase confirmation and winner
- [ ] **Refund handling** through Stripe
- [ ] **Multiple prizes** per raffle (1st, 2nd, 3rd place)
- [ ] **Referral system** for bonus entries
- [ ] **Analytics dashboard** for sales tracking

---

*Last Updated: 2025*
