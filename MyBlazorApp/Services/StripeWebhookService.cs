using Stripe;
using MyBlazorApp.Data;
using MyBlazorApp.Models;
using Microsoft.EntityFrameworkCore;

namespace MyBlazorApp.Services;

/// <summary>
/// Service to handle Stripe webhook events
/// </summary>
public class StripeWebhookService
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;
    private readonly ILogger<StripeWebhookService> _logger;

    public StripeWebhookService(
        IConfiguration configuration,
        AppDbContext context,
        ILogger<StripeWebhookService> logger)
    {
        _configuration = configuration;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Process a Stripe webhook event and validate its signature
    /// </summary>
    public async Task<bool> ProcessWebhookEventAsync(string json, string signatureHeader)
    {
        try
        {
            var endpointSecret = _configuration["Stripe:WebhookSecret"];
            if (string.IsNullOrEmpty(endpointSecret))
            {
                _logger.LogError("Stripe webhook secret not configured");
                return false;
            }

            // Verify the webhook signature
            var stripeEvent = VerifyWebhookSignature(json, signatureHeader, endpointSecret);
            if (stripeEvent == null)
            {
                _logger.LogError("Webhook signature verification failed");
                return false;
            }

            _logger.LogInformation($"Processing Stripe event: {stripeEvent.Type}");

            // Handle different event types
            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                    await HandleCheckoutSessionCompletedAsync(stripeEvent);
                    break;

                case "checkout.session.async_payment_succeeded":
                    await HandleCheckoutSessionAsyncPaymentSucceededAsync(stripeEvent);
                    break;

                case "checkout.session.async_payment_failed":
                    await HandleCheckoutSessionAsyncPaymentFailedAsync(stripeEvent);
                    break;

                case "charge.succeeded":
                    await HandleChargeSucceededAsync(stripeEvent);
                    break;

                case "charge.failed":
                    await HandleChargeFailedAsync(stripeEvent);
                    break;

                case "charge.refunded":
                    await HandleChargeRefundedAsync(stripeEvent);
                    break;

                case "payment_intent.succeeded":
                    await HandlePaymentIntentSucceededAsync(stripeEvent);
                    break;

                case "payment_intent.payment_failed":
                    await HandlePaymentIntentFailedAsync(stripeEvent);
                    break;

                default:
                    _logger.LogInformation($"Unhandled event type: {stripeEvent.Type}");
                    break;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing webhook: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Verify the webhook signature to ensure it comes from Stripe
    /// </summary>
    private Event VerifyWebhookSignature(string json, string signatureHeader, string endpointSecret)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, endpointSecret);
            return stripeEvent;
        }
        catch (StripeException ex)
        {
            _logger.LogError($"Stripe webhook verification failed: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Handle checkout.session.completed event
    /// </summary>
    private async Task HandleCheckoutSessionCompletedAsync(Event stripeEvent)
    {
        try
        {
            var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
            _logger.LogInformation($"Checkout session completed: {session.Id}");

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripeSessionId == session.Id);

            if (payment != null)
            {
                payment.Status = "succeeded";
                payment.StripePaymentIntentId = session.PaymentIntentId;
                payment.UpdatedAt = DateTime.UtcNow;
                payment.CompletedAt = DateTime.UtcNow;

                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Payment {payment.Id} marked as succeeded");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling checkout.session.completed: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle checkout.session.async_payment_succeeded event
    /// </summary>
    private async Task HandleCheckoutSessionAsyncPaymentSucceededAsync(Event stripeEvent)
    {
        try
        {
            var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
            _logger.LogInformation($"Async payment succeeded: {session.Id}");

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripeSessionId == session.Id);

            if (payment != null)
            {
                payment.Status = "succeeded";
                payment.StripePaymentIntentId = session.PaymentIntentId;
                payment.UpdatedAt = DateTime.UtcNow;
                payment.CompletedAt = DateTime.UtcNow;

                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Payment {payment.Id} async payment succeeded");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling async_payment_succeeded: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle checkout.session.async_payment_failed event
    /// </summary>
    private async Task HandleCheckoutSessionAsyncPaymentFailedAsync(Event stripeEvent)
    {
        try
        {
            var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
            _logger.LogInformation($"Async payment failed: {session.Id}");

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripeSessionId == session.Id);

            if (payment != null)
            {
                payment.Status = "failed";
                payment.FailureReason = "Async payment failed";
                payment.UpdatedAt = DateTime.UtcNow;
                payment.RetryCount = (payment.RetryCount ?? 0) + 1;

                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Payment {payment.Id} async payment failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling async_payment_failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle charge.succeeded event
    /// </summary>
    private async Task HandleChargeSucceededAsync(Event stripeEvent)
    {
        try
        {
            var charge = stripeEvent.Data.Object as Charge;
            _logger.LogInformation($"Charge succeeded: {charge.Id}");

            // Find payment by PaymentIntent ID
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == charge.PaymentIntentId);

            if (payment != null)
            {
                payment.Status = "succeeded";
                payment.PaymentMethod = charge.PaymentMethodDetails?.Type ?? "card";
                payment.UpdatedAt = DateTime.UtcNow;
                payment.CompletedAt = DateTime.UtcNow;

                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Payment {payment.Id} confirmed as succeeded via charge event");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling charge.succeeded: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle charge.failed event
    /// </summary>
    private async Task HandleChargeFailedAsync(Event stripeEvent)
    {
        try
        {
            var charge = stripeEvent.Data.Object as Charge;
            _logger.LogInformation($"Charge failed: {charge.Id}");

            // Find payment by PaymentIntent ID
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == charge.PaymentIntentId);

            if (payment != null)
            {
                payment.Status = "failed";
                payment.FailureReason = charge.FailureMessage;
                payment.UpdatedAt = DateTime.UtcNow;
                payment.RetryCount = (payment.RetryCount ?? 0) + 1;

                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Payment {payment.Id} marked as failed due to charge failure");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling charge.failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle charge.refunded event
    /// </summary>
    private async Task HandleChargeRefundedAsync(Event stripeEvent)
    {
        try
        {
            var charge = stripeEvent.Data.Object as Charge;
            _logger.LogInformation($"Charge refunded: {charge.Id}");

            // Find payment by PaymentIntent ID
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == charge.PaymentIntentId);

            if (payment != null)
            {
                payment.Status = "refunded";
                payment.UpdatedAt = DateTime.UtcNow;
                payment.Notes = $"Refunded: {charge.AmountRefunded} cents";

                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Payment {payment.Id} marked as refunded");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling charge.refunded: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle payment_intent.succeeded event
    /// </summary>
    private async Task HandlePaymentIntentSucceededAsync(Event stripeEvent)
    {
        try
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            _logger.LogInformation($"Payment intent succeeded: {paymentIntent.Id}");

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntent.Id);

            if (payment != null)
            {
                payment.Status = "succeeded";
                payment.UpdatedAt = DateTime.UtcNow;
                payment.CompletedAt = DateTime.UtcNow;

                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Payment {payment.Id} confirmed via payment_intent.succeeded");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling payment_intent.succeeded: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle payment_intent.payment_failed event
    /// </summary>
    private async Task HandlePaymentIntentFailedAsync(Event stripeEvent)
    {
        try
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            _logger.LogInformation($"Payment intent failed: {paymentIntent.Id}");

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntent.Id);

            if (payment != null)
            {
                payment.Status = "failed";
                payment.FailureReason = paymentIntent.LastPaymentError?.Message;
                payment.UpdatedAt = DateTime.UtcNow;
                payment.RetryCount = (payment.RetryCount ?? 0) + 1;

                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Payment {payment.Id} marked as failed via payment_intent.payment_failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling payment_intent.payment_failed: {ex.Message}");
        }
    }
}
