namespace MyBlazorApp.Api.Models;

public class Payment
{
    public int Id { get; set; }

    // Stripe IDs
    public string? StripeSessionId { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public string? StripeCustomerId { get; set; }
    public string? StripeInvoiceId { get; set; }

    // User and Product Information
    public int UserId { get; set; }
    public string ProductName { get; set; }
    public string? ProductId { get; set; }
    public string? PriceId { get; set; }
    public decimal Amount { get; set; }
    public int Quantity { get; set; }
    public string Currency { get; set; } = "usd";

    // Customer Information
    public string? CustomerEmail { get; set; }
    public string? CustomerName { get; set; }

    // Payment Status
    public string Status { get; set; } // pending, succeeded, processing, requires_action, failed, canceled
    public string? PaymentMethod { get; set; }
    public string? FailureReason { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Metadata
    public string? Notes { get; set; }
    public int? RetryCount { get; set; } = 0;

    // Navigation
    public User User { get; set; }
}

