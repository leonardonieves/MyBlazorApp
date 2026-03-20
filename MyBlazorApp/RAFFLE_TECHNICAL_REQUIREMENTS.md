===============================================
    RAFFLE PLATFORM - TECHNICAL REQUIREMENTS
    Detailed Specification
===============================================

FUNCTIONAL REQUIREMENTS
===============================================

FR-1: Raffle Management
FR-1.1: Display multiple raffles (3 for Phase 1)
FR-1.2: Show raffle status (open, closed, drawn, completed)
FR-1.3: Display real-time ticket availability
FR-1.4: Show prize information with images
FR-1.5: Display raffle rules and terms
FR-1.6: Show countdown to raffle close date
FR-1.7: Display winner information (post-draw)

FR-2: Ticket Purchase
FR-2.1: User can purchase 1 or more tickets
FR-2.2: User enters: Name, Email, Phone
FR-2.3: Email validation (send confirmation link)
FR-2.4: Display price summary before payment
FR-2.5: Accept Stripe payments
FR-2.6: Assign unique ticket number to purchase
FR-2.7: Send confirmation email immediately
FR-2.8: Prevent duplicate purchases from same email (optional)

FR-3: Payment Processing
FR-3.1: Integrate Stripe Checkout
FR-3.2: Handle payment webhooks
FR-3.3: Confirm ticket on successful payment
FR-3.4: Send receipt email
FR-3.5: Track payment status (pending, succeeded, failed)
FR-3.6: Support payment retries on failure
FR-3.7: Log all transactions for audit

FR-4: Winner Selection
FR-4.1: Randomly select winner from confirmed tickets
FR-4.2: Verify random selection is cryptographically secure
FR-4.3: Create winner record with timestamp
FR-4.4: Send winner notification email
FR-4.5: Display winner on public page
FR-4.6: Allow winner to claim prize
FR-4.7: Generate winner certificate/proof

FR-5: User Interface
FR-5.1: Responsive design (mobile, tablet, desktop)
FR-5.2: Clear navigation between raffles
FR-5.3: Accessible design (WCAG 2.1 AA standard)
FR-5.4: Fast page load times (<3 seconds)
FR-5.5: Error messages are clear and helpful
FR-5.6: Confirmation screens are prominent

FR-6: Administration
FR-6.1: View all ticket sales (admin dashboard)
FR-6.2: View ticket details by raffle
FR-6.3: View payment status
FR-6.4: Trigger winner selection
FR-6.5: View winner history
FR-6.6: Export sales data (CSV)
FR-6.7: View audit log

FR-7: Communication
FR-7.1: Send confirmation email on ticket purchase
FR-7.2: Send receipt with ticket number
FR-7.3: Send reminder before raffle draw
FR-7.4: Send winner notification
FR-7.5: Allow participants to receive communications

FR-8: Compliance & Audit
FR-8.1: Maintain complete audit trail
FR-8.2: Store payment records per PCI requirements
FR-8.3: GDPR-compliant data handling
FR-8.4: Clear privacy policy
FR-8.5: Terms of service acceptance
FR-8.6: Raffle rules acknowledgment

===============================================
NON-FUNCTIONAL REQUIREMENTS
===============================================

NFR-1: Performance
NFR-1.1: Page load time < 3 seconds
NFR-1.2: Payment processing < 5 seconds
NFR-1.3: Search/filter < 1 second
NFR-1.4: Support 1000 concurrent users
NFR-1.5: Support 5000 total users during campaign

NFR-2: Reliability
NFR-2.1: 99.9% uptime during sales period
NFR-2.2: Automatic failover on database failure
NFR-2.3: Backup every 6 hours
NFR-2.4: Zero data loss on payment failure
NFR-2.5: Idempotent webhook processing (no duplicate tickets)

NFR-3: Security
NFR-3.1: HTTPS only (TLS 1.3)
NFR-3.2: PCI-DSS compliant
NFR-3.3: No credit card data storage
NFR-3.4: Encrypt sensitive data at rest
NFR-3.5: Rate limiting on API endpoints
NFR-3.6: CSRF protection
NFR-3.7: SQL injection prevention
NFR-3.8: XSS prevention

NFR-4: Scalability
NFR-4.1: Auto-scale on traffic spikes
NFR-4.2: Horizontal scaling capability
NFR-4.3: Database query optimization
NFR-4.4: Caching strategy for raffle data
NFR-4.5: CDN for static assets

NFR-5: Maintainability
NFR-5.1: Code follows SOLID principles
NFR-5.2: Comprehensive documentation
NFR-5.3: Unit test coverage > 70%
NFR-5.4: Integration test coverage > 80%
NFR-5.5: Automated deployment process

NFR-6: Accessibility
NFR-6.1: WCAG 2.1 Level AA compliance
NFR-6.2: Keyboard navigation support
NFR-6.3: Screen reader compatible
NFR-6.4: Color contrast ratios met
NFR-6.5: Focus indicators visible

===============================================
DATA REQUIREMENTS
===============================================

Data to Collect:

From Participant:
- Full Name (required)
- Email Address (required, validated)
- Phone Number (required, formatted)
- Country (optional, for international participants)
- Ticket Quantity (1-5, default 1)

From Payment:
- Stripe Payment Intent ID
- Payment Status
- Payment Amount
- Payment Date
- Payment Method
- Transaction ID

From System:
- Ticket ID (unique)
- Ticket Number (sequential per raffle)
- Raffle ID
- Purchase Timestamp
- Confirmation Timestamp
- Email sent status
- IP Address (fraud detection)
- Referral Source (marketing tracking)

Data Retention:
- Keep for 7 years (legal requirement for non-profit)
- Archive after raffle completion
- Delete on GDPR request (except legal requirements)

===============================================
TECHNICAL SPECIFICATIONS
===============================================

Technology Stack:

Frontend:
- Blazor Server (C# .NET 8)
- Bootstrap 5
- JavaScript (minimal, for Stripe)

Backend:
- ASP.NET Core 8.0
- Entity Framework Core 8.0
- RESTful API design

Database:
- MySQL 8.0 (Azure Database for MySQL)
- Stored procedures (if needed for performance)
- Indexes on critical fields

External Services:
- Stripe API (Payment processing)
- SendGrid or Azure Mail Service (Email)
- Azure App Service (Hosting)
- Application Insights (Monitoring)

Code Architecture:

Layering:
  Presentation Layer (Blazor Components)
  └─ Application Layer (Services)
     └─ Domain Layer (Models, Business Logic)
        └─ Data Layer (Entity Framework, Repositories)
           └─ External Layer (Stripe API, Email Service)

Design Patterns:
- Repository Pattern (for data access)
- Service Pattern (for business logic)
- Dependency Injection (for loose coupling)
- Async/Await (for performance)
- Unit of Work Pattern (for transactions)

===============================================
API ENDPOINTS
===============================================

Public Endpoints (No Authentication):

GET /api/raffles
  Returns: List of all raffles with status
  Response: { raffles: [...] }

GET /api/raffles/{id}
  Returns: Detailed raffle information
  Response: { id, name, status, ticketsAvailable, ... }

GET /api/raffles/{id}/prizes
  Returns: List of prizes for raffle
  Response: { prizes: [...] }

POST /api/raffles/{id}/purchase
  Payload: { name, email, phone, quantity }
  Returns: { sessionUrl: "https://stripe.com/..." }

GET /api/raffles/{id}/winner
  Returns: Winner information (if drawn)
  Response: { name, announcementDate, ... }

Protected Endpoints (Admin Only):

POST /api/admin/raffles/{id}/draw-winner
  Trigger: Winner selection
  Returns: { winnerId, ticketNumber, ... }

GET /api/admin/raffles/{id}/tickets
  Returns: All tickets for raffle
  Response: { tickets: [...] }

GET /api/admin/raffles/{id}/payments
  Returns: All payments for raffle
  Response: { payments: [...] }

GET /api/admin/audit-log
  Returns: Audit trail
  Response: { log: [...] }

===============================================
DATABASE SCHEMA DETAILS
===============================================

Raffles Table:
  id                    INT PRIMARY KEY AUTO_INCREMENT
  name                  VARCHAR(255) NOT NULL
  description           TEXT
  imageUrl              VARCHAR(500)
  matchDate             DATETIME
  startDate             DATETIME NOT NULL
  endDate               DATETIME NOT NULL
  drawDate              DATETIME NOT NULL
  totalTicketsAvailable INT NOT NULL
  ticketsRemainingCount INT NOT NULL (denormalized for performance)
  ticketPrice           DECIMAL(10,2) NOT NULL
  currency              VARCHAR(3) DEFAULT 'USD'
  status                ENUM('open','closed','drawn','completed')
  createdAt             DATETIME DEFAULT NOW()
  updatedAt             DATETIME DEFAULT NOW()
  
  INDEX idx_status (status)
  INDEX idx_drawDate (drawDate)

Tickets Table:
  id                      INT PRIMARY KEY AUTO_INCREMENT
  raffleId                INT NOT NULL FK
  ticketNumber            INT NOT NULL
  participantName         VARCHAR(255) NOT NULL
  participantEmail        VARCHAR(255) NOT NULL
  participantPhone        VARCHAR(20)
  participantCountry      VARCHAR(100)
  quantity                INT DEFAULT 1
  stripePaymentIntentId   VARCHAR(255) UNIQUE
  stripeSessionId         VARCHAR(255)
  status                  ENUM('pending','confirmed','won','refunded')
  purchaseDate            DATETIME NOT NULL
  confirmationDate        DATETIME
  emailSentDate           DATETIME
  ipAddress               VARCHAR(45) (IPv4/IPv6)
  referralSource          VARCHAR(100)
  createdAt               DATETIME DEFAULT NOW()
  updatedAt               DATETIME DEFAULT NOW()
  
  UNIQUE idx_raffle_ticket (raffleId, ticketNumber)
  INDEX idx_email (participantEmail)
  INDEX idx_paymentIntent (stripePaymentIntentId)
  INDEX idx_status (status)
  INDEX idx_raffleId (raffleId)

Prizes Table:
  id            INT PRIMARY KEY AUTO_INCREMENT
  raffleId      INT NOT NULL FK
  name          VARCHAR(255) NOT NULL
  description   TEXT
  imageUrl      VARCHAR(500)
  displayOrder  INT
  createdAt     DATETIME DEFAULT NOW()

Winners Table:
  id                INT PRIMARY KEY AUTO_INCREMENT
  raffleId          INT NOT NULL FK
  ticketId          INT NOT NULL FK
  participantName   VARCHAR(255) NOT NULL
  participantEmail  VARCHAR(255) NOT NULL
  participantPhone  VARCHAR(20)
  announcementDate  DATETIME NOT NULL
  claimDate         DATETIME
  status            ENUM('selected','announced','claimed')
  notes             TEXT
  createdAt         DATETIME DEFAULT NOW()
  updatedAt         DATETIME DEFAULT NOW()
  
  UNIQUE idx_raffleWinner (raffleId) (only 1 winner per raffle)
  INDEX idx_status (status)

Payments Table (Reuse existing, extend):
  id                    INT PRIMARY KEY AUTO_INCREMENT
  raffleId              INT FK (NEW)
  ticketId              INT FK (NEW)
  userId                INT FK (nullable)
  amount                DECIMAL(10,2) NOT NULL
  currency              VARCHAR(3) DEFAULT 'USD'
  status                VARCHAR(50) NOT NULL
  stripeSessionId       VARCHAR(255)
  stripePaymentIntentId VARCHAR(255)
  stripeCustomerId      VARCHAR(255)
  paymentMethod         VARCHAR(50)
  failureReason         TEXT
  createdAt             DATETIME DEFAULT NOW()
  updatedAt             DATETIME DEFAULT NOW()
  completedAt           DATETIME
  
  INDEX idx_raffle (raffleId)
  INDEX idx_ticket (ticketId)
  INDEX idx_status (status)

AuditLog Table:
  id          INT PRIMARY KEY AUTO_INCREMENT
  action      VARCHAR(100) NOT NULL (ticket_purchased, winner_selected, etc)
  entityType  VARCHAR(50) (Raffle, Ticket, Winner)
  entityId    INT
  details     JSON (action-specific data)
  userId      INT (who performed action, if admin)
  ipAddress   VARCHAR(45)
  createdAt   DATETIME DEFAULT NOW()
  
  INDEX idx_action (action)
  INDEX idx_createdAt (createdAt)

===============================================
STRIPE INTEGRATION DETAILS
===============================================

Payment Flow:

1. User clicks "Buy Ticket"
   - App creates temporary "pending" Ticket record
   - App redirects to Stripe Checkout

2. User enters payment details
   - Stripe securely processes payment
   - Stripe returns Session ID

3. Payment completed
   - Stripe sends webhook: charge.succeeded
   - Webhook handler:
     * Verifies signature
     * Finds Ticket by StripeSessionId
     * Updates status to "confirmed"
     * Sends confirmation email
     * Updates ticket inventory

4. Payment failed
   - Stripe sends webhook: charge.failed
   - Webhook handler:
     * Updates Ticket status to "failed"
     * Sends failure notification

Webhook Events Processed:

- charge.succeeded
  → Update ticket, send confirmation

- charge.failed
  → Update ticket status, cleanup inventory

- charge.refunded
  → Update ticket, refund inventory

- payment_intent.succeeded
  → Double-check ticket is confirmed

- payment_intent.payment_failed
  → Update status if needed

Webhook Retries:
- Stripe retries for 3 days on failure
- App must handle duplicate webhooks (idempotent)
- Use webhook Event ID to prevent duplicates

Error Handling:
- Webhook signature verification failure → Log, don't process
- Database error → Retry webhook (Stripe will retry)
- Email send failure → Log, don't block webhook response

===============================================
TESTING STRATEGY
===============================================

Unit Tests:
- RaffleService business logic
- TicketService validation
- WinnerSelectionService (verify random algorithm)
- PaymentService calculations

Integration Tests:
- Stripe webhook processing
- Database transactions
- Email sending
- Payment status updates

End-to-End Tests:
- Full purchase flow
- Winner selection
- Winner announcement

Load Testing:
- 1000 concurrent users
- 5000 purchase requests/hour
- Database query performance

Security Testing:
- SQL injection attempts
- XSS prevention
- CSRF protection
- Payment fraud detection

User Acceptance Testing:
- Marketing team reviews pages
- Sample users test purchase flow
- Admin tests management features

===============================================
DEPLOYMENT REQUIREMENTS
===============================================

Azure Resources:

Compute:
- App Service: Linux, B2 SKU minimum
- Auto-scale: 2-10 instances
- Memory: 2GB minimum

Database:
- Azure Database for MySQL
- Flexible Server, Standard_B4ms SKU
- Backup: Daily, 35-day retention

Storage:
- Azure Blob Storage for images
- Azure CDN for static assets

Networking:
- Azure App Gateway (optional, for WAF)
- Private endpoints for database
- Network Security Group rules

Monitoring:
- Application Insights
- Alerts on errors, slow requests
- Log Analytics workspace

CI/CD:
- GitHub Actions
- Automated tests on push
- Staging deployment
- Manual approval for production

===============================================
SUPPORT & MAINTENANCE
===============================================

Pre-Launch Support (April 1-15):
- Daily check-ins
- Issue resolution within 2 hours
- Code review and QA

Launch Week Support (April 15-21):
- 24/7 monitoring
- Immediate response to errors
- Performance optimization

Ongoing Support (During Sales):
- Email support 9am-6pm (timezone)
- 4-hour response time
- Bug fixes deployed same day

Post-Launch Support (After Raffle):
- Report generation
- Data export
- Lessons learned documentation

Maintenance:
- Weekly security updates
- Monthly performance reviews
- Quarterly system audits

===============================================
GLOSSARY
===============================================

Raffle: The overall drawing event with specific match/date
Ticket: Individual entry purchased by participant
Prize: Rewards given to winner(s)
Participant: Person who purchases ticket
Winner: Person selected in random draw
Session (Stripe): Payment session created by Stripe
Payment Intent: Stripe's tracking of payment attempt
Webhook: HTTP callback from Stripe to our app
Audit Log: Record of all system actions for compliance

===============================================
