using MyBlazorApp.Data;
using MyBlazorApp.Models;
using Microsoft.EntityFrameworkCore;

namespace MyBlazorApp.Services;

public class PaymentService
{
    private readonly AppDbContext _context;

    public PaymentService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Create a new payment record in the database
    /// </summary>
    public async Task<Payment> CreatePaymentAsync(string stripeSessionId, string productName, decimal amount, string? customerEmail = null)
    {
        var payment = new Payment
        {
            StripeSessionId = stripeSessionId,
            ProductName = productName,
            Amount = amount,
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            CustomerEmail = customerEmail
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return payment;
    }

    /// <summary>
    /// Get all payments
    /// </summary>
    public async Task<List<Payment>> GetAllPaymentsAsync()
    {
        return await _context.Payments.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// Get a payment by stripe session ID
    /// </summary>
    public async Task<Payment?> GetPaymentBySessionIdAsync(string sessionId)
    {
        return await _context.Payments.FirstOrDefaultAsync(p => p.StripeSessionId == sessionId);
    }

    /// <summary>
    /// Update payment status (used by webhooks)
    /// </summary>
    public async Task<Payment?> UpdatePaymentStatusAsync(string stripeSessionId, string newStatus)
    {
        var payment = await GetPaymentBySessionIdAsync(stripeSessionId);
        if (payment == null)
            return null;

        payment.Status = newStatus;
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
            CompletedPayments = payments.Count(p => p.Status == "completed"),
            PendingPayments = payments.Count(p => p.Status == "pending"),
            FailedPayments = payments.Count(p => p.Status == "failed"),
            TotalAmount = payments.Where(p => p.Status == "completed").Sum(p => p.Amount)
        };
    }
}

public class PaymentStats
{
    public int TotalPayments { get; set; }
    public int CompletedPayments { get; set; }
    public int PendingPayments { get; set; }
    public int FailedPayments { get; set; }
    public decimal TotalAmount { get; set; }
}
