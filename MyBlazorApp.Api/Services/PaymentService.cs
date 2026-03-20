using MyBlazorApp.Api.Data;
using MyBlazorApp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MyBlazorApp.Api.Services;

public class PaymentService
{
    private readonly AppDbContext _context;

    public PaymentService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Create a new payment record in the database with enhanced information
    /// </summary>
    public async Task<Payment> CreatePaymentAsync(
        int userId,
        string stripeSessionId,
        string productName,
        string productId,
        string priceId,
        decimal amount,
        int quantity,
        string? customerEmail = null,
        string? customerName = null)
    {
        var payment = new Payment
        {
            UserId = userId,
            StripeSessionId = stripeSessionId,
            ProductName = productName,
            ProductId = productId,
            PriceId = priceId,
            Amount = amount,
            Quantity = quantity,
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CustomerEmail = customerEmail,
            CustomerName = customerName,
            Currency = "usd",
            RetryCount = 0
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return payment;
    }

    /// <summary>
    /// Get all payments for a specific user
    /// </summary>
    public async Task<List<Payment>> GetUserPaymentsAsync(int userId)
    {
        return await _context.Payments
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get all payments (admin)
    /// </summary>
    public async Task<List<Payment>> GetAllPaymentsAsync()
    {
        return await _context.Payments
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get a payment by stripe session ID
    /// </summary>
    public async Task<Payment?> GetPaymentBySessionIdAsync(string sessionId)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.StripeSessionId == sessionId);
    }

    /// <summary>
    /// Get a payment by stripe payment intent ID
    /// </summary>
    public async Task<Payment?> GetPaymentByPaymentIntentIdAsync(string paymentIntentId)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntentId);
    }

    /// <summary>
    /// Update payment status with additional details
    /// </summary>
    public async Task<Payment?> UpdatePaymentStatusAsync(
        string stripeSessionId,
        string newStatus,
        string? stripePaymentIntentId = null,
        string? failureReason = null)
    {
        var payment = await GetPaymentBySessionIdAsync(stripeSessionId);
        if (payment == null)
            return null;

        payment.Status = newStatus;
        payment.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(stripePaymentIntentId))
            payment.StripePaymentIntentId = stripePaymentIntentId;

        if (!string.IsNullOrEmpty(failureReason))
            payment.FailureReason = failureReason;

        if (newStatus == "succeeded")
            payment.CompletedAt = DateTime.UtcNow;

        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();

        return payment;
    }

    /// <summary>
    /// Get payment statistics
    /// </summary>
    public async Task<PaymentStats> GetStatsAsync()
    {
        var payments = await _context.Payments.ToListAsync();

        return new PaymentStats
        {
            TotalPayments = payments.Count,
            SucceededPayments = payments.Count(p => p.Status == "succeeded"),
            PendingPayments = payments.Count(p => p.Status == "pending"),
            FailedPayments = payments.Count(p => p.Status == "failed"),
            TotalAmount = payments.Where(p => p.Status == "succeeded").Sum(p => p.Amount)
        };
    }
}

public class PaymentStats
{
    public int TotalPayments { get; set; }
    public int SucceededPayments { get; set; }
    public int PendingPayments { get; set; }
    public int FailedPayments { get; set; }
    public decimal TotalAmount { get; set; }
}
