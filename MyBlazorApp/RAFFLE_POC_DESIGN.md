===============================================
    RAFFLE & LOTTERY PLATFORM - POC
    Architectural Design Document
    
    Project: "Mundial Raffle" - Phase 1
===============================================

EXECUTIVE SUMMARY
===============================================

This document outlines the architecture for a Proof of Concept (POC) raffle 
and lottery platform built with Blazor Server .NET 8 and Stripe integration.

The platform will enable a non-profit football organization to:
- Showcase 3 different raffles (tournament tickets + experiences)
- Accept ticket purchases via Stripe payment processing
- Track ticket sales and participant information
- Facilitate winner selection and announcement

Target Launch Timeline:
- April 15: Landing page (marketing mode)
- May 1: Ticket sales enabled
- May 25: Winner announcement

===============================================
REFERENCE ARCHITECTURE
===============================================

Current Stack (MyBlazorApp):
✅ Blazor Server .NET 8 (server-side rendering)
✅ Entity Framework Core 8.0 with MySQL
✅ Stripe payment integration with webhooks
✅ User authentication and session management
✅ Responsive Bootstrap 5 UI

Adaptations for Raffle Platform:
- Modify user model (public users, no admin initially)
- Add Raffle, Ticket, Prize, and Winner entities
- Adapt payment flow for ticket sales
- Simplify checkout (remove account creation requirement)
- Add winner selection logic
- Create public-facing landing page

===============================================
DATABASE SCHEMA
===============================================

Core Tables:

1. RAFFLES
   id (PK)
   Name (varchar)                     // e.g., "Argentina vs France"
   Description (text)                 // Match details
   ImageUrl (varchar)                 // Hero image
   StartDate (datetime)               // When tickets go on sale
   EndDate (datetime)                 // When sales close
   DrawDate (datetime)                // When winner is selected
   TotalTicketsAvailable (int)        // Max tickets for sale
   TicketPrice (decimal)              // Price per ticket
   Status (varchar)                   // open, closed, drawn, completed
   CreatedAt (datetime)
   UpdatedAt (datetime)

2. PRIZES
   id (PK)
   RaffleId (FK)                      // Which raffle
   Name (varchar)                     // "Match Tickets (4)"
   Description (text)                 // Detailed description
   DisplayOrder (int)                 // Order on page
   CreatedAt (datetime)

3. TICKETS
   id (PK)
   RaffleId (FK)
   TicketNumber (int)                 // Sequential: 1, 2, 3...
   ParticipantEmail (varchar)         // Buyer email
   ParticipantName (varchar)          // Buyer name
   ParticipantPhone (varchar)         // Contact info
   StripePaymentIntentId (varchar)    // Payment tracking
   Status (varchar)                   // pending, confirmed, won, refunded
   PurchaseDate (datetime)
   CreatedAt (datetime)
   UpdatedAt (datetime)

4. WINNERS
   id (PK)
   RaffleId (FK)
   TicketId (FK)
   ParticipantEmail (varchar)
   ParticipantName (varchar)
   ParticipantPhone (varchar)
   AnnouncementDate (datetime)
   ClaimDate (datetime)               // When winner confirms they'll claim
   Status (varchar)                   // selected, announced, claimed
   Notes (text)                       // Contact info, trip details
   CreatedAt (datetime)

5. PAYMENTS (reuse from existing)
   id (PK)
   RaffleId (FK)                      // NEW
   TicketId (FK)                      // NEW
   UserId (nullable)                  // Existing
   Amount (decimal)
   Status (varchar)
   StripeSessionId (varchar)
   StripePaymentIntentId (varchar)
   CreatedAt (datetime)
   CompletedAt (datetime)

6. AUDIT_LOG
   id (PK)
   Action (varchar)                   // "ticket_purchased", "winner_announced"
   EntityType (varchar)               // "Raffle", "Ticket", "Winner"
   EntityId (int)
   Details (text)                     // JSON with action details
   CreatedAt (datetime)

Indexes:
- Tickets(RaffleId, Status)
- Tickets(StripePaymentIntentId)
- Winners(RaffleId)
- Payments(RaffleId)
- AuditLog(Action, CreatedAt)

===============================================
COMPONENT ARCHITECTURE
===============================================

Public Pages:

1. LandingPage.razor (/)
   - Hero section with campaign messaging
   - 3 Raffle cards showcasing available raffles
   - Call-to-action buttons
   - No authentication required

2. RaffleDetail.razor (/raffle/{id})
   - Full raffle details and rules
   - Prize list with images
   - Ticket availability counter
   - "Buy Ticket" button (links to checkout)
   - Winner announcement section (after draw date)

3. RaffleCheckout.razor (/raffle/{id}/checkout)
   - Participant information form
     * Name
     * Email
     * Phone
   - Ticket quantity selector
   - Price summary
   - Stripe payment processing
   - Confirmation page

4. RaffleSuccess.razor (/raffle/{id}/success)
   - Confirmation of purchase
   - Ticket number assignment
   - Email sent confirmation
   - Option to buy more tickets

5. WinnerAnnouncement.razor (/raffle/{id}/winner)
   - Display selected winner
   - Prize details
   - Announcement date and time
   - "Claim Prize" option (for winner)

Admin Pages (Phase 2):
- RaffleAdmin.razor (dashboard)
- TicketManagement.razor (view sales)
- WinnerSelection.razor (draw winner)
- AuditLog.razor (compliance)

===============================================
SERVICE LAYER
===============================================

1. RaffleService
   GetAllRafflesAsync()
   GetRaffleByIdAsync(id)
   GetRaffleTicketsAsync(raffleId)
   GetRaffleStatusAsync(raffleId)
   UpdateRaffleStatusAsync(raffleId, status)

2. TicketService
   CreateTicketAsync(raffleId, participantInfo)
   GetTicketsByRaffleAsync(raffleId)
   GetTicketsByParticipantAsync(email)
   GetAvailableTicketsAsync(raffleId)
   ConfirmTicketAsync(ticketId, paymentId)
   RefundTicketAsync(ticketId, reason)

3. WinnerSelectionService
   SelectWinnerAsync(raffleId)           // Random selection from confirmed tickets
   GetWinnerAsync(raffleId)
   AnnounceWinnerAsync(winnerId)
   ClaimPrizeAsync(winnerId)
   GenerateWinnerCertificateAsync(winnerId)

4. RafflePaymentService
   CreateRafflePaymentAsync(raffleId, ticketId, amount)
   ProcessRafflePaymentAsync(paymentIntent)
   GenerateInvoiceAsync(paymentId)
   ProcessRefundAsync(ticketId, reason)

5. AuditService
   LogActionAsync(action, entityType, entityId, details)
   GetAuditLogAsync(filters)
   GenerateComplianceReportAsync(raffleId, startDate, endDate)

===============================================
PAYMENT FLOW
===============================================

Current MyBlazorApp Flow:
User → Product Selection → Price Selection → Stripe Checkout → Success

Adapted Raffle Flow:
User → View Raffle → Buy Ticket → Enter Info → Stripe Payment → Confirm Ticket

Key Differences:
1. No user account required (public access)
2. Simpler checkout (participant form, not user registration)
3. Ticket assigned after payment confirmation via webhook
4. Participant info stored with ticket (not user profile)

Webhook Processing:
Payment Success → Create Ticket Record → Send Confirmation Email → Update Raffle Inventory

Stripe Integration Points:
✅ Existing: Session creation, webhook signature verification
✅ Modify: Webhook handlers to create tickets instead of payments
✅ Add: Ticket confirmation via webhook
✅ Add: Refund processing for ticket cancellations

===============================================
USER FLOWS
===============================================

Flow 1: Purchase Ticket
┌─────────────────────────────────────────────┐
│ User opens LandingPage                      │
│ Sees 3 Raffle cards                         │
│ Clicks "Buy Ticket"                         │
└─────────────────┬───────────────────────────┘
                  ↓
┌─────────────────────────────────────────────┐
│ RaffleCheckout opens                        │
│ User enters:                                │
│ - Name, Email, Phone                        │
│ - Selects quantity (usually 1)              │
│ - Reviews price                             │
└─────────────────┬───────────────────────────┘
                  ↓
┌─────────────────────────────────────────────┐
│ Stripe Checkout loaded                      │
│ User enters card details                    │
│ User clicks "Pay"                           │
└─────────────────┬───────────────────────────┘
                  ↓
┌─────────────────────────────────────────────┐
│ Stripe processes payment                    │
│ Sends webhook: charge.succeeded             │
└─────────────────┬───────────────────────────┘
                  ↓
┌─────────────────────────────────────────────┐
│ App receives webhook                        │
│ Creates Ticket record                       │
│ Assigns ticket number                       │
│ Updates raffle inventory                    │
│ Sends confirmation email                    │
└─────────────────┬───────────────────────────┘
                  ↓
┌─────────────────────────────────────────────┐
│ User redirected to SuccessPage              │
│ Sees ticket number and confirmation         │
└─────────────────────────────────────────────┘

Flow 2: Draw Winner
┌─────────────────────────────────────────────┐
│ Admin (Power Platform) triggers draw        │
│ System selects random confirmed ticket      │
│ Creates Winner record                       │
│ Sends notification to winner email          │
└─────────────────┬───────────────────────────┘
                  ↓
┌─────────────────────────────────────────────┐
│ Winner announcement becomes visible         │
│ Public can see winner on WinnerAnnouncement │
│ Winner receives email with claim instructions
└─────────────────────────────────────────────┘

===============================================
IMPLEMENTATION PHASES
===============================================

PHASE 0: Setup (This Week)
- Create database schema
- Create data models (Raffle, Ticket, Prize, Winner)
- Create DbContext configurations
- Run migrations

PHASE 1: Core Raffle Platform (By April 15)
- Create RaffleService and TicketService
- Build LandingPage.razor (3 raffles)
- Build RaffleDetail.razor
- Build RaffleCheckout.razor
- Adapt Stripe integration for tickets
- Create RaffleSuccess.razor
- Deploy to Azure
- Enable marketing mode (sales closed)

PHASE 2: Ticket Sales (By May 1)
- Enable Stripe payment processing
- Test payment flow end-to-end
- Enable ticket purchasing

PHASE 3: Winner Selection (By May 25)
- Implement WinnerSelectionService
- Create winner announcement page
- Enable winner selection (via admin)
- Send winner notification

===============================================
TECHNICAL CONSIDERATIONS
===============================================

1. Security
   - No authentication required for public
   - Rate limiting on ticket purchases (prevent bot purchases)
   - CAPTCHA on checkout (optional)
   - Validate email ownership (send confirmation link)
   - PCI compliance via Stripe

2. Scalability
   - Expected traffic: 1000-5000 users during campaign
   - Current Blazor Server can handle
   - Consider static site generation for LandingPage
   - Cache raffle data (rarely changes)
   - Optimize ticket availability queries

3. Reliability
   - Implement idempotent webhook handlers (handle duplicates)
   - Implement ticket number generation strategy
   - Backup winner selection before announcing
   - Audit trail for all transactions
   - Database transaction isolation for ticket creation

4. Compliance
   - Document raffle rules clearly
   - Maintain audit log of all transactions
   - Comply with state lottery regulations
   - Store payment records per PCI requirements
   - GDPR compliance for EU participants (if applicable)

5. User Experience
   - Mobile-responsive design (Bootstrap 5)
   - Real-time ticket availability (SignalR)
   - Smooth Stripe integration (no errors)
   - Clear success/failure messages
   - Email confirmations at each step

===============================================
DEPLOYMENT ARCHITECTURE
===============================================

Local Development:
- localhost:7184 with ngrok for testing
- MySQL on localhost:3306
- Stripe test mode keys

Azure Staging:
- App Service (Linux, .NET 8)
- Azure SQL Database
- Azure Key Vault for secrets
- Application Insights for monitoring

Azure Production:
- Same as staging, different resource group
- CDN for static assets (images)
- Azure Database for MySQL
- Traffic Manager for failover

Environment Configuration:
- appsettings.Development.json (local)
- appsettings.Staging.json (Azure Staging)
- appsettings.Production.json (Azure Production)
- Key Vault for secrets (Stripe keys, connection strings)

CI/CD:
- GitHub Actions workflow
- Build on push to main
- Deploy to staging
- Manual approval for production
- Automated smoke tests

===============================================
RAFFLE CONFIGURATION EXAMPLE
===============================================

Raffle 1: "Argentina vs France - Final"
- Match Date: June 23, 2024
- Ticket Price: $50 USD
- Total Tickets: 500
- Status: Open (May 1 - May 24)
- Draw Date: May 25, 8:00 PM EST

Prizes:
1. 4 Match Tickets (Upper Level)
2. Pre-Match Party (VIP Access, drinks, food)
3. Post-Match Celebration (5-star hotel, gourmet dinner)
4. Hotel Stay (5 nights, luxury hotel)
5. Round-trip flights (economy class)

Raffle 2: "Semifinal 1"
- Match Date: June 19, 2024
- Ticket Price: $35 USD
- Total Tickets: 300
- Similar prize structure

Raffle 3: "Semifinal 2"
- Match Date: June 20, 2024
- Ticket Price: $35 USD
- Total Tickets: 300
- Similar prize structure

===============================================
SUCCESS CRITERIA
===============================================

Phase 1 (April 15):
✓ Landing page is live and accessible
✓ All 3 raffles are displayed correctly
✓ Marketing campaign can be launched
✓ No errors in logs

Phase 2 (May 1):
✓ Ticket purchases are working
✓ Payment processing is successful
✓ Confirmation emails are being sent
✓ Database correctly tracks sales
✓ Webhooks are processing successfully

Phase 3 (May 25):
✓ Winner selection works correctly
✓ Winner announcement is public
✓ Winner receives notification
✓ Audit trail is complete
✓ No compliance issues

Overall:
✓ User conversion rate > 5% (users visiting → users purchasing)
✓ Payment success rate > 95%
✓ Zero fraud attempts
✓ All participants receive confirmations
✓ Winner selection is verifiable and fair

===============================================
RISK ASSESSMENT
===============================================

Risk 1: Payment Processing Failures
- Likelihood: Medium
- Impact: High (lost revenue)
- Mitigation: Thorough testing, monitoring, fallback email support

Risk 2: Winner Selection Disputes
- Likelihood: Low
- Impact: High (legal/reputation)
- Mitigation: Transparent algorithm, audit trail, random.org verification

Risk 3: High Traffic During Launch
- Likelihood: Medium
- Impact: Medium (poor UX)
- Mitigation: Load testing, Azure auto-scale, caching

Risk 4: Database Corruption
- Likelihood: Low
- Impact: Critical
- Mitigation: Regular backups, transaction isolation, testing

Risk 5: Stripe API Changes
- Likelihood: Low
- Impact: Medium
- Mitigation: Monitor Stripe docs, update SDK regularly

===============================================
NEXT STEPS
===============================================

Week 1 (This Week):
□ Review this design with client
□ Confirm raffle details and prizes
□ Set up Azure resource group
□ Create database schema
□ Create data models

Week 2:
□ Implement RaffleService
□ Implement TicketService
□ Build LandingPage and RaffleDetail
□ Design raffle cards

Week 3:
□ Build RaffleCheckout
□ Integrate Stripe payment
□ Test payment flow
□ Create success page

Week 4 (April 15):
□ Deploy to Azure
□ Final testing
□ Launch landing page (sales closed)

Week 5-6 (April 23 - May 1):
□ Enable ticket sales
□ Monitor for issues
□ Optimize based on feedback

Week 7 (May 25):
□ Run winner selection
□ Announce winners
□ Final reporting

===============================================
APPENDIX: FILE STRUCTURE
===============================================

New Files to Create:

Models/
  ├── Raffle.cs
  ├── Ticket.cs
  ├── Prize.cs
  ├── Winner.cs
  └── AuditLog.cs

Services/
  ├── RaffleService.cs
  ├── TicketService.cs
  ├── WinnerSelectionService.cs
  ├── RafflePaymentService.cs
  └── AuditService.cs

Components/Pages/
  ├── LandingPage.razor
  ├── RaffleDetail.razor
  ├── RaffleCheckout.razor
  ├── RaffleSuccess.razor
  └── WinnerAnnouncement.razor

Components/
  ├── RaffleCard.razor          (component)
  ├── PrizeList.razor           (component)
  ├── TicketCounter.razor       (component)
  ├── ParticipantForm.razor     (component)
  └── WinnerCard.razor          (component)

Migrations/
  └── AddRaffleModels.cs        (EF Core migration)

===============================================
