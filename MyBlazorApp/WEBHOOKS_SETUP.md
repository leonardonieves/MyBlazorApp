===============================================
    STRIPE WEBHOOKS CONFIGURATION GUIDE
    Setting up Stripe Webhooks with ngrok
===============================================

WHAT ARE WEBHOOKS?
===============================================
Webhooks are HTTP callbacks that Stripe uses to notify your application
about events like successful payments, refunds, failed charges, etc.

Why they're important:
- User might close browser after paying but before server gets confirmation
- Webhooks guarantee you capture ALL payment events
- Essential for reliable payment processing
- Handle edge cases like retries, refunds, disputes

===============================================
STEP 1: GET YOUR WEBHOOK SECRET
===============================================

1. Go to: https://dashboard.stripe.com
2. Click "Developers" (left sidebar)
3. Click "Webhooks"
4. Click "Add endpoint"
5. Endpoint URL: https://YOUR_NGROK_URL/api/stripe-webhook
   Example: https://391a-212-15-81-157.ngrok-free.app/api/stripe-webhook

6. Events to send: Select "charge.succeeded", "charge.failed", "charge.refunded",
   "payment_intent.succeeded", "payment_intent.payment_failed",
   "checkout.session.completed", "checkout.session.async_payment_succeeded",
   "checkout.session.async_payment_failed"

7. Click "Add endpoint"

8. Click on the new endpoint
9. You'll see "Signing secret" at the bottom
   Click "Reveal" to see it
   Copy the secret (starts with: whsec_...)

===============================================
STEP 2: UPDATE YOUR CONFIGURATION
===============================================

Open appsettings.json and update:

"Stripe": {
    "PublishableKey": "pk_test_...",
    "SecretKey": "sk_test_...",
    "WebhookSecret": "whsec_test_YOUR_SECRET_HERE"  ← Paste your secret here
}

!!!IMPORTANT!!!
Do NOT commit WebhookSecret to git if it contains real keys
Add to .gitignore: appsettings.json

===============================================
STEP 3: START YOUR APP WITH NGROK
===============================================

Terminal 1 - Start ngrok:
ngrok http https://localhost:7184

Terminal 2 - Update and run app:
1. Update appsettings.json with ngrok URL
2. Update appsettings.json with webhook secret
3. dotnet build
4. dotnet run

===============================================
STEP 4: TEST THE WEBHOOK
===============================================

From Stripe Dashboard:
1. Go to Developers → Webhooks
2. Click your endpoint
3. Click "Send test event"
4. Select any event type (e.g., "charge.succeeded")
5. Click "Send test event"

Check your Terminal 2 for logs:
- Should see: "Received Stripe webhook event"
- Should see: "Processing Stripe event: [event type]"
- Should see: "Webhook processed successfully"

If you see errors:
- Check that WebhookSecret in appsettings.json is correct
- Make sure ngrok URL in appsettings.json matches Stripe configuration
- Check app logs in Terminal 2 for detailed error messages

===============================================
STEP 5: TEST WITH REAL PAYMENT
===============================================

1. In your app, go to "New Payment"
2. Select product, price, quantity
3. Click "Proceed to Payment"
4. Use test card: 4242 4242 4242 4242
5. Any future date, any CVC
6. Click "Pay"

Expected flow:
1. Stripe creates session → redirects to checkout
2. User fills payment form → clicks Pay
3. Stripe processes payment
4. Stripe sends webhook to your app: "checkout.session.completed"
5. Your app receives webhook and updates payment status to "succeeded"
6. User is redirected to /payment-success
7. Payment record in database shows status="succeeded"

===============================================
DATABASE UPDATES
===============================================

Your Payments table now has:
- StripeSessionId: identifies the checkout session
- StripePaymentIntentId: identifies the payment intent
- StripeCustomerId: Stripe's customer ID
- StripeInvoiceId: Stripe's invoice ID
- Status: pending, succeeded, processing, requires_action, failed, canceled, refunded
- PaymentMethod: How they paid (card, bank, etc.)
- FailureReason: Why payment failed (if applicable)
- CompletedAt: When payment was completed
- UpdatedAt: Last time payment was updated
- RetryCount: How many times payment was retried
- Notes: Additional metadata

===============================================
WEBHOOK EVENT TYPES HANDLED
===============================================

Your app processes these Stripe events:

1. checkout.session.completed
   → Payment succeeded, mark as "succeeded"

2. checkout.session.async_payment_succeeded
   → Async payment succeeded (e.g., bank transfer, 3D Secure)

3. checkout.session.async_payment_failed
   → Async payment failed, mark as "failed"

4. charge.succeeded
   → Charge was successful, update payment status

5. charge.failed
   → Charge failed, store failure reason

6. charge.refunded
   → Charge was refunded, mark as "refunded"

7. payment_intent.succeeded
   → Payment intent succeeded (confirmation)

8. payment_intent.payment_failed
   → Payment intent failed, store failure reason

===============================================
PRODUCTION NOTES
===============================================

Local Development (current setup):
- Use ngrok for tunneling
- Use Stripe TEST keys (pk_test_, sk_test_)
- Webhooks go through public ngrok URL
- Safe for testing with test cards

Production Setup:
- Use your real domain (not ngrok)
- Use Stripe LIVE keys (pk_live_, sk_live_)
- Configure webhook endpoint with real domain URL
- Use real HTTPS certificate (not self-signed)
- Enable endpoint signature verification (we already do this)
- Handle webhook retries (Stripe will retry for 3 days)
- Log all webhook events for audit trail
- Set up webhook delivery alerts in Stripe Dashboard

===============================================
DEBUGGING WEBHOOKS
===============================================

If webhooks aren't working:

1. Check Stripe Dashboard:
   Developers → Webhooks → Click endpoint
   - Scroll down to "Recent Events"
   - See delivery status (Success/Failed)
   - Click event to see request/response

2. Check your app logs:
   Terminal 2 should show:
   - "Received Stripe webhook event"
   - "Processing Stripe event: [type]"
   - Any error messages

3. Common issues:
   - WebhookSecret is wrong or missing
   - ngrok URL doesn't match Stripe configuration
   - Firewall blocking webhook requests
   - App not running or crashed
   - Signature verification failed

4. Test with curl:
   ngrok gives you an inspect URL (127.0.0.1:4040)
   You can see all requests there

===============================================
STRIPE WEBHOOK ENDPOINT
===============================================

Your endpoint listens on:
POST /api/stripe-webhook

Signature verification:
- Stripe sends "Stripe-Signature" header
- Your app verifies signature using WebhookSecret
- This ensures requests really come from Stripe
- Prevents man-in-the-middle attacks

Request format:
{
  "id": "evt_1234567890",
  "object": "event",
  "type": "charge.succeeded",
  "created": 1234567890,
  "data": {
    "object": { ... event data ... }
  }
}

===============================================
NEXT STEPS
===============================================

1. Get your webhook secret from Stripe Dashboard
2. Update appsettings.json with secret
3. Start ngrok and your app
4. Test webhook with test event from Stripe Dashboard
5. Make a test payment and verify webhook is received
6. Check database to confirm payment status is updated
7. Monitor Stripe Dashboard → Webhooks for delivery status

===============================================
USEFUL LINKS
===============================================

Stripe Webhooks: https://stripe.com/docs/webhooks
Webhook Events: https://stripe.com/docs/api/events
Stripe Testing: https://stripe.com/docs/testing
ngrok Docs: https://ngrok.com/docs

===============================================
