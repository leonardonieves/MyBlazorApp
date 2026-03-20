===============================================
    RAFFLE PLATFORM - CODE REUSE STRATEGY
    Leveraging MyBlazorApp Foundation
===============================================

OVERVIEW
===============================================

The Raffle Platform will be built ON TOP of MyBlazorApp, reusing:
- Authentication infrastructure (modified for public access)
- Stripe payment integration (adapted for ticket sales)
- Webhook handling (enhanced for ticket confirmation)
- Database patterns (Entity Framework models)
- UI framework (Bootstrap 5, Blazor components)
- Deployment infrastructure (Azure, CI/CD)

This minimizes development time and risk.

===============================================
REUSABLE COMPONENTS
===============================================

1. STRIPE SERVICE (95% Reusable)
───────────────────────────────────────────

Current MyBlazorApp Implementation:
✓ StripeService.cs
  - GetProductsAsync() → Can reuse for raffle info
  - GetPricesForProductAsync() → Can reuse for ticket prices
  - CreateCheckoutSessionAsync() → Reuse with ticket params
  - GetSessionAsync() → Reuse for payment status

Modifications Needed:
- Change parameter names: Product → Raffle, Price → TicketPrice
- Add ticketQuantity parameter
- Add participantInfo (name, email, phone)
- Return ticket ID along with session

New Methods to Add:
- CreateTicketCheckoutSessionAsync(raffleId, quantity, participantInfo)

Code Impact: ~20% modification, 80% reuse

Example Reuse:
```csharp
// Existing
await StripeService.CreateCheckoutSessionAsync(
    priceId, quantity, successUrl, cancelUrl
);

// Adapted for raffle
await StripeService.CreateTicketCheckoutSessionAsync(
    raffleId, ticketPrice, quantity, 
    new ParticipantInfo { Name, Email, Phone },
    successUrl, cancelUrl
);
```

2. STRIPE WEBHOOK SERVICE (90% Reusable)
───────────────────────────────────────────

Current MyBlazorApp Implementation:
✓ StripeWebhookService.cs
  - ProcessWebhookEventAsync() → Use unchanged
  - VerifyWebhookSignature() → Use unchanged
  - HandleCheckoutSessionCompletedAsync() → Adapt for tickets
  - HandleChargeSucceededAsync() → Adapt for tickets
  - HandleChargeFailedAsync() → Adapt for refunds

Modifications Needed:
- Change payment update logic to ticket confirmation
- Create Ticket record on success
- Update raffle ticket inventory
- Send confirmation email

Example Reuse:
```csharp
// Existing in HandleCheckoutSessionCompletedAsync
payment.Status = "succeeded";

// Modified for raffle
ticket.Status = "confirmed";
raffle.TicketsRemainingCount--;
await _ticketService.CreateTicketAsync(...)
```

Code Impact: ~10% modification, 90% reuse

3. WEBHOOK CONTROLLER (100% Reusable)
───────────────────────────────────────────

Current MyBlazorApp Implementation:
✓ StripeWebhookController.cs
  - POST endpoint: /api/stripe-webhook
  - Signature verification
  - Logging

No modifications needed! Endpoint can serve both:
- Payment payments (MyBlazorApp)
- Ticket purchases (Raffle)

The service layer distinguishes between them.

Code Impact: 0% modification, 100% reuse

4. PAYMENT MODEL (80% Reusable)
───────────────────────────────────────────

Current MyBlazorApp Implementation:
✓ Models/Payment.cs with fields:
  - StripeSessionId
  - StripePaymentIntentId
  - Amount, Currency, Status
  - CreatedAt, UpdatedAt, CompletedAt

Modifications Needed:
Add new fields:
  - RaffleId (FK) → links payment to raffle
  - TicketId (FK) → links payment to ticket

Code Impact: ~20% modification (add 2 fields), 80% reuse

Example Migration:
```csharp
// Up
alter table Payments add RaffleId int;
alter table Payments add TicketId int;
alter table Payments add constraint FK_Payments_Raffles
  foreign key (RaffleId) references Raffles(id);

// Down
alter table Payments drop constraint FK_Payments_Raffles;
alter table Payments drop column RaffleId;
alter table Payments drop column TicketId;
```

5. DATABASE CONTEXT (90% Reusable)
───────────────────────────────────────────

Current MyBlazorApp Implementation:
✓ AppDbContext.cs
  - OnModelCreating() for Payment configuration
  - Migration support
  - Connection pooling

Modifications Needed:
Add DbSet properties:
  - DbSet<Raffle> Raffles
  - DbSet<Ticket> Tickets
  - DbSet<Prize> Prizes
  - DbSet<Winner> Winners
  - DbSet<AuditLog> AuditLogs

Extend OnModelCreating() for new entities.

Code Impact: ~15% addition to existing code, rest unchanged

Example Addition:
```csharp
// Existing (unchanged)
modelBuilder.Entity<Payment>(...);

// New additions
modelBuilder.Entity<Raffle>(entity => {
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Name).IsRequired();
    // ... etc
});

// New DbSet (add to class)
public DbSet<Raffle> Raffles { get; set; }
public DbSet<Ticket> Tickets { get; set; }
```

6. SESSION SERVICE (100% Reusable)
───────────────────────────────────────────

Current MyBlazorApp Implementation:
✓ SessionService.cs (Singleton)

Can reuse for admin/staff authentication.

For public (non-authenticated) users:
- No changes needed
- Raffle purchases don't require user account
- Anonymous participants are identified by email only

Code Impact: 0% modification, 100% reuse

7. URL CONFIGURATION SERVICE (100% Reusable)
──────────────────────────────────────────────

Current MyBlazorApp Implementation:
✓ UrlConfigurationService.cs
  - GetBaseUrl() → Returns ngrok or production URL
  - GetStripeSuccessUrl()
  - GetStripeCancelUrl()

Can reuse directly for raffle redirect URLs:
- Success: /raffle/{raffleId}/success
- Cancel: /raffle/{raffleId}/cancel

Code Impact: 0% modification, 100% reuse

8. BOOTSTRAP UI FRAMEWORK (100% Reusable)
────────────────────────────────────────────

Current MyBlazorApp Implementation:
✓ Bootstrap 5 CSS framework
✓ Layout templates
✓ Responsive design patterns

Raffle pages will follow same design patterns:
- Header/footer (shared across site)
- Card-based layouts (for raffle/prize display)
- Forms (for participant data)
- Tables (for admin features)

Code Impact: 0% modification, design system fully reusable

===============================================
NEW COMPONENTS NEEDED
===============================================

Services to Create (Not in MyBlazorApp):

1. RaffleService
   - GetAllRafflesAsync()
   - GetRaffleByIdAsync()
   - GetRaffleStatusAsync()
   - UpdateRaffleStatusAsync()
   Complexity: LOW (CRUD + status logic)

2. TicketService
   - CreateTicketAsync()
   - ConfirmTicketAsync()
   - GetTicketsAsync()
   - GetParticipantTicketsAsync()
   Complexity: MEDIUM (inventory management)

3. WinnerSelectionService
   - SelectWinnerAsync()
   - VerifyWinnerAsync()
   - AnnounceWinnerAsync()
   Complexity: MEDIUM (random algorithm, fairness)

4. RafflePaymentService
   - CreatePaymentRecordAsync()
   - UpdatePaymentStatusAsync()
   - RefundTicketAsync()
   Complexity: LOW (extends PaymentService)

5. AuditService
   - LogActionAsync()
   - GetAuditLogAsync()
   Complexity: LOW (simple logging)

Pages to Create (Not in MyBlazorApp):

1. LandingPage.razor
   - Hero section
   - Raffle cards (3 raffles)
   - CTA buttons

2. RaffleDetail.razor
   - Raffle information
   - Prize list
   - Ticket availability
   - Rules

3. RaffleCheckout.razor
   - Participant form (name, email, phone)
   - Quantity selector
   - Stripe integration (similar to existing)

4. RaffleSuccess.razor
   - Confirmation message
   - Ticket number display
   - Email confirmation details

5. WinnerAnnouncement.razor
   - Display winner information
   - Prize details
   - Claim form

Components to Create (Not in MyBlazorApp):

1. RaffleCard.razor
   - Display single raffle summary
   - Image, name, price
   - Status badge
   - CTA button

2. PrizeList.razor
   - Display prize details
   - Images, descriptions
   - Order by importance

3. TicketCounter.razor
   - Show tickets remaining
   - Update in real-time

4. ParticipantForm.razor
   - Name, email, phone fields
   - Validation
   - Submit button

5. WinnerCard.razor
   - Display winner information
   - Prize details
   - Contact form

===============================================
MIGRATION PATH
===============================================

Step 1: Extend Existing Models (Week 1)
───────────────────────────────────────

File: Models/Payment.cs
Change:
  - Make StripeSessionId nullable (was required)
  - Add RaffleId (FK)
  - Add TicketId (FK)

Result: No code breakage, backwards compatible

Step 2: Create New Models (Week 1)
───────────────────────────────────

Files to create:
  - Models/Raffle.cs
  - Models/Ticket.cs
  - Models/Prize.cs
  - Models/Winner.cs
  - Models/AuditLog.cs

No impact on existing models.

Step 3: Extend AppDbContext (Week 1)
──────────────────────────────────────

File: Data/AppDbContext.cs
Add:
  - DbSet<Raffle> Raffles
  - DbSet<Ticket> Tickets
  - DbSet<Prize> Prizes
  - DbSet<Winner> Winners
  - DbSet<AuditLog> AuditLogs
  - Entity configurations

Migration: EF Core generates migration automatically

Step 4: Create Services (Week 2-3)
───────────────────────────────────

Files to create:
  - Services/RaffleService.cs
  - Services/TicketService.cs
  - Services/WinnerSelectionService.cs
  - Services/RafflePaymentService.cs
  - Services/AuditService.cs

Register in Program.cs:
```csharp
builder.Services.AddScoped<RaffleService>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<WinnerSelectionService>();
builder.Services.AddScoped<RafflePaymentService>();
builder.Services.AddScoped<AuditService>();
```

Step 5: Modify Stripe Integration (Week 2)
────────────────────────────────────────────

File: Services/StripeService.cs
Add new method (without modifying existing):
```csharp
public async Task<string> CreateTicketCheckoutSessionAsync(
    int raffleId, decimal price, int quantity,
    ParticipantInfo participant, string successUrl, string cancelUrl)
{
    // Implementation similar to CreateCheckoutSessionAsync
    // But with participant info and raffle details
}
```

Existing methods unchanged, fully backward compatible.

Step 6: Modify Webhook Service (Week 2)
─────────────────────────────────────────

File: Services/StripeWebhookService.cs
Modify handlers (add new logic, don't remove existing):
```csharp
private async Task HandleCheckoutSessionCompletedAsync(Event stripeEvent)
{
    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
    
    // Existing logic for Payment
    var payment = await _context.Payments
        .FirstOrDefaultAsync(p => p.StripeSessionId == session.Id);
    if (payment != null) { /* update payment */ }
    
    // NEW: Logic for Ticket
    var ticket = await _context.Tickets
        .FirstOrDefaultAsync(t => t.StripeSessionId == session.Id);
    if (ticket != null) { /* create ticket, update inventory */ }
}
```

Step 7: Create Blazor Pages (Week 3-4)
────────────────────────────────────────

Files to create:
  - Components/Pages/LandingPage.razor (/)
  - Components/Pages/RaffleDetail.razor
  - Components/Pages/RaffleCheckout.razor
  - Components/Pages/RaffleSuccess.razor
  - Components/Pages/WinnerAnnouncement.razor

No impact on existing pages.

Step 8: Create Components (Week 3-4)
──────────────────────────────────────

Files to create:
  - Components/RaffleCard.razor
  - Components/PrizeList.razor
  - Components/TicketCounter.razor
  - Components/ParticipantForm.razor
  - Components/WinnerCard.razor

No impact on existing components.

===============================================
CODE REUSE STATISTICS
===============================================

Existing MyBlazorApp: ~2500 lines of code
- Services: ~800 lines
- Controllers: ~150 lines
- Pages: ~600 lines
- Models: ~300 lines
- Data: ~300 lines
- Other: ~350 lines

Raffle Platform will use:
- Services: ~600 reused, ~400 new = 60% reuse
- Controllers: ~150 reused, 0 new = 100% reuse
- Pages: ~0 reused, ~500 new = 0% reuse
- Models: ~300 reused, ~250 new = 55% reuse
- Data: ~300 reused, ~100 new = 75% reuse
- Other: ~350 reused, ~100 new = 78% reuse

Overall Code Reuse: ~65%
New Code Required: ~1350 lines of code

Development Impact:
- Without reuse: ~3000 lines needed
- With reuse: ~1350 lines needed
- Time savings: ~55% faster development
- Risk reduction: 65% of code already tested
- Quality improvement: Proven patterns

===============================================
CONFIGURATION MANAGEMENT
===============================================

appsettings.json Changes:

Add Raffle section:
```json
"Raffle": {
  "DefaultCurrency": "USD",
  "AdminEmail": "admin@example.com",
  "DrawingAlgorithm": "cryptographic"
}
```

Stripe section (unchanged):
```json
"Stripe": {
  "PublishableKey": "...",
  "SecretKey": "...",
  "WebhookSecret": "..."
}
```

All other config unchanged.

Program.cs Changes:

Minimal additions:
```csharp
// Add new services (after existing services)
builder.Services.AddScoped<RaffleService>();
builder.Services.AddScoped<TicketService>();
// ... etc

// Register configuration options (new)
builder.Services.Configure<RaffleOptions>(
    builder.Configuration.GetSection("Raffle"));
```

Rest of Program.cs unchanged.

===============================================
BRANCHING STRATEGY
===============================================

GitHub branching for development:

main (production, stable)
  └─ develop (staging, integration)
      ├─ feature/raffle-core (models, services)
      ├─ feature/raffle-ui (pages, components)
      ├─ feature/raffle-stripe (payment integration)
      └─ feature/raffle-admin (winner selection)

When ready:
- Merge feature branches to develop
- Test in staging
- Merge develop to main
- Deploy to production

MyBlazorApp remains on main, unaffected.

===============================================
BACKWARDS COMPATIBILITY
===============================================

All existing MyBlazorApp functionality remains unchanged:
✓ User authentication still works
✓ Payment processing for products still works
✓ Dashboard still works
✓ Current APIs unchanged
✓ Current pages unchanged

Raffle platform runs ALONGSIDE MyBlazorApp:
- Same database (separate tables)
- Same authentication (modified for public access)
- Same Stripe account (separate webhook handlers)
- Same hosting (same Azure app)

No breaking changes.

===============================================
TESTING STRATEGY
===============================================

Unit Tests (reuse existing tests):
✓ StripeService tests (unchanged)
✓ AuthService tests (unchanged)
✓ PaymentService tests (extend for raffles)

Unit Tests (new):
- RaffleService tests
- TicketService tests
- WinnerSelectionService tests (CRITICAL)
- AuditService tests

Integration Tests (reuse):
✓ Stripe webhook tests (extend for tickets)

Integration Tests (new):
- Ticket purchase flow
- Winner selection flow
- Email notification flow

End-to-End Tests (new):
- Complete purchase journey
- Winner announcement flow

Load Testing (reuse):
✓ Use existing load test scenarios, add raffle paths

===============================================
DEPLOYMENT STRATEGY
===============================================

Same Azure infrastructure, no new resources needed:
✓ App Service: existing (add new pages)
✓ Database: existing (add new tables)
✓ Azure Storage: existing (add raffle images)
✓ Application Insights: existing (monitor new pages)

CI/CD: GitHub Actions workflow (extend, not replace)
- Build (same)
- Test (add raffle tests)
- Deploy to staging (same)
- Deploy to production (same)

Zero downtime deployment:
- Use database migrations (EF Core handles)
- Feature flags for new pages (optional)
- Gradual rollout if needed

===============================================
MAINTENANCE & SUPPORT
===============================================

Code Review Process (unchanged):
- All new code reviewed by existing team
- Follow existing code standards
- Use existing patterns and conventions

Documentation (extended):
- Update existing README
- Document new services
- Update API documentation

Monitoring (extended):
- Monitor new pages in Application Insights
- Set up alerts for new error types
- Dashboard updated with raffle metrics

Support (coordinated):
- Same support team handles both platforms
- Escalation procedures unchanged
- SLA applies to both

===============================================
